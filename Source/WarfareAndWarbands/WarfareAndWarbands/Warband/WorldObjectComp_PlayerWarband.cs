using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    public class WorldObjectComp_PlayerWarband : WorldObjectComp
    {
        public Warband MyWarband => (Warband)this.parent;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.parent.Faction.IsPlayer)
            {
                if (MyWarband.playerWarbandManager.upgradeHolder.CanMove)
                    yield return WarbandUI.MoveWarbandCommand(MyWarband);
                if (MyWarband.playerWarbandManager.upgradeHolder.CanAttack)
                    yield return WarbandUI.OrderWarbandToAttackCommand(MyWarband);
                yield return WarbandUI.DismissWarband(MyWarband);
                yield return WarbandUI.WithdrawWarbandItems(MyWarband);
                yield return WarbandUI.ConfigureWarband(MyWarband);
                yield return WarbandUI.RenameWarband(MyWarband);
                if (MyWarband.playerWarbandManager.upgradeHolder.HasUpgrade)
                {
                    foreach(var gizmo in MyWarband.playerWarbandManager.upgradeHolder.SelectedUpgrade.GetGizmosExtra())
                    {
                        yield return gizmo;
                    }
                }
                if (DebugSettings.godMode)
                {
                    yield return WarbandUI.ResetRaidCooldown(MyWarband);
                }
            }

        }


    }
}
