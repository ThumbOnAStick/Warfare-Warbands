using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.MinorFactions;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.Questline
{
    public static class Quests
    {

        static Quest Generate(string nameKey, string descriptionKey)
        {
            Quest quest = new Quest
            {
                id = Find.UniqueIDsManager.GetNextQuestID(),
                appearanceTick = Find.TickManager.TicksGame,
                challengeRating = 4,
                name = nameKey.Translate(),
                description = descriptionKey.Translate(),
                ticksUntilAcceptanceExpiry = GenDate.TicksPerDay * 5,
                root = WAWDefof.WAW_SaveVillage
            };
            return quest;
        }
         
        public static void GiveVillageQuest()
        {
            //Generate a village
            MinorFaction m = MinorFactionHelper.GenerateMinorFaction(FactionTraitDefOf.WAW_Cautious, TechLevel.Neolithic);

            // Give the quest
            Quest quest = Generate("WAW.SaveVillage", "WAW.SaveVillage.Desc");
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
            Find.QuestManager.Add(quest);
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("WAW.QuestAvailable".Translate(quest.name), quest.description, LetterDefOf.PositiveEvent, null, quest));
        }


    }
}
