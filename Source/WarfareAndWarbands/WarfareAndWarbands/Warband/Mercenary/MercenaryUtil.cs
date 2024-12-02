﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband.Mercenary
{
    public static class MercenaryUtil
    {
        public static List<Pawn> GenerateWarbandPawns(Warband warband)
        {
            List<Pawn> list = new List<Pawn>();
            foreach (var ele in warband.bandMembers)
            {
                for (int i = 0; i < ele.Value; i++)
                {
                    try
                    {
                        GenerateWarbandPawnForPlayer(warband, ele.Key);
                    }
                    catch (Exception e)
                    {
                        Log.Message($"Error while trying to generate Player warband:{e.StackTrace}");
                    }
                    Pawn pawn = warband.Faction == Faction.OfPlayer ? GenerateWarbandPawnForPlayer(warband, ele.Key) : GenerateWarbandPawnForNPC(warband, ele.Key);
                    if (SetMercenaryComp(pawn, warband, ele.Key))
                        list.Add(pawn);
                }

            }
            return list;
        }

        static PawnGenerationRequest GetRequest(Warband warband, string kindDefName)
        {
            PawnKindDef kindDef = PawnKindDefOf.Pirate;
            if (GameComponent_Customization.Instance.customizationRequests.Any(x => x.defName == kindDefName))
            {
                kindDef = GameComponent_Customization.Instance.generatedKindDefs.First(x => x.defName == kindDefName);
            }
            if (WarbandUtil.SoldierPawnKindsCache.Any(x => x.defName == kindDefName))
            {
                kindDef = WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == kindDefName);
            }
            Faction faction = kindDef.defaultFactionType != null ? Find.FactionManager.FirstFactionOfDef(kindDef.defaultFactionType) : warband.Faction;
            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, mustBeCapableOfViolence: true);
            return request;
        }

        static Pawn GenerateWarbandPawnForPlayer(Warband warband, string kindDefName)
        {

            Pawn pawn = GenerateRequestedPawn(warband, kindDefName);
            if (GameComponent_Customization.Instance.customizationRequests.Any(x => x.defName == kindDefName))
            {
                var targetRequest = GameComponent_Customization.Instance.customizationRequests.First(x => x.defName == kindDefName);
                targetRequest?.CustomizePawn(ref pawn);
            }

            var equipments = pawn.equipment.AllEquipmentListForReading;
            if (equipments != null)
                foreach (var equipment in equipments)
                {
                    if (equipment.def.IsWeapon && equipment.TryGetComp<CompBiocodable>() != null)
                    {
                        equipment.TryGetComp<CompBiocodable>().CodeFor(pawn);
                    }
                }

            pawn.apparel?.SetColor(warband.playerWarbandManager.colorOverride.GetColorOverride());
            pawn.apparel?.LockAll();
            return pawn;
        }

        static void SetColor(this Pawn_ApparelTracker apparel, Color color)
        {
            foreach (var item in apparel.WornApparel)
            {
                item.SetColor(color);
            }
        }

        static Pawn GenerateWarbandPawnForNPC(Warband warband, string kindefName)
        {
            Pawn pawn = GenerateRequestedPawn(warband, kindefName);
            return pawn;
        }

        static Pawn GenerateRequestedPawn(Warband warband, string kindefName)
        {
            PawnGenerationRequest request = GetRequest(warband, kindefName);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            pawn.SetFaction(warband.Faction);
            return pawn;
        }


        public static bool SetMercenaryComp(Pawn pawn, Warband warband, string pawnKindName)
        {
            CompMercenary comp = pawn.TryGetComp<CompMercenary>();
            if (comp == null)
            {
                return false;
            }
            if (warband.Faction == Faction.OfPlayer)
                comp.ServesPlayerFaction = true;
            comp.SetWarband(warband);
            comp.SetServingFaction(warband.Faction);
            comp.SetPawnKindName(pawnKindName);
            return true;
        }

    }
}
