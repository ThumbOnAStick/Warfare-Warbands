using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.PlayerWarbandRaid
{
    public class MapComponent_WarbandRaidTracker : MapComponent
    {
        private int startTicks;
        private bool letterSent;
        private static readonly int startCounterTick = 3000;
        private static readonly int forceQuitTicks = GenDate.TicksPerDay;


        public MapComponent_WarbandRaidTracker(Map map) : base(map)
        {
            this.startTicks = GenTicks.TicksGame;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (ValidateMap())
            {
                CheckSendLetter();
                CheckShouldRemoveMap();
            }

        }

        public void CheckShouldRemoveMap()
        {
            if (GenTicks.TicksGame > this.startTicks + forceQuitTicks)
            {
                Current.Game.DeinitAndRemoveMap(map, true);
            }
        }

        public string GetRemainingHours()
        {
            var remianingTicks = this.startTicks + forceQuitTicks - GenTicks.TicksGame;
            int remianingHours = remianingTicks / GenDate.TicksPerHour;
            return remianingHours.ToString();
        }

        public bool LtterSent()
        {
            return this.letterSent;
        }

        public bool ValidateMap()
        {
            return this.map.ParentFaction == Faction.OfPlayer && this.map.Parent != null && this.map.Parent as Warband != null;
        }

        void CheckSendLetter()
        {
            if (letterSent)
                return;
            if (GenTicks.TicksGame - startCounterTick > startTicks && !GenHostility.AnyHostileActiveThreatToPlayer(this.map))
            {
                string label = "WAW.DefenseSuccussful".Translate();
                string desc = "WAW.DefenseSuccussful.Desc".Translate();
                Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.PositiveEvent);
                Find.LetterStack.ReceiveLetter(letter);
                letterSent = true;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startTicks, "startTicks", GenTicks.TicksGame);
            Scribe_Values.Look(ref letterSent, "letterSent", false);

        }
    }
}
