using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.ThingComps
{
    public class CompProperties_AssignableToPawn_OutfitStandsPlusKid : CompProperties_AssignableToPawn
    {
        public CompProperties_AssignableToPawn_OutfitStandsPlusKid()
        {
            this.compClass = typeof(CompAssignableToPawn_OutfitStandsPlusKid);
            this.maxAssignedPawnsCount = 1;
        }

        public CompProperties_AssignableToPawn_OutfitStandsPlusKid(Type compClass)
        {
            this.compClass = compClass;
            this.maxAssignedPawnsCount = 1;
        }
    }
    
    public class CompAssignableToPawn_OutfitStandsPlusKid: CompAssignableToPawn_OutfitStandsPlusBase
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
