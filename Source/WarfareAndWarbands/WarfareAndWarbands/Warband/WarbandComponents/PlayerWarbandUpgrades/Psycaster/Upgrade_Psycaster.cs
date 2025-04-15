using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_VPE;
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

        public override string ModRequired => "VanillaExpanded.VPsycastsE";
        public List<PsycasterInfo> Infos => _infos;
        public int CasterCap => 3;
        public override int UpgradeCost => 15000;
        public override string Label => "WAW.PsyCasterLabel".Translate();
        public override string Lore => "WAW.PsyCasterLore".Translate();

        public override Texture2D TextureOverride()
        {
            return WAWTex.WarbandPsycasterTex;
        }

        public void TryToAddInfo(PsycasterInfo info)
        {
            if (this._infos.Count < CasterCap)
            {
                this.Infos.Add(info);
                Messages.Message("WAW.CasterAdded".Translate(info.GetPsyPathLabel()), MessageTypeDefOf.PositiveEvent);
            }
            else
                Messages.Message("WAW.CastersOverflow".Translate(), MessageTypeDefOf.RejectInput);
        }

        public override IEnumerable<Pawn> ExtraPawns(Warband warband)  
        {
            foreach(var info in _infos)
            {
                yield return info.CreatePsycaster(warband);
            }
        }

        public override IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "WAW.RecruitPsycasters".Translate(),
                defaultDesc = "WAW.RecruitPsycasters.Desc".Translate(),
                icon = this.TextureOverride(),
                action = delegate
                {
                    Find.WindowStack.Add(new Window_VPEWarband(GameComponent_Customization.Instance.CustomizationRequests, this));
                },
            };
        }

        public int GetEltexCost()
        {
            return Infos.Sum(x => x.GetEltexCost());
        }

        bool NoPsycasters()
        {
            bool insuffcient = this._infos.Count < 1;
            if (insuffcient)
            {
                Messages.Message("WAW.InsufficientPsycaster".Translate(), MessageTypeDefOf.RejectInput);
            }
            return insuffcient;
        }

        public override bool CanAttackCurrent()
        {
            return !NoPsycasters() && VPE.TryToSpendEltexFromColony(Find.AnyPlayerHomeMap, this.GetEltexCost());
        }

        public override bool RequiresRelation(out Faction faction, out int relation)
        {
            if(Faction.OfEmpire == null)
            {
                return base.RequiresRelation(out faction, out relation);
            }
            faction = Faction.OfEmpire;
            relation = 60;
            if(Faction.OfPlayer.GoodwillWith(Faction.OfEmpire) >= 60)
            {
                return false;
            }
            return true;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _infos, "_upgradeInfos", LookMode.Deep);
        }


    }
}
