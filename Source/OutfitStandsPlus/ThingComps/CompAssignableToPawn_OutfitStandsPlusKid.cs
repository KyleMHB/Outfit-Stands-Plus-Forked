using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.ThingComps
{
    public class CompProperties_AssignableToPawn_OutfitStandsPlusForkedKid : CompProperties_AssignableToPawn
    {
        public CompProperties_AssignableToPawn_OutfitStandsPlusForkedKid()
        {
            this.compClass = typeof(CompAssignableToPawn_OutfitStandsPlusForkedKid);
            this.maxAssignedPawnsCount = 1;
        }

        public CompProperties_AssignableToPawn_OutfitStandsPlusForkedKid(Type compClass)
        {
            this.compClass = compClass;
            this.maxAssignedPawnsCount = 1;
        }
    }
    
    public class CompAssignableToPawn_OutfitStandsPlusForkedKid: CompAssignableToPawn_OutfitStandsPlusForkedBase
    {
        public override IEnumerable<DevelopmentalStage> AllowedPawnDevelopmentalStages
        {
            get
            {
                return new List<DevelopmentalStage>() {DevelopmentalStage.Child};
            }
        }
    }
}
