using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Psycaster : PlayerWarbandUpgrade
    {
        private List<PsycasterInfo> _infos;

        public Upgrade_Psycaster()
        {
            _infos = new List<PsycasterInfo>(); 
        }

        public List<PsycasterInfo> Infos => _infos;
        public int CasterCap => 3;

        public void TryToAddInfo(PsycasterInfo info)
        {
            if (this._infos.Count < CasterCap)
                this.Infos.Add(info);
            else
                Messages.Message("WAW.CastersOverflow".Translate(), MessageTypeDefOf.RejectInput);
        }

        public override IEnumerable<Pawn> ExtraPawns()
        {
            foreach(var info in _infos)
            {
                yield return info.CreatePsycaster();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _infos, "_upgradeInfos", LookMode.Value);
        }

    }
}
