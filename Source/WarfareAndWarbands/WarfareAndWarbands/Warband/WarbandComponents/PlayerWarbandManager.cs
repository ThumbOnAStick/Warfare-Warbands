using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.WarbandUpdates;
using Verse.Sound;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandManager : IExposable
    {
        public PlayerWarbandManager(Warband warband)
        {
            this.warband = warband;
            this.droppodUpgrade = new DroppodUpgrade(warband);
            this.cooldownManager = new PlayerWarbandCooldownManager();
            this.lootManager = new PlayerWarbandLootManager();
            this.colorOverride = new PlayerWarbandColorOverride();
        }

        public void OrderPlayerWarbandToAttack()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.warband), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.OrderPlayerWarbandToAttack), false, null, false, delegate
            {
                GenDraw.DrawWorldRadiusRing(this.warband.Tile, PlayerWarbandManager.playerAttackRange);
            }, null, null);
        }

        public bool OrderPlayerWarbandToAttack(GlobalTargetInfo info)
        {
            bool flag = !this.warband.bandMembers.Any((KeyValuePair<string, int> x) => x.Value > 0);
            bool result;
            if (flag)
            {
                Messages.Message("WAW.emptyBand".Translate(), MessageTypeDefOf.RejectInput, true);
                result = false;
            }
            else
            {
                bool flag2 = !this.cooldownManager.CanFireRaid();
                if (flag2)
                {
                    Messages.Message("WAW.WaitForCooldown".Translate(this.cooldownManager.GetRemainingDays().ToString("0.0")), MessageTypeDefOf.RejectInput, true);
                    result = false;
                }
                else
                {
                    bool flag3 = info.WorldObject == null || !(info.WorldObject is MapParent) || WarbandUtil.IsWorldObjectNonHostile(info.WorldObject);
                    if (flag3)
                    {
                        Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.RejectInput, true);
                        result = false;
                    }
                    else
                    {
                        bool flag4 = Find.WorldGrid.ApproxDistanceInTiles(info.Tile, this.warband.Tile) > (float)PlayerWarbandManager.playerAttackRange;
                        if (flag4)
                        {
                            Messages.Message("WAW.FarObject".Translate(), MessageTypeDefOf.RejectInput, true);
                            result = false;
                        }
                        else
                        {
                            int cost = (int)PlayerWarbandArrangement.GetCost(this.warband.bandMembers);
                            bool flag5 = !WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost);
                            if (flag5)
                            {
                                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent, true);
                                result = false;
                            }
                            else
                            {
                                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
                                MapParent mapParent = (MapParent)info.WorldObject;
                                this.targetMapP = mapParent;
                                WarbandUI.GetPlayerWarbandAttackOptions(this);
                                result = true;
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void AttackLand()
        {
            bool flag = this.targetMapP != null;
            if (flag)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    this.cooldownManager.SetLastRaidTick();
                    WarbandUtil.OrderPlayerWarbandToAttack(this.targetMapP, this.warband);
                }, "GeneratingMapForNewEncounter", false, null, true, null);
            }
        }

        public void AttackDropPod()
        {
            bool flag = this.targetMapP != null && this.targetMapP.Map != null;
            if (flag)
            {
                this.cooldownManager.SetLastRaidTick();
                this.droppodUpgrade.LaunchWarbandInMap(this.targetMapP.Map);
            }
        }

        public void WithdrawLoot()
        {
            this.lootManager.WidthdrawLoot();
        }

        public void WithdrawLootInSilver()
        {
            this.lootManager.WithdrawLootInSilver();
        }

        public void StoreAll(IEnumerable<Thing> things)
        {
            this.lootManager.StoreAll(things);
        }

        public void StoreThing(ref Thing thing)
        {
            this.lootManager.StoreThing(ref thing);
        }

        public void ResetRaidTick()
        {
            this.cooldownManager.ResetRaidTick();
        }

        public bool OrderPlayerWarbandToResettle(GlobalTargetInfo info, Warband warband)
        {
            return PlayerWarbandResettleManager.OrderPlayerWarbandToResettle(info, warband);
        }

        public void ExposeData()
        {
            this.lootManager.ExposeData();
            this.colorOverride.ExposeData();
        }

        internal bool CanFireRaid()
        {
            return this.cooldownManager.CanFireRaid();
        }

        internal float GetRemainingDays()
        {
            return this.cooldownManager.GetRemainingDays();
        }


        private readonly Warband warband;

        public MapParent targetMapP;

        public DroppodUpgrade droppodUpgrade;

        public PlayerWarbandColorOverride colorOverride;

        public PlayerWarbandLootManager lootManager;

        public PlayerWarbandCooldownManager cooldownManager;

        //public PlayerWarbandResettleManager resettleManager;

        private static readonly int playerAttackRange = 10;
    }
}
