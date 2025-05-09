﻿using RimWorld;
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

        public virtual string Lore => "";

        public virtual float MoveSpeed => 0.0005f;

        public virtual float Wage => 0f;

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

        public virtual IEnumerable<Pawn> ExtraPawns(Warband warband)
        {
            yield break;
        }

        public virtual void OnPawnsGenerated(List<Pawn> pawns)
        {

        }

        public virtual void OnMapLoaded(Map map)
        {

        }

        public virtual bool RequiresRelation(out Faction faction, out int relation)
        {
            faction = null;
            relation = 0;
            return false;
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
            if(this.Wage > 0)
            {
                Letter l = LetterMaker.MakeLetter("WAW.WageDemandingWarband".Translate(), "WAW.WageDemandingWarband.Desc".Translate((Wage * 100).ToString("0.00")), LetterDefOf.NeutralEvent);
                Find.LetterStack.ReceiveLetter(l);
            }
        }

        public virtual IEnumerable<FloatMenuOption> GetExtraAttackFloatMenuOptions(PlayerWarbandManager pManager)
        {
            return new List<FloatMenuOption>();
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
