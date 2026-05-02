using System;
using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlus.Commands;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.ThingComps
{
    public class CompProperties_OutfitStandsPlusUseCommandsComp : CompProperties
    {
        public CompProperties_OutfitStandsPlusUseCommandsComp()
        {
            this.compClass = typeof(OutfitStandsPlusUseCommandsComp);
        }

        public CompProperties_OutfitStandsPlusUseCommandsComp(Type compClass)
        {
            this.compClass = compClass;
        }
    }
    public class OutfitStandsPlusUseCommandsComp: ThingComp
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
                    yield return new OutfitStandsPlusUseGroupCommand(selectedPawns);
                }
                yield break;
            }

            var stands = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>();
            foreach (var stand in stands)
            {
                var c = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusBase>();
                if (c == null || !c.AssignedPawns.Contains(pawn)) { continue; }
                yield return new OutfitStandsPlusUseCommand(stand, pawn);
            }
        }

        public void SetCurrentOutfitStandJobDef(JobDef jobDef)
        {
            CurrentOutfitStandJobDef = jobDef;
        }
    }
}
