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
        public MinorFaction mFaction;

        public MinorFaction MFaction
        {
            get
            {
              
                if(mFaction == null)
                {
                    Log.Message($"{factionID},{GameComponent_League.Instance.FactionsTemp.First().FactionID}");
                    mFaction = GameComponent_League.Instance.FactionsTemp.Any(x => x.FactionID == factionID) ? GameComponent_League.Instance.FactionsTemp.First(x => x.FactionID == factionID) : null;
                }
                return mFaction;    
            }
        }

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("WAW.JoinPlayerLeague".Translate(), delegate (Rect r)
                {
                    if (MFaction != null)
                    {
                        GUI.color = MFaction.FactionColor;
                        GUI.DrawTexture(r, MFaction.GetFactionIcon());
                        GUI.color = Color.white;
                    }
                }, () => MFaction == null ? new TaggedString("") : "WAW.JoinPlayerLeague.ToolTip".Translate(mFaction.FactionName) + "\n" + mFaction.Trait.description);
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
            valueActuallyUsed = 0f;
        }

        public override void Notify_Used()
        {
            base.Notify_Used();
            this.mFaction.JoinPlayer();
            Log.Message("Current Faction Count: " + GameComponent_League.Instance.Factions.Count);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref factionID, "factionID");
        }




    }
}
