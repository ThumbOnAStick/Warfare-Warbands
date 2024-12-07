using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    internal class PlayerWarbandLeader : IExposable
    {
        public Pawn Leader
        {
            get
            {
                return leader;
            }
            set
            {
                leader = value; 
            }
        }

        Pawn leader;

        public bool IsValid()
        {
            return leader == null || leader.Dead || leader.Spawned;
        }

        public void ExposeData()
        {
            leader?.ExposeData();
        }

    }
}
