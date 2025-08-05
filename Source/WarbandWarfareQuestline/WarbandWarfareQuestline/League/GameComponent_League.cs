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
using UnityEngine;

namespace WarbandWarfareQuestline.League
{
    public class GameComponent_League : GameComponent
    {
        public static GameComponent_League Instance;
        private int _lastCheckTick = 0;
        private int _developmentPoints = 0;
        private int _developmentLevel = 0;
        private float _cohesion = 0.5f;
        private bool _isTradeTreatyActive = false;
        private static readonly SimpleCurve _developmentCurve = new SimpleCurve
            {
                new CurvePoint(1, 10),
                new CurvePoint(20, 100)
            };
        private List<MinorFactionSettlement> _minorFactionsSettlements;
        private QuestEvent _questChecker;
        private TaxEvent _taxer;
        private SkirmishEvent _skirmish;
        private PolicyTree _policyTree;
        private MilitaryDrillExecuter _militaryDrill;
        private RoadBuilder _roadbuilder;
        private PolicyCategoryDef _hatedPolicyCategory;
        private PolicyCategoryDef _lovedPolicyCategory;

        private readonly int _baseEventGenerationTicks;
        private const int baseEventGenerationDays = 5;
        private const int acceptedSettlementDistance = 50;

        public GameComponent_League(Game game)
        {
            Instance = this;
            _minorFactionsSettlements = new List<MinorFactionSettlement>();
            _questChecker = new QuestEvent();
            _taxer = new TaxEvent();
            _skirmish = new SkirmishEvent();
            _policyTree = new PolicyTree();
            _roadbuilder = new RoadBuilder();
            _militaryDrill = new MilitaryDrillExecuter();
            _isTradeTreatyActive = false;
            _baseEventGenerationTicks = BaseEventGenrationTicks;
        }

        public FloatRange dateOffset = new FloatRange(0.8f, 0.12f);
        public int BaseEventGenrationTicks => (int)(baseEventGenerationDays * dateOffset.RandomInRange) * GenDate.TicksPerDay;

        public List<MinorFactionSettlement> MinorFactionSettlements => _minorFactionsSettlements;
        public List<MinorFaction> Factions => _minorFactionsSettlements.Select(x => x.MinorFaction).ToList();
        public PolicyTree PolicyTree => _policyTree;
        public RoadBuilder RoadBuilder => _roadbuilder;
        public MilitaryDrillExecuter MilitaryDrill => _militaryDrill;
        public float Cohesion => _cohesion;
        public int DevelopmentPoints => _developmentPoints;
        public int DevelopmentLevel => _developmentLevel;
        public bool IsTradeTreatyActive => _isTradeTreatyActive;

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (ShouldCheckNow())
            {
                // Check village quests 
                CheckQuest();

                // Receive tax every month
                CheckTax();

                // Check skirmish -- Disabled for now
                //CheckSkirmish();

                // Policy Tree Tick
                TickPolicies();

                // Reset
                ResetLastCheckTick();
                Log.Message("League Grand Tick");
            }
        }

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
            LeagueDrawer.Draw();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            AppendDrawingEvent();
            RefreshPolicyTable();
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
            Scribe_Collections.Look(ref _minorFactionsSettlements, "_minorFactions", LookMode.Reference);
            Scribe_Deep.Look(ref _questChecker, "_questChecker");
            Scribe_Deep.Look(ref _taxer, "_taxer");
            Scribe_Deep.Look(ref _policyTree, "_policyTable");
            Scribe_Deep.Look(ref _roadbuilder, "_roadbuilder");
            Scribe_Deep.Look(ref _militaryDrill, "_militaryDrill");
            Scribe_Defs.Look(ref _hatedPolicyCategory, "_hatedPolicy");
            Scribe_Defs.Look(ref _lovedPolicyCategory, "_lovedPolicy");
            Scribe_Values.Look(ref _developmentLevel, "_developmentLevel");
            Scribe_Values.Look(ref _developmentPoints, "_developmentPoints");
            Scribe_Values.Look(ref _cohesion, "_cohesion");
            Scribe_Values.Look(ref _isTradeTreatyActive, "_isTradeTreatyActive");

            if (_taxer == null)
            {
                _taxer = new TaxEvent();
            }
            if (_questChecker == null)
            {
                _questChecker = new QuestEvent();
            }

            if (_minorFactionsSettlements == null)
            {
                _minorFactionsSettlements = new List<MinorFactionSettlement>();
            }
            _minorFactionsSettlements.RemoveAll(x => x == null);

            if (_skirmish == null)
            {
                _skirmish = new SkirmishEvent();
            }
            if (_policyTree == null)
            {
                _policyTree = new PolicyTree();
            }
            if (_roadbuilder == null)
            {
                _roadbuilder = new RoadBuilder();
            }
            if (_militaryDrill == null)
            {
                _militaryDrill = new MilitaryDrillExecuter();
            }
        }

        private bool ShouldCheckNow()
        {
            return GenTicks.TicksGame - _lastCheckTick > _baseEventGenerationTicks;
        }

        private void ResetLastCheckTick()
        {
            _lastCheckTick = GenTicks.TicksGame;
        }

        private void CheckTax()
        {
            _taxer.Check();
        }

        private void CheckSkirmish()
        {
            _skirmish.Check();
        }

        private void CheckQuest()
        {
            _questChecker.Check();
        }

        private void RefreshPolicyTable()
        {
            _policyTree?.Refresh();
        }

        private void AppendDrawingEvent()
        {
            Window_League.AppendDrawingEvent();
        }

        public int GetTownCount()
        {
            return _minorFactionsSettlements.Count(x => x.MinorFaction.TechLevel >= TechLevel.Industrial);
        }

        public int GetRuralCount()
        {
            return _minorFactionsSettlements.Count(x => x.MinorFaction.TechLevel < TechLevel.Industrial);
        }

        public void InformPlayerOnCongressAvailable()
        {
            if (_minorFactionsSettlements.Count == 1)
            {
                Letter letter = LetterMaker.MakeLetter("WAW.CongressAvailable".Translate(), "WAW.CongressAvailable.Desc".Translate(), LetterDefOf.PositiveEvent);
                Find.LetterStack.ReceiveLetter(letter);
            }
        }

        private void ResettleSettlement(MinorFactionSettlement settlement)
        {
            // Implement resettlement logic here
            var playerBaseMap = Find.AnyPlayerHomeMap;
            if (playerBaseMap == null)
            {
                return;
            }
            if (Find.WorldGrid.ApproxDistanceInTiles(settlement.Tile, playerBaseMap.Parent.Tile) > acceptedSettlementDistance)
                MinorFactionBaseUtil.ResettleSettlement(settlement);
        }

        public void JoinPlayer(MinorFactionSettlement settlement)
        {
            if (!_minorFactionsSettlements.Contains(settlement))
            {
                // Decide hated policy
                int commons = _minorFactionsSettlements.Count(m => m.MinorFaction.Trait.dislikedCategory == settlement.MinorFaction.Trait.dislikedCategory) + 1;
                if (commons >= (float)_minorFactionsSettlements.Count / 2)
                {
                    _hatedPolicyCategory = settlement.MinorFaction.Trait.dislikedCategory;
                    Log.Message($"Disliked trait changed: {settlement.MinorFaction.Trait.dislikedCategory}");
                }
                _minorFactionsSettlements.Add(settlement);

                // If the settlement is too far away, resettle the settlement
                ResettleSettlement(settlement);

                // Inform the player if the faction is the first one
                InformPlayerOnCongressAvailable();
            }
        }

        void DoRandomFactionResign()
        {
            if(_minorFactionsSettlements.Count < 1)
            {
                return;
            }
            RemoveRandomFactionFromLeague(out string factionName);
            Letter l = LetterMaker.MakeLetter("WAW.RandomResign".Translate(), "WAW.RandomResign.Desc".Translate(factionName), LetterDefOf.NegativeEvent);
            Find.LetterStack.ReceiveLetter(l);
        }

        void RemoveRandomFactionFromLeague(out string factionName)
        {
            var settlement = _minorFactionsSettlements.RandomElement();
            factionName = settlement.MinorFaction.FactionName;  
            RemoveFactionFromLeague(settlement);
        }

        public void RemoveFactionFromLeague(MinorFactionSettlement settlement)
        {
            this._minorFactionsSettlements.RemoveAll(x => x.MinorFaction == settlement.MinorFaction);
            settlement.Destroy();
        }

        public bool NoFactionInLeague()
        {
            return _minorFactionsSettlements.Count <= 0;
        }

        public bool PointsInssufficient()
        {
            return _developmentPoints < GetNeededPoints();
        }

        public int GetNeededPoints()
        {
            return (int)_developmentCurve.Evaluate(_developmentLevel + 1);
        }

        public void LevelUP()
        {
            _developmentPoints -= GetNeededPoints();
            _developmentLevel++;
        }

        public void AddDevelopmentPoints(int points)
        {
            _developmentPoints += points;
            if (PointsInssufficient())
            {
                return;
            }
            // If the points are enough to level up, notify the player to choose a policy
            Letter inform = LetterMaker.MakeLetter("WAW.CanChoosePolicy".Translate(), "WAW.CanChoosePolicy.Desc".Translate(), LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(inform);
        }

        public void FullfillDevelopmentPoints()
        {
            _developmentPoints = GetNeededPoints();
        }

        public string GetPointsAndNeededPoints()
        {
            return $"{_developmentPoints}/{GetNeededPoints()}(L{_developmentLevel})";
        }

        public void OnPolicyChosen(PolicyDef policy)
        {
            LevelUP();
        }

        public void AffectCohesion(float amount)
        {
            if (_cohesion + amount < 0)
            {
                //Remove a random faction
                DoRandomFactionResign();
                return;
            }
            _cohesion = Mathf.Min(Mathf.Max(_cohesion + amount, 0), 1);
        }

        public void SetTradeTreaty(bool isActive)
        {
            _isTradeTreatyActive = isActive;
        }

        private void TickPolicies()
        {
            _policyTree?.Tick();
        }

       
    }
}
