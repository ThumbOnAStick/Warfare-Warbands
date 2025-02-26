using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline
{
    public class GameComponent_Questline : GameComponent
    {
        public static GameComponent_Questline Instance;

        public GameComponent_Questline(Game game)
        {
            Instance = this;
        }
    
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("WAW: questline module active");
        }
    }
}
