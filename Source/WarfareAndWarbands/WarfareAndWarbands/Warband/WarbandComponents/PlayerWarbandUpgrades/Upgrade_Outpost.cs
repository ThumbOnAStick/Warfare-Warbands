using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Outpost : PlayerWarbandUpgrade
    {
        public override bool CostsSilver => false;
        public override bool CanMove => false;
        public override int UpgradeCost => 5000;
        public override string Label => "WAW.OutpostLabel".Translate();
        public override string Lore => "WAW.OutpostLore".Translate();
        public override float Wage => .05f; 

        public override Texture2D TextureOverride()
        {
            return warbandOutPost;
        }

        private readonly Texture2D warbandOutPost;

        public Upgrade_Outpost()
        {
            warbandOutPost = WAWTex.WarbandOutpost;
        }


    }
}
