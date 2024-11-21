using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace WarfareAndWarbands.Warband
{
    public class PlayerWarbandArrangement : IExposable
    {
        List<string> stringBuffers = new List<string>();
        List<int> intBuffers = new List<int>();

        public Dictionary<string, int> bandMembers;

        public PlayerWarbandArrangement()
        {
            bandMembers = new Dictionary<string, int>();
            var allPawnKinds = WarbandUtil.SoldierPawnKinds();
            foreach (var kind in allPawnKinds)
            {
                bandMembers.Add(kind.defName, 0);
            }
        }
       

        public float GetCost()
        {
            float cost = 0;
            foreach (var val in bandMembers)
            {
                cost += val.Value * WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == val.Key).combatPower;
            }
            return cost;
        }

        public void CreateWorldObject()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true);
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if(target.WorldObject != null)
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
        }


    }
}
