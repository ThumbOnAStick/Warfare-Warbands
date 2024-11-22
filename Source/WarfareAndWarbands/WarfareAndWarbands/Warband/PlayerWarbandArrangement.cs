using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace WarfareAndWarbands.Warband
{
    public class PlayerWarbandArrangement : IExposable
    {
        List<string> stringBuffers = new List<string>();
        List<int> intBuffers = new List<int>();
        public Map currentMap;
        public Dictionary<string, int> bandMembers;
        public TechLevel techLevel = TechLevel.Industrial;

        public PlayerWarbandArrangement()
        {
            bandMembers = new Dictionary<string, int>();
            var allPawnKinds = WarbandUtil.SoldierPawnKinds();
            foreach (var kind in allPawnKinds)
            {
                bandMembers.Add(kind.defName, 0);
            }
            techLevel = TechLevel.Industrial;
        }

        public static float GetCost(Dictionary<string, int> bandMembers)
        {
            float cost = 0;
            foreach (var val in bandMembers)
            {
                cost += val.Value * WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == val.Key).combatPower;
            }
            return cost;
        }

        public int GetCostEstablishment()
        {
            return (int)GetCost(bandMembers) * WAWSettings.establishFeeMultiplier;
        }

        public void CreateWorldObject(Map currMap)
        {
            this.currentMap = currMap;
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, onUpdate: delegate
            {
                if (GenWorld.MouseTile() > 0)
                    GenDraw.DrawWorldRadiusRing(GenWorld.MouseTile(), Warband.playerAttackRange);
            });
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if (target.WorldObject != null)
            {
                return false;
            }
            if (currentMap == null)
            {
                Log.Error("Invalid map!");
                Log.TryOpenLogWindow();
                return false;
            }
            int cost = GetCostEstablishment();
            if (!WarbandUtil.TryToSpendSilver(currentMap, cost))
            {
                return false;
            }
     
            WarbandUtil.SpawnWarband(Faction.OfPlayer, target);
            SoundDefOf.ExecuteTrade.PlayOneShot(SoundInfo.OnCamera());
            return true;
        }

        

        public void ExposeData()
        {
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
    "bandMembers", LookMode.Value, LookMode.Value, ref stringBuffers, ref intBuffers);
            Scribe_References.Look<Map>(ref this.currentMap, "currentMap");
            Scribe_Values.Look(ref this.techLevel, "techLevel");

        }


    }
}
