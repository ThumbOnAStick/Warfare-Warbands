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
using WarfareAndWarbands.Warband.WarbandRecruiting;
using static UnityEngine.GraphicsBuffer;

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
            Refresh();
        }

        public void Refresh()
        {
            bandMembers = new Dictionary<string, int>();
            var allPawnKinds = WarbandUtil.SoldierPawnKinds();
            foreach (var kind in allPawnKinds)
            {
                if (!bandMembers.ContainsKey(kind.defName))
                {
                    bandMembers.Add(kind.defName, 0);
                }

            }
            colorOverride = Color.white;
        }

        public static float GetCostOriginal(Dictionary<string, int> bandMembers)
        {
            float cost = 0;
            foreach (var val in bandMembers)
            {
                if (WarbandUtil.SoldierPawnKindsCache.Any(x => x.defName == val.Key))
                    cost += val.Value * WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == val.Key).combatPower;
            }
            return cost;
        }

        public int GetCostExtra(Dictionary<string, int> bandMembers)
        {
            return (int)Math.Max(GetCostNormal() - GetCostNormal(bandMembers), 0);
        }

        public float GetCostNormal(Dictionary<string, int> bandMembers)
        {
            return (int)GetCostOriginal(bandMembers) * WAWSettings.establishFeeMultiplier * .75f;
        }

        public float GetCostNormal()
        {
            return (int)GetCostOriginal(bandMembers) * WAWSettings.establishFeeMultiplier * .75f;
        }

        public int GetCostEstablishment()
        {
            return (int)GetCostOriginal(bandMembers) * WAWSettings.establishFeeMultiplier;
        }

        public int GetCostEstablishmentImmediate()
        {
            return (int)GetCostOriginal(bandMembers) * WAWSettings.establishFeeMultiplier * 2;
        }

        public bool ValidateCreation(Caravan caravan = null)
        {
            if (caravan == null && !CommsConsoleUtility.PlayerHasPoweredCommsConsole())
            {
                Messages.Message("WAW.NoComms".Translate(), MessageTypeDefOf.RejectInput);
                var label = "WAW.CaravanWarband".Translate();
                var desc = "WAW.CaravanWarband.Desc".Translate();
                Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent);
                Find.LetterStack.ReceiveLetter(letter);
                return false;
            }
            if (!bandMembers.Any(x => x.Value > 0))
            {
                Messages.Message("WAW.NoMembers".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (bandMembers.Sum(x => x.Value) < 5)
            {
                Messages.Message("WAW.WeakBand".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (!WarbandUtil.CanPlaceMoreWarbands())
            {
                Messages.Message("WAW.WarbandLeak".Translate(WAWSettings.maxPlayerWarband), MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }


        public void CreateWarbandWorldObject(Map currMap)
        {
            if (!ValidateCreation())
            {
                return;
            }
            this.currentMap = currMap;
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.CreateWarbandWorldObject), true, onUpdate: delegate
            {
                if (GenWorld.MouseTile() > 0)
                    GenDraw.DrawWorldRadiusRing(GenWorld.MouseTile(), Warband.playerAttackRange);
            });


        }

        public IEnumerable<FloatMenuOption> SelectWarbandWorldObjectOptions(GlobalTargetInfo target)
        {
            yield return new FloatMenuOption(
                "WAW.Default".Translate(GetCostEstablishment()),
                delegate
                {
                    if (!WarbandUtil.TryToSpendSilverFromColony(currentMap, GetCostEstablishment()))
                    {
                        return;
                    }
                    if (WarbandRecruitingUtil.SpawnRecruitingWarband(target))
                        SoundDefOf.ExecuteTrade.PlayOneShot(SoundInfo.OnCamera());

                });
            yield return new FloatMenuOption(
                "WAW.Immediate".Translate(GetCostEstablishmentImmediate()),
            delegate
            {
                if (!WarbandUtil.TryToSpendSilverFromColony(currentMap, GetCostEstablishmentImmediate()))
                {
                    return;
                }
                var warband = WarbandUtil.SpawnWarband(Faction.OfPlayer, target);
                warband.playerWarbandManager.colorOverride.SetColorOverride();
                SoundDefOf.ExecuteTrade.PlayOneShot(SoundInfo.OnCamera());
            });
        }




        public void SetNewWarBandMembers(Warband playerWarbandSite)
        {
            if (playerWarbandSite == null)
            {
                return;
            }
            if (!bandMembers.Any(x => x.Value > 0))
            {
                Messages.Message("WAW.emptyBand".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            int cost = GetCostExtra(playerWarbandSite.bandMembers);
            if (!WarbandUtil.TryToSpendSilverFromColony(currentMap, cost))
            {
                return;
            }
            if (cost > 0)
                SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            playerWarbandSite.bandMembers = new Dictionary<string, int>(bandMembers);
            playerWarbandSite.playerWarbandManager.colorOverride.SetColorOverride(this.colorOverride);
        }

        private bool CreateWarbandWorldObject(GlobalTargetInfo target)
        {
            if (target.WorldObject != null)
            {
                return false;
            }

            if (Find.World.Impassable(target.Tile))
            {
                return false;
            }

            if (currentMap == null)
            {  
                return false;
            }
            int cost = GetCostEstablishment();
            IEnumerable<FloatMenuOption> opts = SelectWarbandWorldObjectOptions(target);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
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
