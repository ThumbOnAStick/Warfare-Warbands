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
    internal class Upgrade_Elite : PlayerWarbandUpgrade
    {
  
        public Upgrade_Elite()
        {

        }

        public override int MaintainDays => 3;
        public override bool CostsSilver => false;
        public override bool CanMove => true;

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
