using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.ThingComps
{
    public class CompProperties_OutfitStandsPlusMainComp : CompProperties
    {
        public CompProperties_OutfitStandsPlusMainComp()
        {
            this.compClass = typeof(OutfitStandsPlusMainComp);
        }

        public CompProperties_OutfitStandsPlusMainComp(Type compClass)
        {
            this.compClass = compClass;
        }
    }
    
    public class OutfitStandsPlusMainComp: ThingComp
    {
        private bool _allowAddingItems = true;

        public bool AllowAddingItems
        {
            get => _allowAddingItems;
            set => _allowAddingItems = value;
        }
        public decimal EquipSpeedFactor
        {
            get
            {
                var power = parent.GetComp<CompPowerTrader>();
                var modSettings = LoadedModManager.GetMod<OutfitStandsPlusMod>()
                    .GetSettings<OutfitStandsPlusModSettings>();
                return (power != null && power.PowerOn ? modSettings.EquipSpeedFactorPowered : modSettings.EquipSpeedFactorUnpowered) * 0.01m;
            }
        }

        public int ApplyEquipSpeedFactor(int duration)
        {
            return (int) (duration * EquipSpeedFactor);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _allowAddingItems, "_allowAddingItems", true);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Toggle command = new Command_Toggle();
            command.icon = TexCommand.ForbidOff;
            command.isActive = () => AllowAddingItems;
            command.defaultLabel = "OutfitStandsPlus_AllowAddingItems_Label".Translate();
            command.defaultDesc = "OutfitStandsPlus_AllowAddingItems_Desc".Translate();
            command.toggleAction = delegate { AllowAddingItems = !AllowAddingItems; };
            yield return command;
        }
    }
    
}