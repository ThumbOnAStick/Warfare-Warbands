using RimWorld;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Engineer : PlayerWarbandUpgrade
    {
        Dictionary<ThingDef, int> _shells;
        private List<ThingDef> _defBuffer;
        private List<int> _numberBuffer;
        private int _fromTile;
        private MapParent _targetMapP;

        public Upgrade_Engineer() : base()
        {
            _shells = new Dictionary<ThingDef, int>();
            _defBuffer = new List<ThingDef>();
            _numberBuffer = new List<int>();
        }

        public Dictionary<ThingDef, int> Shells => _shells;
        public override int UpgradeCost => 10000;
        public override string Label => "WAW.Engineer.Label".Translate();
        public override string Lore => "WAW.Engineer.Lore".Translate();

        public override RimWorld.QualityCategory GearQuality => RimWorld.QualityCategory.Good;

        public override Texture2D TextureOverride()
        {
            return WAWTex.WarbandEngineerTex;
        }

        public override IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action()
            {
                Order = 3000,
                icon = WAWTex.WarbandEngineerTex,
                defaultLabel = "WAW.ManageLoadout".Translate(),
                defaultDesc = "WAW.ManageLoadoutDesc".Translate(),
                action = delegate { Find.WindowStack.Add(new Window_EngineerManagerment(this)); }
            };
        }

        public int GetTotalShells()
        {
            int sum = 0;
            foreach (var pair in _shells)
            {
                if (pair.Key == null)
                {
                    continue;
                }
                sum += pair.Value;
            }
            return sum;
        }

        public void TryToChangeShells(ThingDef shell, int number)
        {
            if (number < 0 || number > 50)
                return;

            if (_shells.ContainsKey(shell))
            {
                _shells[shell] = number;
            }
            else
            {
                _shells.Add(shell, number);
            }

        }

        bool ConsumeShells()
        {
            int sum = 0;
            foreach (var pair in _shells)
            {
                if(pair.Key == null)
                {
                    continue;
                }
                float marketValue = pair.Key.BaseMarketValue * pair.Value;
                sum += (int)marketValue;
            }
            return WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, sum);
        }

        void FireAtMap(LocalTargetInfo info)
        {
            var map = Find.CurrentMap;
            int z = (int)(map.Center.z * 0.95f);
            int x = (int)(map.Center.x * 0.95f);
            // Launch a round of shell
            if (!ConsumeShells())
            {
                return;
            }
            foreach (var pair in this.Shells)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    // Calculate angle
                    var way = Find.WorldGrid.GetDirection8WayFromTo(_targetMapP.Tile, this._fromTile);

                    IntVec3 startingPoint = new IntVec3(0, 0, 0);
                    switch (way)
                    {
                        case Direction8Way.North:
                            startingPoint = map.Center + IntVec3.North * z;
                            break;
                        case Direction8Way.NorthEast:
                            startingPoint = map.Center + IntVec3.North * z + IntVec3.East *x;
                            break;
                        case Direction8Way.East:
                            startingPoint = map.Center + IntVec3.East *x;
                            break;
                        case Direction8Way.SouthEast:
                            startingPoint = map.Center + IntVec3.South * z + IntVec3.East *x;
                            break;
                        case Direction8Way.South:
                            startingPoint = map.Center + IntVec3.South * z;
                            break;
                        case Direction8Way.SouthWest:
                            startingPoint = map.Center + IntVec3.South * z + IntVec3.West *x;
                            break;
                        case Direction8Way.West:
                            startingPoint = map.Center + IntVec3.West *x;
                            break;
                        case Direction8Way.NorthWest:
                            startingPoint = map.Center + IntVec3.North * z + IntVec3.West *x;
                            break;
                        default:
                            startingPoint = map.Center;
                            break;
                    }
                    Projectile shell = (Projectile)GenSpawn.Spawn(pair.Key.projectileWhenLoaded, startingPoint, map);
                    int maxExclusive = GenRadial.NumCellsInRadius(10);
                    int num = Rand.Range(0, maxExclusive);
                    var rndCell = info.Cell + GenRadial.RadialPattern[num];
                    shell.Launch(launcher: null, usedTarget: rndCell, intendedTarget: info, ProjectileHitFlags.All);
                }
            }
        }


        void TryToFireArtillery()
        {
            var map = this._targetMapP.Map;
            var cell = CellFinder.StandableCellNear(map.Center, map, 10);
            CameraJumper.TryJump(cell, map);
            Targeter targeter = Find.Targeter;
            TargetingParameters targetParams = TargetingParameters.ForDropPodsDestination();
            targeter.BeginTargeting(targetParams: targetParams, action: FireAtMap);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this._shells,
"_shells", LookMode.Def, LookMode.Value, ref this._defBuffer, ref _numberBuffer);
            // If something is null, remove it
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this._shells.RemoveAll(x => x.Key == null);
            }
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
