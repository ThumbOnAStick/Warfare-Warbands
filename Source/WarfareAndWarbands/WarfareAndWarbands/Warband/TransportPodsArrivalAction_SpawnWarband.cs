using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband
{
    public class TransportPodsArrivalAction_SpawnWarband : TransportersArrivalAction
    {
        public TransportPodsArrivalAction_SpawnWarband(Warband savedWarband)
        {
            this.savedWarband = savedWarband;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            savedWarband.ResettleTo(tile);
        }
 
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref savedWarband, "savedWarband");
        }

        public Warband savedWarband;

        public override bool GeneratesMap => false;
    }
}
