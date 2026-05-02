using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.Things
{
    public class MendingHandler : IExposable
    {
        protected Building_OutfitStand OutfitStand;
        public int MendTickCounter = 0;
        private int? RareTicksPerMendCached;
        public MendingHandler(Building_OutfitStand stand)
        {
            OutfitStand = stand;
        }
        public int RareTicksPerMend
        {
            get
            {
                if (RareTicksPerMendCached == null)
                {
                    RareTicksPerMendCached = LoadedModManager.GetMod<OutfitStandsPlusMod>().GetSettings<OutfitStandsPlusModSettings>().RareTicksPerMend;
                }
                return (int) RareTicksPerMendCached;
            }
        }
        public void MendContents()
        {
            var mendTargets = new List<Thing>(OutfitStand.HeldItems);
            mendTargets = mendTargets.FindAll(a => a.HitPoints < a.MaxHitPoints);
            if (mendTargets.Empty())
            {
                return;
            }
            mendTargets = mendTargets.OrderBy(a => a.HitPoints).ToList();
            var mendTarget = mendTargets.First(); 
            mendTarget.HitPoints++;
            // Log.Warning(mendTarget + " " + mendTarget.HitPoints);
        }
        public void HandleMending()
        {
            var power = OutfitStand.GetComp<CompPowerTrader>();
            if (power is null)
            {
                return;
            }
            if (power.PowerOn)
            {
                MendTickCounter++;
            }
            if (MendTickCounter >= RareTicksPerMend)
            {
                MendContents();
                MendTickCounter = 0;
            }
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref MendTickCounter, "OutfitStandsPlusMendTickCounter", 0);
        }
        
    }
    public class MendingOutfitStand: MechanizedOutfitStand
    {
        private MendingHandler MendingHandler;
        public MendingOutfitStand()
        {
            MendingHandler = new MendingHandler(this);
        }
        public override void TickRare()
        {
            MendingHandler.HandleMending();
            base.TickRare();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref MendingHandler, "OutfitStandsPlusMendingHandler", this);
        }
    }

    public class MendingKidOutfitStand : Building_KidOutfitStand
    {
        private MendingHandler MendingHandler;
        public MendingKidOutfitStand()
        {
            MendingHandler = new MendingHandler(this);
        }
        public override void TickRare()
        {
            MendingHandler.HandleMending();
            base.TickRare();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref MendingHandler, "OutfitStandsPlusMendingHandler", this);
        }
    }
}