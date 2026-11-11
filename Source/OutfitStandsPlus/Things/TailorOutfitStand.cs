using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlusForked.Things
{
    public class TailorOutfitStand : Building_OutfitStand
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            TailorStandOverlay.Draw(this, drawLoc);
        }
    }

    public class TailorKidOutfitStand : Building_KidOutfitStand
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            TailorStandOverlay.Draw(this, drawLoc);
        }
    }

    /// <summary>
    /// Core draws a stand's mannequin from fixed texture paths, so the tailor's stand looks like any other stand. The
    /// sewing-kit overlay is drawn here, on top of the base, body and head layers, to tell the two apart on the map.
    /// </summary>
    [StaticConstructorOnStartup]
    internal static class TailorStandOverlay
    {
        private const string TexPath = "Things/Building/OutfitStand/TailorStand_SewingKit";
        private const float DrawSize = 1.4f;
        private const float YOffset = 0.06f;

        // Loaded at startup on the main thread, as the game requires for graphics held in static fields.
        private static readonly Graphic Overlay = GraphicDatabase.Get<Graphic_Multi>(
            TexPath, ShaderDatabase.Cutout, new Vector2(DrawSize, DrawSize), Color.white);

        public static void Draw(Thing stand, Vector3 drawLoc)
        {
            Overlay.Draw(drawLoc.WithYOffset(YOffset), stand.Rotation, stand);
        }
    }
}
