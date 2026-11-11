using System;
using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using OutfitStandsPlusForked.Utility;
using RimWorld;
using RimWorldDevTests;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.DevTests;

// Expected behaviour: docs/specs/tailor-outfit-stand.md, "Testing Decisions". Mechanical level: stands are god-mode placed, apparel and
// cloth are developer-spawned and the game's own work giver chooses the mending job; only the swap/auto-use cases order a job
// directly. Observations are item hit points, textile stack counts on the map, the pawn's current job and the stand comp's
// public surface (toggle, stored paid HP). Each case builds its own stand and the previous one is removed first, so the single
// tailor can only ever mend the stand under test.
public class TailorBatch : Batch
{
    private const string Adult = "OutfitStandsPlus_TailorOutfitStand", Kid = "OutfitStandsPlus_TailorKidOutfitStand";
    private const int JobTicks = 2500, MendTicks = 12000;

    private static readonly string[] Textures =
    {
        "Things/Building/OutfitStand/TailorStand_SewingKit_north", "Things/Building/OutfitStand/TailorStand_SewingKit_east",
        "Things/Building/OutfitStand/TailorStand_SewingKit_south", "Things/Building/OutfitStand/TailorOutfitStand_MenuIcon_south",
        "Things/Building/OutfitStand/TailorKidOutfitStand_MenuIcon_south",
    };

    public override string Id => "osp-tailor";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "osp.tailor-defs", "osp.tailor-eligibility", "osp.tailor-threshold", "osp.tailor-limit", "osp.tailor-cheapest",
        "osp.tailor-forbidden", "osp.tailor-interrupt", "osp.tailor-toggle", "osp.tailor-yield-swap", "osp.tailor-yield-autouse",
        "osp.tailor-needs-cloth", "osp.tailor-reload",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    private static JobDef MendJob => OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_MendTailorStand;

    // Everything one case needs: the stand, its comp, the tailor and the area's cells.
    private sealed class Scene
    {
        public CellRect Area;
        public Pawn Tailor;
        public Building_OutfitStand Stand;
        public CompTailorMending Comp;
        public bool SawMend;
        public IntVec3 At(int x, int z) => Area.Min + new IntVec3(x, 0, z);
        public Action Watch => () => { SawMend |= Tailor.CurJob?.def == MendJob; Kit.KeepRested(Tailor)(); };
    }

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        if (index == 1)
        {
            Reloaded(ctx);
            yield break;
        }

        bool biotech = ModsConfig.BiotechActive;
        ctx.Setup($"Odyssey active {ModsConfig.OdysseyActive}; Biotech active {biotech}; {ModsConfig.ActiveModsInLoadOrder.Count()} active mods");
        Kit.Research(ctx, "ComplexClothing");

        // ---- Defs, research, work giver, art, text ------------------------------------------------------------------
        var adultDef = Kit.DefOrNull(Adult);
        var kidDef = Kit.DefOrNull(Kid);
        var giver = DefDatabase<WorkGiverDef>.GetNamedSilentFail("OutfitStandsPlus_MendTailorStand");
        var billGiver = DefDatabase<WorkGiverDef>.GetNamedSilentFail("DoBillsMakeApparel");
        var missingTex = Textures.Where(t => !biotech && t.Contains("Kid") ? false : ContentFinder<Texture2D>.Get(t, false) == null).ToList();
        var prereqs = adultDef?.researchPrerequisites?.Select(r => r.defName).ToList() ?? new List<string>();
        var cats = adultDef?.stuffCategories?.Select(c => c.defName).ToList() ?? new List<string>();
        ctx.Begin("osp.tailor-defs").Expect("adultStand", "loaded").Expect("kidStand", biotech ? "loaded" : "absent")
            .Expect("research", "ComplexClothing only").Expect("stuffCategories", "Metallic, Woody").Expect("workGiver", "Tailoring, after bills")
            .Expect("missingTextures", "none")
            .Observe("adultStand", adultDef != null ? "loaded" : "missing").Observe("kidStand", kidDef != null ? "loaded" : "absent")
            .Observe("research", string.Join(", ", prereqs)).Observe("stuffCategories", string.Join(", ", cats.OrderBy(c => c)))
            .Observe("workGiver", giver == null ? "missing" : $"{giver.workType?.defName} {giver.priorityInType} (bills {billGiver?.priorityInType})")
            .Observe("missingTextures", missingTex.Count == 0 ? "none" : string.Join(", ", missingTex))
            .Check(adultDef != null && (kidDef != null) == biotech && prereqs.Count == 1 && prereqs[0] == "ComplexClothing" &&
                   cats.Count == 2 && cats.Contains("Metallic") && cats.Contains("Woody") &&
                   giver != null && giver.workType?.defName == "Tailoring" && billGiver != null && giver.priorityInType < billGiver.priorityInType &&
                   missingTex.Count == 0,
                "a tailor stand def, its research or stuff, the work giver ordering or a texture is wrong");

        Kit.RemoveStartingColonists(ctx);
        var scene = new Scene { Area = Kit.Area(ctx, 14, 8) };
        scene.Tailor = Kit.Worker(ctx, scene.At(4, 6), DefDatabase<WorkTypeDef>.GetNamed("Tailoring"), SkillDefOf.Crafting);
        var cloth = ThingDefOf.Cloth;
        var leather = Kit.Def("Leather_Plain");
        var devilstrand = Kit.Def("DevilstrandCloth");

        // ---- Only textile apparel below the limit is mended -----------------------------------------------------------
        NewStand(ctx, scene);
        var shirt = Make("Apparel_BasicShirt", 0.3f);
        var pants = Make("Apparel_Pants", 0.6f);
        var vest = Make("Apparel_FlakVest", 0.3f);
        var taintedHat = Make("Apparel_CowboyHat", 0.3f);
        taintedHat.WornByCorpse = true;
        bool added = scene.Stand.AddApparel(shirt) & scene.Stand.AddApparel(pants) & scene.Stand.AddApparel(vest) & scene.Stand.AddApparel(taintedHat);
        Stock(ctx, scene, cloth, 200, 8, 5);
        int vestHp = vest.HitPoints, hatHp = taintedHat.HitPoints;
        if (Kit.Require(ctx, "osp.tailor-eligibility", (added, "shirt, pants, flak vest and tainted hat all went onto the stand")))
        {
            yield return Wait.Until(() => shirt.HitPoints >= shirt.MaxHitPoints && pants.HitPoints >= pants.MaxHitPoints && scene.Tailor.CurJob?.def != MendJob, MendTicks, scene.Watch);
            bool untouched = vest.HitPoints == vestHp && taintedHat.HitPoints == hatHp;
            ctx.Begin("osp.tailor-eligibility").Subject(scene.Tailor)
                .Expect("shirtMended", true).Expect("pantsMended", true).Expect("flakVestUntouched", true).Expect("taintedHatUntouched", true)
                .Observe("shirtMended", shirt.HitPoints >= shirt.MaxHitPoints).Observe("pantsMended", pants.HitPoints >= pants.MaxHitPoints)
                .Observe("flakVestUntouched", vest.HitPoints == vestHp).Observe("taintedHatUntouched", taintedHat.HitPoints == hatHp)
                .Check(shirt.HitPoints >= shirt.MaxHitPoints && pants.HitPoints >= pants.MaxHitPoints && untouched,
                    "a textile item was not mended, or armour or tainted apparel was touched");
        }
        Clear(ctx, scene);

        // ---- Start threshold: above it nothing starts; below it the tailor comes -------------------------------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.95f);
        scene.Stand.AddApparel(shirt);
        Stock(ctx, scene, cloth, 100, 8, 5);
        int aboveHp = shirt.HitPoints;
        yield return Wait.Ticks(900, scene.Watch);
        bool idleAbove = !scene.SawMend && shirt.HitPoints == aboveHp;
        shirt.HitPoints = Mathf.RoundToInt(shirt.MaxHitPoints * 0.8f);
        yield return Wait.Until(() => scene.SawMend, 1500, scene.Watch);
        ctx.Begin("osp.tailor-threshold").Subject(scene.Tailor)
            .Expect("idleAt95Percent", true).Expect("startsAt80Percent", true)
            .Observe("idleAt95Percent", idleAbove).Observe("startsAt80Percent", scene.SawMend)
            .Check(idleAbove && scene.SawMend, "an item above the start threshold was mended, or one below it was ignored");
        Clear(ctx, scene);

        // ---- Mend-up-to limit of 80% ---------------------------------------------------------------------------------
        int oldLimit = OutfitStandsPlusForkedModSettings.TailorMendLimitPercent, oldStart = OutfitStandsPlusForkedModSettings.TailorMendStartPercent;
        try
        {
            OutfitStandsPlusForkedModSettings.TailorMendLimitPercent = 80;
            ctx.Setup("Mod setting TailorMendLimitPercent set to 80 for this case; restored afterwards");
            NewStand(ctx, scene);
            shirt = Make("Apparel_BasicShirt", 0.3f);
            scene.Stand.AddApparel(shirt);
            Stock(ctx, scene, cloth, 100, 8, 5);
            int expected = (int) Math.Ceiling(shirt.MaxHitPoints * 0.8);
            yield return Wait.Until(() => shirt.HitPoints >= expected && scene.Tailor.CurJob?.def != MendJob, MendTicks, scene.Watch);
            yield return Wait.Ticks(400, scene.Watch);
            ctx.Begin("osp.tailor-limit").Subject(scene.Tailor)
                .Expect("finalHitPoints", expected).Observe("finalHitPoints", shirt.HitPoints).Observe("maxHitPoints", shirt.MaxHitPoints)
                .Check(shirt.HitPoints == expected, "mending did not stop exactly at the 80% limit");
            Clear(ctx, scene);
        }
        finally
        {
            OutfitStandsPlusForkedModSettings.TailorMendLimitPercent = oldLimit;
            OutfitStandsPlusForkedModSettings.TailorMendStartPercent = oldStart;
        }

        // ---- Cost: one textile per 10 HP, cheapest textile first -----------------------------------------------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.3f);
        scene.Stand.AddApparel(shirt);
        var clothStack = Stock(ctx, scene, cloth, 100, 8, 4);
        var leatherStack = Stock(ctx, scene, leather, 100, 9, 4);
        var strandStack = Stock(ctx, scene, devilstrand, 50, 10, 4);
        var cheapest = cloth.BaseMarketValue <= leather.BaseMarketValue ? cloth : leather;
        int missing = shirt.MaxHitPoints - shirt.HitPoints, blocks = (missing + 9) / 10;
        var before = new[] { cloth, leather, devilstrand }.ToDictionary(d => d, d => Kit.CountOnMap(ctx.Map, d));
        yield return Wait.Until(() => shirt.HitPoints >= shirt.MaxHitPoints && scene.Tailor.CurJob?.def != MendJob, MendTicks, scene.Watch);
        var used = before.ToDictionary(p => p.Key, p => p.Value - Kit.CountOnMap(ctx.Map, p.Key));
        ctx.Begin("osp.tailor-cheapest").Subject(scene.Tailor)
            .Expect("textileUsed", $"{blocks} {cheapest.defName}, nothing else")
            .Observe("textileUsed", string.Join(", ", used.Select(p => $"{p.Value} {p.Key.defName}")))
            .Observe("missingHp", missing).Observe("marketValues", $"{cloth.BaseMarketValue} cloth, {leather.BaseMarketValue} leather, {devilstrand.BaseMarketValue} devilstrand")
            .Check(used[cheapest] == blocks && used.Where(p => p.Key != cheapest).All(p => p.Value == 0) && shirt.HitPoints >= shirt.MaxHitPoints,
                "mending did not charge one textile per 10 HP from the cheapest stack only");
        Clear(ctx, scene);

        // ---- A forbidden stack is not used ----------------------------------------------------------------------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.3f);
        scene.Stand.AddApparel(shirt);
        var forbiddenCloth = Stock(ctx, scene, cloth, 100, 8, 4);
        forbiddenCloth.SetForbidden(true, false);
        Stock(ctx, scene, leather, 100, 9, 4);
        missing = shirt.MaxHitPoints - shirt.HitPoints;
        blocks = (missing + 9) / 10;
        int clothBefore = Kit.CountOnMap(ctx.Map, cloth), leatherBefore = Kit.CountOnMap(ctx.Map, leather);
        yield return Wait.Until(() => shirt.HitPoints >= shirt.MaxHitPoints && scene.Tailor.CurJob?.def != MendJob, MendTicks, scene.Watch);
        int clothUsed = clothBefore - Kit.CountOnMap(ctx.Map, cloth), leatherUsed = leatherBefore - Kit.CountOnMap(ctx.Map, leather);
        ctx.Begin("osp.tailor-forbidden").Subject(scene.Tailor)
            .Expect("forbiddenClothUsed", 0).Expect("leatherUsed", blocks)
            .Observe("forbiddenClothUsed", clothUsed).Observe("leatherUsed", leatherUsed)
            .Check(clothUsed == 0 && leatherUsed == blocks, "a forbidden textile stack was used, or the allowed one was charged wrongly");
        Clear(ctx, scene);

        // ---- Interruption keeps applied HP and stored paid HP; the next job uses stored HP before charging --------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.3f);
        scene.Stand.AddApparel(shirt);
        Stock(ctx, scene, cloth, 100, 8, 4);
        int hp0 = shirt.HitPoints;
        missing = shirt.MaxHitPoints - hp0;
        blocks = (missing + 9) / 10;
        clothBefore = Kit.CountOnMap(ctx.Map, cloth);
        yield return Wait.Until(() => shirt.HitPoints >= hp0 + 25, MendTicks, scene.Watch);
        scene.Tailor.jobs.EndCurrentJob(JobCondition.InterruptForced);
        int applied = shirt.HitPoints - hp0, stored = scene.Comp.StoredPaidHp, charged = clothBefore - Kit.CountOnMap(ctx.Map, cloth);
        bool ledger = applied >= 25 && stored == charged * TailorMendingUtility.HpPerTextile - applied && stored >= 0 && stored < TailorMendingUtility.HpPerTextile;
        yield return Wait.Until(() => shirt.HitPoints >= shirt.MaxHitPoints && scene.Tailor.CurJob?.def != MendJob, MendTicks, scene.Watch);
        int chargedTotal = clothBefore - Kit.CountOnMap(ctx.Map, cloth);
        ctx.Begin("osp.tailor-interrupt").Subject(scene.Tailor)
            .Expect("storedPaidHpAfterInterrupt", "paid HP minus applied HP").Expect("totalTextileCharged", blocks)
            .Observe("appliedHp", applied).Observe("storedPaidHpAfterInterrupt", stored).Observe("textileChargedAtInterrupt", charged)
            .Observe("totalTextileCharged", chargedTotal).Observe("finalHitPoints", shirt.HitPoints)
            .Check(ledger && chargedTotal == blocks && shirt.HitPoints >= shirt.MaxHitPoints,
                "interrupted mending lost applied HP, lost or invented paid HP, or the next job charged again before using it");
        Clear(ctx, scene);

        // ---- Toggle off prevents automatic jobs; a forced order still mends -------------------------------------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.3f);
        scene.Stand.AddApparel(shirt);
        Stock(ctx, scene, cloth, 100, 8, 4);
        scene.Comp.MendingEnabled = false;
        hp0 = shirt.HitPoints;
        yield return Wait.Ticks(900, scene.Watch);
        bool idleOff = !scene.SawMend && shirt.HitPoints == hp0;
        var forced = TailorMendingUtility.TryMakeMendJob(scene.Tailor, scene.Stand, true, out string why);
        if (forced != null) scene.Tailor.jobs.TryTakeOrderedJob(forced, JobTag.Misc);
        yield return Wait.Until(() => shirt.HitPoints >= shirt.MaxHitPoints, MendTicks, scene.Watch);
        ctx.Begin("osp.tailor-toggle").Subject(scene.Tailor)
            .Expect("idleWhileOff", true).Expect("forcedOrderMends", true)
            .Observe("idleWhileOff", idleOff).Observe("forcedJob", forced == null ? "none: " + why : "made").Observe("forcedOrderMends", shirt.HitPoints >= shirt.MaxHitPoints)
            .Check(idleOff && shirt.HitPoints >= shirt.MaxHitPoints, "a switched-off stand started mending by itself, or the forced order did not mend");
        Clear(ctx, scene);

        // ---- Swap and auto-use take the stand from a mending tailor ----------------------------------------------------
        var swapper = Kit.Worker(ctx, scene.At(2, 6), WorkTypeDefOf.Hauling);
        foreach (var w in YieldCase(ctx, scene, swapper, "osp.tailor-yield-swap", s => s.jobs.StartJob(JobMaker.MakeJob(JobDefOf.UseOutfitStand, scene.Stand), JobCondition.InterruptForced)))
            yield return w;
        foreach (var w in YieldCase(ctx, scene, swapper, "osp.tailor-yield-autouse", s =>
                 {
                     scene.Stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>()?.TryAssignPawn(s);
                     OutfitStandAutoUseUtility.TryStartAssignedStandJob(s);
                 }))
            yield return w;
        swapper.Destroy();

        // ---- Blocked by material --------------------------------------------------------------------------------------
        NewStand(ctx, scene);
        shirt = Make("Apparel_BasicShirt", 0.3f);
        scene.Stand.AddApparel(shirt);
        int loose = ctx.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver).Count(t => t.Spawned && TailorMendingUtility.IsTextileDef(t.def));
        string needsCloth = "OutfitStandsPlusForked_TailorNeedsCloth".Translate().ToString();
        string inspectEmpty = scene.Stand.GetInspectString();
        Stock(ctx, scene, cloth, 50, 8, 4);
        yield return Wait.Ticks(150, scene.Watch);
        string inspectStocked = scene.Stand.GetInspectString();
        ctx.Begin("osp.tailor-needs-cloth").Subject(scene.Tailor)
            .Expect("inspectWithoutTextile", "mentions needs cloth").Expect("inspectWithTextile", "does not")
            .Observe("textileOnMapAfterClear", loose).Observe("inspectWithoutTextile", inspectEmpty.Replace('\n', '|')).Observe("inspectWithTextile", inspectStocked.Replace('\n', '|'))
            .Check(loose == 0 && inspectEmpty.Contains(needsCloth) && !inspectStocked.Contains(needsCloth), "the inspect string did not report the missing textile, or kept reporting it after textile was added");
        Clear(ctx, scene);

        // ---- Save and reload ------------------------------------------------------------------------------------------
        NewStand(ctx, scene);
        scene.Comp.MendingEnabled = false;
        scene.Comp.StoredPaidHp = 7;
        ctx.Remember("stand", scene.Stand.ThingID);
        ctx.Setup($"Developer set stand {scene.Stand.ThingID}: mending off, 7 paid HP stored, then saved and reloaded");
        yield return Wait.Reload();
    }

    // Mends on a stand while a second pawn takes it over with the given action; the tailor must yield and keep its progress.
    private static IEnumerable<Wait> YieldCase(BatchContext ctx, Scene scene, Pawn swapper, string id, Action<Pawn> takeStand)
    {
        NewStand(ctx, scene);
        var shirt = Make("Apparel_BasicShirt", 0.3f);
        var pants = Make("Apparel_Pants", 0.3f);
        scene.Stand.AddApparel(shirt);
        scene.Stand.AddApparel(pants);
        swapper.apparel.DestroyAll();
        ctx.Setup($"Developer stripped {swapper.LabelShort} so the stand's apparel can be worn");
        Stock(ctx, scene, ThingDefOf.Cloth, 200, 8, 4);
        int hp0 = shirt.HitPoints;
        yield return Wait.Until(() => shirt.HitPoints >= hp0 + 5, MendTicks, scene.Watch);
        int hpAtTakeover = shirt.HitPoints;
        bool mending = scene.Tailor.CurJob?.def == MendJob;
        bool yielded = false;
        takeStand(swapper);
        yield return Wait.Until(() => swapper.apparel.WornApparel.Contains(shirt), JobTicks,
            () => { yielded |= scene.Tailor.CurJob?.def != MendJob; Kit.KeepRested(scene.Tailor)(); });
        bool worn = swapper.apparel.WornApparel.Contains(shirt);
        ctx.Begin(id).Subject(scene.Tailor, swapper)
            .Expect("tailorWasMending", true).Expect("tailorYielded", true).Expect("swapperWearsShirt", true).Expect("progressKept", true)
            .Observe("tailorWasMending", mending).Observe("tailorYielded", yielded).Observe("swapperWearsShirt", worn)
            .Observe("hpAtTakeover", hpAtTakeover).Observe("hpAfter", shirt.HitPoints).Observe("progressKept", shirt.HitPoints >= hpAtTakeover)
            .Check(mending && yielded && worn && shirt.HitPoints >= hpAtTakeover,
                "the swap or auto-use path was blocked by, or did not end, the mending job");
        swapper.jobs.EndCurrentJob(JobCondition.InterruptForced);
        swapper.apparel.DestroyAll();
        Clear(ctx, scene, swapper);
    }

    private static void Reloaded(BatchContext ctx)
    {
        string id = ctx.Recall("stand");
        var stand = ctx.Map.listerBuildings.allBuildingsColonist.OfType<Building_OutfitStand>().FirstOrDefault(s => s.ThingID == id);
        var comp = stand?.GetComp<CompTailorMending>();
        ctx.Begin("osp.tailor-reload").Expect("standFound", true).Expect("mendingEnabled", false).Expect("storedPaidHp", 7)
            .Observe("standFound", stand != null).Observe("mendingEnabled", comp?.MendingEnabled).Observe("storedPaidHp", comp?.StoredPaidHp)
            .Check(comp != null && !comp.MendingEnabled && comp.StoredPaidHp == 7, "the tailor stand lost its toggle or stored paid HP across save and reload");
    }

    private static Apparel Make(string def, float hpFraction)
    {
        var thingDef = Kit.Def(def);
        var apparel = (Apparel) ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? ThingDefOf.Cloth : null);
        apparel.HitPoints = Mathf.Max(1, Mathf.RoundToInt(apparel.MaxHitPoints * hpFraction));
        return apparel;
    }

    private static void NewStand(BatchContext ctx, Scene scene)
    {
        scene.Stand = (Building_OutfitStand) Kit.Place(ctx, Adult, scene.At(4, 2), Rot4.South, ThingDefOf.WoodLog);
        scene.Comp = scene.Stand.GetComp<CompTailorMending>();
        scene.SawMend = false;
    }

    private static Thing Stock(BatchContext ctx, Scene scene, ThingDef def, int count, int x, int z)
    {
        var thing = ThingMaker.MakeThing(def);
        thing.stackCount = count;
        GenPlace.TryPlaceThing(thing, scene.At(x, z), ctx.Map, ThingPlaceMode.Near);
        ctx.Setup($"Developer spawned {count} {def.defName} near {scene.At(x, z)}");
        return thing;
    }

    // End the tailor's job, drop the stand and everything on it, and remove every loose textile so cases never share state.
    private static void Clear(BatchContext ctx, Scene scene, Pawn other = null)
    {
        scene.Tailor.jobs.EndCurrentJob(JobCondition.InterruptForced);
        other?.jobs.EndCurrentJob(JobCondition.InterruptForced);
        foreach (var apparel in scene.Stand.HeldItems.OfType<Apparel>().ToList())
        {
            scene.Stand.RemoveApparel(apparel);
            apparel.Destroy();
        }

        scene.Stand.Destroy();
        var textile = ctx.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver).Where(t => t.Spawned && TailorMendingUtility.IsTextileDef(t.def)).ToList();
        foreach (var t in textile) t.Destroy();
        scene.Tailor.carryTracker?.DestroyCarriedThing();
    }
}
