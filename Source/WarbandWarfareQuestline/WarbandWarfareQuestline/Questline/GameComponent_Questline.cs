using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline
{
    public class GameComponent_Questline : GameComponent
    {
        public GameComponent_Questline(Game game) 
        {

        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Log.Message("WAW: questline module active");
        }
    }
}
