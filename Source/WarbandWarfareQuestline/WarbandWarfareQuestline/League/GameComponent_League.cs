﻿using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.UI;
using WarbandWarfareQuestline.League.WAWScheduled;
using WarbandWarfareQuestline.Questline;
using WarbandWarfareQuestline.Skirmish;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline.League
{
    public class GameComponent_League:GameComponent
    {
        public static GameComponent_League Instance;
        private int _lastCheckTick = 0;
        private List<MinorFaction> _minorFactions;
        private List<MinorFaction> _minorFactionsTemp;
        private QuestEvent _questChecker;
        private TaxEvent _taxer;
        private readonly int baseEventGenerationDays = 5;
        private readonly int minorFactionEventGenerationDays = 30;
        private readonly int skirmishInterval = 3;


        public GameComponent_League(Game game)
        {
            Instance = this;
            _minorFactions = new List<MinorFaction>();
            _minorFactionsTemp = new List<MinorFaction>();
            _questChecker = new QuestEvent();   
            _taxer = new TaxEvent();    
        }

        public FloatRange dateOffset = new FloatRange(.8f, .12f);
        public int BaseEventGenrationTicks => (int)(baseEventGenerationDays * dateOffset.RandomInRange) * GenDate.TicksPerDay;
        public int SkirmishIntervalTicks => BaseEventGenrationTicks + skirmishInterval * GenDate.TicksPerDay;

        public List<MinorFaction> Factions => _minorFactions;
        public List<MinorFaction> FactionsTemp => _minorFactionsTemp;

        bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > BaseEventGenrationTicks;
        }

        bool ShouldDoSkirmishNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > SkirmishIntervalTicks;

        }

        void ResetLastCheckTick()
        {
            _lastCheckTick = GenTicks.TicksGame;
        }

        void CheckTax()
        {
            this._taxer.Check();
        }

        void CheckSkirmish()
        {
            if(ShouldDoSkirmishNow())
            {
                GameComponent_Skrimish.Instance.CreateRandomSkirmsish();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (ShouldCheckNow())
            {
                //Check quests, for the sake of players, diable this shit for now.
                //this._questChecker.Check();

                // Receive Tax Every Month
                CheckTax();

                // Check 
                CheckSkirmish();

                ResetLastCheckTick();
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            // Disable village quest for now
            if (!Prefs.DevMode)
            {
                return;
            }
            Quests.GiveVillageQuest();
            Window_League.AppendDrawingEvent();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            Window_League.AppendDrawingEvent();
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
            Scribe_Deep.Look(ref _questChecker, "_questChecker", LookMode.Deep);
            Scribe_Deep.Look(ref _taxer, "_taxer", LookMode.Deep);
            if(_taxer == null)
            {
                _taxer = new TaxEvent();
            }
            if(_questChecker == null)
            {
                _questChecker = new QuestEvent();
            }
            if(_minorFactions == null)
            {
                _minorFactions = new List<MinorFaction>();
            }
            if(_minorFactionsTemp == null)
            {
                _minorFactionsTemp = new List<MinorFaction>();
            }

        }

    }
}
