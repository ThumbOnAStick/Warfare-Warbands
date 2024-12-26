using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WAWLeadership.UI;

namespace WAWLeadership
{
    public class GameComponent_Leadership:GameComponent
    {
        public GameComponent_Leadership(Game game)
        {

        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            //Find.WindowStack.Add(new Window_UpgradeWarband());
        }
    }
}
