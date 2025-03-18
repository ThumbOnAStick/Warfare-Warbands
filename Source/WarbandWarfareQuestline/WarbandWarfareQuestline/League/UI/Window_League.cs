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
        static readonly int townPosY = 50;
        static readonly int villagePosY = 100;
        static readonly int bankPosY = 150;
        static readonly int settlementCountDisplayOffsetX = 75;
        static readonly int settlementCountDisplayOffsetY = 25;
        static readonly int policyTreeButtonY = 60;
        static readonly int policyTreeButtonX = 300;
        static readonly int policyTreeButtonHight = 50;
        static readonly int policyTreeButtonWidth = 100;




        static int villageCount = 0;
        static int townCount = 0;


        public static void AppendDrawingEvent() 
        {
            WAWUI.onLeagueDrawn.AddListener(new UnityAction(Draw));
            WAWUI.onLeagueInit.AddListener(new UnityAction(RecalculateSettlementCount));

        }

        public static void RecalculateSettlementCount()
        {
            villageCount = GameComponent_League.Instance.GetRuralCount();
            townCount = GameComponent_League.Instance.GetTownCount();
        }

        public static void Draw()
        {
            // left
            Widgets.DrawTextureFitted(new Rect(0, townPosY, 50, 50), WAWTex.Town, 1);
            Widgets.Label(new Rect(settlementCountDisplayOffsetX, townPosY + settlementCountDisplayOffsetY, 100, 50), townCount.ToString());
            Widgets.DrawTextureFitted(new Rect(0, villagePosY, 50, 50), WAWTex.Village, 1);
            Widgets.Label(new Rect(settlementCountDisplayOffsetX, villagePosY + settlementCountDisplayOffsetY, 100, 50), villageCount.ToString());
            Widgets.DrawTextureFitted(new Rect(0, bankPosY, 50, 50), WAWTex.BankAccount, 1);
            Widgets.Label(new Rect(settlementCountDisplayOffsetX, bankPosY + settlementCountDisplayOffsetY, 100, 50), $"${GameComponent_WAW.playerBankAccount.Balance}");

            //right
            Rect policyButtonR = new Rect(policyTreeButtonX, policyTreeButtonY, policyTreeButtonWidth, policyTreeButtonHight);
            bool openPolicyTree = Widgets.ButtonText(policyButtonR, "WAW.PolicyTreeButton".Translate());
            if (openPolicyTree)
            {
                Find.WindowStack.Add(new Window_PolicyTree(GameComponent_League.Instance.PolicyTree));
            }
        }
    }
}
