using System.Collections.Generic;
using System.Text;
using OutfitStandsPlusForked.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlusForked.ThingComps
{
    public class CompProperties_TailorMending : CompProperties
    {
        public CompProperties_TailorMending()
        {
            compClass = typeof(CompTailorMending);
        }
    }

    /// <summary>
    /// State of a tailor's outfit stand: the per-stand mending toggle and the hit points already paid for in textile but
    /// not yet applied to an item. Both are saved. Kept apart from the storage settings so copying filters or storage
    /// groups never changes them.
    /// </summary>
    public class CompTailorMending : ThingComp
    {
        private const int TextileCheckInterval = 120;

        private bool mendingEnabled = true;
        private int storedPaidHp;
        private int textileCheckedTick = -9999;
        private bool textileAvailable;

        public bool MendingEnabled
        {
            get => mendingEnabled;
            set => mendingEnabled = value;
        }

        /// <summary>Hit points paid for with a textile but not yet restored to an item.</summary>
        public int StoredPaidHp
        {
            get => storedPaidHp;
            set => storedPaidHp = Mathf.Max(0, value);
        }

        private Building_OutfitStand Stand => parent as Building_OutfitStand;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref mendingEnabled, "tailorMendingEnabled", true);
            Scribe_Values.Look(ref storedPaidHp, "tailorStoredPaidHp");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get("Things/Building/OutfitStand/TailorOutfitStand_MenuIcon_south", false),
                isActive = () => MendingEnabled,
                defaultLabel = "OutfitStandsPlusForked_TailorMending_Label".Translate(),
                defaultDesc = "OutfitStandsPlusForked_TailorMending_Desc".Translate(
                    TailorMendingUtility.StartPercent.ToString(),
                    TailorMendingUtility.LimitPercent.ToString()),
                toggleAction = delegate { MendingEnabled = !MendingEnabled; },
            };
        }

        public override string CompInspectStringExtra()
        {
            var stand = Stand;
            if (stand == null || parent.Faction != Faction.OfPlayer)
            {
                return null;
            }

            var text = new StringBuilder();
            text.Append("OutfitStandsPlusForked_TailorMendingState".Translate(
                (MendingEnabled ? "On" : "Off").Translate()));
            if (StoredPaidHp > 0)
            {
                text.Append(", ").Append("OutfitStandsPlusForked_TailorStoredHp".Translate(StoredPaidHp));
            }

            if (MendingEnabled && StoredPaidHp <= 0 && NeedsCloth(stand))
            {
                text.Append(", ").Append("OutfitStandsPlusForked_TailorNeedsCloth".Translate());
            }

            return text.ToString();
        }

        // The stand has something to mend but no textile the colony could fetch. Cached, as the inspect pane asks every frame.
        private bool NeedsCloth(Building_OutfitStand stand)
        {
            if (!stand.Spawned || TailorMendingUtility.TextileNeeded(stand) <= 0)
            {
                return false;
            }

            int now = Find.TickManager.TicksGame;
            if (now - textileCheckedTick >= TextileCheckInterval || now < textileCheckedTick)
            {
                textileCheckedTick = now;
                textileAvailable = TailorMendingUtility.AnyTextileFor(stand);
            }

            return !textileAvailable;
        }
    }
}
