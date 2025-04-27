using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_InfrastructureDevelopment : PolicyWorker
    {
        private int _lastExecuteTick = -tickDuration;

        private const int tickDuration = 60000;

        void SetLastExecuteTick()
        {
            _lastExecuteTick = GenTicks.TicksGame;
        }

        public override void Tick()
        {
            base.Tick();
            if (GenTicks.TicksGame >= _lastExecuteTick + tickDuration)
            {
                SetLastExecuteTick();
                Log.Message("Infrastructure Development policy ticked");
                GameComponent_League.Instance.AffectCohesion(0.01f);
            }
        }
    }
}
