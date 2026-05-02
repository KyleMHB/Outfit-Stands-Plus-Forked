using System;
using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlusForked.Commands;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.ThingComps
{
    public class CompProperties_OutfitStandsPlusForkedUseCommandsComp : CompProperties
    {
        public CompProperties_OutfitStandsPlusForkedUseCommandsComp()
        {
            this.compClass = typeof(OutfitStandsPlusForkedUseCommandsComp);
        }

        public CompProperties_OutfitStandsPlusForkedUseCommandsComp(Type compClass)
        {
            this.compClass = compClass;
        }
    }
    public class OutfitStandsPlusForkedUseCommandsComp: ThingComp
    {
        private JobDef _currentOutfitStandJobDef;

        public JobDef CurrentOutfitStandJobDef
        {
            get => _currentOutfitStandJobDef ?? JobDefOf.UseOutfitStand;
            set => _currentOutfitStandJobDef = value;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _currentOutfitStandJobDef, "_currentOutfitStandJobDef");
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!(parent is Pawn pawn)) { yield break; }
            if (Find.CurrentMap?.listerBuildings == null)
            {
                yield break;
            }

            List<Pawn> selectedPawns = Find.Selector.SelectedObjects.OfType<Pawn>()
                .Where(selectedPawn => selectedPawn.IsColonistPlayerControlled)
                .ToList();
            if (selectedPawns.Count > 1)
            {
                if (selectedPawns.FirstOrDefault() == pawn)
                {
                    yield return new OutfitStandsPlusForkedUseGroupCommand(selectedPawns);
                }
                yield break;
            }

            var stands = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>();
            foreach (var stand in stands)
            {
                var c = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                if (c == null || !c.AssignedPawns.Contains(pawn)) { continue; }
                yield return new OutfitStandsPlusForkedUseCommand(stand, pawn);
            }
        }

        public void SetCurrentOutfitStandJobDef(JobDef jobDef)
        {
            CurrentOutfitStandJobDef = jobDef;
        }
    }
}
