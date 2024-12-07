using RimWorld;
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
        public CompTransporter CompTransporter => this.parent.TryGetComp<CompTransporter>();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.CompTransporter.innerContainer.Count > 0)
                yield return WarbandUI.TransferContent(this);
        }
        public void AssignWarband(Warband warband)
        {
            this.warband = warband;
        }
        public void TransferToWarband()
        {
            if (this.CompTransporter == null || this.CompTransporter.innerContainer.Count < 1)
            {
                return;
            }
            if (this.warband == null) 
            {
                this.parent.Destroy();
                return;
            }
            warband?.StoreAll(CompTransporter.innerContainer.ToList());
            CompTransporter.innerContainer.Clear();
            CompTransporter.CancelLoad();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref warband, "warband");
        }



    }
}
