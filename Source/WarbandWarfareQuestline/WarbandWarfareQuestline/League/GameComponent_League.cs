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
using WarbandWarfareQuestline.League.MinorFactions;
using WarbandWarfareQuestline.League.Policies;
using WarbandWarfareQuestline.League.RoadBuilding;
using WarbandWarfareQuestline.League.Policies.UI;
using UnityEngine;

namespace WarbandWarfareQuestline.League
{
    public class GameComponent_League:GameComponent
    {
        public static GameComponent_League Instance;
        private int _lastCheckTick = 0;
        private int _developmentPoints = 0;
        private int _developmentLevel = 0;
        private bool _canChoosePolicyNow = false;
        private float _cohesion = .5f;
        private static readonly SimpleCurve _developmentCurve = new SimpleCurve
        {
            new CurvePoint(1, 10),
            new CurvePoint(20, 100)
        };
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
        public bool CanChoosePolicyNow => _canChoosePolicyNow;
        public float Cohesion => _cohesion;
        public int DevelopmentPoints => _developmentPoints;
        public int DevelopmentLevel => _developmentLevel;   

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

        public bool NoFactionInLeague()
        {
            return _minorFactions.Count <= 0;
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
            _developmentLevel++;
            _developmentPoints = 0;
        }

        public void AddDevelopmentPoints(int points)
        {
            _developmentPoints += points;
        }

        public void FullfillDevelopmentPoints()
        {
            this._developmentPoints = GetNeededPoints();
        }   

        public string GetPointsAndNeededPoints()
        {
            return $"{_developmentPoints}/{GetNeededPoints()}(L{_developmentLevel})";
        }

        public void OnPolicyChosen(PolicyDef policy)
        {
            this.LevelUP();
            _canChoosePolicyNow = false; 
        }

        public void AddCohesion(float amount)
        {
            if(_cohesion + amount < 0)
            {
                // TODO: Add cohesion penalty
                return;
            }
            _cohesion = Mathf.Min(Mathf.Max(_cohesion + amount, 0));
        }   

        public void SetCohesion(float amount)
        {
            _cohesion = amount;
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
            Scribe_Values.Look(ref _developmentLevel, "_developmentLevel");
            Scribe_Values.Look(ref _developmentPoints, "_developmentPoints");
            Scribe_Values.Look(ref _canChoosePolicyNow, "_canChoosePolicyNow");
            Scribe_Values.Look(ref _cohesion, "_cohesion");

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
