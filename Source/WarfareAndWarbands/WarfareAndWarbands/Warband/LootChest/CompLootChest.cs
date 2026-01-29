using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using Verse.Sound;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    public class CompLootChest : ThingComp
    {
        public Warband warband;
        public CompProperties_LootChest Props => (CompProperties_LootChest)this.props;
        public CompTransporter MyTransporter => this.parent.TryGetComp<CompTransporter>();
        private List<IntVec3> tradeableCells = new List<IntVec3>();
        private readonly int _quickLootRadius = 10;

        public int QuickLootRadius => this._quickLootRadius;

        public void AssignWarband(Warband warband)
        {
            this.warband = warband;
        }
   

        public void TransferToWarband()
        {
            if (this.MyTransporter == null || this.MyTransporter.innerContainer.Count < 1)
            {
                return;
            }
            if (this.warband == null) 
            {
                this.parent.Destroy();
                return;
            }
            warband?.StoreAll(MyTransporter.innerContainer.ToList());
            MyTransporter.innerContainer.Clear();
            MyTransporter.CancelLoad();
            Messages.Message("WAW.TransportedToWarband".Translate(), MessageTypeDefOf.PositiveEvent);
        }

        private void LoadInstantly()
        {
            TransporterUtility.InitiateLoading(new List<CompTransporter>() { MyTransporter } );
        }

        public  List<IntVec3> TradeableCellsAround()
        {
            tradeableCells.Clear();
            IntVec3 pos = this.parent.Position;
            Map map = this.parent.MapHeld;
            if (!pos.InBounds(map))
            {
                return  tradeableCells;
            }
            Region region = pos.GetRegion(map, RegionType.Set_Passable);
            if (region == null)
            {
                return tradeableCells;
            }
            RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate (Region r)
            {
                foreach (IntVec3 item in r.Cells)
                {
                    if (item.InHorDistOf(pos, _quickLootRadius))
                    {
                        tradeableCells.Add(item);
                    }
                }
                return false;
            }, 15, RegionType.Set_Passable);
            return tradeableCells;
        }

        private void MakeMatchingStockpile()
        {
            Designator des = DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>();
            des.DesignateMultiCell(from c in TradeableCellsAround()
                                   where des.CanDesignateCell(c).Accepted
                                   select c);
        }

        public void LoadEverythingNear()
        {
            MakeMatchingStockpile();
            LoadInstantly();
             foreach ( var t in AllSendableItems(MyTransporter, parent.MapHeld, true))
            {
                if(t.def.category != ThingCategory.Item)
                {
                    continue;
                }
                CompForbiddable compForbiddable = t.TryGetComp<CompForbiddable>();
                if (compForbiddable != null && compForbiddable.Forbidden)
                {
                    compForbiddable.Forbidden = false;
                }
                Log.Message($"Thing loaded: {t.Label}");
                TransferableOneWay transferableOneWay = new TransferableOneWay() { things = new List<Thing>() { t } };
                MyTransporter.AddToTheToLoadList(transferableOneWay, t.stackCount);
            }
        }

        public IEnumerable<Thing> AllSendableItems(CompTransporter transporter, Map map, bool autoLoot)
        {
            List<Thing> items = CaravanFormingUtility.AllReachableColonyItems(
                map,
                autoLoot, 
                transporter.Props.canChangeAssignedThingsAfterStarting && transporter.LoadingInProgressOrReadyToLaunch, 
                autoLoot).Where(t => t.Position.DistanceTo(parent.Position) <= _quickLootRadius).ToList();
            return items;
        
        }

        public override void CompTick()
        {
            base.CompTick();
            var t = MyTransporter;
            if (t != null && !t.AnythingLeftToLoad)
            {
                TransferToWarband();
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref warband, "warband");
        }


    }
}
