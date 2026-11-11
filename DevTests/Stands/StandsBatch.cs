using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using OutfitStandsPlusForked.DefOfs;
using RimWorld;
using RimWorldDevTests;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.DevTests;

// Expected behaviour: the stands work the same with or without Odyssey (README "Odyssey"). Without Odyssey the NoOdyssey load
// folder supplies the outfit stand defs, the UseOutfitStand/PutApparelOnOutfitStand jobs, text and art, and a patch lets
// apparel be force-worn onto stands. Mechanical setup: stands are god-mode placed, apparel is put on stands directly and
// jobs are ordered directly; the build menu, hauling and the mod's gizmos are not clicked.
public class StandsBatch : Batch
{
    private const string Plain = "Building_OutfitStand", Kid = "Building_KidOutfitStand";
    private const string Mechanized = "OutfitStandsPlus_MechanizedOutfitStand", MechanizedKid = "OutfitStandsPlus_MechanizedKidOutfitStand";
    private const string Mending = "OutfitStandsPlus_MendingOutfitStand", MendingKid = "OutfitStandsPlus_MendingKidOutfitStand";
    private const int JobTicks = 2500;

    private static readonly string[] LayerTextures =
    {
        "Things/Building/OutfitStand/OutfitStand_Base_north", "Things/Building/OutfitStand/OutfitStand_Body_south",
        "Things/Building/OutfitStand/OutfitStand_BodyChild_south", "Things/Building/OutfitStand/OutfitStand_Head_south",
        "Things/Building/OutfitStand/OutfitStand_MenuIcon_south", "Things/Building/OutfitStand/KidOutfitStand_MenuIcon_south",
        "UI/Commands/SwapOutfits",
    };

    private static readonly string[] TextKeys =
    {
        "ContainedApparelBeauty", "SwapOutfit", "SwapOutfitGizmo", "SwapOutfitDesc", "CannotSwapOutfit", "OutfitStandEmpty",
        "EjectApparelTooltip", "CommandAllowRemovingApparel", "CommandAllowRemovingApparelDesc",
    };

    public override string Id => "osp-stands";
    public override int SegmentCount => 2;
    public override int MapSize => 75;
    public override IReadOnlyList<PlannedCase> Plan { get; } = new[]
    {
        "osp.defs", "osp.art-text", "osp.force-wear", "osp.render", "osp.use-plain", "osp.use-mechanized", "osp.return-mechanized",
        "osp.reload",
    }.Select(id => new PlannedCase(id, Levels.Mechanical)).ToArray();

    public override IEnumerable<Wait> Segment(int index, BatchContext ctx)
    {
        if (index == 1)
        {
            Reloaded(ctx);
            yield break;
        }

        bool odyssey = ModsConfig.OdysseyActive, biotech = ModsConfig.BiotechActive;
        ctx.Setup($"Odyssey active {odyssey}; Biotech active {biotech}; {ModsConfig.ActiveModsInLoadOrder.Count()} active mods");

        // Defs and job bindings.
        var expectedDefs = new List<string> { Plain, Mechanized, MechanizedKid, Mending, MendingKid };
        if (biotech) expectedDefs.Add(Kid);
        var missing = expectedDefs.Where(d => Kit.DefOrNull(d) == null).ToList();
        bool kidAbsentWithoutBiotech = biotech || Kit.DefOrNull(Kid) == null;
        ctx.Begin("osp.defs").Expect("missingDefs", "none").Expect("UseOutfitStand", "bound").Expect("PutApparelOnOutfitStand", "bound")
            .Observe("missingDefs", missing.Count == 0 ? "none" : string.Join(", ", missing))
            .Observe("UseOutfitStand", JobDefOf.UseOutfitStand == null ? "null" : "bound")
            .Observe("PutApparelOnOutfitStand", JobDefOf.PutApparelOnOutfitStand == null ? "null" : "bound")
            .Observe("plainStandMod", Kit.DefOrNull(Plain)?.modContentPack?.PackageId ?? "missing")
            .Observe("kidStandPresent", Kit.DefOrNull(Kid) != null)
            .Check(missing.Count == 0 && JobDefOf.UseOutfitStand != null && JobDefOf.PutApparelOnOutfitStand != null && kidAbsentWithoutBiotech,
                "a stand def or stand job is missing, or the vanilla kid stand loaded without Biotech");

        var missingTex = LayerTextures.Where(p => ContentFinder<Texture2D>.Get(p, false) == null).ToList();
        var missingText = TextKeys.Where(k => !k.CanTranslate()).ToList();
        ctx.Begin("osp.art-text").Expect("missingTextures", "none").Expect("missingKeys", "none")
            .Observe("missingTextures", missingTex.Count == 0 ? "none" : string.Join(", ", missingTex))
            .Observe("missingKeys", missingText.Count == 0 ? "none" : string.Join(", ", missingText))
            .Observe("swapGizmoLabel", "SwapOutfitGizmo".Translate().ToString())
            .Check(missingTex.Count == 0 && missingText.Count == 0, "a stand texture or text key does not resolve");

        Kit.RemoveStartingColonists(ctx);
        var area = Kit.Area(ctx, 18, 8);
        IntVec3 At(int x, int z) => area.Min + new IntVec3(x, 0, z);
        var steel = ThingDefOf.Steel;
        var plain = (Building_OutfitStand)Kit.Place(ctx, Plain, At(2, 2), Rot4.South, steel);
        var mech = (Building_OutfitStand)Kit.Place(ctx, Mechanized, At(5, 2), Rot4.South, steel);
        // Without Biotech the kid stand is out of the build menu but still loads (old saves), so spawn it as a save would.
        var mechKid = biotech
            ? (Building_OutfitStand)Kit.Place(ctx, MechanizedKid, At(8, 2), Rot4.East, steel)
            : SpawnDirect(ctx, MechanizedKid, At(8, 2), Rot4.East, steel);
        var mending = (Building_OutfitStand)Kit.Place(ctx, Mending, At(11, 2), Rot4.North, steel);
        Building_OutfitStand kid = Kit.DefOrNull(Kid) == null ? null : (Building_OutfitStand)Kit.Place(ctx, Kid, At(14, 2), Rot4.South, ThingDefOf.WoodLog);
        Kit.HoldPower(ctx, mech, true);
        Kit.HoldPower(ctx, mending, true);

        var pawn = Kit.Worker(ctx, At(2, 6), WorkTypeDefOf.Hauling);
        // Put apparel on each stand so the render case draws clothing layers too.
        Apparel Make(string def) => (Apparel)ThingMaker.MakeThing(Kit.Def(def), GenStuff.DefaultStuffFor(Kit.Def(def)));
        var plainDuster = Make("Apparel_Duster");
        bool plainAdded = plain.AddApparel(plainDuster);
        var mechHat = Make("Apparel_CowboyHat");
        bool mechAdded = mech.AddApparel(mechHat);
        bool mendingAdded = mending.AddApparel(Make("Apparel_BasicShirt"));
        ctx.Setup($"Developer put a duster on {plain.ThingID} ({plainAdded}), a cowboy hat on {mech.ThingID} ({mechAdded}), a shirt on {mending.ThingID} ({mendingAdded})");

        // Force-wear targeting: core gates stand targets on Odyssey; without Odyssey the mod's patch must re-enable it.
        var targeting = TargetingParameters.ForForceWear(pawn);
        bool canTarget = targeting.CanTarget(new TargetInfo(plain));
        var patchOwners = PatchedOwners();
        ctx.Begin("osp.force-wear").Expect("standIsTarget", true).Expect("patchApplied", !odyssey)
            .Observe("standIsTarget", canTarget).Observe("patchedMethods", patchOwners)
            .Observe("patchApplied", patchOwners != "none")
            .Check(canTarget && (patchOwners != "none") == !odyssey, "stands are not force-wear targets, or the patch state does not match Odyssey");

        // Draw every stand on screen for a while; drawing errors are attributed to this case.
        var render = ctx.Begin("osp.render").Expect("drawn", "all stands visible, no errors").Expect("loggedErrors", 0);
        Find.CameraDriver.JumpToCurrentMapLoc(area.CenterCell);
        long errorMark = CaseErrors.Mark;
        yield return Wait.Frames(90);
        var drawErrors = CaseErrors.Since(errorMark).ToList();
        render.Observe("drawn", "90 frames").Observe("kidStandWithoutBiotech", !biotech ? mechKid.ThingID : "n/a")
            .Observe("loggedErrors", drawErrors.Count).Observe("errors", drawErrors.Count == 0 ? "none" : string.Join(" | ", drawErrors))
            .Check(drawErrors.Count == 0, "errors were logged while the stands were drawn");

        // Vanilla use job on the plain stand: the colonist should end up wearing the duster from the stand.
        pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.UseOutfitStand, plain), JobCondition.InterruptForced);
        yield return Wait.Until(() => pawn.apparel.WornApparel.Contains(plainDuster), JobTicks);
        ctx.Begin("osp.use-plain").Expect("wearingStandDuster", true).Subject(pawn)
            .Observe("wearingStandDuster", pawn.apparel.WornApparel.Contains(plainDuster))
            .Observe("standHolds", string.Join(", ", plain.HeldItems.Select(t => t.def.defName)))
            .Check(pawn.apparel.WornApparel.Contains(plainDuster), "UseOutfitStand did not move the stand's duster onto the colonist");

        // Same vanilla job through the mod's mechanized stand (the mod's DoTransfer prefix handles the swap).
        pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.UseOutfitStand, mech), JobCondition.InterruptForced);
        yield return Wait.Until(() => pawn.apparel.WornApparel.Contains(mechHat), JobTicks);
        ctx.Begin("osp.use-mechanized").Expect("wearingStandHat", true).Subject(pawn)
            .Observe("wearingStandHat", pawn.apparel.WornApparel.Contains(mechHat))
            .Check(pawn.apparel.WornApparel.Contains(mechHat), "UseOutfitStand on the mechanized stand did not equip its hat");

        // The mod's own return job: the hat goes back onto the mechanized stand.
        pawn.jobs.StartJob(JobMaker.MakeJob(OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand, mech), JobCondition.InterruptForced);
        yield return Wait.Until(() => mech.HeldItems.Contains(mechHat), JobTicks);
        ctx.Begin("osp.return-mechanized").Expect("hatOnStand", true).Subject(pawn)
            .Observe("hatOnStand", mech.HeldItems.Contains(mechHat)).Observe("pawnWearing", string.Join(", ", pawn.apparel.WornApparel.Select(a => a.def.defName)))
            .Check(mech.HeldItems.Contains(mechHat), "the return job did not put the hat back on the mechanized stand");

        ctx.Remember("plain", plain.ThingID);
        ctx.Remember("mech", mech.ThingID);
        ctx.Remember("mechKid", mechKid.ThingID);
        ctx.Remember("mending", mending.ThingID);
        yield return Wait.Reload();
    }

    private static Building_OutfitStand SpawnDirect(BatchContext ctx, string defName, IntVec3 cell, Rot4 rot, ThingDef stuff)
    {
        var thing = ThingMaker.MakeThing(Kit.Def(defName), stuff);
        thing.SetFaction(Faction.OfPlayer);
        var spawned = (Building_OutfitStand)GenSpawn.Spawn(thing, cell, ctx.Map, rot);
        TestCamera.Include(ctx.Map, spawned.OccupiedRect());
        ctx.Setup($"Developer spawned {defName} ({spawned.ThingID}) at {cell} facing {rot} directly (not in the build menu without Biotech)");
        return spawned;
    }

    private static string PatchedOwners()
    {
        var methods = Harmony.GetAllPatchedMethods()
            .Where(m => m.DeclaringType != null && (m.DeclaringType.FullName.StartsWith("RimWorld.Building_OutfitStand") || m.DeclaringType.FullName.StartsWith("RimWorld.TargetingParameters")))
            .Where(m => Harmony.GetPatchInfo(m)?.Transpilers.Any(p => p.owner == "khamenman.OutfitStandsPlusForked") == true)
            .Select(m => m.DeclaringType.Name + "." + m.Name).ToList();
        return methods.Count == 0 ? "none" : string.Join(", ", methods);
    }

    private static void Reloaded(BatchContext ctx)
    {
        var ids = new[] { "plain", "mech", "mechKid", "mending" }.Select(k => ctx.Recall(k)).ToList();
        var stands = ctx.Map.listerBuildings.allBuildingsColonist.OfType<Building_OutfitStand>().ToList();
        var found = ids.Where(id => stands.Any(s => s.ThingID == id)).ToList();
        var held = stands.Sum(s => s.HeldItems.Count);
        ctx.Begin("osp.reload").Expect("standsAfterReload", ids.Count).Observe("standsAfterReload", found.Count).Observe("heldItems", held)
            .Check(found.Count == ids.Count, "a stand was lost across save and reload");
    }
}
