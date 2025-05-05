using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaWeaponsExpanded;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class PlayerWarbandUpgradeHolder : IExposable
    {
        public bool CostsSilver => !HasUpgrade || SelectedUpgrade.CostsSilver;
        public bool CanAttack => !HasUpgrade || SelectedUpgrade.CanAttack;
        public bool CanMove => !HasUpgrade || SelectedUpgrade.CanMove;
        public bool CanDroppod => !HasUpgrade || SelectedUpgrade.CanDroppod;
        public bool CanAttackCurrent => !HasUpgrade || SelectedUpgrade.CanAttackCurrent();
        public bool HasUpgrade => SelectedUpgrade != null;
        public float MoveSpeed => HasUpgrade? SelectedUpgrade.MoveSpeed : 0.0005f;
        public QualityCategory GearQuality => HasUpgrade? selectedUpgrade.GearQuality : QualityCategory.Normal;

        public PlayerWarbandUpgrade SelectedUpgrade => selectedUpgrade;
        private PlayerWarbandUpgrade selectedUpgrade;

        public PlayerWarbandUpgradeHolder()
        {
            selectedUpgrade = null;
        }

        public void GainVehilceUpgrade()
        {
            Upgrade_Vehicle vehicle = new Upgrade_Vehicle();
            SetUpgrade(vehicle);
        }

        public void GainEliteUpgrade()
        {
            Upgrade_Elite outpost = new Upgrade_Elite();
            SetUpgrade(outpost);
        }

        public void GainOutpostUpgrade()
        {
            Upgrade_Outpost outpost = new Upgrade_Outpost();
            SetUpgrade(outpost);
        }

        public void GainPsycasterUpgrade()
        {
            Upgrade_Psycaster psy = new Upgrade_Psycaster();
            SetUpgrade(psy);
        }

        public void GainEngineerUpgrade()
        {
            Upgrade_Engineer eng = new Upgrade_Engineer();
            SetUpgrade(eng);
        }

        public T GetUpgrade<T>() where T: PlayerWarbandUpgrade
        {
            return this.selectedUpgrade as T;
        }

        public void SetUpgrade(PlayerWarbandUpgrade upgrade)
        {
            selectedUpgrade = upgrade;
            selectedUpgrade.OnUpgraded();
        }

        public Texture2D TextureOverride()
        {
            if (!HasUpgrade)
            {
                return null;
            }
            return selectedUpgrade.TextureOverride();
        }

        public bool IsUpgrade<T>() where T : PlayerWarbandUpgrade
        {
            return selectedUpgrade is PlayerWarbandUpgrade;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref selectedUpgrade, "selectedUpgrade");
        }
    }
}
