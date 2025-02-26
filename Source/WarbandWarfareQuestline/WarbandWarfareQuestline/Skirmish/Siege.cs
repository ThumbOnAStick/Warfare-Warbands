using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline.Skirmish
{
    public class Siege : Skirmish
    {
        private WorldObject _target;

        public override int Bonus => 3000; 

        public Siege(List<Warband> warbands, int creationTick, WorldObject target) : base(warbands, creationTick)
        {
            _target = target;
        }

        public override bool ShouldGiveBonus()
        {
            return _target == null || _target.Destroyed;
        }

        public void TryToDestroyBase()
        {
            var range = new FloatRange(0, 1);
            if(range.RandomInRange >= .5 && _target == null &&!_target.Destroyed && this._faction != null)
            {
                var settlement = WarbandUtil.AddNewHome(_target.Tile, this._faction, _target.def);
                _target.Destroy();
                Messages.Message("WAW.EnemyBaseDestroyed".Translate(), lookTargets: settlement, MessageTypeDefOf.NeutralEvent);
            }
        }
        public override void NotifyPlayer()
        {
            Messages.Message("WAW.SiegeEvent".Translate(), lookTargets: this._target, MessageTypeDefOf.NeutralEvent);
        }

        public override bool ShouldDestroy()
        {
            return (base.ShouldDestroy()) && !HasMap();
        }


        public override void PostDestroy()
        {
            base.PostDestroy();
            TryToDestroyBase();

        }


    }
}
