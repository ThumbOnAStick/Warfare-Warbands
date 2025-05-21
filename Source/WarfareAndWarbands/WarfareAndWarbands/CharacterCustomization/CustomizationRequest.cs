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
        public Dictionary<string, string> thingDefsAndStyles;
        public Dictionary<ThingDef, ThingDef> itemAndStuff;
        public ThingDef weaponRequest;
        public ThingDef sideArm;
        private int combatPowerCache = 0;
        private List<ThingDef> itemCache;
        private List<ThingDef> stuffCache;
        private List<string> defNameCache;
        private List<string> styleCache;
        private XenotypeDef xenoTypeCache;

        public CustomizationRequest()
        {
            apparelRequests = new List<ThingDef>();
            itemAndStuff = new Dictionary<ThingDef, ThingDef>();
            thingDefsAndStyles = new Dictionary<string, string>();
            if (ModsConfig.BiotechActive)
                this.xenoTypeDefName = XenotypeDefOf.Baseliner.defName;
        }

        public CustomizationRequest(string name, string label)
        {
            this.defName = name;
            this.label = label;
            apparelRequests = new List<ThingDef>();
            itemAndStuff = new Dictionary<ThingDef, ThingDef>();
            thingDefsAndStyles = new Dictionary<string, string>();
        }

        public CustomizationRequest(CustomizationRequest other)
        {
            this.defName = Guid.NewGuid().ToString(); 
            this.label = other.label;
            this.xenoTypeDefName = other.xenoTypeDefName;
            this.alienDefName = other.alienDefName;
            this.apparelRequests = new List<ThingDef>(other.apparelRequests);
            this.thingDefsAndStyles = new Dictionary<string, string>(other.thingDefsAndStyles);
            this.itemAndStuff = new Dictionary<ThingDef, ThingDef>(other.itemAndStuff);
            this.weaponRequest = other.weaponRequest;
            this.sideArm = other.sideArm;
        }

        public int CombatPowerCache => combatPowerCache;

        public void CustomizePawn(ref Pawn p)
        {
            GenerateWeaponFor(ref p);
            GenerateSidearmFor(ref p);  
            GenerateApparelsFor(ref p);
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
            if (GameComponent_Customization.Instance.GeneratedKindDefs.Any(x => x.defName == defName))
            {
                PawnKindDef targetDef = GameComponent_Customization.Instance.GeneratedKindDefs.First(x => x.defName == defName);
                combatPowerCache = GetCombatPower();
                targetDef.combatPower = combatPowerCache;
                targetDef.label = label;
                if (HARActive())
                {
                    targetDef.SetAlienRace(this);
                }
            }
        }

        public int GetApperalValue()
        {
            return (int)GenerateApparels().Sum(x => x.MarketValue);
        }

        public int GetWeaponValue()
        {
            return weaponRequest == null ? 0 : (int)GenerateWeapon().MarketValue;
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

        public ThingWithComps GenerateWeaponItem(ThingDef def)
        {
            if (def == null)
            {
                return null;
            }
            var stuff = GetStuff(def);
            ThingWithComps weapon = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
            weapon.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, null);
            TryToStyleEquipment(ref weapon);
            return weapon;
        }

        public ThingWithComps GenerateSidearm()
        {
            return GenerateWeaponItem(sideArm);
        }

        public ThingWithComps GenerateWeapon()
        {
            return GenerateWeaponItem(weaponRequest);
        }

        void TryToStyleEquipment(ref Apparel equipment)
        {
            if (ModsConfig.IdeologyActive)
            {
                equipment.StyleDef = CustomizationUtil.GetStyle(this, equipment.def);
            }
        }

        void TryToStyleEquipment(ref ThingWithComps equipment)
        {
            if (ModsConfig.IdeologyActive)
            {
                equipment.StyleDef = CustomizationUtil.GetStyle(this, equipment.def);
            }
        }

        public ThingWithComps GenerateSidearmFor(ref Pawn p)
        {
            var weapon = GenerateSidearm();
            if ((weapon!=null))
               p.inventory.TryAddAndUnforbid(weapon);
            return weapon;
        }
        public ThingWithComps GenerateWeaponFor(ref Pawn p)
        {
            var weapon = GenerateWeapon();
            p.equipment?.DestroyAllEquipment();
            if (weapon != null)
                p.equipment?.AddEquipment(weapon);
            return weapon;
        }

        public List<ThingWithComps> GenerateApparelsFor(ref Pawn p)
        {
            List<ThingWithComps> result = new List<ThingWithComps>();
            p.apparel?.DestroyAll();
            foreach (ThingDef apparelRequest in apparelRequests)
            {
                ThingDef stuff = GetStuff(apparelRequest);
                Apparel apparel = (Apparel)ThingMaker.MakeThing(apparelRequest, stuff);
                apparel.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, null);
                TryToStyleEquipment(ref apparel);
                p.apparel.Wear(apparel);
                result.Add(apparel);
            }

            return result;
        }
        public List<ThingWithComps> GenerateApparels()
        {
            List<ThingWithComps> result = new List<ThingWithComps>();
            foreach (ThingDef apparelRequest in apparelRequests)
            {
                try
                {
                    ThingDef stuff = GetStuff(apparelRequest);
                    ThingWithComps apparel = (Apparel)ThingMaker.MakeThing(apparelRequest, stuff);
                    apparel.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, null);
                    TryToStyleEquipment(ref apparel);
                    result.Add(apparel);
                }
                catch (Exception e)
                {
                    Log.Error("WAW: Failed to generate apparel " + apparelRequest.defName + " " + e);

                }
            }
            return result;
        }



        public ThingDef GetStuff(ThingDef def)
        {
            if (!def.MadeFromStuff)
            {
                return null;
            }
            if (this.itemAndStuff == null)
            {
                this.itemAndStuff = new Dictionary<ThingDef, ThingDef>();
            }
            if (this.itemAndStuff.ContainsKey(def) && itemAndStuff[def] != null)
            {
                return this.itemAndStuff[def];
            }
            return GenStuff.DefaultStuffFor(def);

        }

        public void SetStuff(ThingDef def, ThingDef stuff)
        {
            if (this.itemAndStuff.ContainsKey(def))
            {
                itemAndStuff[def] = stuff;
            }
            else
            {
                itemAndStuff.Add(def, stuff);
            }
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

        public string GetUniqueLoadID()
        {
            return "CustomizationRequest_" + this.defName;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defName, "defName");
            Scribe_Values.Look(ref this.label, "label");
            Scribe_Values.Look(ref this.xenoTypeDefName, "xenoTypeDefName");
            Scribe_Values.Look(ref this.alienDefName, "alienDefName");
            Scribe_Collections.Look(ref this.apparelRequests, "apparelRequests", LookMode.Def);
            Scribe_Defs.Look(ref this.weaponRequest, "weaponRequest");
            Scribe_Defs.Look(ref this.sideArm, "sideArm");
            Scribe_Collections.Look<string, string>(ref this.thingDefsAndStyles,
"thingDefsAndStyles", LookMode.Value, LookMode.Value, ref defNameCache, ref styleCache, false);
            Scribe_Collections.Look<ThingDef, ThingDef>(ref this.itemAndStuff,
"itemAndStuff", LookMode.Def, LookMode.Def, ref itemCache, ref stuffCache, false);
            if(thingDefsAndStyles == null)
            {
                thingDefsAndStyles = new Dictionary<string, string>();
            }
        }
    }

}
