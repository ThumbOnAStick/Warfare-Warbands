using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Elite : PlayerWarbandUpgrade
    {
  
        public Upgrade_Elite()
        {

        }

        public override int MaintainDays => 3;
        public override bool CostsSilver => false;
        public override bool CanMove => true;
        public override int UpgradeCost => 10000;
        public override string Label => "WAW.EliteLabel".Translate();
        public override string Lore => "WAW.EliteLore".Translate();
        public override float Wage => .1f; 

        public override QualityCategory GearQuality => QualityCategory.Masterwork;

        public override Texture2D TextureOverride()
        {
            return WAWTex.WarbandEliteTex;
        }

        public override void OnArrived(List<Pawn> pawns)
        {
            base.OnArrived(pawns);
            if (ModsConfig.RoyaltyActive)
            {
                SoundDefOf.MechClusterDefeated.PlayOneShotOnCamera();
            }
        }




    }
}
