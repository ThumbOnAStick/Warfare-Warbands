using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;
using static System.Collections.Specialized.BitVector32;

namespace WarbandWarfareQuestline.Questline
{
    public class Reward_MinorFactionJoin : Reward
    {
        public MinorFaction mFaction;

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("WAW.JoinPlayerLeague".Translate(), delegate (Rect r)
                {
                    GUI.color = mFaction.FactionColor;
                    GUI.DrawTexture(r, mFaction.GetFactionIcon());
                    GUI.color = Color.white;
                }, () => "WAW.JoinPlayerLeague.ToolTip".Translate(mFaction.FactionName) + "\n" + mFaction.Trait.description);
            }
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            yield return new QuestPart_DescriptionPart();
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            return "WAW.JoinPlayerLeague.ToolTip".Translate(mFaction.FactionName) + "\n" + mFaction.Trait.description;
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            throw new NotImplementedException();
        }
    }
}
