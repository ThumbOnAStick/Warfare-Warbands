using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;
using WarbandWarfareQuestline.League;
using WarbandWarfareQuestline.League.MinorFactions;

namespace WarbandWarfareQuestline.Questline
{
    public class Reward_MinorFactionJoin : Reward
    {
        public MinorFactionSettlement mFactionBase;

        private const int developmentPointsBonus = 20;

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("WAW.JoinPlayerLeague".Translate(), delegate (Rect r)
                {
                    if (mFactionBase != null)
                    {
                        GUI.color = mFactionBase.MinorFaction.FactionColor;
                        GUI.DrawTexture(r, mFactionBase.MinorFaction.GetFactionIcon());
                        GUI.color = Color.white;
                    }
                }, () => mFactionBase == null ? new TaggedString("") : "WAW.JoinPlayerLeague.ToolTip".Translate(mFactionBase.MinorFaction.FactionName) + "\n" + mFactionBase.MinorFaction.Trait.description);
            }
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            yield return new QuestPart_DescriptionPart();
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            return "WAW.JoinPlayerLeague.ToolTip".Translate(mFactionBase.MinorFaction.FactionName) + "\n" + mFactionBase.MinorFaction.Trait.description;
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            valueActuallyUsed = 0f;
        }

        public override void Notify_Used()
        {
            base.Notify_Used();
            mFactionBase.JoinPlayer();
            GameComponent_League.Instance.AddDevelopmentPoints(developmentPointsBonus);
            Log.Message("Current Faction Count: " + GameComponent_League.Instance.Factions.Count);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mFactionBase, "factionID");
        }
    }
}
