using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.UI;

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
        Warband warband;
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.ServesPlayerFaction)
            {
                if (!Find.CurrentMap.listerThings.AllThings.Any(x => x.def == WAWDefof.WAW_LootChest))
                    yield return WarbandUI.PlaceLootChest(this.warband);
                yield return WarbandUI.RetreatPawn(this);
            }

        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            if (servesPlayerFaction)
            {
                Messages.Message("WAW.WarbandLoss".Translate(), MessageTypeDefOf.NegativeEvent);
                string pawnKindName = Mercenary.kindDef.defName;
                this.warband?.TryToRemovePawn(pawnKindName);
            }
        }

 
        public void Retreat()
        {
            this.servesPlayerFaction = false;
            Mercenary.SetFaction(Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband));
        }

        public void SetWarband(Warband warband)
        {
            this.warband = warband;
        }
        public Warband GetWarband()
        {
            return this.warband;
        }


        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            var inventory = Mercenary.inventory;
            for (int i = 0; i < inventory.innerContainer.Count; i++)
            {
                Thing t = inventory.innerContainer[i];
                this.warband?.Store(ref t);
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
                    Retreat();
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
