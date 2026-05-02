using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace OutfitStandsPlus.ThingComps
{
    public class CompProperties_OutfitStandsPlusStandComp : CompProperties
    {
        public CompProperties_OutfitStandsPlusStandComp()
        {
            compClass = typeof(OutfitStandsPlusStandComp);
        }
    }

    public class OutfitStandsPlusStandComp : ThingComp
    {
        private string customName;
        private List<int> syncedApparelPolicyIds = new List<int>();
        private int lastPolicySyncTick = -1;

        public Building_OutfitStand Stand => parent as Building_OutfitStand;

        public Pawn AssignedPawn
        {
            get
            {
                var assignmentComp = Stand?.GetComp<CompAssignableToPawn_OutfitStandsPlusBase>();
                return assignmentComp?.AssignedPawns.FirstOrDefault();
            }
        }

        public string DisplayName
        {
            get
            {
                Pawn assignedPawn = AssignedPawn;
                if (assignedPawn != null)
                {
                    return "OutfitStandsPlus_AutoStandName".Translate(GetPawnNickName(assignedPawn));
                }

                if (!customName.NullOrEmpty())
                {
                    return customName;
                }
                return parent.LabelCap;
            }
        }

        public bool HasCustomName => !customName.NullOrEmpty();

        public bool HasAssignedPawnName => AssignedPawn != null;

        public IReadOnlyList<int> SyncedApparelPolicyIds => syncedApparelPolicyIds;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref customName, "OutfitStandsPlusCustomName");
            Scribe_Collections.Look(ref syncedApparelPolicyIds, "OutfitStandsPlusSyncedApparelPolicyIds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && syncedApparelPolicyIds == null)
            {
                syncedApparelPolicyIds = new List<int>();
            }
        }

        public void SetCustomName(string name)
        {
            customName = name.NullOrEmpty() ? null : name.Trim();
        }

        private static string GetPawnNickName(Pawn pawn)
        {
            string shortName = pawn.Name?.ToStringShort;
            if (!shortName.NullOrEmpty())
            {
                return shortName;
            }
            return pawn.LabelShort;
        }

        public bool IsPolicySelected(ApparelPolicy policy)
        {
            return policy != null && syncedApparelPolicyIds.Contains(policy.id);
        }

        public void TogglePolicy(ApparelPolicy policy)
        {
            if (policy == null)
            {
                return;
            }
            if (syncedApparelPolicyIds.Contains(policy.id))
            {
                syncedApparelPolicyIds.Remove(policy.id);
            }
            else
            {
                syncedApparelPolicyIds.Add(policy.id);
            }
            lastPolicySyncTick = -1;
            SyncApparelPolicyFilter();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            SyncApparelPolicyFilter();

            yield return new Command_Action
            {
                icon = TexButton.Rename,
                defaultLabel = "OutfitStandsPlus_RenameStand_Label".Translate(),
                defaultDesc = "OutfitStandsPlus_RenameStand_Desc".Translate(),
                action = delegate { Find.WindowStack.Add(new Dialog_OutfitStandsPlusRenameStand(this)); }
            };

            yield return new Command_Action
            {
                icon = TexButton.Copy,
                defaultLabel = "OutfitStandsPlus_ApparelPolicySync_Label".Translate(syncedApparelPolicyIds.Count),
                defaultDesc = "OutfitStandsPlus_ApparelPolicySync_Desc".Translate(),
                action = ShowPolicyMenu
            };
        }

        public void SyncApparelPolicyFilter()
        {
            Building_OutfitStand stand = Stand;
            if (stand == null || syncedApparelPolicyIds.NullOrEmpty() || Current.Game?.outfitDatabase == null)
            {
                return;
            }

            int currentTick = Find.TickManager?.TicksGame ?? -1;
            if (lastPolicySyncTick == currentTick)
            {
                return;
            }
            lastPolicySyncTick = currentTick;

            List<ApparelPolicy> policies = Current.Game.outfitDatabase.AllOutfits
                .Where(policy => syncedApparelPolicyIds.Contains(policy.id))
                .ToList();

            if (policies.Count == 0)
            {
                syncedApparelPolicyIds.Clear();
                return;
            }

            foreach (ThingDef apparelDef in DefDatabase<ThingDef>.AllDefs.Where(def => def.apparel != null))
            {
                bool allowed = policies.Any(policy => policy.filter.Allows(apparelDef));
                stand.StoreSettings.filter.SetAllow(apparelDef, allowed);
            }
        }

        private void ShowPolicyMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            List<ApparelPolicy> policies = Current.Game?.outfitDatabase?.AllOutfits;
            if (policies == null || policies.Count == 0)
            {
                options.Add(new FloatMenuOption("OutfitStandsPlus_NoApparelPolicies".Translate(), null));
            }
            else
            {
                foreach (ApparelPolicy policy in policies)
                {
                    ApparelPolicy localPolicy = policy;
                    string prefix = IsPolicySelected(policy) ? "[x] " : "[ ] ";
                    options.Add(new FloatMenuOption(prefix + policy.label, delegate { TogglePolicy(localPolicy); }));
                }
            }

            if (!syncedApparelPolicyIds.NullOrEmpty())
            {
                options.Add(new FloatMenuOption("OutfitStandsPlus_ClearApparelPolicySync".Translate(), delegate
                {
                    syncedApparelPolicyIds.Clear();
                    lastPolicySyncTick = -1;
                    SyncApparelPolicyFilter();
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    public class Dialog_OutfitStandsPlusRenameStand : Window
    {
        private readonly OutfitStandsPlusStandComp comp;
        private string nameBuffer;

        public override Vector2 InitialSize => new Vector2(420f, 160f);

        public Dialog_OutfitStandsPlusRenameStand(OutfitStandsPlusStandComp comp)
        {
            this.comp = comp;
            nameBuffer = comp.HasCustomName ? comp.DisplayName : string.Empty;
            doCloseX = true;
            closeOnAccept = true;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "OutfitStandsPlus_RenameStand_Label".Translate());
            Text.Font = GameFont.Small;

            nameBuffer = Widgets.TextField(new Rect(0f, 42f, inRect.width, 32f), nameBuffer, 60);

            float buttonWidth = 120f;
            Rect saveRect = new Rect(inRect.width - buttonWidth, inRect.height - 35f, buttonWidth, 35f);
            Rect resetRect = new Rect(inRect.width - buttonWidth * 2f - 10f, inRect.height - 35f, buttonWidth, 35f);

            if (Widgets.ButtonText(resetRect, "OutfitStandsPlus_ClearName".Translate()))
            {
                comp.SetCustomName(null);
                Close();
            }

            if (Widgets.ButtonText(saveRect, "OK".Translate()))
            {
                comp.SetCustomName(nameBuffer);
                Close();
            }
        }
    }
}
