using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_Vehicle;
using WarfareAndWarbands.Warband.Compatibility_VPE;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;

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
            //Test
           
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastTick, "lastTick");
        }

     
        private int lastTick = 0;
    }
}
