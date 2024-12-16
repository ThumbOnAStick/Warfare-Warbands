using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;

namespace WarfareAndWarbands.Misc
{
    public class Alert_ClosingPlayerWarbandMap : Alert
    {
        public override AlertPriority Priority => AlertPriority.High;

        public override string GetJumpToTargetsText => base.GetJumpToTargetsText;

        protected override Color BGColor => new Color(.65f, .65f, 0, .75f);

        private GlobalTargetInfo CurrPlayerWarband
        {
            get
            {
                this.currPlayerWarband = GlobalTargetInfo.Invalid;

                if (Find.CurrentMap == null)
                {
                    return GlobalTargetInfo.Invalid;
                }
                if (Find.CurrentMap.Parent as Warband.Warband != null &&
                    Find.CurrentMap.ParentFaction == Faction.OfPlayer)
                {
                    currPlayerWarband = Find.CurrentMap.Parent;
                }

                return this.currPlayerWarband;
            }
        }

        private GlobalTargetInfo currPlayerWarband = GlobalTargetInfo.Invalid;

        public override void AlertActiveUpdate()
        {
            base.AlertActiveUpdate();
        }

        public override Rect DrawAt(float topY, bool minimized)
        {
            return base.DrawAt(topY, minimized);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override TaggedString GetExplanation()
        {
            return "WAW.CloseMapIn.Desc".Translate();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GetLabel()
        {
            var map = Find.CurrentMap;
            var compPlayerWarbandMap = (MapComponent_WarbandRaidTracker)map.GetComponent(typeof(MapComponent_WarbandRaidTracker));
            if (compPlayerWarbandMap != null)
            {
                var closeIn = compPlayerWarbandMap.GetRemainingHours();
                return "WAW.CloseMapIn".Translate(closeIn);
            }
            return "";
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(CurrPlayerWarband);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void OnClick()
        {
            base.OnClick();
        }
    }
}
