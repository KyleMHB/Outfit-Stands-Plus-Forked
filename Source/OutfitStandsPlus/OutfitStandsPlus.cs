using System.Linq;
using OutfitStandsPlusForked.Things;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlusForked
{
    public class OutfitStandsPlusForkedModSettings : ModSettings
    {
        public static int RareTicksPerMendDefault = 10;
        public static int EquipSpeedFactorPoweredDefault = 25;
        public static int EquipSpeedFactorUnpoweredDefault = 100;
        public static bool RemoveMechanizedStandsDefault = false;
        public static bool RemoveMendingStandsDefault = false;
        public static bool RemoveInteractionCellsDefault = false;
        
        public int RareTicksPerMend = RareTicksPerMendDefault;
        public int EquipSpeedFactorPowered = EquipSpeedFactorPoweredDefault;
        public int EquipSpeedFactorUnpowered = EquipSpeedFactorUnpoweredDefault;
        public static bool RemoveMechanizedStands = RemoveMechanizedStandsDefault;
        public static bool RemoveMendingStands = RemoveMendingStandsDefault;
        public static bool RemoveInteractionCells = RemoveInteractionCellsDefault;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref RareTicksPerMend, "RareTicksPerMend", RareTicksPerMendDefault);
            Scribe_Values.Look(ref EquipSpeedFactorPowered, "EquipSpeedFactorPowered", EquipSpeedFactorPoweredDefault);
            Scribe_Values.Look(ref EquipSpeedFactorUnpowered, "EquipSpeedFactorUnpowered", EquipSpeedFactorUnpoweredDefault);
            Scribe_Values.Look(ref RemoveMechanizedStands, "RemoveMechanizedStands", RemoveMechanizedStandsDefault);
            Scribe_Values.Look(ref RemoveMendingStands, "RemoveMendingStands", RemoveMendingStandsDefault);
            Scribe_Values.Look(ref RemoveInteractionCells, "RemoveInteractionCells", RemoveInteractionCellsDefault);
        }
    }
    public class OutfitStandsPlusForkedMod : Verse.Mod
    {
        public OutfitStandsPlusForkedModSettings Settings;
        
        public OutfitStandsPlusForkedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<OutfitStandsPlusForkedModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            
            listingStandard.Label(
                "OutfitStandsPlusForked_RareTicksPerMendModSetting_Label".Translate(), 
                -1f, 
                "OutfitStandsPlusForked_RareTicksPerMendModSetting_Tooltip".Translate());
            var buffer1 = Settings.RareTicksPerMend.ToString();
            listingStandard.IntEntry(ref Settings.RareTicksPerMend, ref buffer1, 1);
            
            listingStandard.Label(
                "OutfitStandsPlusForked_EquipSpeedFactorPowered_Label".Translate(), 
                -1f, 
                "OutfitStandsPlusForked_EquipSpeedFactor_Tooltip".Translate());
            var buffer2 = Settings.EquipSpeedFactorPowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorPowered, ref buffer2);
            
            listingStandard.Label(
                "OutfitStandsPlusForked_EquipSpeedFactorUnpowered_Label".Translate(), 
                -1f, 
                "OutfitStandsPlusForked_EquipSpeedFactor_Tooltip".Translate());
            var buffer3 = Settings.EquipSpeedFactorUnpowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorUnpowered, ref buffer3);
            
            listingStandard.GapLine();
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlusForked_RemoveInteractionCells".Translate(), 
                ref OutfitStandsPlusForkedModSettings.RemoveInteractionCells, 
                "OutfitStandsPlusForked_RemoveInteractionCells_Tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlusForked_RemoveMechanizedStands".Translate(),
                ref OutfitStandsPlusForkedModSettings.RemoveMechanizedStands,
                "OutfitStandsPlusForked_RemoveMechanizedStands_Tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlusForked_RemoveMendingStands".Translate(),
                ref OutfitStandsPlusForkedModSettings.RemoveMendingStands,
                "OutfitStandsPlusForked_RemoveMendingStands_Tooltip".Translate());
            
            listingStandard.GapLine();
            
            if (listingStandard.ButtonText("OutfitStandsPlusForked_RestoreDefaultSettings_Label".Translate()))
            {
                Settings.RareTicksPerMend = OutfitStandsPlusForkedModSettings.RareTicksPerMendDefault;
                Settings.EquipSpeedFactorPowered = OutfitStandsPlusForkedModSettings.EquipSpeedFactorPoweredDefault;
                Settings.EquipSpeedFactorUnpowered = OutfitStandsPlusForkedModSettings.EquipSpeedFactorUnpoweredDefault;
                OutfitStandsPlusForkedModSettings.RemoveMechanizedStands =
                    OutfitStandsPlusForkedModSettings.RemoveMechanizedStandsDefault;
                OutfitStandsPlusForkedModSettings.RemoveMendingStands =
                    OutfitStandsPlusForkedModSettings.RemoveMendingStandsDefault;
                OutfitStandsPlusForkedModSettings.RemoveInteractionCells =
                    OutfitStandsPlusForkedModSettings.RemoveInteractionCellsDefault;
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "OutfitStandsPlusForked_ModName".Translate();
        }
    }

    [StaticConstructorOnStartup]
    internal static class OutfitStandsRemovalUtility
    {
        static OutfitStandsRemovalUtility()
        {
            if (OutfitStandsPlusForkedModSettings.RemoveMechanizedStands)
            {
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedOutfitStand"));
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedKidOutfitStand"));
                HideResearch(DefDatabase<ResearchProjectDef>.GetNamed("OutfitStandsPlus_MechanizedOutfitStandsResearch"));
            }
            if (OutfitStandsPlusForkedModSettings.RemoveMendingStands)
            {
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingOutfitStand"));
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingKidOutfitStand"));
                HideResearch(DefDatabase<ResearchProjectDef>.GetNamed("OutfitStandsPlus_MendingOutfitStandsResearch"));
            }

            if (OutfitStandsPlusForkedModSettings.RemoveInteractionCells)
            {
                RemoveInteractionSpot(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedOutfitStand"));
                RemoveInteractionSpot(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedKidOutfitStand"));
                RemoveInteractionSpot(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingOutfitStand"));
                RemoveInteractionSpot(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingKidOutfitStand"));
            }
        }

        static void HideThing(ThingDef def)
        {
            def.BaseMarketValue = 0f;
            def.tradeability = Tradeability.None;
            def.thingCategories?.Clear();
            def.thingSetMakerTags?.Clear();
            var designationCategory = def.designationCategory;
            def.designationCategory = null;
            designationCategory.ResolveReferences();
            def.researchPrerequisites = null;
            def.minifiedDef = null;
            def.building.claimable = false;
            def.building.storageGroupTag = null;
            def.building.buildingTags?.Clear();
        }

        static void HideResearch(ResearchProjectDef def)
        {
            var removeMethod = typeof(DefDatabase<ResearchProjectDef>).GetMethod("Remove",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            removeMethod.Invoke(null, new[] {def});
        }

        static void RemoveInteractionSpot(ThingDef def)
        {
            def.hasInteractionCell = false;
        }
    }
}
