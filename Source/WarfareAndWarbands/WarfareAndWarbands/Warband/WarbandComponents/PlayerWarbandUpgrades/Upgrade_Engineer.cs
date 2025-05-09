using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Engineer : PlayerWarbandUpgrade
    {
        private Dictionary<ThingDef, int> _shells;

        private List<ThingDef> _defBuffer;
        private List<int> _numberBuffer;
        private List<ThingDef> _defBuffer1;
        private List<int> _numberBuffer1;

        private int _fromTile;
        private MapParent _targetMapP;

        public Upgrade_Engineer()
        {
            _shells = new Dictionary<ThingDef, int>();
            _defBuffer = new List<ThingDef>();
            _numberBuffer = new List<int>();
            _defBuffer1 = new List<ThingDef>();
            _numberBuffer1 = new List<int>();
        }

        public Dictionary<ThingDef, int> Shells => _shells;
        public override int UpgradeCost => 10000;
        public override string Label => "WAW.Engineer.Label".Translate();
        public override string Lore => "WAW.Engineer.Lore".Translate();
        public override RimWorld.QualityCategory GearQuality => RimWorld.QualityCategory.Good;

        public override Texture2D TextureOverride() => WAWTex.WarbandEngineerTex;

        public override IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action
            {
                Order = 3000,
                icon = WAWTex.WarbandEngineerTex,
                defaultLabel = "WAW.ManageLoadout".Translate(),
                defaultDesc = "WAW.ManageLoadoutDesc".Translate(),
                action = () => Find.WindowStack.Add(new Window_EngineerManagerment(this))
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _shells, "_shells", LookMode.Def, LookMode.Value, ref _defBuffer, ref _numberBuffer);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _shells.RemoveAll(pair => pair.Key == null);
            }
        }

        
        public int GetTotalShells() => _shells.Values.Sum();

        public void TryToChangeShells(ThingDef shell, int number)
        {
            if (number < 0 || number > 50) return;

            _shells[shell] = number;
        }

        private bool ConsumeShells()
        {
            int totalCost = _shells
                .Where(pair => pair.Key != null)
                .Sum(pair => (int)(pair.Key.BaseMarketValue * pair.Value));

            return WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, totalCost);
        }

        private IntVec3 GetShellEnterMapSpot()
        {
            var map = Find.CurrentMap;
            var direction = Find.WorldGrid.GetDirection8WayFromTo(_targetMapP.Tile, _fromTile);

            return GetDirectionOffset(direction, map);

        }

        private IntVec3 GetDirectionOffset(Direction8Way direction, Map map)
        {
            switch (direction)
            {
                case Direction8Way.North:
                    return map.Center + IntVec3.North * (int)(map.Center.z * 0.95f);
                case Direction8Way.NorthEast:
                    return map.Center + IntVec3.North * (int)(map.Center.z * 0.95f) + IntVec3.East * (int)(map.Center.x * 0.95f);
                case Direction8Way.East:
                    return map.Center + IntVec3.East * (int)(map.Center.x * 0.95f);
                case Direction8Way.SouthEast:
                    return map.Center + IntVec3.South * (int)(map.Center.z * 0.95f) + IntVec3.East * (int)(map.Center.x * 0.95f);
                case Direction8Way.South:
                    return map.Center + IntVec3.South * (int)(map.Center.z * 0.95f);
                case Direction8Way.SouthWest:
                    return map.Center + IntVec3.South * (int)(map.Center.z * 0.95f) + IntVec3.West * (int)(map.Center.x * 0.95f);
                case Direction8Way.West:
                    return map.Center + IntVec3.West * (int)(map.Center.x * 0.95f);
                case Direction8Way.NorthWest:
                    return map.Center + IntVec3.North * (int)(map.Center.z * 0.95f) + IntVec3.West * (int)(map.Center.x * 0.95f);
                default:
                    return map.Center;
            }
        }

        private void FireAtMap(LocalTargetInfo targetInfo)
        {
            if (!ConsumeShells()) return;

            var map = Find.CurrentMap;
            var startingPoint = GetShellEnterMapSpot();

            foreach (var (shell, count) in _shells)
            {
                for (int i = 0; i < count; i++)
                {
                    var projectile = (Projectile)GenSpawn.Spawn(shell.projectileWhenLoaded, CellFinder.RandomClosewalkCellNear(startingPoint, map, 10), map);
                    var randomCell = targetInfo.Cell + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(10))];
                    projectile.Launch(null, randomCell, targetInfo, ProjectileHitFlags.All);
                }
            }
        }

        private void TryToFireArtillery()
        {
            var map = _targetMapP.Map;
            var cell = CellFinder.StandableCellNear(map.Center, map, 10);
            CameraJumper.TryJump(cell, map);

            Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), FireAtMap);
        }


        public override IEnumerable<FloatMenuOption> GetExtraAttackFloatMenuOptions(PlayerWarbandManager pManager)
        {
            _fromTile = pManager.AttackTile;
            _targetMapP = pManager.targetMapP;

            if (GetTotalShells() > 0)
            {
                yield return new FloatMenuOption("WAW.ArtilleryStrike".Translate(), TryToFireArtillery);
            }
            else
            {
                yield return new FloatMenuOption("WAW.ArtilleryStrikeNoAmmo".Translate(), null);
            }


        }
    }
}
