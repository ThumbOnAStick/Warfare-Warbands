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
using UnityEngine;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandManager : IExposable
    {


        public MapParent targetMapP;
        public PlayerWarbandDropRaid dropPodRaider;
        public PlayerWarbandColorOverride colorOverride;
        public PlayerWarbandInjuries injuriesManager;
        public PlayerWarbandLootManager lootManager;
        public PlayerWarbandCooldownManager cooldownManager;
        public PlayerWarbandLeader leader;
        public PlayerWarbandSkillBonus skillBonus;
        public PlayerWarbandUpgradeHolder upgradeHolder;
        public float RespawnChance => respawnChance;
        public float NewRecruitCostMultiplier => newRecruitCostMultiplier;
        private float respawnChance;
        private float newRecruitCostMultiplier;

        private readonly Warband warband;
        public static readonly int playerAttackRange = 10;
        public static readonly int mortarRaidAttackRange = 5;
        public static readonly int mortarFireRange = 5;

        public PlayerWarbandManager(Warband warband)
        {
            this.warband = warband;
            this.dropPodRaider = new PlayerWarbandDropRaid(warband);
            this.cooldownManager = new PlayerWarbandCooldownManager();
            this.lootManager = new PlayerWarbandLootManager();
            this.colorOverride = new PlayerWarbandColorOverride();
            this.injuriesManager = new PlayerWarbandInjuries();
            this.leader = new PlayerWarbandLeader();
            this.skillBonus = new PlayerWarbandSkillBonus();
            upgradeHolder = new PlayerWarbandUpgradeHolder();
            leader.onLeaderChanged.AddListener(skillBonus.ResetSkillBonus);
            leader.onLeaderChanged.AddListener(lootManager.ResetLootMultiplier);
            leader.onLeaderChanged.AddListener(ResetNewRecruitCostMultiplier);
            leader.onLeaderChanged.AddListener(ResetRespawnChance);
            newRecruitCostMultiplier = 1;
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
                Messages.Message("WAW.NoMembers".Translate(), MessageTypeDefOf.RejectInput, true);
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


        public void AttackLand()
        {
            bool flag = this.targetMapP != null;
            if (flag)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    if (!upgradeHolder.CanAttackCurrent)
                        return;
                    if (upgradeHolder.CostsSilver && !WarbandUtil.CantAffordToAttack(warband))
                        return;
                    GameComponent_WAW.Instance.OnRaid(this.leader.Leader);
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
                this.dropPodRaider.LaunchWarbandInMap(this.targetMapP.Map);
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

        public void WithdrawLootToBank()
        {
            this.lootManager.WithdrawLootToBank();

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
                if (leader.IsLeaderAvailable())
                {
                    outString += $"\n({injuriesManager.LeaderFactorString()})";
                }
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
            if (upgradeHolder.HasUpgrade)
                cooldownManager.SetMinCooldownDays(upgradeHolder.SelectedUpgrade.MaintainDays);
            return this.cooldownManager.CanFireRaid();
        }

        internal float GetRemainingDays()
        {
            if (upgradeHolder.HasUpgrade)
                cooldownManager.SetMinCooldownDays(upgradeHolder.SelectedUpgrade.MaintainDays);
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

        public void SetRespawnChance(float value)
        {
            respawnChance = value;  
        }

        public void SetNewRecruitCostMultiplier(float value)
        {
            newRecruitCostMultiplier = value;
        }

        void ResetRespawnChance()
        {
            SetRespawnChance(0);
        }

        void ResetNewRecruitCostMultiplier()
        {
            SetNewRecruitCostMultiplier(1);
        }

        public Texture2D TextureOverride()
        {
            return upgradeHolder.TextureOverride();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref respawnChance, "respawnChance", 0);
            Scribe_Values.Look(ref newRecruitCostMultiplier, "newRecruitCostMultiplier", 1);
            Scribe_Deep.Look(ref upgradeHolder, "upgradeHolder");
            Scribe_Deep.Look(ref lootManager, "lootManager");
            Scribe_Deep.Look(ref cooldownManager, "cooldownManager");
            if (lootManager == null)
            {
                lootManager = new PlayerWarbandLootManager();
            }
            if(upgradeHolder == null)
            {
                upgradeHolder = new PlayerWarbandUpgradeHolder();
            }
            if(cooldownManager == null)
            {
                cooldownManager = new PlayerWarbandCooldownManager();
            }
            this.colorOverride?.ExposeData();
            this.injuriesManager?.ExposeData();
            this.leader?.ExposeData();
            this.skillBonus?.ExposeData();
        }

    }
}
