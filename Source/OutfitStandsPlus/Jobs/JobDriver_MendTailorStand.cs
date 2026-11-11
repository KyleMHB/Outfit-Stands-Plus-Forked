using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using OutfitStandsPlusForked.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.Jobs
{
    /// <summary>
    /// A tailor mends the textile apparel on a tailor's outfit stand: fetch textile for the whole stand, walk to the stand,
    /// then restore one hit point at a time, paying one textile per ten hit points at the start of each block. Applied
    /// hit points and the unspent part of a paid block (kept on <see cref="CompTailorMending"/>) survive any interruption.
    /// TargetA is the stand, TargetB the first textile stack, <c>job.count</c> the textile to carry.
    /// </summary>
    public class JobDriver_MendTailorStand : JobDriver
    {
        private const int MaxGatherSteps = 8;
        private const int ContestCheckInterval = 10;
        private const float SkillXpPerTick = 0.1f;

        private ThingDef textileDef;
        private float progress;
        private int lastTick;
        private int gatherSteps;
        private float speed;
        private Apparel current;

        private Building_OutfitStand Stand => (Building_OutfitStand) job.targetA.Thing;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref textileDef, "textileDef");
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref gatherSteps, "gatherSteps");
        }

        public override string GetReport()
        {
            return "OutfitStandsPlusForked_MendClothesReport".Translate(job.targetA.Thing?.LabelShort ?? string.Empty);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            var layer = OutfitStandsPlusForkedReservationLayerDefOf.OutfitStandsPlus_Mending;
            if (!pawn.Reserve(job.targetA, job, 1, -1, layer, errorOnFailed))
            {
                return false;
            }

            var stack = job.targetB.Thing;
            return stack == null ||
                   pawn.Reserve(job.targetB, job, 1, Mathf.Max(1, Mathf.Min(job.count, stack.stackCount)), null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            // The stand is reserved on a private layer, so a swap, return or auto-use never waits for us: when another pawn
            // takes the default reservation, mending ends here and the work giver restarts it later.
            AddFailCondition(() =>
                (Find.TickManager.TicksGame + pawn.thingIDNumber) % ContestCheckInterval == 0 &&
                TailorMendingUtility.IsStandInUseByOthers(Stand, pawn));
            AddFinishAction(condition => DropLeftovers());

            var findStack = ToilMaker.MakeToil("FindTextile");
            var gotoStack = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B);
            var pickUp = ToilMaker.MakeToil("PickUpTextile");
            var gotoStand = Toils_Goto.GotoThing(TargetIndex.A, TailorMendingUtility.StandPathEndMode(job.targetA.Thing));
            var work = ToilMaker.MakeToil("MendOnStand");

            findStack.defaultCompleteMode = ToilCompleteMode.Instant;
            findStack.initAction = () =>
            {
                if (textileDef == null && job.targetB.Thing != null)
                {
                    textileDef = job.targetB.Thing.def;
                }

                int want = job.count - (pawn.carryTracker.CarriedThing?.stackCount ?? 0);
                if (textileDef == null || want <= 0 || gatherSteps++ >= MaxGatherSteps)
                {
                    JumpToToil(gotoStand);
                    return;
                }

                var stack = job.targetB.Thing;
                if (stack == null || stack.Destroyed || !stack.Spawned || stack.def != textileDef || stack.IsForbidden(pawn))
                {
                    stack = TailorMendingUtility.FindTextileOfDef(pawn, textileDef);
                }

                if (stack == null ||
                    !pawn.Reserve(stack, job, 1, Mathf.Max(1, Mathf.Min(want, stack.stackCount)), null, false))
                {
                    JumpToToil(gotoStand);
                    return;
                }

                job.SetTarget(TargetIndex.B, stack);
            };

            pickUp.defaultCompleteMode = ToilCompleteMode.Instant;
            pickUp.initAction = () =>
            {
                var stack = job.targetB.Thing;
                int have = pawn.carryTracker.CarriedThing?.stackCount ?? 0;
                if (stack == null || !stack.Spawned)
                {
                    JumpToToil(gotoStand);
                    return;
                }

                int take = Mathf.Min(job.count - have, stack.stackCount, pawn.carryTracker.AvailableStackSpace(stack.def));
                int taken = take > 0 ? pawn.carryTracker.TryStartCarry(stack, take, false) : 0;
                if (taken <= 0)
                {
                    JumpToToil(gotoStand);
                    return;
                }

                if (have + taken < job.count)
                {
                    JumpToToil(findStack);
                }
            };

            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.handlingFacing = true;
            work.activeSkill = () => SkillDefOf.Crafting;
            work.initAction = () =>
            {
                lastTick = Find.TickManager.TicksGame;
                speed = 0f;
            };
            work.tickAction = MendTick;

            yield return findStack;
            yield return gotoStack;
            yield return pickUp;
            yield return gotoStand;
            yield return work;
        }

        private void MendTick()
        {
            var stand = Stand;
            var comp = stand.GetComp<CompTailorMending>();
            int now = Find.TickManager.TicksGame;
            int delta = Mathf.Clamp(now - lastTick, 1, 60);
            lastTick = now;
            pawn.rotationTracker.FaceTarget(stand);

            // Pick the most damaged item that is still below the "mend up to" limit.
            if (current == null || current.Destroyed || !stand.HeldItems.Contains(current) || !TailorMendingUtility.BelowLimit(current))
            {
                current = TailorMendingUtility.EligibleItems(stand, true).FirstOrDefault();
                if (current == null)
                {
                    ReadyForNextToil();
                    return;
                }
            }

            // Pay for a 10 HP block when none is left over from an earlier visit.
            if (comp.StoredPaidHp <= 0)
            {
                if (!ConsumeOneTextile())
                {
                    ReadyForNextToil();
                    return;
                }

                comp.StoredPaidHp += TailorMendingUtility.HpPerTextile;
            }

            if (speed <= 0f || now % 30 == 0)
            {
                speed = Mathf.Max(0.01f, pawn.GetStatValue(StatDefOf.GeneralLaborSpeed));
            }

            pawn.skills?.Learn(SkillDefOf.Crafting, SkillXpPerTick * delta);
            progress += speed * delta;
            while (progress >= TailorMendingUtility.TicksPerHp && comp.StoredPaidHp > 0 && TailorMendingUtility.BelowLimit(current))
            {
                progress -= TailorMendingUtility.TicksPerHp;
                current.HitPoints++;
                comp.StoredPaidHp--;
            }

            progress = Mathf.Min(progress, TailorMendingUtility.TicksPerHp);
        }

        private bool ConsumeOneTextile()
        {
            var carried = pawn.carryTracker.CarriedThing;
            if (carried == null || carried.stackCount <= 0 || !TailorMendingUtility.IsTextileDef(carried.def))
            {
                return false;
            }

            carried.SplitOff(1).Destroy();
            return true;
        }

        private void DropLeftovers()
        {
            if (pawn?.carryTracker?.CarriedThing != null && pawn.Spawned)
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
            }
        }
    }
}
