using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using static RimWorld.ColonistBar;
using WarfareAndWarbands.Warband.WarbandComponents.WarbandUpdates;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandAttackManager
    {

        public PlayerWarbandAttackManager(Warband warband)
        {
            this.warband = warband;
            droppodUpgrade = new DroppodUpgrade(warband);
        }

        public void OrderPlayerWarbandToAttack()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(warband), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(OrderPlayerWarbandToAttack), false,
                onUpdate: delegate
                {
                    GenDraw.DrawWorldRadiusRing(warband.Tile, playerAttackRange);
                });
        }

        public bool OrderPlayerWarbandToAttack(GlobalTargetInfo info)
        {
            if (!warband.bandMembers.Any(x => x.Value > 0))
            {
                Messages.Message("WAW.emptyBand".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (!warband.playerWarbandCoolDown.CanFireRaid())
            {
                Messages.Message("WAW.WaitForCooldown".Translate(warband.playerWarbandCoolDown.GetRemainingDays().ToString("0.0")), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (info.WorldObject == null ||
                info.WorldObject as MapParent == null ||
                WarbandUtil.IsWorldObjectNonHostile(info.WorldObject))
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (Find.WorldGrid.ApproxDistanceInTiles(info.Tile, warband.Tile) > playerAttackRange)
            {
                Messages.Message("WAW.FarObject".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            int cost = (int)PlayerWarbandArrangement.GetCost(warband.bandMembers);
            if (!WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            var enemy = (MapParent)info.WorldObject;
            targetMapP = enemy;
            WarbandUI.GetPlayerWarbandAttackOptions(this);
            return true;
        }


        public void AttackLand()
        {
            if (targetMapP != null)
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    warband.playerWarbandCoolDown.SetLastRaidTick();
                    WarbandUtil.OrderPlayerWarbandToAttack(targetMapP, warband);
                }, "GeneratingMapForNewEncounter", false, null, true);

        }

        public void AttackDropPod()
        {
            if (targetMapP != null && targetMapP.Map != null)
            {
                warband.playerWarbandCoolDown.SetLastRaidTick();
                droppodUpgrade.LaunchWarbandInMap(targetMapP.Map);
            }
        }

        private readonly Warband warband;
        private static readonly int playerAttackRange = 10;
        public MapParent targetMapP;
        public DroppodUpgrade droppodUpgrade;
    }
}
