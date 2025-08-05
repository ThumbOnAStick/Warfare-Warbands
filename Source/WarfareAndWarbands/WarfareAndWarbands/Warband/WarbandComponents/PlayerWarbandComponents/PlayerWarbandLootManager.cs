using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static System.Collections.Specialized.BitVector32;
using Verse.Noise;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandLootManager: IExposable, IThingHolder
    {
        private ThingOwner<Thing> _storage;
        private List<Thing> _toBesold;
        private float lootValueMultiplier;

        public PlayerWarbandLootManager()
        {
            this._storage = new ThingOwner<Thing>();
            this._toBesold = new List<Thing>();
            lootValueMultiplier = 0.3f;
        }

        public ThingOwner<Thing> Storage => this._storage;

        public IThingHolder ParentHolder => throw new NotImplementedException();

        public void StoreAll(IEnumerable<Thing> things)
        {
            foreach (Thing thing in things)
            {
                if (thing.Spawned)
                    thing.DeSpawn();
                _storage.TryAddOrTransfer(thing);
            }
        }

        public void StoreThing(ref Thing thing)
        {
            if (thing.Spawned)
                thing.DeSpawn();
            _storage.TryAddOrTransfer(thing);
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



        public void SetToBeSold(List<Thing> things)
        {
            this._toBesold = things;
        }

        /// <summary>
        /// Sends stacks of silvers to player map
        /// </summary>
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

        /// <summary>
        /// Withdraws the loot to the bank account of the player.
        /// </summary>
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

        /// <summary>
        /// Sends items to the player map.
        /// </summary>
        /// <param name="items"></param>
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
        void LaunchItemsToHome(ref List<Thing> itemList)
        {
            Log.Message($"Sent {itemList.Count} items to home");

            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            if (DropCellFinder.TryFindDropSpotNear(playerMap.Center, playerMap, out IntVec3 cell, false, false))
            {
                CameraJumper.TryJump(cell, playerMap);
                foreach (Thing thing in itemList)
                {
                    ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
                    activeTransporterInfo.innerContainer.TryAddOrTransfer(thing, true);
                    DropPodUtility.MakeDropPodAt(cell, playerMap, activeTransporterInfo);
                }
            }
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

            foreach (var thing in _toBesold)
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
            Scribe_Deep.Look(ref _storage, "storage");
            Scribe_Values.Look(ref lootValueMultiplier, "lootValueMultiplier", 0.3f);
            if(lootValueMultiplier < 0.3f)
            {
                lootValueMultiplier = .3f;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this._storage;
        }
    }
}
