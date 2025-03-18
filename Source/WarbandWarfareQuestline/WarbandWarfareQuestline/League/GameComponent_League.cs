using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.UI;
using WarbandWarfareQuestline.League.WAWScheduled;
using WarbandWarfareQuestline.League.MinorFactions;
using WarbandWarfareQuestline.League.Policies;

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
        private SkirmishEvent _skirmish;
        private PolicyTable _policyTable;
        private readonly int baseEventGenerationTicks;
        private readonly int baseEventGenerationDays = 5;


        public GameComponent_League(Game game)
        {
            Instance = this;
            _minorFactions = new List<MinorFaction>();
            _minorFactionsTemp = new List<MinorFaction>();
            _questChecker = new QuestEvent();   
            _taxer = new TaxEvent();
            _skirmish = new SkirmishEvent();
            _policyTable = new PolicyTable();   
            baseEventGenerationTicks = BaseEventGenrationTicks;
        }

        public FloatRange dateOffset = new FloatRange(.8f, .12f);
        public int BaseEventGenrationTicks => (int)(baseEventGenerationDays * dateOffset.RandomInRange) * GenDate.TicksPerDay;

        public List<MinorFaction> Factions => _minorFactions;
        public List<MinorFaction> FactionsTemp => _minorFactionsTemp;

        bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > baseEventGenerationTicks;
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
            this._skirmish.Check();
        }

        void CheckQuest()
        {
            this._questChecker.Check();
        }

        void RefreshPolicyTable()
        {
            this._policyTable?.Refresh();
        }

        void AppendDrawingEvent()
        {
            Window_League.AppendDrawingEvent();
        }

        public int GetTownCount()
        {
            return this._minorFactions.Where(x => x.TechLevel >= TechLevel.Industrial).Count(); 
        }
        public int GetRuralCount()
        {
            return this._minorFactions.Where(x => x.TechLevel < TechLevel.Industrial).Count();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (ShouldCheckNow())
            {
                //Check village quests,but for now, disable the quest
                CheckQuest();

                // Receive tax every month
                CheckTax();

                // Check skirmish
                CheckSkirmish();

                ResetLastCheckTick();

                Log.Message("Check League");
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            AppendDrawingEvent();
            RefreshPolicyTable();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    Window_League.AppendDrawingEvent();
        }


        public override void LoadedGame()
        {
            base.LoadedGame();
            AppendDrawingEvent();
            RefreshPolicyTable(); 

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
            Scribe_Deep.Look(ref _questChecker, "_questChecker");
            Scribe_Deep.Look(ref _taxer, "_taxer");
            Scribe_Deep.Look(ref _policyTable, "_policyTable");

            if (_taxer == null)
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
            if(_skirmish == null)
            {
                _skirmish = new SkirmishEvent();    
            }
            if(_minorFactionsTemp == null)
            {
                _minorFactionsTemp = new List<MinorFaction>();
            }
            if(_policyTable == null)
            {
                _policyTable = new PolicyTable();
            }

        }

    }
}
