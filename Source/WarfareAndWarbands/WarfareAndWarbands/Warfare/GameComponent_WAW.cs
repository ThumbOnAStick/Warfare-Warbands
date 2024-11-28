using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands
{
    public class GameComponent_WAW : GameComponent
    {
        List<Faction> factions = new List<Faction>();
        List<int> durabilitities = new List<int>();
        Dictionary<Faction, int> factionsAndWarDurabilities = new Dictionary<Faction, int>();
        public static PlayerWarbandArrangement playerWarband;
        public static GameComponent_WAW Instance;

        public GameComponent_WAW(Game game)
        {
            GameComponent_WAW.Instance = this;
            playerWarband = new PlayerWarbandArrangement();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(ref this.factionsAndWarDurabilities,
                "factionsAndWarDurabilities", LookMode.Reference, LookMode.Value, ref factions, ref durabilitities);
            Scribe_Values.Look(ref lastTick, "lastTick");
            playerWarband.ExposeData();
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

        public override void LoadedGame()
        {
            base.LoadedGame();
            LoadAllFactions();

        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            LoadAllFactions();
            GiveModLetter();
        }

        void GiveModLetter()
        {
            Letter modLoadded = LetterMaker.MakeLetter("WAW.Welcome".Translate(), "WAW.Welcome.Desc".Translate(),LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(modLoadded);
        }



        private int lastTick = 0;

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastTick > WAWSettings.eventFrequency * 60000)
            {
                lastTick = Find.TickManager.TicksGame;
                SelfTick();
            }
        }

        void SelfTick()
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

    }
}
