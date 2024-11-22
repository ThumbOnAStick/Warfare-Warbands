using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband
{
    public class GameComponent_Warband : GameComponent
    {
        public static GameComponent_Warband Instance;

        public GameComponent_Warband(Game game)
        {
            GameComponent_Warband.Instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastTick, "lastTick");
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastTick > WAWSettings.eventFrequency * 60000)
            {
                lastTick = Find.TickManager.TicksGame;
                SelfTick();
            }
        }

        void SelfTick()
        {
            var validFactions = WarfareUtil.GetValidWarFactions();
            int len = validFactions.Count;
            IntRange r = new IntRange(0, len-1);
            int rndIdx = r.RandomInRange;
            var pickedFaction = validFactions.ElementAt(rndIdx);
            var worldObject = WarbandUtil.RandomHostileSettlement(pickedFaction);
            if (worldObject == null)
            {
                return;
            }
            WarbandUtil.SpawnWarbandTargetingBase(pickedFaction, worldObject);
        }
        private int lastTick = 0;
    }
}
