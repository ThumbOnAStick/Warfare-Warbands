using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League;

namespace WarbandWarfareQuestline.Questline
{
    public static class Quests
    {
        public static void GiveVillageQuest()
        {
            //Generate a village
            MinorFaction m = MinorFactionHelper.GenerateMinorFaction(FactionTraitDefOf.WAW_Cautious, TechLevel.Neolithic, FactionDefOf.TribeCivil.DefaultColor);

            // Give player a quest
            Quest quest = new Quest
            {
                challengeRating = 4,
                name = "WAW.SaveVillage".Translate(),
                description = "WAW.SaveVillage.Desc".Translate(),
                ticksUntilAcceptanceExpiry = GenDate.TicksPerDay * 5
            };
            List<QuestPart_Choice.Choice> choices =
            new List<QuestPart_Choice.Choice>()
            {
                new QuestPart_Choice.Choice()
                {
                    questParts =  new List<QuestPart>(),
                    rewards = new List<Reward>(){ new Reward_MinorFactionJoin() { mFaction = m, factionID = m.FactionID} }
                }
            };
            quest.AddPart(new QuestPart_Choice() { choices = choices });
            quest.AddPart(new QuestPart_VillageLooted() { inSignalEnable = QuestGen.slate.Get<string>("inSignal", null, false), faction = m });
            quest.root = QuestScriptDefOf.WandererJoins;
            Find.QuestManager.Add(quest);
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("WAW.QuestAvailable".Translate(quest.name), quest.description, LetterDefOf.PositiveEvent, null, quest));
        }


    }
}
