using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;

namespace WAWLeadership
{
    public class CompProperties_Leadership : CompProperties
    {
        public CompProperties_Leadership()
        {
            this.compClass = typeof(CompLeadership);
        }
    }
}
