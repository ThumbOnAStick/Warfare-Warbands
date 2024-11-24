using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandLootManager: IExposable
    {
        public PlayerWarbandLootManager()
        {
            this.storage = new List<Thing>();
        }

        public void StoreAll(IEnumerable<Thing> things)
        {
            foreach (Thing thing in things)
            {
                if (thing.Spawned)
                    thing.DeSpawn();
                storage.Add(thing);
            }
        }

        public void StoreThing(ref Thing thing)
        {
            if (thing.Spawned)
                thing.DeSpawn();
            storage.Add(thing);
        }

        public void DumpLoot()
        {
            if (storage.Count < 1)
            {
                Message m = new Message("WAW.EmptyWarband".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }

            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(storage, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            storage.Clear();
            LaunchItemsToHome(activeDropPodInfo);
        }

      

        public void DumpLootInSilver()
        {
            if (storage.Count < 1)
            {
                Message m = new Message("WAW.EmptyWarband".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }

            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            List<Thing> silvers = new List<Thing>();
            float value = 0;
            int stackCount = ThingDefOf.Silver.stackLimit;
            foreach (var thing in storage)
            {
                value += thing.MarketValue * thing.stackCount;
            }
            while (value > 1)
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = Math.Min(stackCount, (int)value);
                silvers.Add(silver);
                value -= stackCount;
            }
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(silvers, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            storage.Clear();
            LaunchItemsToHome(activeDropPodInfo);

        }

        void LaunchItemsToHome(ActiveDropPodInfo activeDropPodInfo)
        {
            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            var cell = CellFinder.StandableCellNear(playerMap.Center, playerMap, 10);
            CameraJumper.TryJump(cell, playerMap);
            DropPodUtility.MakeDropPodAt(cell, playerMap, activeDropPodInfo);
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref storage, "storage", LookMode.Reference);
        }

        private List<Thing> storage;
    }
}
