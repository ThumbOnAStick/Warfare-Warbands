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

        public virtual int UpgradeCost => 1;

        public virtual string Label => "";

        public virtual string ModRequired => ""; 

        public virtual TaggedString CostLabel => ($"${this.UpgradeCost}").Colorize(new Color(.8f,.8f, .2f));

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

   

        public virtual void OnArrived(List<Pawn> pawns)
        {

        }

        public bool RequiredModLoaded()
        {
            return ModRequired == "" || ModsConfig.IsActive(ModRequired);
        }

        public virtual TaggedString GetInspectString()
        {
            TaggedString outString = "";
            if (GearQuality != RimWorld.QualityCategory.Normal)
                outString += "WAW.QualityOverride".Translate(GearQuality.GetLabel()).Colorize(new Color(1f, 1f, 0.6f)) + "\n";
            if(MaintainDays > 1)
                outString += "WAW.ServesDays".Translate(MaintainDays) + "\n";
            if (!CostsSilver)
                outString += "WAW.IsFree".Translate() + "\n";
            if (!CanDroppod)
                outString += "WAW.CantDroppod".Translate().ToString().Colorize(Color.red) + "\n";
            if (!CanAttack)
                outString += "WAW.CantAttack".Translate().ToString().Colorize(Color.red) + "\n";
            if (!CanMove)
                outString += "WAW.CantMove".Translate().ToString().Colorize(Color.red) + "\n";

            return outString;
        }

        public virtual void OnUpgraded()
        {

        }

        public virtual bool CanAttackCurrent()
        {
            return true;
        }

        public virtual void ExposeData()
        {

        }
    }
}
