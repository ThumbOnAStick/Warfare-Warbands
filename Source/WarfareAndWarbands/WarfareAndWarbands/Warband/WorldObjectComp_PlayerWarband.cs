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
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.parent.Faction.IsPlayer)
            {
                yield return WarbandUI.MoveWarbandCommand((Warband)this.parent);
                yield return WarbandUI.OrderWarbandToAttackCommand((Warband)this.parent);
                yield return WarbandUI.DismissWarband((Warband)this.parent);

            }

        }

        public override void CompTick()
        {
            base.CompTick();
        }



    }
}
