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
        public string factionID;
        public MinorFactionSettlement mFactionBase;

        public MinorFactionSettlement MFactionBase
        {
            get
            {
                if (mFactionBase == null)
                {
                    mFactionBase = GameComponent_League.Instance.MinorFactionSettlements.Any(x => x.MinorFaction.FactionID == factionID)
                        ? GameComponent_League.Instance.MinorFactionSettlements.First(x => x.MinorFaction.FactionID == factionID)
                        : null;
                }
                return mFactionBase;
            }
        }

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("WAW.JoinPlayerLeague".Translate(), delegate (Rect r)
                {
                    if (MFactionBase != null)
                    {
                        GUI.color = MFactionBase.MinorFaction.FactionColor;
                        GUI.DrawTexture(r, MFactionBase.MinorFaction.GetFactionIcon());
                        GUI.color = Color.white;
                    }
                }, () => MFactionBase == null ? new TaggedString("") : "WAW.JoinPlayerLeague.ToolTip".Translate(MFactionBase.MinorFaction.FactionName) + "\n" + MFactionBase.MinorFaction.Trait.description);
            }
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            yield return new QuestPart_DescriptionPart();
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            return "WAW.JoinPlayerLeague.ToolTip".Translate(MFactionBase.MinorFaction.FactionName) + "\n" + MFactionBase.MinorFaction.Trait.description;
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            valueActuallyUsed = 0f;
        }

        public override void Notify_Used()
        {
            base.Notify_Used();
            MFactionBase.JoinPlayer();
            Log.Message("Current Faction Count: " + GameComponent_League.Instance.Factions.Count);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionID, "factionID");
        }
    }
}
