using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.Things
{
    internal interface IMendingOutfitStand
    {
        void TickMending();
    }

    public class CompProperties_MendingOutfitStand : CompProperties
    {
        public CompProperties_MendingOutfitStand()
        {
            compClass = typeof(CompMendingOutfitStand);
        }
    }

    public class CompMendingOutfitStand : ThingComp
    {
        public override void CompTick()
        {
            if (parent.Spawned && parent is IMendingOutfitStand stand)
            {
                stand.TickMending();
            }
        }
    }
    public class MendingHandler : IExposable
    {
        protected Building_OutfitStand OutfitStand;
        public int MendTickCounter = 0;
        private int? RareTicksPerMendCached;
        private int normalTickCounter;
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
                    RareTicksPerMendCached = LoadedModManager.GetMod<OutfitStandsPlusForkedMod>().GetSettings<OutfitStandsPlusForkedModSettings>().RareTicksPerMend;
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
        }
        internal void Tick()
        {
            // Normal ticking avoids the extra holder hash gate in RimWorld 1.7.
            // Keep MendTickCounter in rare-tick units so existing saves retain progress.
            normalTickCounter++;
            if (normalTickCounter >= 250)
            {
                normalTickCounter = 0;
                HandleMending();
            }
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
            Scribe_Values.Look(ref normalTickCounter, "OutfitStandsPlusMendNormalTickCounter", 0);
        }
        
    }
    public class MendingOutfitStand: MechanizedOutfitStand, IMendingOutfitStand
    {
        private MendingHandler MendingHandler;
        public MendingOutfitStand()
        {
            MendingHandler = new MendingHandler(this);
        }
        void IMendingOutfitStand.TickMending()
        {
            MendingHandler.Tick();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref MendingHandler, "OutfitStandsPlusMendingHandler", this);
        }
    }

    public class MendingKidOutfitStand : Building_KidOutfitStand, IMendingOutfitStand
    {
        private MendingHandler MendingHandler;
        public MendingKidOutfitStand()
        {
            MendingHandler = new MendingHandler(this);
        }
        void IMendingOutfitStand.TickMending()
        {
            MendingHandler.Tick();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref MendingHandler, "OutfitStandsPlusMendingHandler", this);
        }
    }
}
