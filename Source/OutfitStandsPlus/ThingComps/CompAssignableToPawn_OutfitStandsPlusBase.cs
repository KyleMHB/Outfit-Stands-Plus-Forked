using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.ThingComps
{
    public class CompAssignableToPawn_OutfitStandsPlusForkedBase: CompAssignableToPawn
    {
        private static bool loggedTrimmedAssignments;

        public virtual IEnumerable<DevelopmentalStage> AllowedPawnDevelopmentalStages
        {
            get
            {
                return new List<DevelopmentalStage>() {DevelopmentalStage.Adult};
            }
        }
        
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                var candidates = base.AssigningCandidates;
                foreach (var candidate in candidates)
                {
                    if (AllowedPawnDevelopmentalStages.Contains(candidate.DevelopmentalStage))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            if (this.assignedPawns.Contains(pawn))
                return;
            if (Find.CurrentMap?.listerBuildings == null)
            {
                return;
            }

            foreach (Pawn assignedPawn in assignedPawns.ToList())
            {
                TryUnassignPawn(assignedPawn, sort: false);
            }

            var stands = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>();
            foreach (var stand in stands)
            {
                var c = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                if (c == null)
                {
                    continue;
                }
                c.TryUnassignPawn(pawn);
            }
            assignedPawns.Add(pawn);
            this.SortAssignedPawns();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            TrimExtraAssignedPawns();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TrimExtraAssignedPawns();
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            if (Find.CurrentMap?.listerBuildings == null)
            {
                return false;
            }
            var stands = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>();
            foreach (var stand in stands)
            {
                var c = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                if (c != null && c.AssignedPawns.Contains(pawn))
                {
                    return true;
                }
            }
            return false;
        }

        private void TrimExtraAssignedPawns()
        {
            if (assignedPawns == null || assignedPawns.Count <= 1)
            {
                return;
            }

            assignedPawns.RemoveRange(1, assignedPawns.Count - 1);
            SortAssignedPawns();
            if (!loggedTrimmedAssignments)
            {
                loggedTrimmedAssignments = true;
                Log.Warning("[Outfit Stands Plus Forked] Trimmed legacy armor stand assignments to enforce one pawn per stand.");
            }
        }
    }
}
