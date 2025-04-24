using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using WAWLeadership.LeadershipAttributes;

namespace WAWLeadership.UI
{
    public class Window_UpgradeWarband : Window
    {
        private readonly int _elemenetWidth = 50;
        private readonly int _elemenetHight = 50;
        private readonly int _labelHight = 30;
        private readonly int _unavailableLabelHight = 100;
        private readonly int _buttonWidth = 200;
        private readonly int _buttonHeight = 50;
        private readonly int _groupHeight = 250;
        private readonly int _loreWidth= 500;
        private readonly int _lorehight = 100;
        private readonly int _margin = 10;
        private PlayerWarbandUpgradeHolder _upgradeHolder;
        private PlayerWarbandLeader _targetLeader;
        private PlayerWarbandUpgrade _selectedUpgrade;
        private Vector2 _scrollPosition;
        public Window_UpgradeWarband()
        {
            _upgradeHolder = new PlayerWarbandUpgradeHolder();
            _targetLeader = new PlayerWarbandLeader();

        }
        public Window_UpgradeWarband(PlayerWarbandUpgradeHolder upgradeHolder, PlayerWarbandLeader leadership)
        {
            _upgradeHolder = upgradeHolder;
            _targetLeader = leadership;
        }
        public override Vector2 InitialSize => new Vector2(800, 500);


        public override void DoWindowContents(Rect inRect)
        {
            //Draw Close button
            WarbandUI.DrawExitButton(this, inRect);

            //Draw Upgrades
            List<PlayerWarbandUpgrade> upgrades = new List<PlayerWarbandUpgrade>()
            {
                new Upgrade_Outpost(),
                new Upgrade_Elite(),
                new Upgrade_Vehicle(),
                new Upgrade_Psycaster()

            };
            float distance = _buttonWidth / 2 - _elemenetWidth / 2;
            Rect outRect = new Rect(inRect.position, new Vector2(InitialSize.x, _groupHeight));
            Rect viewRect = new Rect(inRect.position, new Vector2((_buttonWidth + _margin + distance) * upgrades.Count, _groupHeight));
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            for (int i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                Rect upgradeRect = new Rect(10 + i * (_buttonWidth + _margin) + distance, 10, _elemenetWidth, _elemenetHight);
                Rect upgradeLabelRect = new Rect(upgradeRect.x, upgradeRect.yMax + _margin, _buttonWidth, _labelHight);
                Rect upgradeCostLabelRect = new Rect(upgradeRect.x, upgradeLabelRect.yMax + _margin, _elemenetWidth, _labelHight);
                Rect cannotSelectRect = new Rect(upgradeRect.x, upgradeCostLabelRect.yMax + _margin, _buttonWidth, _unavailableLabelHight);
                Rect boxRect = new Rect(upgradeRect.x, upgradeRect.y, _buttonWidth, _labelHight + _elemenetHight + Margin + _unavailableLabelHight);
                Widgets.DrawTextureFitted(upgradeRect, upgrade.TextureOverride(), 1f);
                Widgets.Label(upgradeLabelRect, upgrade.Label);
                Widgets.Label(upgradeCostLabelRect, upgrade.CostLabel);

                if (_selectedUpgrade != null && _selectedUpgrade.Label == upgrade.Label)
                {
                    Widgets.DrawBox(boxRect);
                }

                if (Widgets.ButtonInvisible(boxRect))
                {
                    _selectedUpgrade = upgrade;
                }

                if (CannotUpgrade(upgrade, out string reason))
                {
                    Widgets.Label(cannotSelectRect, reason);
                }

            }
            Widgets.EndScrollView();
            Rect upgradeLoreRect = WarbandUI.CenterRectFor(
                inRect,
                new Vector2(_loreWidth, _lorehight),
                Vector2.up * 100);

            if (_selectedUpgrade == null)
            {
                Widgets.Label(upgradeLoreRect, "WAW.PleaseSelectUpgrade".Translate());
                return;
            }
       
            Rect upgradeButtonRect = WarbandUI.CenterRectFor(
                inRect,
                new Vector2(_buttonWidth, _buttonHeight),
                Vector2.up * 200);

            Widgets.Label(upgradeLoreRect, this._selectedUpgrade.Lore);
            if (CannotUpgrade(_selectedUpgrade, out string reason1))
            {
                return;
            }
            if (!this._upgradeHolder.HasUpgrade)
                if (Widgets.ButtonText(upgradeButtonRect, "WAW.SelectUpgrade".Translate()))
                {
                    if (Mouse.IsOver(upgradeButtonRect))
                    {
                        TooltipHandler.TipRegion(upgradeButtonRect, _selectedUpgrade.GetInspectString());
                    }
                    if (WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, _selectedUpgrade.UpgradeCost))
                    {
                        SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                        _upgradeHolder.SetUpgrade(_selectedUpgrade);
                    }
                }
        }

        public bool CannotUpgrade(PlayerWarbandUpgrade upgrade, out string reason)
        {
            reason = "";
            if (this._upgradeHolder.HasUpgrade)
            {
                reason = "WAW.AlreadyUpgraded".Translate();
                return true;
            }
            if (upgrade is Upgrade_Elite && !GameComponent_WAW.Instance.CanPlayerUseEliteUpgrade())
            {
                reason = "WAW.EliteUpgradeRequirement".Translate();
                return true;
            }
            if (!upgrade.RequiredModLoaded())
            {
                reason = "WAW.ModNotLoaded".Translate(upgrade.ModRequired);
                return true;
            }
            if (!AttributeRequirementSatisfiedFor(upgrade.GetType(), out LeadershipAttribute attribute, out int minLevel)&&
                WAWSettings.upgradeRequiresLeader)
            {
                reason = "WAW.RequireSkill".Translate(attribute.GetLabel(), minLevel);
                return true;
            }
            if (upgrade.RequiresRelation(out Faction empire, out int relation))
            {
                reason = "WAW.RequireRelation".Translate(empire.Name, relation);
                return true;
            }
            return false;
        }

        public bool AttributeRequirementSatisfiedFor(Type T, out LeadershipAttribute attribute, out int minLevel)
        {
            attribute = null;
            minLevel = 0;
            switch (T.Name)
            {
                case nameof(Upgrade_Elite):
                    return RequireSkill<Attribute_Commanding>(out attribute, out minLevel);
                case nameof(Upgrade_Vehicle):
                    return RequireSkill<Attribute_Engineering>(out attribute, out minLevel);
                case nameof(Upgrade_Psycaster):
                    return RequireSkill<Attribute_Diplomacy>(out attribute, out minLevel);
            }

            return true;
        }

        bool RequireSkill<T>(out LeadershipAttribute attribute, out int minLevel) where T : LeadershipAttribute
        {
            attribute = new LeadershipAttribute();
            minLevel = 2;
            if (!this._targetLeader.IsLeaderAvailable())
                return false;
            var compLeadership = this._targetLeader.Leader.TryGetComp<CompLeadership>();
            if (compLeadership == null)
                return false;
            attribute = compLeadership.Leadership.AttributeSet.GetAttribute<T>();
            return attribute.GetLevel() >= minLevel;
        }
    }
}
