using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OutfitStandsPlusForked.ThingComps
{
    public class CompProperties_AssignableToPawn_OutfitStandsPlusForkedAdult : CompProperties_AssignableToPawn
    {
        public CompProperties_AssignableToPawn_OutfitStandsPlusForkedAdult()
        {
            this.compClass = typeof(CompAssignableToPawn_OutfitStandsPlusForkedAdult);
            this.maxAssignedPawnsCount = 1;
        }

        public CompProperties_AssignableToPawn_OutfitStandsPlusForkedAdult(Type compClass)
        {
            this.compClass = compClass;
            this.maxAssignedPawnsCount = 1;
        }
    }
    public class CompAssignableToPawn_OutfitStandsPlusForkedAdult: CompAssignableToPawn_OutfitStandsPlusForkedBase
    {
    }
}
