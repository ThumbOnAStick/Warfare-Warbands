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


        public MapParent targetMapP;
        public DroppodUpgrade droppodUpgrade;
        public PlayerWarbandColorOverride colorOverride;
        public PlayerWarbandInjuries injuriesManager;
        public PlayerWarbandLootManager lootManager;
        public PlayerWarbandCooldownManager cooldownManager;
        public PlayerWarbandLeader leader;

        private readonly Warband warband;

        private static readonly int playerAttackRange = 10;
        public PlayerWarbandManager(Warband warband)
        {
            this.warband = warband;
            this.droppodUpgrade = new DroppodUpgrade(warband);
            this.cooldownManager = new PlayerWarbandCooldownManager();
            this.lootManager = new PlayerWarbandLootManager();
            this.colorOverride = new PlayerWarbandColorOverride();
            this.injuriesManager = new PlayerWarbandInjuries();
            leader = new PlayerWarbandLeader();
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
            Dictionary<string, int> activeMembers;
            if (this.injuriesManager != null)
                activeMembers = this.injuriesManager.GetActiveMembers(warband.bandMembers);
            else activeMembers = warband.bandMembers;
            bool emptyWarband = !activeMembers.Any((KeyValuePair<string, int> x) => x.Value > 0);
            bool result;
            if (emptyWarband)
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
                    bool flag3 = info.WorldObject == null || 
                        !(info.WorldObject is MapParent) || 
                        WarbandUtil.IsWorldObjectNonHostile(info.WorldObject)
                        ||Find.World.Impassable(info.Tile);
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
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
                            MapParent mapParent = (MapParent)info.WorldObject;
                            this.targetMapP = mapParent;
                            WarbandUI.GetPlayerWarbandAttackOptions(this);
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        bool CantAffordToAttack()
        {
            int cost = (int)PlayerWarbandArrangement.GetCostOriginal(this.warband.bandMembers);
            bool cantAfford = !WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost);
            if (cantAfford)
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent, true);
                return false;
            }
            return true;
        }

        public void AttackLand()
        {
            bool flag = this.targetMapP != null;
            if (flag)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    if (!CantAffordToAttack())
                        return;
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
                if (!CantAffordToAttack())
                    return;
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
            this.lootManager?.ExposeData();
            this.colorOverride?.ExposeData();
            this.injuriesManager?.ExposeData();
            this.leader?.ExposeData();
        }

        public string GetInspectString()
        {
            var injuries = this.injuriesManager.GetInjuries();
            var outString = "WAW.ActiveMembers".Translate();
            foreach (var member in this.injuriesManager.GetActiveMembers(warband.bandMembers, injuries))
            {
                if (member.Value > 0)
                    outString += "\n" + WarbandUtil.GetSoldierLabel(member.Key) + "(" + member.Value + ")";
            }

            if (injuries.Count > 0)
            {
                outString += "\n" + "WAW.Injuries".Translate();
                outString += "WAW.RecoverIn".Translate(injuriesManager.GetRecoveryDays().ToString("0.0"));
                foreach (var member in injuries)
                {
                    if (member.Value > 0)
                        outString += "\n" + WarbandUtil.GetSoldierLabel(member.Key) + "(" + member.Value + ")";
                }
            }

            if (!this.cooldownManager.CanFireRaid())
            {
                string cooldownString = "WAW.AvialableIn".Translate(GetRemainingDays().ToString("0.0"));
                outString += "\n" + cooldownString;
            }

            if (this.warband.HasLeader())
            {
                string leaderString = "WAW.Leader".Translate(this.leader.Leader.NameFullColored);
                outString += "\n" + leaderString;
            }


            return outString;
        }

        internal bool CanFireRaid()
        {
            return this.cooldownManager.CanFireRaid();
        }

        internal float GetRemainingDays()
        {
            return this.cooldownManager.GetRemainingDays();
        }

        internal void Tick()
        {
            if(warband.Faction != Faction.OfPlayer)
            {
                return;
            }
            injuriesManager?.Tick();
            if (ShouldPlayerWarbandBeRemoved())
            {
                warband.Destroy();
            }
        }

        bool ShouldPlayerWarbandBeRemoved()
        {
            return warband.GetMemberCount() < 1 && !warband.HasLeader();
        }


    }
}
