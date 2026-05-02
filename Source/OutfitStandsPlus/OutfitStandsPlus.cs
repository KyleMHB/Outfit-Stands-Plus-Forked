using System.Linq;
using OutfitStandsPlus.Things;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlus
{
    public class OutfitStandsPlusModSettings : ModSettings
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
    public class OutfitStandsPlusMod : Verse.Mod
    {
        public OutfitStandsPlusModSettings Settings;
        
        public OutfitStandsPlusMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<OutfitStandsPlusModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            
            listingStandard.Label(
                "OutfitStandsPlus_RareTicksPerMendModSetting_Label".Translate(), 
                -1f, 
                "OutfitStandsPlus_RareTicksPerMendModSetting_Tooltip".Translate());
            var buffer1 = Settings.RareTicksPerMend.ToString();
            listingStandard.IntEntry(ref Settings.RareTicksPerMend, ref buffer1, 1);
            
            listingStandard.Label(
                "OutfitStandsPlus_EquipSpeedFactorPowered_Label".Translate(), 
                -1f, 
                "OutfitStandsPlus_EquipSpeedFactor_Tooltip".Translate());
            var buffer2 = Settings.EquipSpeedFactorPowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorPowered, ref buffer2);
            
            listingStandard.Label(
                "OutfitStandsPlus_EquipSpeedFactorUnpowered_Label".Translate(), 
                -1f, 
                "OutfitStandsPlus_EquipSpeedFactor_Tooltip".Translate());
            var buffer3 = Settings.EquipSpeedFactorUnpowered.ToString();
            listingStandard.TextFieldNumeric(ref Settings.EquipSpeedFactorUnpowered, ref buffer3);
            
            listingStandard.GapLine();
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlus_RemoveInteractionCells".Translate(), 
                ref OutfitStandsPlusModSettings.RemoveInteractionCells, 
                "OutfitStandsPlus_RemoveInteractionCells_Tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlus_RemoveMechanizedStands".Translate(),
                ref OutfitStandsPlusModSettings.RemoveMechanizedStands,
                "OutfitStandsPlus_RemoveMechanizedStands_Tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "OutfitStandsPlus_RemoveMendingStands".Translate(),
                ref OutfitStandsPlusModSettings.RemoveMendingStands,
                "OutfitStandsPlus_RemoveMendingStands_Tooltip".Translate());
            
            listingStandard.GapLine();
            
            if (listingStandard.ButtonText("OutfitStandsPlus_RestoreDefaultSettings_Label".Translate()))
            {
                Settings.RareTicksPerMend = OutfitStandsPlusModSettings.RareTicksPerMendDefault;
                Settings.EquipSpeedFactorPowered = OutfitStandsPlusModSettings.EquipSpeedFactorPoweredDefault;
                Settings.EquipSpeedFactorUnpowered = OutfitStandsPlusModSettings.EquipSpeedFactorUnpoweredDefault;
                OutfitStandsPlusModSettings.RemoveMechanizedStands =
                    OutfitStandsPlusModSettings.RemoveMechanizedStandsDefault;
                OutfitStandsPlusModSettings.RemoveMendingStands =
                    OutfitStandsPlusModSettings.RemoveMendingStandsDefault;
                OutfitStandsPlusModSettings.RemoveInteractionCells =
                    OutfitStandsPlusModSettings.RemoveInteractionCellsDefault;
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        
        public override string SettingsCategory()
        {
            return "OutfitStandsPlus_ModName".Translate();
        }
    }

    [StaticConstructorOnStartup]
    internal static class OutfitStandsRemovalUtility
    {
        static OutfitStandsRemovalUtility()
        {
            if (OutfitStandsPlusModSettings.RemoveMechanizedStands)
            {
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedOutfitStand"));
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MechanizedKidOutfitStand"));
                HideResearch(DefDatabase<ResearchProjectDef>.GetNamed("OutfitStandsPlus_MechanizedOutfitStandsResearch"));
            }
            if (OutfitStandsPlusModSettings.RemoveMendingStands)
            {
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingOutfitStand"));
                HideThing(DefDatabase<ThingDef>.GetNamed("OutfitStandsPlus_MendingKidOutfitStand"));
                HideResearch(DefDatabase<ResearchProjectDef>.GetNamed("OutfitStandsPlus_MendingOutfitStandsResearch"));
            }

            if (OutfitStandsPlusModSettings.RemoveInteractionCells)
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