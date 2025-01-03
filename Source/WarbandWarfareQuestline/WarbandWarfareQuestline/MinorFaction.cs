using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline
{
    internal class MinorFaction : IExposable
    {
        private string _factionName;


        public void ExposeData()
        {
            Scribe_Values.Look(ref _factionName, "_factionName");
        }
    }
}
