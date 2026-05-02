using System;
using System.Collections.Generic;
using OutfitStandsPlus.Utility;
using OutfitStandsPlus.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace OutfitStandsPlus.Jobs
{
    public class JobDriver_OutfitStandsPlusReturnToStand : JobDriver
    {
        private int duration;
        private HashSet<Apparel> wornApparelToTransferToStand;
        private HashSet<Apparel> standApparelToTransferToPawn;

        private Building_OutfitStand OutfitStand => (Building_OutfitStand) this.job.targetA.Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.duration, "duration");
            Scribe_Collections.Look<Apparel>(ref this.wornApparelToTransferToStand, "wornApparelToTransferToStand",
                LookMode.Reference);
            Scribe_Collections.Look<Apparel>(ref this.standApparelToTransferToPawn, "standApparelToTransferToPawn",
                LookMode.Reference);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.pawn.Reserve(this.job.targetA, this.job, errorOnFailed: errorOnFailed);

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            standApparelToTransferToPawn = new HashSet<Apparel>();
            wornApparelToTransferToStand = new HashSet<Apparel>();
            List<Apparel> wornApparel = new List<Apparel>(pawn.apparel.WornApparel);
            List<Apparel> actualApparel = new List<Apparel>(pawn.apparel.WornApparel);
            List<Thing> heldItems = new List<Thing>(OutfitStand.HeldItems);
            List<Thing> actualItems = new List<Thing>(OutfitStand.HeldItems);
            duration = 0;
            foreach (Apparel apparel in wornApparel)
            {
                if (OutfitStand.StoreSettings.AllowedToAccept(apparel) && !pawn.apparel.IsLocked(apparel))
                {
                    wornApparelToTransferToStand.Add(apparel);
                    actualApparel.Remove(apparel);
                    actualItems.Add(apparel);
                    duration += (int) (apparel.GetStatValue(StatDefOf.EquipDelay) * 60.0);
                }
            }

            foreach (Thing thing in heldItems)
            {
                if (!(thing is Apparel apparel)) { continue;}
                var canStoreApparel = true;
                if (OutfitStand.StoreSettings.AllowedToAccept(apparel))
                {
                    foreach (Thing thing1 in actualItems)
                    {
                        if (!(thing1 is Apparel apparel1 && apparel != apparel1)) { continue; }
                        if (!ApparelUtility.CanWearTogether(apparel.def, apparel1.def, this.pawn.RaceProps.body))
                        {
                            canStoreApparel = false;
                            break;
                        }
                    }
                }
                else
                {
                    canStoreApparel = false;
                }
                if (!canStoreApparel)
                {
                    standApparelToTransferToPawn.Add(apparel);
                    actualItems.Remove(apparel);
                    actualApparel.Add(apparel);
                }
            }
            
            var mainComp = OutfitStand.GetComp<OutfitStandsPlusMainComp>();
            if (mainComp != null)
            {
                var newDuration = mainComp.ApplyEquipSpeedFactor(duration);
                // Log.Warning(duration + " => " + newDuration);
                duration = newDuration;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_OutfitStandsPlusReturnToStand f = this;
            f.FailOnBurningImmobile<JobDriver_OutfitStandsPlusReturnToStand>(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
                .FailOnDespawnedNullOrForbidden<Toil>(TargetIndex.A);
            Toil toil1 = ToilMaker.MakeToil(nameof(MakeNewToils));
            toil1.WithProgressBarToilDelay(TargetIndex.A);
            toil1.defaultCompleteMode = ToilCompleteMode.Delay;
            toil1.defaultDuration = f.duration;
            yield return toil1;
            Toil toil2 = ToilMaker.MakeToil(nameof(MakeNewToils));
            toil2.AddFinishAction(new Action(f.DoTransfer));
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }

        private void DoTransfer()
        {
            OutfitStandTransferUtility.ReturnToStand(pawn, OutfitStand);
        }
        private static bool PawnCanWieldWeapon(Thing weapon, Pawn pawn) =>
            (!weapon.def.IsWeapon || !pawn.WorkTagIsDisabled(WorkTags.Violent)) &&
            (!weapon.def.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) &&
            (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
             (!pawn.IsQuestLodger() || EquipmentUtility.QuestLodgerCanEquip(weapon, pawn))) &&
            EquipmentUtility.CanEquip(weapon, pawn, out string _);
    }
}
