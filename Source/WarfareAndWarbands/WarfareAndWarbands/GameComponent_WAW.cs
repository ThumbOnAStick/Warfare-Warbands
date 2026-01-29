using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using UnityEngine.Events;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warfare.Bank;
using WarfareAndWarbands.Warfare.UI;

namespace WarfareAndWarbands
{
    public class GameComponent_WAW : GameComponent  
    {
        List<Faction> factions = new List<Faction>();
        List<int> durabilitities = new List<int>();
        List<ThingDef> _mortarShells = new List<ThingDef>();
        List<ThingDef> _minifiables = new List<ThingDef>();
        List<ThingDef> _rawResources = new List<ThingDef>();
        Dictionary<Faction, int> factionsAndWarDurabilities = new Dictionary<Faction, int>();
        public static PlayerWarbandArrangement playerWarbandPreset;
        public static WAWBankAccount playerBankAccount;
        public static GameComponent_WAW Instance;
        public UnityEvent onRaid;
        public UnityEvent onRaided;
        public UnityEvent onLeaderAbilityUsed;
        private bool everUsedQuickRaid = false;
        private bool everAssignedWarbandLeader = false;
        private bool everInformedAboutTownBuilding = false;
        private bool _isEliteUpgradeAvailable = false;
        private Pawn raidLeaderCache;
        private int lastTick = 0;

        public GameComponent_WAW(Game game)
        {
            CleanupOldInstance();
            
            GameComponent_WAW.Instance = this;
            playerWarbandPreset = new PlayerWarbandArrangement();
            onRaid = new UnityEvent();
            onRaided = new UnityEvent();
            onLeaderAbilityUsed = new UnityEvent();
            playerBankAccount = new WAWBankAccount();
        }


        public bool IsEliteUpgradeAvailable => _isEliteUpgradeAvailable;

        public List<ThingDef> MortarShells => _mortarShells;
        public List<ThingDef> Minifiables => _minifiables;
        public List<ThingDef> RawResources => _rawResources;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(ref this.factionsAndWarDurabilities,
                "factionsAndWarDurabilities", LookMode.Reference, LookMode.Value, ref factions, ref durabilitities);
            Scribe_Values.Look(ref lastTick, "lastTick", GenTicks.TicksGame);
            Scribe_Values.Look(ref everAssignedWarbandLeader, "everAssignedWarbandLeader");
            Scribe_Values.Look(ref everUsedQuickRaid, "everUsedQuickRaid");
            Scribe_Values.Look(ref everInformedAboutTownBuilding, "everInformedAboutTownBuilding");
            Scribe_Deep.Look(ref playerBankAccount, "playerBank");
            playerWarbandPreset.ExposeData();
            if (playerBankAccount == null)
            {
                playerBankAccount = new WAWBankAccount(); 
            }
        }


        public override void LoadedGame()
        {
            base.LoadedGame();
            LoadAllFactions();
            RefreshPlayerWarbands();
            TryToSetRelation();
            LoadSpecialThingdefs();
        }


        public override void StartedNewGame()
        {
            base.StartedNewGame();
            GiveModLetter();
            LoadAllFactions();
            RefreshPlayerWarbands();
            TryToSetRelation();
            LoadSpecialThingdefs();
        }

        void LoadSpecialThingdefs()
        {
            _mortarShells = DefDatabase<ThingDef>.AllDefs.Where(x => x.projectileWhenLoaded != null).ToList();
            _minifiables = DefDatabase<ThingDef>.AllDefs.Where(x => x.Minifiable && x.building !=null && x.building.IsTurret).ToList();
            _rawResources = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsStuff).ToList();
        }

        public bool CanPlayerUseEliteUpgrade()
        {
            return true;
        }

        public void SetEliteUpgradeAvailable(bool value)
        {
            this._isEliteUpgradeAvailable = value;
        }   

        public void AppendFactionInfoToTable(Faction faction)
        {
            factionsAndWarDurabilities.Add(faction, 100);
        }

        public void DecreaseDurability(Faction f, int value)
        {
            if (f.Hidden || f.IsPlayer)
            {
                return;
            }
            if (!factionsAndWarDurabilities.ContainsKey(f))
            {
                AppendFactionInfoToTable(f);
            }
            factionsAndWarDurabilities[f] = factionsAndWarDurabilities[f] - value;
            if (factionsAndWarDurabilities[f] <= 0)
            {
                WarfareUtil.TryToDefeatFaction(f);
            }
        }


        public bool EverUsedQuickRaid()
        {
            return this.everUsedQuickRaid;
        }

        public bool EverAssignedLeader()
        {
            return this.everAssignedWarbandLeader;
        }

        public void SetAlreadyUseQuickRaid()
        {
            this.everUsedQuickRaid = true;
        }

        public void SetAlreadyAssignedLeader()
        {
            this.everAssignedWarbandLeader = true;  
        }

        public void AddDurability(Faction f, int value)
        {
            if (f.Hidden)
            {
                return;
            }
            if (!factionsAndWarDurabilities.ContainsKey(f))
            {
                AppendFactionInfoToTable(f);
            }
            factionsAndWarDurabilities[f] = Math.Min(factionsAndWarDurabilities[f] + value, 100);
        }

        public int GetDurability(Faction faction)
        {
            if (!factionsAndWarDurabilities.ContainsKey(faction))
            {
                return 0;
            }
            return factionsAndWarDurabilities[faction];
        }

        public void SetDurability(Faction f, int value)
        {
            if (!factionsAndWarDurabilities.ContainsKey(f))
            {
                AppendFactionInfoToTable(f);
            }
            factionsAndWarDurabilities[f] = value;
            if (value <= 0)
            {
                WarfareUtil.TryToDefeatFaction(f);
            }
        }


        public void DecreaseDurabilityBy(Faction f, int amount)
        {
            DecreaseDurability(f, amount);
            Log.Message($"Succeed! Current durability: {GetDurability(f)}");
        }

        public void LoadAllFactions()
        {
            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                if (!factionsAndWarDurabilities.ContainsKey(f) && !f.IsPlayer && !f.Hidden)
                {
                    AppendFactionInfoToTable(f);
                }
            }
        }
        
        Faction GetPlayerWarband()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband);
            if (f == null)
            {
                FactionGeneratorParms factionGeneratorParms = new FactionGeneratorParms(WAWDefof.PlayerWarband, default(IdeoGenerationParms), new bool?(true));
                f = FactionGenerator.NewGeneratedFaction(factionGeneratorParms);
                Find.FactionManager.Add(f);
            }
            return f;
        }

        void TryToSetRelation()
        {
            var playerWarband = GetPlayerWarband();
            if(playerWarband == null)
            {
                return;
            }
            foreach(var faction in Find.FactionManager.AllFactions)
            {
                if(faction == playerWarband)
                {
                    return;
                }
                FactionRelation r = new FactionRelation
                {
                    other = playerWarband,
                    kind = FactionRelationKind.Neutral
                };
                faction?.SetRelation(r);
            }
        }
        

 
        void RefreshPlayerWarbands()
        {
            WarbandUtil.RefreshAllPlayerWarbands();

        }

        void GiveModLetter()
        {
            Letter modLoadded = LetterMaker.MakeLetter("WAW.Welcome".Translate(), "WAW.Welcome.Desc".Translate(), LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(modLoadded);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastTick > WAWSettings.eventFrequency * 60000)
            {
                lastTick = Find.TickManager.TicksGame;
                SelfTick();
            }
        }

        void ReturnInterest()
        {
            if(!playerBankAccount.CanSpend())
            {
                return;
            }
            var interest = playerBankAccount.Interest;
            playerBankAccount.ReturnInterestPerSeason();
            Letter l = LetterMaker.MakeLetter("WAW.ReturnInterest".Translate(), "WAW.ReturnInterest.Desc".Translate(interest), LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(l);
        }

        void SelfTick()
        {
            ReturnInterest();
        }

        public void OnRaid(Pawn raidLeader = null)
        {
            SetRaidLeaderCache(raidLeader);
            this.onRaid.Invoke();
        }

        public void OnRaided(Pawn raidLeader = null)
        {
            SetRaidLeaderCache(raidLeader);
            this.onRaided.Invoke();
        }

        public void OnLeaderAbilityUsed(Pawn raidLeader = null)
        {
            SetRaidLeaderCache(raidLeader);
            this.onLeaderAbilityUsed.Invoke();
        }


        public void SetRaidLeaderCache(Pawn raidLeaderCache)
        {
            this.raidLeaderCache = raidLeaderCache;
        }
        
        public Pawn GetRaidLeaderCache()
        {
            return this.raidLeaderCache;
        }

        public void InformPlayerToBuildATown()
        {
            if (everInformedAboutTownBuilding)
            {
                return;
            }
            this.everInformedAboutTownBuilding = true;
            Letter l = LetterMaker.MakeLetter("WAW.BuildTown".Translate(), "WAW.BuildTown.Desc".Translate
                (
                WAWSettings.townConstructionSkillRequirement.ToString(),
                WAWSettings.townConstructionCost.ToString().Colorize(Color.yellow),
                WAWSettings.townConstructionDuration.ToString().Colorize(Color.cyan)
                ), LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(l);
        }
        private void CleanupOldInstance()
        {
            if (Instance != null && Instance != this)
            {
                // 清理旧实例的 UnityEvent
                Instance.onRaid?.RemoveAllListeners();
                Instance.onRaided?.RemoveAllListeners();
                Instance.onLeaderAbilityUsed?.RemoveAllListeners();
            }
        }

        public void Cleanup()
        {
            onRaid?.RemoveAllListeners();
            onRaided?.RemoveAllListeners();
            onLeaderAbilityUsed?.RemoveAllListeners();
            
            if (Instance == this)
            {
                Instance = null;
                playerWarbandPreset = null;
                playerBankAccount = null;
            }
        }
    }
}
