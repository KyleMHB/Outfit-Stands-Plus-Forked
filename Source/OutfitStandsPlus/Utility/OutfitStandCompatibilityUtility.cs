using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace OutfitStandsPlus.Utility
{
    public static class OutfitStandCompatibilityUtility
    {
        private static readonly Lazy<bool> GearUpAndGoLoaded = new Lazy<bool>(() => ModLister.GetActiveModWithIdentifier("jungooji.GearUpAndGo") != null);
        private static readonly Lazy<bool> DefensivePositionsLoaded = new Lazy<bool>(() => ModLister.GetActiveModWithIdentifier("GonDragon.DefensivePositions") != null);
        private static bool loggedGearFlow;

        public static bool IsCompatibilityModLoaded => GearUpAndGoLoaded.Value || DefensivePositionsLoaded.Value;

        public static bool IsExternalGearFlowJob(Job job)
        {
            if (job?.def == null)
            {
                return false;
            }

            string defName = job.def.defName;
            return defName == "GearUpAndGo" ||
                   defName == "DPDraftToPosition" ||
                   defName == "Wear" ||
                   defName == "RemoveApparel";
        }

        public static bool HasExternalGearFlowQueued(Pawn pawn)
        {
            if (pawn?.jobs == null || !IsCompatibilityModLoaded)
            {
                return false;
            }

            if (IsExternalGearFlowJob(pawn.jobs.curJob))
            {
                return true;
            }

            return pawn.jobs.jobQueue.Any(queuedJob => IsExternalGearFlowJob(queuedJob.job));
        }

        public static void LogExternalGearFlowOnce(Pawn pawn, Thing stand)
        {
            if (loggedGearFlow)
            {
                return;
            }

            loggedGearFlow = true;
            Log.Warning($"[Outfit Stands Plus] Detected external gear flow while using stand. Pawn={pawn?.LabelShort ?? "null"}, Stand={stand?.LabelCap ?? "null"}. Transfers will be revalidated to avoid stale apparel moves.");
        }
    }
}
