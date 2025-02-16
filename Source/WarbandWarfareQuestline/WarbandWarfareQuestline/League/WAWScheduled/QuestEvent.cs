using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarbandWarfareQuestline.League.WAWScheduled
{
    public class QuestEvent : WAWScheduledEvent
    {
        public override int BaseDurationDate => 30;

        public override void MakeAction()
        {
            base.MakeAction();
            Questline.Quests.GiveVillageQuest();
        }
    }
}
