using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.Questline;

namespace WarbandWarfareQuestline.League
{
    public class GameComponent_League:GameComponent
    {
        private int _lastCheckTick = 0;
        public static GameComponent_League Instance;
        private readonly int policyGenerationDays = 15;
        private readonly int eventGenerationDays = 15;

        public GameComponent_League(Game game)
        {
            Instance = this;
        }

        public int EventGenrationTicks => eventGenerationDays * GenDate.TicksPerDay;

        bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > EventGenrationTicks;
        }

        void ResetLastCheckTick()
        {
            _lastCheckTick = GenTicks.TicksGame;
        }

        void GiveVillageQuest()
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
                    rewards = new List<Reward>(){ new Reward_MinorFactionJoin() { mFaction = m} }
                }
            };
            quest.AddPart(new QuestPart_Choice() { choices = choices });
            quest.AddPart(new QuestPart_VillageLooted() { inSignalEnable = QuestGen.slate.Get<string>("inSignal", null, false), faction = m});
            quest.root = new QuestScriptDef();
            Find.QuestManager.Add(quest);
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("WAW.QuestAvailable".Translate(quest.name), quest.description, LetterDefOf.PositiveEvent, null, quest));
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (ShouldCheckNow())
            {
                ResetLastCheckTick();
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            GiveVillageQuest();
         }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("WAW: league module active");
        }

    }
}
