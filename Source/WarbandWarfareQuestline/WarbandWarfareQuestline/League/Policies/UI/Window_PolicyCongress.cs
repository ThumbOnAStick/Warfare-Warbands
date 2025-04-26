using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using WarbandWarfareQuestline.League.MinorFactions;
using WarbandWarfareQuestline.League.UI;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    class Window_PolicyCongress : Window_Congress
    {
        private readonly Policy _policy;

        public Window_PolicyCongress() : base()
        {
            _policy = new Policy(PolicyDefOf.TaxReform, true);
        }

        public Window_PolicyCongress(List<MinorFaction> pros, List<MinorFaction> dissenters, Policy policy) : base(pros, dissenters)
        {
            _policy = policy;
        }

        void DrawHeader()
        {
            Widgets.Label(_topTextDrawingArea, "WAW.Weshouldapprove".Translate(_policy.Def.label));
        }

        void DrawPolicyDescription()
        {
            _topTextDrawingArea.position += new Vector2(0, _topTextDrawingArea.height);
            string desc = $"<color=#808080>{_policy.Def.description}</color>";
            if (_policy.Def.equipmentBudgetLimitOffset > 0)
            {
                desc += " " + "WAW.EquipmentImprovingPolicy".Translate(_policy.Def.equipmentBudgetLimitOffset);
            }
            Widgets.Label(_topTextDrawingArea, desc + "WAW.TaxOffset".Translate(String.Format("{0:P2}", _policy.Def.taxBonus)));
        }

        void DrawVotingResult()
        {
            _topTextDrawingArea.position += new Vector2(0, _topTextDrawingArea.height);
            if (_leftPercentage < 0.5f)
            {
                GUI.color = Color.red;
                Widgets.Label(_topTextDrawingArea, "WAW.PolicyWillBeDisapproved".Translate(_policy.Def.label));
            }
            else
            {
                GUI.color = Color.green;
                Widgets.Label(_topTextDrawingArea, "WAW.PolicyWillBeApproved".Translate(_policy.Def.label));
            }
            GUI.color = Color.white;
        }


        protected override void DoApproveResult()
        {
            base.DoApproveResult();
            GameComponent_League.Instance.OnPolicyChosen(_policy.Def);
            this._policy.Execute();
        }


        protected override void DrawTopicDescriptions(Rect inRect)
        {

            base.DrawTopicDescriptions(inRect);
            // Draw header
            DrawHeader();

            // Draw policy description
            DrawPolicyDescription();

            // Draw voting results
            DrawVotingResult();

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
