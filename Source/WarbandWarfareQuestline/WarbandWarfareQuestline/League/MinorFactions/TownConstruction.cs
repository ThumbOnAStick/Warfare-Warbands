using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.MinorFactions
{
    public class TownConstruction : WorldObject
    {
        private int _terminationTick;
        private static readonly float _durationDays = 5.0f;

        private WorldObject _product;
        private MinorFaction _fac;

        public float RemainingConstructionDays => (float)(this._terminationTick - GenTicks.TicksGame)/GenDate.TicksPerDay;

        void NotifyPlayer()
        {
            if (_product != null && _fac != null)
            {
                Letter l = LetterMaker.MakeLetter("WAW.ConstructionComplete".Translate(), "WAW.ConstructionComplete.Desc".Translate(_fac.FactionName), LetterDefOf.PositiveEvent, lookTargets: _product);
                Find.LetterStack.ReceiveLetter(l);
            }
        }

        bool ShouldComplete()
        {
            return GenTicks.TicksGame > _terminationTick;
        }

        void CreateProduct()
        {
            _fac = MinorFactionHelper.GenerateRandomMinorFactionOnTileAndJoinPlayer(TechLevel.Industrial, this.Tile, out this._product);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            _terminationTick = GenTicks.TicksGame + (int)(_durationDays * GenDate.TicksPerDay);
        }


        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (ShouldComplete())
            {
                // Complete Construction
                CreateProduct();
                // Notify player about the new settlement
                NotifyPlayer();
                // Clean itself
                Destroy();
            }
        }
 
        public override string GetInspectString()
        {
            return "WAW.RemainingConstructionDays".Translate(RemainingConstructionDays.ToString("0.0"), _durationDays.ToString("0.0"));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this._terminationTick, "_terminationTick");

        }





    }
}
