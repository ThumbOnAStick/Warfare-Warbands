using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    public class GameComponent_WAW : GameComponent
    {
        List<Faction> factions = new List<Faction>();
        List<int> durabilitities = new List<int>();
        Dictionary<Faction, int> factionsAndWarDurabilities = new Dictionary<Faction, int>();

        public static GameComponent_WAW Instance;

        public GameComponent_WAW(Game game)
        {
            GameComponent_WAW.Instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(ref this.factionsAndWarDurabilities,
                "factionsAndWarDurabilities", LookMode.Reference, LookMode.Value, ref factions, ref durabilitities);
        }

        public void AppendFactionInfoToTable(Faction faction)
        {
            factionsAndWarDurabilities.Add(faction, 100);
        }

        public void DecreaseDurability(Faction f, int value)
        {
            if (!factionsAndWarDurabilities.ContainsKey(f))
            {
                AppendFactionInfoToTable(f);
            }
            factionsAndWarDurabilities[f] = factionsAndWarDurabilities[f] - value;
            if (factionsAndWarDurabilities[f] <= 0)
            {
                WarfareUtil.DefeatFaction(f);
            }
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
                WarfareUtil.DefeatFaction(f);
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
        }

        public override void GameComponentTick()
        {

        }
    }
}
