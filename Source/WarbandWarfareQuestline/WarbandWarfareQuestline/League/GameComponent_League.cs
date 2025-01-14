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
        public static GameComponent_League Instance;
        private int _lastCheckTick = 0;
        private List<MinorFaction> _minorFactions;
        private List<MinorFaction> _minorFactionsTemp;
        private readonly int policyGenerationDays = 15;
        private readonly int eventGenerationDays = 15;
        

        public GameComponent_League(Game game)
        {
            Instance = this;
            _minorFactions = new List<MinorFaction>();
            _minorFactionsTemp = new List<MinorFaction>();
        }

        public int EventGenrationTicks => eventGenerationDays * GenDate.TicksPerDay;
        public List<MinorFaction> Factions => _minorFactions;
        public List<MinorFaction> FactionsTemp => _minorFactionsTemp;

        bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > EventGenrationTicks;
        }

        void ResetLastCheckTick()
        {
            _lastCheckTick = GenTicks.TicksGame;
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
            //Quests.GiveVillageQuest();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("WAW: league module active");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _minorFactions, "_minorFactions", LookMode.Deep);
            Scribe_Collections.Look(ref _minorFactionsTemp, "_minorFactionsTemp", LookMode.Deep);
        }

    }
}
