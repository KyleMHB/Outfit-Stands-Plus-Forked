using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlus.Things
{
    public class OutfitStandPlusMinified : MinifiedThing
    {
        private static bool loggedMissingInnerThing;

        private bool HasInnerThing => InnerThing != null;

        public override string LabelNoCount => HasInnerThing ? base.LabelNoCount : "OutfitStandsPlus_InvalidMinifiedStandLabel".Translate().ToString();

        public override Graphic Graphic => HasInnerThing ? base.Graphic : DefaultGraphic;

        protected override Graphic LoadCrateFrontGraphic()
        {
            return HasInnerThing ? base.LoadCrateFrontGraphic() : DefaultGraphic;
        }

        public override void Print(SectionLayer layer)
        {
            if (HasInnerThing)
            {
                base.Print(layer);
                return;
            }

            LogMissingInnerThingOnce();
            DefaultGraphic.Print(layer, this, 0f);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (HasInnerThing)
            {
                base.DrawAt(drawLoc, flip);
                return;
            }

            LogMissingInnerThingOnce();
            Graphic.Draw(drawLoc, Rotation, this);
        }

        public override string GetInspectString()
        {
            return HasInnerThing ? base.GetInspectString() : "OutfitStandsPlus_InvalidMinifiedStandInspect".Translate().ToString();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Spawned && Map != null && InnerThing != null && mode == DestroyMode.Deconstruct)
            {
                var outfitStand = InnerThing as Building_OutfitStand;
                if (outfitStand == null)
                {
                    base.Destroy(mode);
                    return;
                }
                var heldItems = new List<Thing>(outfitStand.HeldItems);
                foreach (var thing in heldItems)
                {
                    Thing result;
                    if (thing is Apparel apparel)
                    {
                        outfitStand.RemoveApparel(apparel);
                        GenDrop.TryDropSpawn(apparel, PositionHeld, Map, ThingPlaceMode.Near, out result);
                    }
                    else
                    {
                        outfitStand.RemoveHeldWeapon(thing);
                        GenDrop.TryDropSpawn(thing, PositionHeld, Map, ThingPlaceMode.Near, out result);
                    }
                }
            }
            base.Destroy(mode);
        }

        private static void LogMissingInnerThingOnce()
        {
            if (loggedMissingInnerThing)
            {
                return;
            }

            loggedMissingInnerThing = true;
            Log.Warning("[Outfit Stands Plus] OutfitStandPlusMinified was spawned without an inner stand. This is usually caused by dev-spawning the internal minified crate def directly.");
        }
    }
}
