using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarbandWarfareQuestline.Skirmish;

namespace WarbandWarfareQuestline.League.WAWScheduled
{
    public class SkirmishEvent : WAWScheduledEvent
    {
        public SkirmishEvent()
        {

        }

        public override int BaseDurationDate => 10;

        public override void MakeAction()
        {
            base.MakeAction();
            GameComponent_Skrimish.Instance.CreateRandomSkirmsish();
        }
    }
}
