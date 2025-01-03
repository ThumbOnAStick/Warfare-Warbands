﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandLootManager: IExposable
    {
        private List<Thing> storage;
        private float lootValueMultiplier;

        public PlayerWarbandLootManager()
        {
            this.storage = new List<Thing>();
            lootValueMultiplier = 0.3f;
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

        public void WidthdrawLoot()
        {
            if (storage.Count < 1)
            {
                Message m = new Message("WAW.EmptyStorage".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }

            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(storage, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            storage.Clear();
            LaunchItemsToHome(activeDropPodInfo);
        }



        public void WithdrawLootInSilver()
        {

            if (storage.Count < 1)
            {
                Message m = new Message("WAW.EmptyWarband".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            List<Thing> silvers = GetLootValueInSilver();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(silvers, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            storage?.Clear();
            Messages.Message("WAW.LootValue".Translate(this.lootValueMultiplier * 100), MessageTypeDefOf.NeutralEvent);
            LaunchItemsToHome(activeDropPodInfo);

        }

        public List<Thing> GetLootValueInSilver()
        {
            float value = GetLootValue() * this.lootValueMultiplier;
            int stackCount = ThingDefOf.Silver.stackLimit;
            List<Thing> silvers = new List<Thing>();
            while (value > 1)
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = Math.Min(stackCount, (int)value);
                silvers.Add(silver);
                value -= stackCount;
            }

            return silvers;
        }

        public float GetLootValue()
        {
            float value = 0;

            foreach (var thing in storage)
            {
                if (!thing.DestroyedOrNull())
                    value += thing.MarketValue * thing.stackCount;
            }
            return value;
        }

        public int GetLootCount()
        {
            return this.storage.Count;
        }
        void LaunchItemsToHome(ActiveDropPodInfo activeDropPodInfo)
        {
            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            var cell = DropCellFinder.RandomDropSpot(playerMap);
            CameraJumper.TryJump(cell, playerMap);
            DropPodUtility.MakeDropPodAt(cell, playerMap, activeDropPodInfo);
        }

        public float GetLootMultiplier()
        {
            return this.lootValueMultiplier;  
        }

        public void SetLootMultiplier(float value)
        {
            this.lootValueMultiplier = value;
        }

        public void ResetLootMultiplier()
        {
            this.lootValueMultiplier = .3f;
        }


        public void ExposeData()
        {
            Scribe_Collections.Look(ref storage, "storage", LookMode.Reference);
            Scribe_Values.Look(ref lootValueMultiplier, "lootValueMultiplier", 0.3f);
            if(lootValueMultiplier < 0.3f)
            {
                lootValueMultiplier = .3f;
            }
        }

    }
}
