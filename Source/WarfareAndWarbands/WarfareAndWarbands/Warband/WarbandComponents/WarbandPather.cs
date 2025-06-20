﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class WarbandPather:IExposable
    {
        public bool moving;
		public int lastPathedTargetTile;
        public WorldPath curPath;
        private Vector3 tweenedPos = Vector3.zero;
        private int nextTile;
        private int destTile;
        public float nextTileCostLeft = 0f;
        private readonly Warband warband;

        public WarbandPather(Warband warband)
        {
            this.warband = warband;
        }

        public Vector3 TweenedPos
        {
            get
            {
                if (!moving)
                {
                    return Find.WorldGrid.GetTileCenter(warband.Tile);
                }
                return this.tweenedPos;
            }
        }

        public bool StartPath(int destTile)
        {

            this.moving = true;
            this.destTile = destTile;
            this.tweenedPos = Find.WorldGrid.GetTileCenter(warband.Tile);
            if (this.TrySetNewPath())
            {
                this.nextTile = warband.Tile;
                this.TryEnterNextPathTile();
            }
            return true;
        }
        private void PatherArrived()
        {
            moving = false;
        }

        private bool AtDestinationPosition()
        {
            return this.warband.Tile == this.destTile;
        }

        private void SetupMoveIntoNextTile()
        {
            if (this.curPath.NodesLeftCount < 2)
            {
                Log.Error(string.Concat(new object[]
                {
                    this.warband.Label,
                    " at ",
                    this.warband.Tile,
                    " ran out of path nodes while pathing to ",
                    this.destTile,
                    "."
                }));
                this.PatherFailed();
                return;
            }
            this.nextTile = this.curPath.ConsumeNextNode();
             if (Find.World.Impassable(this.nextTile))
            {
                Log.Error(string.Concat(new object[]
                {
                    this.warband.Label,
                    " entering ",
                    this.nextTile,
                    " which is unwalkable."
                }));
            }
            this.nextTileCostLeft = 1;
        }

        private void TryEnterNextPathTile()
        {
            if (!this.IsNextTilePassable())
            {
                this.PatherFailed();
                return;
            }
         
            this.warband.Tile = this.nextTile;
            if (this.AtDestinationPosition())
            {
                this.PatherArrived();
                return;
            }
            if(this.curPath == null && !TrySetNewPath())
            {
                return;
            }
            if (this.curPath.NodesLeftCount == 0)
            {
                return;
            }
            this.SetupMoveIntoNextTile();
        }

        private bool IsPassable(int tile)
        {
            return !Find.World.Impassable(tile);
        }

        public bool IsNextTilePassable()
        {
            return this.IsPassable(this.nextTile);
        }

        private void PatherFailed()
        {

        }

        private bool TrySetNewPath()
        {
            WorldPath worldPath = this.GenerateNewPath();
            if (!worldPath.Found)
            {
                this.PatherFailed();
                return false;
            }
            this.curPath?.ReleaseToPool();
            this.curPath = worldPath;
            return true;
        }

        private WorldPath GenerateNewPath()
        {
            this.lastPathedTargetTile = this.destTile;
            WorldPath worldPath = Find.WorldPathFinder.FindPath(this.warband.Tile, this.destTile, null);
            if (worldPath.Found)
            {
                worldPath.AddNodeAtStart(this.warband.Tile);
            }
            return worldPath;
        }

        void ResolveDrawPos()
        {
            Vector3 warbandPos = Find.WorldGrid.GetTileCenter(this.warband.Tile);
            Vector3 nextPos= Find.WorldGrid.GetTileCenter(this.nextTile);
            Vector3 dir = nextPos - warbandPos;
            tweenedPos = warbandPos + dir *  (1 - nextTileCostLeft);
        }

        float MovermentPerTick()
        {
            return warband.playerWarbandManager.upgradeHolder.MoveSpeed / Find.WorldGrid.GetRoadMovementDifficultyMultiplier(this.warband.Tile, this.nextTile);
        }

        public void Tick()
        {
            if (!this.moving)
            {
                return;
            }
            if (this.nextTileCostLeft > 0f)
            {
                this.nextTileCostLeft -= MovermentPerTick();
                ResolveDrawPos();
                return;
            }

            this.TryEnterNextPathTile();
            
        }

        public void ResetPath()
        {
            this.nextTileCostLeft = 0f;
            this.moving = false;    
        }

        public void DrawPath()
        {
            if(this.curPath == null)
            {
                return;
            }
            if (!this.curPath.Found)
            {
                return;
            }
            if (curPath.NodesLeftCount > 0)
            {
                WorldGrid worldGrid = Find.WorldGrid;
                float d = 0.05f;
                for (int i = 0; i < curPath.NodesLeftCount - 1; i++)
                {
                    Vector3 a = worldGrid.GetTileCenter(curPath.Peek(i));
                    Vector3 vector = worldGrid.GetTileCenter(curPath.Peek(i + 1));
                    a += a.normalized * d;
                    vector += vector.normalized * d;
                    GenDraw.DrawWorldLineBetween(a, vector, 1f);
                }
                if (warband != null)
                {
                    Vector3 a2 = warband.DrawPos;
                    Vector3 vector2 = worldGrid.GetTileCenter(curPath.Peek(0));
                    a2 += a2.normalized * d;
                    vector2 += vector2.normalized * d;
                    if ((a2 - vector2).sqrMagnitude > 0.005f)
                    {
                        GenDraw.DrawWorldLineBetween(a2, vector2, 1f);
                    }
                }
            }
        }


        public void ExposeData()
        {
            Scribe_Values.Look(ref this.moving, "moving", false);
            Scribe_Values.Look(ref this.lastPathedTargetTile, "lastPathedTargetTile");
            Scribe_Values.Look(ref this.tweenedPos, "tweenedPos");
            Scribe_Values.Look(ref this.nextTile, "nextTile");
            Scribe_Values.Look(ref this.nextTileCostLeft, "nextTileCostLeft");
            Scribe_Values.Look(ref this.destTile, "destTile");
        }

            
    }
}
