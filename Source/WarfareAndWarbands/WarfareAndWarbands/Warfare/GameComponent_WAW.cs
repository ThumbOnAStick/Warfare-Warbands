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
        Dictionary<Faction, int> factionsAndWarDurabilities = new Dictionary<Faction, int>();
        public static PlayerWarbandArrangement playerWarband;
        public static WAWBankAccount playerBankAccount;
        public static GameComponent_WAW Instance;
        public UnityEvent onRaid;
        public UnityEvent onRaided;
        public UnityEvent onLeaderAbilityUsed;
        private bool everUsedQuickRaid = false;
        private bool everAssignedWarbandLeader = false;
        private Pawn raidLeaderCache;
        private int lastTick = 0;


        public GameComponent_WAW(Game game)
        {
            GameComponent_WAW.Instance = this;
            playerWarband = new PlayerWarbandArrangement();
            onRaid = new UnityEvent();
            onRaided = new UnityEvent();
            onLeaderAbilityUsed = new UnityEvent();
            playerBankAccount = new WAWBankAccount();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(ref this.factionsAndWarDurabilities,
                "factionsAndWarDurabilities", LookMode.Reference, LookMode.Value, ref factions, ref durabilitities);
            Scribe_Values.Look(ref lastTick, "lastTick", GenTicks.TicksGame);
            Scribe_Values.Look(ref everAssignedWarbandLeader, "everAssignedWarbandLeader");
            Scribe_Values.Look(ref everUsedQuickRaid, "everUsedQuickRaid");
            playerWarband.ExposeData();
            Scribe_Deep.Look(ref playerBankAccount, "playerBank");
            if(playerBankAccount == null)
            {
                playerBankAccount = new WAWBankAccount(); 
            }
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
        

        public override void LoadedGame()
        {
            base.LoadedGame();
            LoadAllFactions();
            RefreshPlayerWarbands();
            TryToSetRelation();
        }


        public override void StartedNewGame()
        {
            base.StartedNewGame();
            GiveModLetter();
            LoadAllFactions();
            RefreshPlayerWarbands();
            TryToSetRelation();
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

        void SpawnRandomWarband()
        {
            var validFactions = WarfareUtil.GetValidWarFactions();
            int len = validFactions.Count;
            IntRange r = new IntRange(0, len - 1);
            int rndIdx = r.RandomInRange;
            var pickedFaction = validFactions.ElementAt(rndIdx);
            var worldObject = WarfareUtil.RandomHostileSettlement(pickedFaction);
            if (worldObject == null)
            {
                return;
            }
            WarfareUtil.SpawnWarbandTargetingBase(pickedFaction, worldObject);
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
    }
}
