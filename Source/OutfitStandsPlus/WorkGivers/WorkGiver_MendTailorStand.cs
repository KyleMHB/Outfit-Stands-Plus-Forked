using System.Collections.Generic;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.Utility;
using RimWorld;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.WorkGivers
{
    /// <summary>
    /// Tailoring work: mend the textile apparel stored on a tailor's outfit stand. Ordered after the tailoring-bench
    /// bill giver (see the WorkGiverDef) so repairs never starve clothing production.
    /// </summary>
    public class WorkGiver_MendTailorStand : WorkGiver_Scanner
    {
        private static ThingDef adultDef, kidDef;

        private static ThingDef AdultDef => adultDef ?? (adultDef = DefDatabase<ThingDef>.GetNamedSilentFail(TailorMendingUtility.AdultStandDefName));
        private static ThingDef KidDef => kidDef ?? (kidDef = DefDatabase<ThingDef>.GetNamedSilentFail(TailorMendingUtility.KidStandDefName));

        // Both stand defs share the interaction-cell setting, so the adult def speaks for the scanner.
        public override PathEndMode PathEndMode => TailorMendingUtility.StandPathEndMode(AdultDef);

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            var buildings = pawn.Map?.listerBuildings;
            return buildings == null ||
                   !(AdultDef != null && buildings.ColonistsHaveBuilding(AdultDef)) &&
                   !(KidDef != null && buildings.ColonistsHaveBuilding(KidDef));
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var building in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>())
            {
                if (TailorMendingUtility.IsTailorStand(building))
                {
                    yield return building;
                }
            }
        }

        public override ReservationLayerDef GetReservationLayer(Pawn pawn, LocalTargetInfo target)
        {
            return OutfitStandsPlusForkedReservationLayerDefOf.OutfitStandsPlus_Mending;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobOnThing(pawn, t, forced) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return t is Building_OutfitStand stand
                ? TailorMendingUtility.TryMakeMendJob(pawn, stand, forced, out _)
                : null;
        }
    }
}
