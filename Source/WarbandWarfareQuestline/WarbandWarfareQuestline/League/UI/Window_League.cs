using System;
using UnityEngine;
using UnityEngine.Events;
using Verse;
using WarbandWarfareQuestline.League.Policies.UI;
using WarfareAndWarbands;
using WarfareAndWarbands.Warfare.UI;
namespace WarbandWarfareQuestline.League.UI
{
    internal static class Window_League
    {
        static readonly int iconStartPosY = 0;
        static readonly int iconSize = 50;
        static readonly int settlementCountDisplayOffsetX = 75;
        static readonly int policyTreeButtonY = 50;
        static readonly int policyTreeButtonX = 300;
        static readonly int policyTreeButtonHeight = 50;
        static readonly int policyTreeButtonWidth = 100;
        static readonly int margin = 25;

        static int villageCount = 0;
        static int townCount = 0;

        public static void AppendDrawingEvent()
        {
            WAWUI.onLeagueDrawn.AddListener(Draw);
            WAWUI.onLeagueInit.AddListener(RecalculateSettlementCount);
        }

        public static void RecalculateSettlementCount()
        {
            villageCount = GameComponent_League.Instance.GetRuralCount();
            townCount = GameComponent_League.Instance.GetTownCount();
        }

        public static void Draw()
        {
            DrawLeftPanel();
            DrawRightPanel();
        }

        private static void DrawLeftPanel()
        {
            Rect iconRect = new Rect(0, iconStartPosY, iconSize, iconSize);
            DrawIconInfoFromUpToBottom(ref iconRect, WAWTex.Town, townCount.ToString(), tooltip: "WAW.TownsIcon".Translate());
            DrawIconInfoFromUpToBottom(ref iconRect, WAWTex.Village, villageCount.ToString(), tooltip: "WAW.VillageIcon".Translate());
            DrawIconInfoFromUpToBottom(ref iconRect, WAWTex.BankAccount, GameComponent_WAW.playerBankAccount.Balance.ToString(), true);
            DrawIconInfoFromUpToBottom(ref iconRect, WAWTex.DevelopmentPoints, GameComponent_League.Instance.GetPointsAndNeededPoints(), tooltip: "WAW.DevelopmentPoints".Translate());
            DrawIconInfoFromUpToBottom(ref iconRect, WAWTex.Cohesion, String.Format("{0:P2}", GameComponent_League.Instance.Cohesion), tooltip: "WAW.Cohesion".Translate());
        }

        private static void DrawIconInfoFromUpToBottom(ref Rect iconRect, Texture2D icon, TaggedString value, bool isCurrency = false, string tooltip = null)
        {
            iconRect.position += Vector2.up * iconRect.height;
            DrawIconInfo(iconRect, icon, value, isCurrency);
            if (tooltip != null)
                TooltipHandler.TipRegion(iconRect, tooltip);
        }

        private static void DrawIconInfo(Rect iconRect, Texture2D icon, TaggedString value, bool isCurrency = false)
        {
            Widgets.DrawTextureFitted(iconRect, icon, 1);
            string label = isCurrency ? $"${value}" : value.ToString();
            Widgets.Label(new Rect(settlementCountDisplayOffsetX, iconRect.y + margin, 100, 50), label);
        }

        private static void DrawRightPanel()
        {
            float verticalOffset = margin + policyTreeButtonHeight;

            if (DrawButton(new Rect(policyTreeButtonX, policyTreeButtonY, policyTreeButtonWidth, policyTreeButtonHeight), "WAW.PolicyTreeButton"))
            {
                Find.WindowStack.Add(new Window_PolicyTree(GameComponent_League.Instance.PolicyTree));
            }

            if (DrawButton(new Rect(policyTreeButtonX, policyTreeButtonY + verticalOffset, policyTreeButtonWidth, policyTreeButtonHeight), "WAW.ManageLeague"))
            {
                // Open League Management Window
            }

            if (DrawButton(new Rect(policyTreeButtonX, policyTreeButtonY + 2 * verticalOffset, policyTreeButtonWidth, policyTreeButtonHeight), "WAW.LeagueAction"))
            {
                // Open Action Float Menu
                LeagueActions.OpenActionsMenu();
            }
        }

        private static bool DrawButton(Rect rect, string labelKey)
        {
            return Widgets.ButtonText(rect, labelKey.Translate());
        }
    }

}
