using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Verse;
using Verse.Noise;
using Verse.Sound;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    public class PlayerWarbandArrangement : IExposable
    {
        List<string> stringBuffers = new List<string>();
        List<int> intBuffers = new List<int>();
        public Map currentMap;
        public Dictionary<string, int> bandMembers;
        public TechLevel techLevel = TechLevel.Industrial;
        public Color colorOverride;

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

        public int GetCostExtra(Dictionary<string, int> bandMembers)
        {
            return (int)Math.Max(GetCostNormal() - GetCost(bandMembers), 0);
        }

        public float GetCostNormal()
        {
            return GetCost(bandMembers);
        }

        public int GetCostEstablishment()
        {
            return (int)GetCost(bandMembers) * WAWSettings.establishFeeMultiplier;
        }

        public void CreatWarbandeWorldObject(Map currMap)
        {
            this.currentMap = currMap;
            if (!bandMembers.Any(x => x.Value > 0))
            {
                Messages.Message("WAW.emptyBand".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, onUpdate: delegate
            {
                if (GenWorld.MouseTile() > 0)
                    GenDraw.DrawWorldRadiusRing(GenWorld.MouseTile(), Warband.playerAttackRange);
            });
        }

        public void SetNewWarBandMembers(Warband playerWarbandSite)
        {
            if (playerWarbandSite == null)
            {
                return;
            }
            if(!bandMembers.Any(x => x.Value > 0))
            {
                Messages.Message("WAW.emptyBand".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            int cost = GetCostExtra(playerWarbandSite.bandMembers);
            if (!WarbandUtil.TryToSpendSilver(currentMap, cost))
            {
                return;
            }
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            playerWarbandSite.bandMembers = new Dictionary<string, int>(bandMembers);
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
            Scribe_Values.Look(ref this.colorOverride, "colorOverride", Color.white);
            // cache player warband arrangement
            foreach (var ele in WarbandUtil.SoldierPawnKindsCache)
            {
                if (!bandMembers.ContainsKey(ele.defName))
                {
                    bandMembers.Add(ele.defName, 0);
                }
            }

        }


    }
}
