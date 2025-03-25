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
using WarbandWarfareQuestline.League.RoadBuilding;
using WarbandWarfareQuestline.League.Policies.UI;

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
        private PolicyTree _policyTree;
        private RoadBuilder _roadbuilder;
        private PolicyCategoryDef _hatedPolicyCategory;
        private PolicyCategoryDef _lovedPolicyCategory;
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
            _policyTree = new PolicyTree();
            _roadbuilder = new RoadBuilder();
            baseEventGenerationTicks = BaseEventGenrationTicks;
        }

        public FloatRange dateOffset = new FloatRange(.8f, .12f);
        public int BaseEventGenrationTicks => (int)(baseEventGenerationDays * dateOffset.RandomInRange) * GenDate.TicksPerDay;

        public List<MinorFaction> Factions => _minorFactions;
        public List<MinorFaction> FactionsTemp => _minorFactionsTemp;
        public PolicyTree PolicyTree => _policyTree;
        public RoadBuilder RoadBuilder => _roadbuilder;

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
            this._policyTree?.Refresh();
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

        public void JoinPlayer(MinorFaction f)
        {
            bool ContainsFaction(MinorFaction x) => x.FactionID == f.FactionID;

            _minorFactionsTemp.RemoveAll(ContainsFaction);

            if (!_minorFactions.Contains(f))
            {
                // Decide hated policy
                int commons = _minorFactions.Count(m => m.Trait.dislikedCategory == f.Trait.dislikedCategory) + 1;
                if (commons >= (float)_minorFactions.Count / 2)
                {
                    _hatedPolicyCategory = f.Trait.dislikedCategory;
                    Log.Message($"Disliked trait changed: {f.Trait.dislikedCategory}");
                }
                _minorFactions.Add(f);
            }
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
            RefreshPolicyTable();

            // Create a congress window with randomized minor factions
            //Find.WindowStack.Add(new Window_Congress(GenerateRandomMinorFactions(), GenerateRandomMinorFactions(), new Policies.Policy(PolicyDefOf.TaxReform, false)));
        }

        /// <summary>
        /// This function is temproray and should be deleted after testing
        /// </summary>

        List<MinorFaction> GenerateRandomMinorFactions()
        {
            List<MinorFaction> factions = new List<MinorFaction>();
            int count = new IntRange(1, 5).RandomInRange;
            for (int i = 0; i < count; i++)
            {
                factions.Add(MinorFactionHelper.GenerateMinorFaction(FactionTraitDefOf.WAW_Cautious, RandomTechLevel()));
            }

            return factions;
        }

        TechLevel RandomTechLevel()
        {
            Array values = Enum.GetValues(typeof(TechLevel));
            Random random = new Random();
            return (TechLevel)values.GetValue(random.Next(values.Length));
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
            Scribe_Deep.Look(ref _policyTree, "_policyTable");
            Scribe_Deep.Look(ref _roadbuilder, "_roadbuilder");
            Scribe_Defs.Look(ref _hatedPolicyCategory, "_hatedPolicy");
            Scribe_Defs.Look(ref _lovedPolicyCategory, "_lovedPolicy");

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
            if(_policyTree == null)
            {
                _policyTree = new PolicyTree();
            }
            if (_roadbuilder == null)
            {
                _roadbuilder = new RoadBuilder();
            }

        }

       

    }
}
