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
        private List<Thing> _storage;
        private List<Thing> _toBesold;
        private float lootValueMultiplier;

        public PlayerWarbandLootManager()
        {
            this._storage = new List<Thing>();
            this._toBesold = new List<Thing>();
            lootValueMultiplier = 0.3f;
        }

        public List<Thing> Storage => this._storage;

        public void StoreAll(IEnumerable<Thing> things)
        {
            foreach (Thing thing in things)
            {
                if (thing.Spawned)
                    thing.DeSpawn();
                _storage.Add(thing);
            }
        }

        public void StoreThing(ref Thing thing)
        {
            if (thing.Spawned)
                thing.DeSpawn();
            _storage.Add(thing);
        }

        private void RemoveSoldItems()
        {
            for (int i = 0; i < _toBesold.Count; i++)
            {
                var thing = _toBesold[i];
                for (int j = _storage.Count - 1; j >= 0; j--)
                {
                    if (_storage[j].ThingID == thing.ThingID)
                    {
                        _storage.RemoveAt(j);
                    }
                }
            }
            _toBesold.Clear();
        }

        public void WithdrawLoot()
        {
            if (_toBesold.Count < 1)
            {
                Message m = new Message("WAW.EmptyStorage".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }
            LaunchItemsToHome(ref _toBesold);
            RemoveSoldItems();
        }

        public void SetToBeSold(List<Thing> things)
        {
            this._toBesold = things;
        }

        public void WithdrawLootInSilver()
        {

            if (_toBesold.Count < 1)
            {
                Message m = new Message("WAW.EmptyWarband".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }
            List<Thing> silvers = GetLootValueInSilver();
            Messages.Message("WAW.LootValue".Translate(this.lootValueMultiplier * 100), MessageTypeDefOf.NeutralEvent);
            LaunchItemsToHome(ref silvers);
            RemoveSoldItems();
        }

        public void WithdrawLootToBank()
        {

            if (_toBesold.Count < 1)
            {
                Message m = new Message("WAW.EmptyWarband".Translate(), MessageTypeDefOf.RejectInput);
                Messages.Message(m);
                return;
            }
            int amount = (int)(GetLootValue() * this.lootValueMultiplier);
            Messages.Message("WAW.LootValueBankAccount".Translate(this.lootValueMultiplier * 100, amount), MessageTypeDefOf.NeutralEvent);
            GameComponent_WAW.playerBankAccount.Deposit(amount);
            RemoveSoldItems();
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

            foreach (var thing in _storage)
            {
                if (!thing.DestroyedOrNull())
                    value += thing.MarketValue * thing.stackCount;
            }
            return value;
        }

        public int GetLootCount()
        {
            return this._storage.Count;
        }
        void LaunchItemsToHome(ref List<Thing> items)
        {
            Log.Message(items.Count);
            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            if (DropCellFinder.TryFindDropSpotNear(playerMap.Center, playerMap, out IntVec3 cell, false, false))
            {
                CameraJumper.TryJump(cell, playerMap);
                //DropPodUtility.MakeDropPodAt(cell, playerMap, activeDropPodInfo);
                DropPodUtility.DropThingsNear(cell, playerMap, items, canRoofPunch: false, forbid: false);
            }
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
            Scribe_Collections.Look(ref _storage, "storage", LookMode.Deep);
            Scribe_Values.Look(ref lootValueMultiplier, "lootValueMultiplier", 0.3f);
            if(lootValueMultiplier < 0.3f)
            {
                lootValueMultiplier = .3f;
            }
        }

    }
}
