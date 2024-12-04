using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization.Compatibility;
using static RimWorld.FleshTypeDef;

namespace WarfareAndWarbands.CharacterCustomization
{
    public class CustomizationRequest : IExposable
    {
        public string defName;
        public string label;
        public string xenoTypeDefName;
        public string alienDefName;
        public List<ThingDef> apparelRequests;
        public ThingDef weaponRequest;

        private XenotypeDef xenoTypeCache;

        public CustomizationRequest()
        {
            apparelRequests = new List<ThingDef>();
            if (ModsConfig.BiotechActive)
                this.xenoTypeDefName = XenotypeDefOf.Baseliner.defName;
        }

        public CustomizationRequest(string name, string label)
        {
            this.defName = name;
            this.label = label;
            apparelRequests = new List<ThingDef>();
        }

        public void CustomizePawn(ref Pawn p)
        {
            GenerateWeapon(ref p);
            GenerateApparels(ref p);
            SetXenoForPawn(ref p);
        }

        public int GetCombatPower()
        {
            return (int)CombatPowerCurve().Evaluate((GetMarketValue() * 0.1f));
            
        }

        bool HARActive()
        {
            return ModsConfig.IsActive("erdelf.HumanoidAlienRaces");
        }

        public int GetMarketValue()
        {
            int apparelValue = GetApperalValue();
            int weaponValue = GetWeaponValue();
            float raceOffset = 1;
            if (HARActive())
            {
                raceOffset = this.GetAlienRaceValueOffset();
            }
            return (int)((apparelValue + weaponValue) * raceOffset);
        }

        public void UpdatePawnKindDef()
        {
            if (GameComponent_Customization.Instance.generatedKindDefs.Any(x => x.defName == defName))
            {
                PawnKindDef targetDef = GameComponent_Customization.Instance.generatedKindDefs.First(x => x.defName == defName);
                targetDef.combatPower = GetCombatPower();
                targetDef.label = label;
                if (HARActive())
                {
                    targetDef.SetAlienRace(this);
                }
            }
        }

        public int GetApperalValue()
        {
            return apparelRequests.Sum(GetThingValue);
        }

        public int GetWeaponValue()
        {
            return weaponRequest == null ? 0 : (int)weaponRequest.BaseMarketValue;
        }

        public int GetThingValue(ThingDef def)
        {
            return (int)def.BaseMarketValue;
        }

        public SimpleCurve CombatPowerCurve()
        {
            SimpleCurve curve = new SimpleCurve();
            List<CurvePoint> points = new List<CurvePoint>()
            {
                new CurvePoint(0,50),
                new CurvePoint(500, 300),
                new CurvePoint(1000, 400),
                new CurvePoint(2000, 700)
            };
            curve.SetPoints(points);
            return curve;
        }

        public ThingWithComps GenerateWeapon(ref Pawn p)
        {
           if(this.weaponRequest == null)
            {
                return null;
            }
            ThingWithComps weapon = (ThingWithComps)ThingMaker.MakeThing(weaponRequest);
            weapon.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, null);
            p.equipment?.DestroyAllEquipment();
            p.equipment?.AddEquipment(weapon);
            if (ModsConfig.IsActive("CETeam.CombatExtended"))
            {
                CE.GenerateAmmoFor(p);
            }
            return weapon;
        }


        public List<ThingWithComps> GenerateApparels(ref Pawn p)
        {
            List<ThingWithComps> result = new List<ThingWithComps>();
            p.apparel?.DestroyAll();
            foreach (ThingDef apparelRequest in apparelRequests)
            {
                ThingDef stuff = null;
                if (apparelRequest.MadeFromStuff)
                {
                    stuff = GenStuff.DefaultStuffFor(apparelRequest);
                }
                Apparel apparel = (Apparel)ThingMaker.MakeThing(apparelRequest, stuff);
                apparel.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, null);
                p.apparel.Wear(apparel);
                result.Add(apparel);
            }

            return result;
        }

        public XenotypeDef TryGetXeno()
        {
            bool anyXeno = DefDatabase<XenotypeDef>.AllDefs.Any(x => x.defName == this.xenoTypeDefName);
            if (!anyXeno)
            {
                return XenotypeDefOf.Baseliner;
            }
            
            return xenoTypeCache ?? DefDatabase<XenotypeDef>.AllDefs.First(x => x.defName == this.xenoTypeDefName);
        }

        public void SetXeno(XenotypeDef xeno)
        {
            this.xenoTypeDefName = xeno.defName;
            this.xenoTypeCache = xeno;
        }

        public void SetAlienRace(string alienRaceName)
        {
            this.alienDefName = alienRaceName;
        }

        public void SetXenoForPawn(ref Pawn p)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            var xeno = TryGetXeno();
            if (xeno != null)
            {
                p.genes?.SetXenotype(xeno);
            }
        }

        public void AddApperal(string apparelName)
        {
            ThingDef apparelDef = DefDatabase<ThingDef>.GetNamed(apparelName);
            apparelRequests.Add(apparelDef);
        }
        public void SetWeapon(string weaponName)
        {
            ThingDef weaponDef = DefDatabase<ThingDef>.GetNamed(weaponName);
            weaponRequest = weaponDef;
        }
    

        public string GetUniqueLoadID()
        {
            return "CustomizationRequest_" + this.defName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defName, "defName");
            Scribe_Values.Look(ref this.label, "label");
            Scribe_Values.Look(ref this.xenoTypeDefName, "xenoTypeDefName");

            Scribe_Collections.Look(ref this.apparelRequests, "apparelRequests", LookMode.Def);
            Scribe_Defs.Look(ref this.weaponRequest, "weaponRequest");

        }
    }

}
