using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlus.ThingComps;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.Utility
{
    public static class OutfitStandTransferUtility
    {
        public static void UseStand(Pawn pawn, Building_OutfitStand stand)
        {
            if (!ValidatePawnAndStand(pawn, stand))
            {
                return;
            }

            stand.GetComp<OutfitStandsPlusStandComp>()?.SyncApparelPolicyFilter();
            if (OutfitStandCompatibilityUtility.HasExternalGearFlowQueued(pawn))
            {
                OutfitStandCompatibilityUtility.LogExternalGearFlowOnce(pawn, stand);
            }

            List<Apparel> standApparel = stand.HeldItems.OfType<Apparel>().ToList();
            HashSet<Apparel> pawnToStand = new HashSet<Apparel>();
            HashSet<Apparel> standToPawn = new HashSet<Apparel>();
            List<Apparel> projectedPawnApparel = pawn.apparel.WornApparel.ToList();

            foreach (Apparel apparel in standApparel)
            {
                if (!CanPawnWearApparel(pawn, apparel, projectedPawnApparel.Where(ap => !pawnToStand.Contains(ap))))
                {
                    LogSkip(pawn, stand, apparel, "stand apparel is not wearable with locked/current apparel");
                    continue;
                }

                List<Apparel> conflicts = projectedPawnApparel
                    .Where(worn => worn != apparel && !pawnToStand.Contains(worn))
                    .Where(worn => !ApparelUtility.CanWearTogether(apparel.def, worn.def, pawn.RaceProps.body))
                    .ToList();

                if (conflicts.Any(conflict => pawn.apparel.IsLocked(conflict)))
                {
                    LogSkip(pawn, stand, apparel, "conflicts with locked apparel");
                    continue;
                }

                if (conflicts.Any(conflict => !stand.StoreSettings.AllowedToAccept(conflict)))
                {
                    LogSkip(pawn, stand, apparel, "conflicting pawn apparel is not accepted by stand");
                    continue;
                }

                foreach (Apparel conflict in conflicts)
                {
                    pawnToStand.Add(conflict);
                    projectedPawnApparel.Remove(conflict);
                }

                standToPawn.Add(apparel);
                projectedPawnApparel.Add(apparel);
            }

            TransferApparel(pawn, stand, pawnToStand, standToPawn);
            SwapWeapons(pawn, stand);
            pawn.GetComp<OutfitStandsPlusUseCommandsComp>().CurrentOutfitStandJobDef = DefOfs.OutfitStandsPlusJobDefOf.OutfitStandsPlus_JobReturnToStand;
        }

        public static void ReturnToStand(Pawn pawn, Building_OutfitStand stand)
        {
            if (!ValidatePawnAndStand(pawn, stand))
            {
                return;
            }

            stand.GetComp<OutfitStandsPlusStandComp>()?.SyncApparelPolicyFilter();
            if (OutfitStandCompatibilityUtility.HasExternalGearFlowQueued(pawn))
            {
                OutfitStandCompatibilityUtility.LogExternalGearFlowOnce(pawn, stand);
            }

            HashSet<Apparel> pawnToStand = new HashSet<Apparel>();
            HashSet<Apparel> standToPawn = new HashSet<Apparel>();
            List<Apparel> projectedStandApparel = stand.HeldItems.OfType<Apparel>().ToList();
            List<Apparel> projectedPawnApparel = pawn.apparel.WornApparel.ToList();

            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
            {
                if (pawn.apparel.IsLocked(apparel))
                {
                    LogSkip(pawn, stand, apparel, "pawn apparel is locked");
                    continue;
                }

                if (!stand.StoreSettings.AllowedToAccept(apparel))
                {
                    continue;
                }

                pawnToStand.Add(apparel);
                projectedPawnApparel.Remove(apparel);
                projectedStandApparel.Add(apparel);
            }

            foreach (Apparel apparel in stand.HeldItems.OfType<Apparel>().ToList())
            {
                bool canRemainOnStand = stand.StoreSettings.AllowedToAccept(apparel) &&
                                        projectedStandApparel
                                            .Where(other => other != apparel)
                                            .All(other => ApparelUtility.CanWearTogether(apparel.def, other.def, pawn.RaceProps.body));

                if (canRemainOnStand)
                {
                    continue;
                }

                if (!CanPawnWearApparel(pawn, apparel, projectedPawnApparel))
                {
                    LogSkip(pawn, stand, apparel, "stand apparel cannot be returned to pawn");
                    continue;
                }

                standToPawn.Add(apparel);
                projectedStandApparel.Remove(apparel);
                projectedPawnApparel.Add(apparel);
            }

            TransferApparel(pawn, stand, pawnToStand, standToPawn);
            ReturnWeaponToStand(pawn, stand);
            pawn.GetComp<OutfitStandsPlusUseCommandsComp>().CurrentOutfitStandJobDef = JobDefOf.UseOutfitStand;
        }

        private static bool ValidatePawnAndStand(Pawn pawn, Building_OutfitStand stand)
        {
            return pawn?.apparel != null &&
                   pawn.equipment != null &&
                   stand != null &&
                   !stand.Destroyed &&
                   stand.HeldItems != null &&
                   stand.StoreSettings != null;
        }

        private static bool CanPawnWearApparel(Pawn pawn, Apparel apparel, IEnumerable<Apparel> otherApparel)
        {
            if (pawn == null || apparel == null || apparel.Destroyed || apparel.def?.apparel == null)
            {
                return false;
            }

            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
            {
                return false;
            }

            return otherApparel.All(other => other == apparel || ApparelUtility.CanWearTogether(apparel.def, other.def, pawn.RaceProps.body));
        }

        private static void TransferApparel(Pawn pawn, Building_OutfitStand stand, IEnumerable<Apparel> pawnToStand, IEnumerable<Apparel> standToPawn)
        {
            List<Apparel> pawnItems = pawnToStand.Where(apparel => pawn.apparel.Wearing(apparel)).ToList();
            List<Apparel> standItems = standToPawn.Where(apparel => stand.HeldItems.Contains(apparel)).ToList();

            foreach (Apparel apparel in standItems)
            {
                if (!stand.RemoveApparel(apparel))
                {
                    LogSkip(pawn, stand, apparel, "stand refused to remove apparel");
                }
            }

            foreach (Apparel apparel in pawnItems)
            {
                if (pawn.apparel.Wearing(apparel))
                {
                    pawn.apparel.Remove(apparel);
                }
            }

            foreach (Apparel apparel in standItems)
            {
                if (CanPawnWearApparel(pawn, apparel, pawn.apparel.WornApparel))
                {
                    pawn.apparel.Wear(apparel, false);
                }
                else
                {
                    LogSkip(pawn, stand, apparel, "wear validation failed after removal; returning to stand");
                    stand.AddApparel(apparel);
                }
            }

            foreach (Apparel apparel in pawnItems)
            {
                if (!stand.StoreSettings.AllowedToAccept(apparel) || !stand.AddApparel(apparel))
                {
                    LogSkip(pawn, stand, apparel, "stand rejected pawn apparel; returning to pawn");
                    if (CanPawnWearApparel(pawn, apparel, pawn.apparel.WornApparel))
                    {
                        pawn.apparel.Wear(apparel, false);
                    }
                }
            }
        }

        private static void SwapWeapons(Pawn pawn, Building_OutfitStand stand)
        {
            ThingWithComps standWeapon = stand.HeldWeapon;
            ThingWithComps pawnWeapon = pawn.equipment.Primary;

            if (standWeapon == null)
            {
                ReturnWeaponToStand(pawn, stand);
                return;
            }

            if (!PawnCanWieldWeapon(standWeapon, pawn))
            {
                LogSkip(pawn, stand, standWeapon, "pawn cannot wield stand weapon");
                return;
            }

            if (pawnWeapon != null && !stand.StoreSettings.AllowedToAccept(pawnWeapon))
            {
                LogSkip(pawn, stand, pawnWeapon, "stand does not accept pawn weapon");
                return;
            }

            if (!stand.RemoveHeldWeapon(standWeapon))
            {
                LogSkip(pawn, stand, standWeapon, "stand refused to remove weapon");
                return;
            }

            if (pawnWeapon != null)
            {
                pawn.equipment.Remove(pawnWeapon);
                if (!stand.TryAddHeldWeapon(pawnWeapon))
                {
                    LogSkip(pawn, stand, pawnWeapon, "stand rejected pawn weapon; rolling back");
                    pawn.equipment.AddEquipment(pawnWeapon);
                    stand.TryAddHeldWeapon(standWeapon);
                    return;
                }
            }

            pawn.equipment.AddEquipment(standWeapon);
        }

        private static void ReturnWeaponToStand(Pawn pawn, Building_OutfitStand stand)
        {
            ThingWithComps pawnWeapon = pawn.equipment.Primary;
            if (pawnWeapon == null || stand.HeldWeapon != null)
            {
                return;
            }

            if (!stand.StoreSettings.AllowedToAccept(pawnWeapon))
            {
                LogSkip(pawn, stand, pawnWeapon, "stand does not accept returned weapon");
                return;
            }

            if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanUnequip(pawnWeapon, pawn))
            {
                LogSkip(pawn, stand, pawnWeapon, "quest lodger cannot unequip weapon");
                return;
            }

            pawn.equipment.Remove(pawnWeapon);
            if (!stand.TryAddHeldWeapon(pawnWeapon))
            {
                LogSkip(pawn, stand, pawnWeapon, "stand rejected returned weapon; rolling back");
                pawn.equipment.AddEquipment(pawnWeapon);
            }
        }

        private static bool PawnCanWieldWeapon(Thing weapon, Pawn pawn)
        {
            return (!weapon.def.IsWeapon || !pawn.WorkTagIsDisabled(WorkTags.Violent)) &&
                   (!weapon.def.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) &&
                   pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                   (!pawn.IsQuestLodger() || EquipmentUtility.QuestLodgerCanEquip(weapon, pawn)) &&
                   EquipmentUtility.CanEquip(weapon, pawn, out string _);
        }

        private static void LogSkip(Pawn pawn, Building_OutfitStand stand, Thing item, string reason)
        {
            Log.Warning($"[Outfit Stands Plus] Skipped transfer. Pawn={pawn?.LabelShort ?? "null"}, Stand={stand?.LabelCap ?? "null"}, Item={item?.LabelCap ?? "null"}, Reason={reason}");
        }
    }
}
