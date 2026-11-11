using RimWorld;

namespace OutfitStandsPlusForked.Things
{
    public class MechanizedOutfitStand: Building_OutfitStand
    {
        
    }
    
    public class MechanizedKidOutfitStand: Building_KidOutfitStand
    {
        // BodyTypeDefOf.Child only exists with Biotech; core's HeadOffsetAt throws on a null body type every frame.
        protected override BodyTypeDef BodyTypeDefForRendering => base.BodyTypeDefForRendering ?? BodyTypeDefOf.Male;
    }
}
