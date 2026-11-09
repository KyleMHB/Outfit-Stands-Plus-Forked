using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.HarmonyPatches
{
    /// <summary>
    /// Core only lets apparel be force-worn onto an outfit stand when Odyssey is active. Without Odyssey this mod supplies
    /// the stands, so treat that check as true in the stand-targeting code. Applied only when Odyssey is inactive.
    /// </summary>
    public static class ForceWearWithoutOdysseyPatch
    {
        private static readonly MethodInfo OdysseyActiveGetter = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.OdysseyActive));

        public static void Apply(Harmony harmony)
        {
            if (ModsConfig.OdysseyActive)
            {
                return;
            }

            var targets = CandidateMethods(typeof(Building_OutfitStand))
                .Concat(CandidateMethods(typeof(TargetingParameters)))
                .Where(GatesStandOnOdyssey)
                .ToList();
            if (targets.Count == 0)
            {
                Log.Warning("[Outfit Stands Plus Forked] Found no Odyssey force-wear check to patch; force-wearing onto stands may be unavailable.");
                return;
            }

            var transpiler = new HarmonyMethod(typeof(ForceWearWithoutOdysseyPatch), nameof(Transpiler));
            foreach (MethodInfo method in targets)
            {
                harmony.Patch(method, transpiler: transpiler);
            }
        }

        private static IEnumerable<MethodInfo> CandidateMethods(Type type)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            return new[] { type }.Concat(type.GetNestedTypes(all))
                .SelectMany(t => t.GetMethods(all))
                .Where(m => !m.IsAbstract && !m.ContainsGenericParameters);
        }

        // Only methods that read OdysseyActive and test for Building_OutfitStand, i.e. the force-wear checks.
        private static bool GatesStandOnOdyssey(MethodInfo method)
        {
            try
            {
                var body = PatchProcessor.ReadMethodBody(method).ToList();
                return body.Any(i => Equals(i.Value, OdysseyActiveGetter))
                    && body.Any(i => i.Key == OpCodes.Isinst && Equals(i.Value, typeof(Building_OutfitStand)));
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(OdysseyActiveGetter))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                    continue;
                }
                yield return instruction;
            }
        }
    }
}
