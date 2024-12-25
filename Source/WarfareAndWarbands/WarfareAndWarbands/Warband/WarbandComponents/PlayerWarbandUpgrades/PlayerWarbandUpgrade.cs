using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public abstract class PlayerWarbandUpgrade : IExposable
    {
        public virtual bool CanDroppod=> true;

        public virtual bool CostsSilver => true;

        public virtual bool CanAttack => true;

        public virtual bool CanMove => true;

        public virtual int MaintainDays => 1;

        public virtual RimWorld.QualityCategory GearQuality => RimWorld.QualityCategory.Normal;

        public PlayerWarbandUpgrade()
        {

        }

        public virtual IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield break;
        }

        public virtual Texture2D TextureOverride()
        {
            return null;
        }

        public virtual void ExposeData()
        {

        }

        public virtual void OnArrived(List<Pawn> pawns)
        {

        }

        public virtual string GetDescription()
        {
            string outString = "";
            if (GearQuality != RimWorld.QualityCategory.Normal)
                outString += "\n" + "WAW.QualityOverride".Translate(GearQuality.GetLabel());
            return outString;
        }
    }
}
