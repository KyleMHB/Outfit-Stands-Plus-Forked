using System.Collections.Generic;
using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using OutfitStandsPlusForked.Utility;
using Verse.AI;

namespace OutfitStandsPlusForked.HarmonyPatches
{
    [HarmonyPatch(typeof(Building_OutfitStand), nameof(Building_OutfitStand.GetInspectString))]
    public class OutfitStandGetInspectStringPatch
    {
        static string Postfix(string result, Building_OutfitStand __instance)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (result.Length > 0)
            {
                stringBuilder.AppendLine(result);   
            }
            if (__instance.Faction == Faction.OfPlayer)
            {
                var c = __instance.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                Pawn owner = c != null && c.AssignedPawns.Any() ? c.AssignedPawns.First() : null;
                var owner_string = owner != null ? owner.Label : "Nobody".Translate().ToString();
                var standComp = __instance.GetComp<OutfitStandsPlusForkedStandComp>();
                if (standComp != null && (standComp.HasCustomName || standComp.HasAssignedPawnName))
                {
                    stringBuilder.AppendLine("OutfitStandsPlusForked_StandName".Translate() + ": " + standComp.DisplayName);
                }
                stringBuilder.AppendLine("Owner".Translate() + ": " + owner_string);    
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
    
    [HarmonyPatch(typeof(Building_OutfitStand), nameof(Building_OutfitStand.GetFloatMenuOptions))]
    public class OutfitStandCompFloatMenuOptionsPatch
    {
        static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> values, Building_OutfitStand __instance, Pawn selPawn)
        {
            int pos = 0;
            foreach (var value in values)
            {
                yield return value;
                if (pos == 0)
                {
                    var standComp = __instance.GetComp<OutfitStandsPlusForkedStandComp>();
                    string label = standComp != null && (standComp.HasCustomName || standComp.HasAssignedPawnName)
                        ? "OutfitStandsPlusForked_ReturnToNamedStandLabel".Translate(standComp.DisplayName).CapitalizeFirst()
                        : "OutfitStandsPlusForked_ReturnToStandLabel".Translate().CapitalizeFirst();
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, (Action) (() =>
                    {
                        Job job = JobMaker.MakeJob(OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand, (LocalTargetInfo) (Thing) __instance);
                        job.playerForced = true;
                        selPawn.jobs.TryTakeOrderedJob(job);
                    })), selPawn, (LocalTargetInfo) (Thing) __instance);    
                }
                pos++;
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_UseOutfitStand), "DoTransfer")]
    public class OutfitStandSwapOutfitPatch
    {
        static bool Prefix(JobDriver_UseOutfitStand __instance)
        {
            if (__instance?.job?.targetA.Thing is Building_OutfitStand outfitStand)
            {
                OutfitStandTransferUtility.UseStand(__instance.pawn, outfitStand);
            }
            return false;
        }    
    }
    
    [HarmonyPatch(typeof(JobDriver_UseOutfitStand), "Notify_Starting")]
    public class OutfitStandSwapDurationPatch
    {
        static void Postfix(JobDriver_UseOutfitStand __instance)
        {
            var outfitStand = (Building_OutfitStand) __instance.job.targetA.Thing;
            outfitStand.GetComp<OutfitStandsPlusForkedStandComp>()?.SyncApparelPolicyFilter();
            var comp = outfitStand.GetComp<OutfitStandsPlusForkedMainComp>();
            if (comp is null)
            {
                return;
            }
            
            var durationField = typeof(JobDriver_UseOutfitStand).GetField("duration",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (durationField == null)
            {
                Log.Warning("[Outfit Stands Plus Forked] Could not find JobDriver_UseOutfitStand.duration; equip speed factor skipped.");
                return;
            }
            var duration = (int) durationField.GetValue(__instance);
            var newDuration = comp.ApplyEquipSpeedFactor(duration);
            // Log.Warning(duration + " => " + newDuration);
            durationField.SetValue(__instance, newDuration);
        }
    }
    
    [HarmonyPatch(typeof(Building_OutfitStand))]
    public class OutfitStandAllowAddingPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(Building_OutfitStand).GetInterfaceMap(typeof(IHaulDestination)).TargetMethods.First(m => m.Name == "RimWorld.IHaulDestination.get_HaulDestinationEnabled");
        }
        static bool Prefix(ref bool __result, Building_OutfitStand __instance)
        {
            __instance.GetComp<OutfitStandsPlusForkedStandComp>()?.SyncApparelPolicyFilter();
            var comp = __instance.GetComp<OutfitStandsPlusForkedMainComp>();
            if (comp != null && !comp.AllowAddingItems)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_OutfitTracker), nameof(Pawn_OutfitTracker.CurrentApparelPolicy), MethodType.Setter)]
    public class PawnOutfitPolicyChangedPatch
    {
        private static readonly FieldInfo PawnField = typeof(Pawn_OutfitTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

        static void Prefix(Pawn_OutfitTracker __instance, ref ApparelPolicy __state)
        {
            __state = __instance?.CurrentApparelPolicy;
        }

        static void Postfix(Pawn_OutfitTracker __instance, ApparelPolicy value, ApparelPolicy __state)
        {
            if (__state == value || value == null)
            {
                return;
            }

            Pawn pawn = PawnField?.GetValue(__instance) as Pawn;
            OutfitStandAutoUseUtility.TryStartAssignedStandJob(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public class PawnUndraftedPatch
    {
        private static readonly FieldInfo PawnField = typeof(Pawn_DraftController).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

        static void Prefix(Pawn_DraftController __instance, ref bool __state)
        {
            __state = __instance?.Drafted == true;
        }

        static void Postfix(Pawn_DraftController __instance, bool value, bool __state)
        {
            if (!__state || value)
            {
                return;
            }

            Pawn pawn = PawnField?.GetValue(__instance) as Pawn;
            OutfitStandAutoUseUtility.TryStartAssignedStandJob(pawn);
        }
    }
    
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            var harmony = new Harmony("khamenman.OutfitStandsPlusForked");
            harmony.PatchAll();
            ForceWearWithoutOdysseyPatch.Apply(harmony);
        }
    }
}
