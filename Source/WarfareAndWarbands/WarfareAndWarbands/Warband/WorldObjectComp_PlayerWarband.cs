﻿using RimWorld;
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
                yield return WarbandUI.MoveWarbandCommand((Warband)this.parent);
                yield return WarbandUI.OrderWarbandToAttackCommand((Warband)this.parent);
                yield return WarbandUI.DismissWarband((Warband)this.parent);
                yield return WarbandUI.WithdrawWarbandItems((Warband)this.parent);
                yield return WarbandUI.ConfigureWarband((Warband)this.parent);
                yield return WarbandUI.RenameWarband((Warband)this.parent);

                if (DebugSettings.godMode)
                {
                    yield return WarbandUI.ResetRaidCooldown((Warband)this.parent);

                }
            }

        }


    }
}
