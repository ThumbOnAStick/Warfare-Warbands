using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.Mercenary;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;

namespace WarfareAndWarbands.Warband.VassalWarband
{
    public class WorldObject_VassalWarband : WorldObject
    {
        private Dictionary<string, int> _bandMembers;
        private Color _colorOverride;
        private List<string> keyBuffers;
        private List<int> valueBuffers;

        public WorldObject_VassalWarband()
        {
            _colorOverride = Color.white;
        }

        public Dictionary<string, int> BandMembers => _bandMembers;
        public override Color ExpandingIconColor => _colorOverride;

        public void SetBandMembers(Dictionary<string, int> bandMembers)
        {
            this._bandMembers = bandMembers;
        }

        public void SetColor(Color color)
        {
            this._colorOverride = color;    
        }

        public Caravan SpawnWarbandCaravan(int tile)
        {
            List<Pawn> list = this.GenerateVassalPawnsFromTable();
        
            Caravan caravan = CaravanMaker.MakeCaravan(list, Faction.OfPlayer, tile, true);
            return caravan;
        }

        public void EnterMap(MapParent mapP)
        {
            Caravan caravan = SpawnWarbandCaravan(mapP.Tile);
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
        }

        public void AttackLand(MapParent p)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                EnterMap(p);
                this.Destroy();
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        public bool SelectAttackTarget(GlobalTargetInfo info)
        {
            bool emptyWarband = !_bandMembers.Any((KeyValuePair<string, int> x) => x.Value > 0);
            bool result;
            if (emptyWarband)
            {
                Messages.Message("WAW.NoMembers".Translate(), MessageTypeDefOf.RejectInput, true);
                result = false;
            }
            else
            {
                bool hasWorldObject = info.WorldObject != null && info.WorldObject is MapParent;
                bool reservedByQuest = false;
                List<Quest> quests = Find.QuestManager.QuestsListForReading;
                for (int i = 0; i < quests.Count; i++)
                {
                    Quest quest = quests[i];
                    if (!quest.hidden && !quest.Historical && !quest.dismissed && quest.QuestLookTargets.Contains(info.WorldObject))
                    {
                        reservedByQuest = true;
                    }
                }
                bool flag3 = !hasWorldObject ||
            WarbandUtil.IsWorldObjectNonHostile(info.WorldObject)
                    || Find.World.Impassable(info.Tile);
                if (flag3 && !reservedByQuest)
                {
                    Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.RejectInput, true);
                    result = false;
                }
                else
                {
                    bool flag4 = Find.WorldGrid.ApproxDistanceInTiles(info.Tile, this.Tile) > (float)PlayerWarbandManager.playerAttackRange;
                    if (flag4)
                    {
                        Messages.Message("WAW.FarObject".Translate(), MessageTypeDefOf.RejectInput, true);
                        result = false;
                    }
                    else
                    {
                        MapParent mapParent = (MapParent)info.WorldObject;
                        this.GetVassalWarbandAttackOptions(mapParent);
                        result = true;
                    }
                }
            }
            return result;
        }

        public void OderAttack()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.SelectAttackTarget), false, null, false, delegate
            {
                GenDraw.DrawWorldRadiusRing(this.Tile, PlayerWarbandManager.playerAttackRange);
            }, null, null);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action()
            {
                icon = TexCommand.Attack,
                defaultLabel = "WAW.OrderAttack".Translate(),
                defaultDesc = "WAW.OrderAttack.Desc".Translate(),
                action = () =>
                {
                    OderAttack();
                }
            };
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this._bandMembers,
 "vassalbandMembers", LookMode.Value, LookMode.Value, ref keyBuffers, ref valueBuffers);
        }
    }
}
