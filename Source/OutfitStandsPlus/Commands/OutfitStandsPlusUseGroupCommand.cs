using System.Collections.Generic;
using System.Linq;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.Commands
{
    public class OutfitStandsPlusForkedUseGroupCommand : Command
    {
        private readonly List<Pawn> pawns;

        public OutfitStandsPlusForkedUseGroupCommand(IEnumerable<Pawn> selectedPawns)
        {
            pawns = selectedPawns.Where(pawn => pawn != null).Distinct().ToList();
            icon = ContentFinder<Texture2D>.Get("UI/Commands/SwapOutfits");
            defaultLabel = "OutfitStandsPlusForked_GroupUseAssignedStands_Label".Translate();
            defaultDesc = "OutfitStandsPlusForked_GroupUseAssignedStands_Desc".Translate();
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                yield return new FloatMenuOption("SwapOutfitGizmo".Translate().CapitalizeFirst(), delegate
                {
                    SetSelectedPawnMode(JobDefOf.UseOutfitStand);
                });
                yield return new FloatMenuOption("OutfitStandsPlusForked_ReturnToStandGizmoLabel".Translate().CapitalizeFirst(), delegate
                {
                    SetSelectedPawnMode(OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand);
                });
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            int started = 0;
            int skipped = 0;
            foreach (Pawn pawn in pawns)
            {
                Building_OutfitStand stand = FindAssignedStand(pawn);
                if (stand == null || !TryStartStandJob(pawn, stand))
                {
                    skipped++;
                    continue;
                }
                started++;
            }

            if (started == 0)
            {
                Messages.Message("OutfitStandsPlusForked_GroupUseAssignedStands_None".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            }
            else if (skipped > 0)
            {
                Messages.Message("OutfitStandsPlusForked_GroupUseAssignedStands_Skipped".Translate(started, skipped), MessageTypeDefOf.NeutralEvent, historical: false);
            }
        }

        private static void SetSelectedPawnMode(JobDef jobDef)
        {
            foreach (Pawn pawn in Find.Selector.SelectedObjects.OfType<Pawn>())
            {
                pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>()?.SetCurrentOutfitStandJobDef(jobDef);
            }
        }

        private static bool TryStartStandJob(Pawn pawn, Building_OutfitStand stand)
        {
            if (pawn == null || stand == null || pawn.Downed || !pawn.CanReserveAndReach(stand, PathEndMode.InteractionCell, Danger.Deadly))
            {
                return false;
            }

            stand.GetComp<OutfitStandsPlusForkedStandComp>()?.SyncApparelPolicyFilter();
            var comp = pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>();
            JobDef selectedJobDef = comp?.CurrentOutfitStandJobDef ?? JobDefOf.UseOutfitStand;
            Job job = JobMaker.MakeJob(selectedJobDef, stand);
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            return true;
        }

        public static Building_OutfitStand FindAssignedStand(Pawn pawn)
        {
            if (pawn == null || Find.CurrentMap?.listerBuildings == null)
            {
                return null;
            }

            foreach (Building_OutfitStand stand in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>())
            {
                var assignmentComp = stand.GetComp<CompAssignableToPawn_OutfitStandsPlusForkedBase>();
                if (assignmentComp != null && assignmentComp.AssignedPawns.Contains(pawn))
                {
                    return stand;
                }
            }

            return null;
        }
    }
}
