using System;
using System.Collections.Generic;
using System.Reflection;
using OutfitStandsPlusForked.DefOfs;
using OutfitStandsPlusForked.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OutfitStandsPlusForked.Commands
{
    public class OutfitStandsPlusForkedUseCommand: Command
    {
        public Building_OutfitStand OutfitStand;
        public Pawn Pawn;

        public OutfitStandsPlusForkedUseCommand(Building_OutfitStand outfitStand, Pawn pawn)
        {
            OutfitStand = outfitStand;
            Pawn = pawn;
            icon = ContentFinder<Texture2D>.Get("UI/Commands/SwapOutfits");
            defaultIconColor = outfitStand.Stuff.stuffProps.color;
        }

        public override string Label
        {
            get
            {
                var selectedJobDef = Pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>().CurrentOutfitStandJobDef;
                if (selectedJobDef == OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand)
                {
                    return "OutfitStandsPlusForked_ReturnToNamedStandGizmoLabel".Translate(DisplayStandName).CapitalizeFirst();
                }
                return "OutfitStandsPlusForked_SwapNamedStandGizmoLabel".Translate(DisplayStandName).CapitalizeFirst();
            }
        }

        public override string Desc
        {
            get
            {
                var selectedJobDef = Pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>().CurrentOutfitStandJobDef;
                if (selectedJobDef == OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand)
                {
                    return "OutfitStandsPlusForked_ReturnToNamedStandGizmoDescription".Translate(DisplayStandName);
                }
                return "OutfitStandsPlusForked_SwapNamedStandGizmoDescription".Translate(DisplayStandName);
            }
        }

        private string DisplayStandName => OutfitStand.GetComp<OutfitStandsPlusForkedStandComp>()?.DisplayName ?? OutfitStand.LabelCap;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                var self = this;
                yield return new FloatMenuOption("SwapOutfitGizmo".Translate().CapitalizeFirst(),
                    delegate
                    {
                        Pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>().CurrentOutfitStandJobDef = JobDefOf.UseOutfitStand;
                    });
                yield return new FloatMenuOption("OutfitStandsPlusForked_ReturnToStandGizmoLabel".Translate().CapitalizeFirst(),
                    delegate
                    {
                        Pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>().CurrentOutfitStandJobDef = OutfitStandsPlusForkedJobDefOf.OutfitStandsPlus_JobReturnToStand;
                    });
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (Pawn.Downed)
            {
                Messages.Message("IsIncapped".Translate(Pawn.LabelShort, Pawn), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            if (Pawn.CanReserveAndReach(OutfitStand, PathEndMode.InteractionCell, Danger.Deadly))
            {
                // SetAllowHauling is private for whatever reason so this has to be done
                var setAllowHauling = typeof(Building_OutfitStand).GetMethod("SetAllowHauling", BindingFlags.NonPublic | BindingFlags.Instance);
                if (setAllowHauling != null)
                {
                    var allow_raw = new[] {false};
                    var allow = Array.ConvertAll(allow_raw, x => (object)x);
                    setAllowHauling.Invoke(OutfitStand, allow);
                }
                else
                {
                    Log.Warning("[Outfit Stands Plus Forked] Could not find Building_OutfitStand.SetAllowHauling; continuing without changing hauling state.");
                }
                
                OutfitStand.GetComp<OutfitStandsPlusForkedStandComp>()?.SyncApparelPolicyFilter();
                var selectedJobDef = Pawn.GetComp<OutfitStandsPlusForkedUseCommandsComp>().CurrentOutfitStandJobDef;
                Job job = JobMaker.MakeJob(selectedJobDef, OutfitStand);
                job.playerForced = true;
                Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

        }
    }
}
