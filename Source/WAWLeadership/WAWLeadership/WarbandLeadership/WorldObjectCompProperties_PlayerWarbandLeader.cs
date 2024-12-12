using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAWLeadership.WarbandLeadership;

namespace WAWLeadership
{
    public class WorldObjectCompProperties_PlayerWarbandLeader : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_PlayerWarbandLeader()
        {
            this.compClass = typeof(WorldObjectComp_PlayerWarbandLeader);

        }
    }
}
