using RimWorld.Planet;
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

        public void Tick()
        {
           
            if (this.nextTileCostLeft > 0f)
            {
                this.nextTileCostLeft -= warband.playerWarbandManager.upgradeHolder.MoveSpeed;
                ResolveDrawPos();
                return;
            }
            if (this.moving)
            {
                this.TryEnterNextPathTile();
            }
        }

        public void ResetPath()
        {
            this.nextTileCostLeft = 0f;
            this.moving = false;    
        }


        public void ExposeData()
        {
            Scribe_Values.Look(ref this.moving, "moving", false);
            Scribe_Values.Look(ref this.lastPathedTargetTile, "lastPathedTargetTile");
            Scribe_Values.Look(ref this.tweenedPos, "tweenedPos");
            Scribe_Values.Look(ref this.nextTile, "nextTile");
            Scribe_Values.Look(ref this.nextTileCostLeft, "nextTileCostLeft");

        }


    }
}
