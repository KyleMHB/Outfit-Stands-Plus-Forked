# Runs production mending classes against game doubles. The 1.7 scheduler reproduces
# TickList bucket selection and Thing.DoTick's holder gate from 1.7.4982 rev1280.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$stubs = @"
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
namespace Verse {
    public enum TickerType { Never, Normal, Rare }
    public class ThingDef { public TickerType tickerType; }
    public interface IExposable { void ExposeData(); }
    public interface IThingHolder {}
    public class CompProperties { public Type compClass; }
    public class ThingComp { public Thing parent; public virtual void CompTick() {} }
    public class Thing {
        public ThingDef def = new ThingDef();
        public int thingIDNumber;
        public bool Spawned = true;
        public int HitPoints = 50;
        public int MaxHitPoints = 100;
        public int RareCalls;
        public List<ThingComp> AllComps = new List<ThingComp>();
        public virtual void TickRare() { RareCalls++; }
        public virtual void ExposeData() {}
        public void DoTick(int tick, bool version17) {
            if (def.tickerType == TickerType.Normal) {
                foreach (var comp in AllComps) comp.CompTick();
            } else if (def.tickerType == TickerType.Rare &&
                (!version17 || !(this is IThingHolder) || unchecked(tick + HashOffset(thingIDNumber)) % 250 == 0)) {
                TickRare();
            }
        }
        public static int HashOffset(int seed) {
            unchecked { return (int)(seed ^ (169495093 + 2654435769u + (seed << 6) + (seed >> 2))); }
        }
    }
    public static class CollectionExtensions { public static bool Empty<T>(this List<T> list) => list.Count == 0; }
    public static class LoadedModManager {
        public static T GetMod<T>() where T:new() => new T();
    }
    public static class Scribe_Values {
        public static bool Loading;
        public static Dictionary<string,int> Values = new Dictionary<string,int>();
        public static void Look(ref int value, string key, int defaultValue) {
            if (Loading) value = Values.TryGetValue(key, out var saved) ? saved : defaultValue;
            else Values[key] = value;
        }
    }
    public static class Scribe_Deep {
        public static void Look<T>(ref T value, string key, params object[] args) where T:IExposable {
            value.ExposeData();
        }
    }
}
namespace RimWorld {
    public class CompPowerTrader { public bool PowerOn = true; }
    public class Building_OutfitStand : Thing, IThingHolder {
        public List<Thing> HeldItems = new List<Thing>();
        public CompPowerTrader Power = new CompPowerTrader();
        public T GetComp<T>() where T:class => Power as T;
    }
    public class Building_KidOutfitStand : Building_OutfitStand {}
}
namespace OutfitStandsPlusForked {
    public class OutfitStandsPlusForkedMod {
        public T GetSettings<T>() where T:new() => new T();
    }
    public class OutfitStandsPlusForkedModSettings {
        public int RareTicksPerMend = TestSettings.RareTicksPerMend;
    }
    public static class TestSettings { public static int RareTicksPerMend = 10; }
}
"@
$production = @('Things\MechanizedOutfitStand.cs','Things\MendingOutfitStand.cs') | ForEach-Object {
    (Get-Content -Raw -LiteralPath (Join-Path $root "Source\OutfitStandsPlus\$_")) -replace '(?m)^using .*;\r?\n',''
}
Add-Type -TypeDefinition ($stubs + ($production -join "`n")) -CompilerOptions '/nowarn:0649,0414'
[xml]$defs = Get-Content -Raw -LiteralPath (Join-Path $root 'Defs\ThingDefs\Building_MendingOutfitStand.xml')
$baseDef = $defs.Defs.ThingDef | Where-Object { $_.Name -eq 'OutfitStandsPlusForked_MendingOutfitStandBase' }
$script:checks = 0
function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) { throw "FAIL: $message. Expected $expected, got $actual" }
    $script:checks++
}
function New-Stand([string]$kind, [int]$id=1) {
    $stand = New-Object "OutfitStandsPlusForked.Things.$kind"
    $stand.thingIDNumber = $id
    $stand.def.tickerType = [Verse.TickerType]([string]$baseDef.tickerType)
    foreach ($node in $baseDef.comps.li) {
        $type = ([string]$node.Class) -as [type]
        if ($null -ne $type -and [Verse.CompProperties].IsAssignableFrom($type)) {
            $props = [Activator]::CreateInstance($type)
            $comp = [Activator]::CreateInstance($props.compClass)
            $comp.parent = $stand
            $stand.AllComps.Add($comp)
        }
    }
    $stand.HeldItems.Add([Verse.Thing]::new())
    return $stand
}
function Step-Stand($stand, [int]$count, [bool]$version17=$true, [int]$start=0) {
    for ($tick=$start; $tick -lt $start+$count; $tick++) {
        if ($stand.def.tickerType -eq [Verse.TickerType]::Normal -or $tick % 250 -eq $stand.thingIDNumber % 250) {
            $stand.DoTick($tick, $version17)
        }
    }
}
foreach ($kind in @('MendingOutfitStand','MendingKidOutfitStand')) {
    foreach ($version17 in @($false,$true)) {
        $stand = New-Stand $kind
        Step-Stand $stand 25000 $version17
        Assert-Equal 60 $stand.HeldItems[0].HitPoints "$kind repairs through $(if($version17){'1.7'}else{'1.6'}) scheduler"
    }
    $stand = New-Stand $kind
    Step-Stand $stand 2499
    Assert-Equal 50 $stand.HeldItems[0].HitPoints "$kind waits for full repair interval"
    Step-Stand $stand 1 $true 2499
    Assert-Equal 51 $stand.HeldItems[0].HitPoints "$kind repairs after 2500 game ticks"
    $stand.Power.PowerOn = $false
    Step-Stand $stand 5000
    Assert-Equal 51 $stand.HeldItems[0].HitPoints "$kind pauses without power"
    $stand.Power.PowerOn = $true
    Step-Stand $stand 2500
    Assert-Equal 52 $stand.HeldItems[0].HitPoints "$kind resumes with power"
    $stand.Power = $null
    Step-Stand $stand 2500
    Assert-Equal 52 $stand.HeldItems[0].HitPoints "$kind skips missing power comp"
    $stand = New-Stand $kind
    $stand.Spawned = $false
    Step-Stand $stand 25000
    Assert-Equal 50 $stand.HeldItems[0].HitPoints "$kind skips unspawned stands"
    $stand = New-Stand $kind
    $stand.HeldItems[0].HitPoints=100
    $second=[Verse.Thing]::new(); $second.HitPoints=25; $stand.HeldItems.Add($second)
    Step-Stand $stand 2500
    Assert-Equal 100 $stand.HeldItems[0].HitPoints "$kind leaves full items unchanged"
    Assert-Equal 26 $second.HitPoints "$kind repairs most damaged item"
    $stand.HeldItems.Clear()
    Step-Stand $stand 2500
    $stand = New-Stand $kind
    Step-Stand $stand 1375
    [Verse.Scribe_Values]::Values.Clear(); [Verse.Scribe_Values]::Loading=$false
    $stand.ExposeData()
    Assert-Equal 5 ([Verse.Scribe_Values]::Values['OutfitStandsPlusMendTickCounter']) "$kind preserves old counter units"
    $restored = New-Stand $kind
    [Verse.Scribe_Values]::Loading=$true; $restored.ExposeData(); [Verse.Scribe_Values]::Loading=$false
    Step-Stand $restored 1124
    Assert-Equal 50 $restored.HeldItems[0].HitPoints "$kind retains partial interval after reload"
    Step-Stand $restored 1
    Assert-Equal 51 $restored.HeldItems[0].HitPoints "$kind resumes saved progress exactly"
    [Verse.Scribe_Values]::Values.Clear(); [Verse.Scribe_Values]::Values['OutfitStandsPlusMendTickCounter']=9
    $restored = New-Stand $kind
    [Verse.Scribe_Values]::Loading=$true; $restored.ExposeData(); [Verse.Scribe_Values]::Loading=$false
    Step-Stand $restored 250
    Assert-Equal 51 $restored.HeldItems[0].HitPoints "$kind loads legacy saved progress"
    [OutfitStandsPlusForked.TestSettings]::RareTicksPerMend=2
    $stand = New-Stand $kind
    Step-Stand $stand 500
    Assert-Equal 51 $stand.HeldItems[0].HitPoints "$kind honors configured repair speed"
    [OutfitStandsPlusForked.TestSettings]::RareTicksPerMend=10
    foreach ($id in 1..1000) {
        $stand = New-Stand $kind $id
        Step-Stand $stand 2500
        Assert-Equal 51 $stand.HeldItems[0].HitPoints "$kind ticks for ID $id"
    }
}
"PASS: $script:checks mending checks, including both schedulers, 2000 stand IDs, power, saved progress and repair settings."
