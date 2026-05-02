using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitStandsPlus.ThingComps
{
    public class CompProperties_AssignableToPawn_OutfitStandsPlusAdult : CompProperties_AssignableToPawn
    {
        public CompProperties_AssignableToPawn_OutfitStandsPlusAdult()
        {
            this.compClass = typeof(CompAssignableToPawn_OutfitStandsPlusAdult);
            this.maxAssignedPawnsCount = 1;
        }

        public CompProperties_AssignableToPawn_OutfitStandsPlusAdult(Type compClass)
        {
            this.compClass = compClass;
            this.maxAssignedPawnsCount = 1;
        }
    }
    public class CompAssignableToPawn_OutfitStandsPlusAdult: CompAssignableToPawn_OutfitStandsPlusBase
    {
    }
}
