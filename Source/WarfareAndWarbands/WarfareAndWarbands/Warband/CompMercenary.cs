using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband
{
    public class CompProperties_Mercenary : CompProperties
    {
        public CompProperties_Mercenary()
        {
            this.compClass = typeof(CompMercenary);
        }
    }

    public class CompMercenary : ThingComp
    {
        bool servesPlayerFaction = false;
        int lastServeTick = 0;

        public CompProperties_Mercenary Props
        {
            get
            {
                return (CompProperties_Mercenary)this.props;
            }
        }

        public Pawn Mercenary
        {
            get
            {
                return (Pawn)this.parent;
            }
        }


        public bool ServesPlayerFaction
        {
            get
            {
                return servesPlayerFaction;
            }
            set
            {
                servesPlayerFaction = value;
            }
        }

        public int LastServeTick
        {
            get
            {
                return lastServeTick;
            }
            set
            {
                lastServeTick = value;
            }
        }


        public override void PostPostMake()
        {
            base.PostPostMake();
            lastServeTick = Find.TickManager.TicksGame;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.servesPlayerFaction)
            {
                if (Find.TickManager.TicksGame - LastServeTick > serveDuration)
                {
                    LastServeTick = Find.TickManager.TicksGame;
                    this.servesPlayerFaction = false;
                    Mercenary.SetFaction(Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband));
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref servesPlayerFaction, "servesPlayerFaction");
            Scribe_Values.Look(ref lastServeTick, "lastServeTick");

        }

        readonly int serveDuration = 60000;

    }
}
