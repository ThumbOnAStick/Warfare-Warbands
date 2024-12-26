using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.Compatibility_Vehicle;

namespace WarfareAndWarbands.Warband
{
    public class GameComponent_Warband : GameComponent
    {
        public static GameComponent_Warband Instance;

        public GameComponent_Warband(Game game)
        {
            GameComponent_Warband.Instance = this;
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastTick, "lastTick");
        }

     
        private int lastTick = 0;
    }
}
