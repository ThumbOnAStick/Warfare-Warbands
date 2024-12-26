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
        private readonly int _margin = 10;
        private PlayerWarbandUpgradeHolder _upgradeHolder;
        private PlayerWarbandLeader _targetLeader;

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
                new Upgrade_Vehicle()
            };

            for (int i = 0; i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                float distance = _buttonWidth / 2 - _elemenetWidth / 2;
                Rect upgradeRect = new Rect(10 + i * (_buttonWidth + _margin) + distance, 10, _elemenetWidth, _elemenetHight);
                Rect upgradeLabelRect = new Rect(upgradeRect.x, upgradeRect.yMax + _margin, _buttonWidth, _labelHight);
                Rect upgradeCostLabelRect = new Rect(upgradeRect.x, upgradeLabelRect.yMax + _margin, _elemenetWidth, _labelHight);
                Rect upgradeButtonRect = new Rect(upgradeRect.x - distance, upgradeCostLabelRect.yMax + _margin, _buttonWidth, _labelHight);
                Rect cannotSelectRect = new Rect(upgradeRect.x, upgradeCostLabelRect.yMax + _margin, _buttonWidth, _unavailableLabelHight);


                Widgets.DrawTextureFitted(upgradeRect, upgrade.TextureOverride(), 1f);
                Widgets.Label(upgradeLabelRect, upgrade.Label);
                Widgets.Label(upgradeCostLabelRect, upgrade.CostLabel);
                if(Mouse.IsOver(upgradeButtonRect))
                {
                    TooltipHandler.TipRegion(upgradeButtonRect, upgrade.GetInspectString());
                }
                if (CannotUpgrade(upgrade, out string reason))
                {
                    Widgets.Label(cannotSelectRect, reason);
                }
                else
                {
                    if(Widgets.ButtonText(upgradeButtonRect, "WAW.SelectUpgrade".Translate()))
                    {
                        if(WarbandUtil.TryToSpendSilverFromColony(Find.AnyPlayerHomeMap, upgrade.UpgradeCost))
                        {
                            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                            _upgradeHolder.SetUpgrade(upgrade);
                        }
                    }
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
            else if (!upgrade.RequiredModLoaded())
            {
                reason = "WAW.ModNotLoaded".Translate(upgrade.ModRequired);
                return true;
            }
            else if (!AttributeRequirementSatisfiedFor(upgrade.GetType(), out LeadershipAttribute attribute, out int minLevel)&&
                WAWSettings.upgradeRequiresLeader)
            {
                reason = "WAW.RequireSkill".Translate(attribute.GetLabel(), minLevel);
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
                    attribute = new Attribute_Commanding();
                    minLevel = 3;
                    if (!this._targetLeader.IsLeaderAvailable())
                        return false;
                    var compLeadership1 = this._targetLeader.Leader.TryGetComp<CompLeadership>();
                    if (compLeadership1 == null)
                        return false;
                    attribute = compLeadership1.Leadership.AttributeSet.GetAttribute<Attribute_Commanding>();
                    return attribute.GetLevel() >= minLevel;


                case nameof(Upgrade_Vehicle):
                    attribute = new Attribute_Engineering();
                    minLevel = 2;
                    if (!this._targetLeader.IsLeaderAvailable())
                        return false;
                    var compLeadership2 = this._targetLeader.Leader.TryGetComp<CompLeadership>();
                    if (compLeadership2 == null)
                        return false;
                    attribute = compLeadership2.Leadership.AttributeSet.GetAttribute<Attribute_Engineering>();
                    return attribute.GetLevel() >= minLevel;
                 
            }

            return true;
        }
    }
}
