using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.Utility
{
    /// <summary>
    /// The one shared query behind the tailor's outfit stand: which stored items are eligible, how much textile the stand
    /// needs and which textile to use. The work giver, the job driver, the float menu, the inspect string and the dev tests
    /// all read it, so they cannot disagree.
    /// </summary>
    public static class TailorMendingUtility
    {
        public const string AdultStandDefName = "OutfitStandsPlus_TailorOutfitStand";
        public const string KidStandDefName = "OutfitStandsPlus_TailorKidOutfitStand";

        /// <summary>Hit points one textile pays for.</summary>
        public const int HpPerTextile = 10;

        /// <summary>Ticks per restored hit point at General labour speed 1.</summary>
        public const float TicksPerHp = 30f;

        // ---- Settings -------------------------------------------------------------------------------------------------
        public static int LimitPercent => Mathf.Clamp(OutfitStandsPlusForkedModSettings.TailorMendLimitPercent, 2, 100);
        public static int StartPercent => Mathf.Clamp(OutfitStandsPlusForkedModSettings.TailorMendStartPercent, 1, LimitPercent - 1);

        // ---- Eligibility ----------------------------------------------------------------------------------------------
        public static bool IsTailorStand(Thing thing) => thing is Building_OutfitStand && thing.TryGetComp<CompTailorMending>() != null;

        public static bool IsTextileDef(ThingDef def)
        {
            if (def == null || def.category != ThingCategory.Item || !def.IsStuff || def.IsApparel)
            {
                return false;
            }

            return HasTextileCategory(def);
        }

        private static bool HasTextileCategory(ThingDef stuff)
        {
            var categories = stuff?.stuffProps?.categories;
            return categories != null &&
                   (categories.Contains(StuffCategoryDefOf.Fabric) || categories.Contains(StuffCategoryDefOf.Leathery));
        }

        /// <summary>Apparel made from a fabric or leather stuff and not taken from a corpse.</summary>
        public static bool IsMendable(Thing thing)
        {
            return thing is Apparel apparel && !apparel.WornByCorpse && HasTextileCategory(apparel.Stuff);
        }

        public static int LimitHp(Thing thing)
        {
            return Mathf.Clamp(Mathf.CeilToInt(thing.MaxHitPoints * LimitPercent / 100f), 0, thing.MaxHitPoints);
        }

        public static bool BelowLimit(Thing thing) => thing.HitPoints < LimitHp(thing);

        public static bool BelowStartThreshold(Thing thing) => thing.HitPoints < thing.MaxHitPoints * StartPercent / 100f;

        /// <summary>
        /// Items that would be mended, most damaged first. An automatic start also needs one item below the start
        /// threshold; a forced order (<paramref name="forced"/>) and a job already under way only need an item below the limit.
        /// </summary>
        public static List<Apparel> EligibleItems(Building_OutfitStand stand, bool forced)
        {
            var result = new List<Apparel>();
            if (stand?.HeldItems == null)
            {
                return result;
            }

            foreach (var thing in stand.HeldItems)
            {
                if (IsMendable(thing) && BelowLimit(thing))
                {
                    result.Add((Apparel) thing);
                }
            }

            if (!forced && !result.Any(BelowStartThreshold))
            {
                result.Clear();
            }

            result.Sort((a, b) => ((float) a.HitPoints / a.MaxHitPoints).CompareTo((float) b.HitPoints / b.MaxHitPoints));
            return result;
        }

        /// <summary>Hit points still missing up to the limit on every item below it.</summary>
        public static int MissingHp(Building_OutfitStand stand)
        {
            int missing = 0;
            if (stand?.HeldItems == null)
            {
                return 0;
            }

            foreach (var thing in stand.HeldItems)
            {
                if (IsMendable(thing) && BelowLimit(thing))
                {
                    missing += LimitHp(thing) - thing.HitPoints;
                }
            }

            return missing;
        }

        /// <summary>Whole textiles needed to finish the stand, after the paid hit points already stored.</summary>
        public static int TextileNeeded(Building_OutfitStand stand)
        {
            int stored = stand.GetComp<CompTailorMending>()?.StoredPaidHp ?? 0;
            int unpaid = Mathf.Max(0, MissingHp(stand) - stored);
            return Mathf.CeilToInt(unpaid / (float) HpPerTextile);
        }

        public static PathEndMode StandPathEndMode(Thing stand) => StandPathEndMode(stand?.def);

        // The def's interaction cell is what the "Remove interaction cells" setting switches off at startup.
        public static PathEndMode StandPathEndMode(ThingDef standDef)
        {
            return standDef != null && standDef.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;
        }

        // ---- Reservations ---------------------------------------------------------------------------------------------
        // Mending reserves the stand on its own reservation layer. Swapping, returning, auto-use and the group command
        // reserve it on the default layer, so a tailor never blocks them and one tailor still excludes another.

        /// <summary>True when a pawn other than <paramref name="mender"/> holds a default-layer reservation on the stand.</summary>
        public static bool IsStandInUseByOthers(Building_OutfitStand stand, Pawn mender)
        {
            if (stand?.Map == null)
            {
                return false;
            }

            var reservations = stand.Map.reservationManager.ReservationsReadOnly;
            for (int i = 0; i < reservations.Count; i++)
            {
                var reservation = reservations[i];
                if (reservation.Layer == null && reservation.Claimant != mender && reservation.Target.Thing == stand)
                {
                    return true;
                }
            }

            return false;
        }

        // ---- Textile --------------------------------------------------------------------------------------------------
        private static IEnumerable<Thing> UsableTextile(Pawn pawn)
        {
            foreach (var thing in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
            {
                if (thing.Spawned && IsTextileDef(thing.def) && !thing.IsForbidden(pawn) &&
                    pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, Danger.Deadly, 1, 1))
                {
                    yield return thing;
                }
            }
        }

        /// <summary>The nearest stack of the cheapest (by unit market value) textile this pawn can fetch.</summary>
        public static Thing FindCheapestTextile(Pawn pawn)
        {
            Thing best = null;
            float bestValue = float.MaxValue, bestDistance = float.MaxValue;
            foreach (var thing in UsableTextile(pawn))
            {
                float value = thing.def.BaseMarketValue;
                float distance = (thing.Position - pawn.Position).LengthHorizontalSquared;
                if (value < bestValue || (Mathf.Approximately(value, bestValue) && distance < bestDistance))
                {
                    best = thing;
                    bestValue = value;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>The nearest other stack of a given textile def, used to top up a partly filled trip.</summary>
        public static Thing FindTextileOfDef(Pawn pawn, ThingDef def)
        {
            Thing best = null;
            float bestDistance = float.MaxValue;
            foreach (var thing in UsableTextile(pawn))
            {
                if (thing.def != def)
                {
                    continue;
                }

                float distance = (thing.Position - pawn.Position).LengthHorizontalSquared;
                if (distance < bestDistance)
                {
                    best = thing;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>True when any non-forbidden textile the colony could fetch lies on the stand's map.</summary>
        public static bool AnyTextileFor(Building_OutfitStand stand)
        {
            var map = stand?.Map;
            if (map == null)
            {
                return false;
            }

            var traverse = TraverseParms.For(TraverseMode.PassDoors);
            foreach (var thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
            {
                if (thing.Spawned && IsTextileDef(thing.def) && !thing.IsForbidden(Faction.OfPlayer) &&
                    map.reachability.CanReach(stand.Position, thing, PathEndMode.ClosestTouch, traverse))
                {
                    return true;
                }
            }

            return false;
        }

        // ---- Starting a job -------------------------------------------------------------------------------------------
        /// <summary>
        /// Builds a mending job for <paramref name="pawn"/> on <paramref name="stand"/>, or returns null and says why not.
        /// <paramref name="forced"/> skips the start threshold and the per-stand toggle but never the "mend up to" limit.
        /// </summary>
        public static Job TryMakeMendJob(Pawn pawn, Building_OutfitStand stand, bool forced, out string whyNot)
        {
            whyNot = null;
            var comp = stand?.GetComp<CompTailorMending>();
            if (comp == null || !stand.Spawned || stand.Map != pawn.Map || stand.Faction != pawn.Faction)
            {
                whyNot = "OutfitStandsPlusForked_MendBlocked_Unavailable".Translate();
                return null;
            }

            if (!forced && !comp.MendingEnabled)
            {
                whyNot = "OutfitStandsPlusForked_MendBlocked_Off".Translate();
                return null;
            }

            if (EligibleItems(stand, forced).Count == 0)
            {
                whyNot = "OutfitStandsPlusForked_MendBlocked_NothingToMend".Translate();
                return null;
            }

            if (pawn.WorkTypeIsDisabled(OutfitStandsPlusForkedWorkTypeDefOf.Tailoring))
            {
                whyNot = "OutfitStandsPlusForked_MendBlocked_NoTailoring".Translate(pawn.LabelShort);
                return null;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                whyNot = "Incapable".Translate();
                return null;
            }

            if (stand.IsForbidden(pawn) || !pawn.CanReach(stand, StandPathEndMode(stand), Danger.Deadly))
            {
                whyNot = "NoPath".Translate();
                return null;
            }

            var layer = OutfitStandsPlusForkedReservationLayerDefOf.OutfitStandsPlus_Mending;
            if (!pawn.CanReserve(stand, 1, -1, layer) || IsStandInUseByOthers(stand, pawn))
            {
                whyNot = "Reserved".Translate();
                return null;
            }

            int needed = TextileNeeded(stand);
            Thing textile = needed > 0 ? FindCheapestTextile(pawn) : null;
            if (needed > 0 && textile == null && comp.StoredPaidHp <= 0)
            {
                whyNot = "OutfitStandsPlusForked_MendBlocked_NoCloth".Translate();
                return null;
            }

            Job job = textile != null
                ? JobMaker.MakeJob(OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_MendTailorStand, stand, textile)
                : JobMaker.MakeJob(OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_MendTailorStand, stand);
            if (textile != null)
            {
                job.count = Mathf.Max(1, Mathf.Min(needed, pawn.carryTracker.MaxStackSpaceEver(textile.def)));
            }

            return job;
        }
    }
}
