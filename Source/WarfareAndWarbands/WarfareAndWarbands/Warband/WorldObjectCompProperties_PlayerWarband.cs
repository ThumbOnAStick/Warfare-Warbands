using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarfareAndWarbands.Warband
{
    public class WorldObjectCompProperties_PlayerWarband:WorldObjectCompProperties
    {
        public WorldObjectCompProperties_PlayerWarband()
        {
            this.compClass = typeof(WorldObjectComp_PlayerWarband);
        }
    }
}
