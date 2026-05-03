using System.Linq;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.Utility
{
    public static class OutfitStandAutoUseUtility
    {
        public static bool TryStartAssignedStandJobForPolicyChange(Pawn pawn)
        {
            if (!CanAutoUseStand(pawn) || HasBlockingGearJob(pawn))
            {
                return false;
            }

            Building_OutfitStand stand = FindAssignedStand(pawn);
            if (stand == null)
            {
                return false;
            }

            stand.GetComp<OutfitStandsPlusForkedStandComp>()?.SyncApparelPolicyFilter();
            if (!StandHasUsefulGear(stand, pawn) ||
                !pawn.CanReserveAndReach(stand, PathEndMode.InteractionCell, Danger.Deadly))
            {
                return false;
            }

            Job job = JobMaker.MakeJob(JobDefOf.UseOutfitStand, stand);
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            return true;
        }

        public static Building_OutfitStand FindAssignedStand(Pawn pawn)
        {
            if (pawn?.Map?.listerBuildings == null)
            {
                return null;
            }

            return pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>()
                .FirstOrDefault(stand =>
                {
                    var assignmentComp = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                    return assignmentComp?.AssignedPawns.Contains(pawn) == true;
                });
        }

        private static bool CanAutoUseStand(Pawn pawn)
        {
            return pawn != null &&
                   pawn.Spawned &&
                   !pawn.Dead &&
                   !pawn.Downed &&
                   pawn.IsColonistPlayerControlled &&
                   pawn.jobs != null &&
                   pawn.apparel != null &&
                   pawn.equipment != null &&
                   pawn.outfits?.CurrentApparelPolicy?.filter != null;
        }

        private static bool HasBlockingGearJob(Pawn pawn)
        {
            if (pawn?.jobs == null)
            {
                return true;
            }

            if (IsBlockingGearJob(pawn.jobs.curJob))
            {
                return true;
            }

            return pawn.jobs.jobQueue.Any(queuedJob => IsBlockingGearJob(queuedJob.job));
        }

        private static bool IsBlockingGearJob(Job job)
        {
            if (job?.def == null)
            {
                return false;
            }

            return job.def == JobDefOf.UseOutfitStand ||
                   job.def == OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand ||
                   job.def == JobDefOf.Wear ||
                   job.def.defName == "RemoveApparel" ||
                   job.def.defName == "GearUpAndGo";
        }

        private static bool StandHasUsefulGear(Building_OutfitStand stand, Pawn pawn)
        {
            return stand?.HeldItems != null &&
                   (stand.HeldItems.OfType<Apparel>().Any(apparel => IsUsefulApparel(stand, pawn, apparel)) ||
                    IsUsefulWeapon(stand.HeldWeapon, pawn));
        }

        private static bool IsUsefulApparel(Building_OutfitStand stand, Pawn pawn, Apparel apparel)
        {
            if (apparel == null ||
                apparel.Destroyed ||
                apparel.def?.apparel == null ||
                pawn.outfits?.CurrentApparelPolicy?.filter == null ||
                !pawn.outfits.CurrentApparelPolicy.filter.Allows(apparel) ||
                pawn.apparel.WornApparel.Any(worn => worn.def == apparel.def) ||
                !ApparelUtility.HasPartsToWear(pawn, apparel.def))
            {
                return false;
            }

            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (ApparelUtility.CanWearTogether(apparel.def, worn.def, pawn.RaceProps.body))
                {
                    continue;
                }

                if (pawn.apparel.IsLocked(worn) || !stand.StoreSettings.AllowedToAccept(worn))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsUsefulWeapon(ThingWithComps weapon, Pawn pawn)
        {
            if (weapon == null || pawn?.equipment == null || pawn.equipment.Primary?.def == weapon.def)
            {
                return false;
            }

            return EquipmentUtility.CanEquip(weapon, pawn, out string _);
        }
    }
}
