using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.CharacterCustomization.Compatibility;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.VassalWarband;

namespace WarfareAndWarbands.Warband.Mercenary
{
    public static class MercenaryUtil
    {
        public static List<Pawn> GenerateWarbandPawns(Warband warband)
        {
            var members = warband.Faction == Faction.OfPlayer ? warband.playerWarbandManager.injuriesManager.GetActiveMembers(warband.bandMembers) : warband.bandMembers;
            //member table
            List<Pawn> list = GeneratePawnsFromTable(warband, members);
 
            //leader
            if (warband.Faction == Faction.OfPlayer &&
                warband.playerWarbandManager.leader != null &&
                warband.playerWarbandManager.leader.spawnLeader &&
                warband.playerWarbandManager.leader.IsLeaderAvailable() &&
                !warband.playerWarbandManager.leader.Leader.Spawned)
            {
                var leader = warband.playerWarbandManager.leader.Leader;
                if (SetMercenaryComp(leader, warband, "", warband.Faction))
                    list.Add(leader);
                leader.health.hediffSet.hediffs.RemoveAll(x => x.def.tendable && x.def.isBad);
                leader.health.hediffSet.hediffs.ForEach(x => leader.health.Notify_HediffChanged(x));
                leader.needs.SetInitialLevels();
                if (CEActive())
                {
                    CE.GenerateAmmoFor(leader);
                }
            }
            //extra pawns from upgrade
            if (warband.playerWarbandManager.upgradeHolder.HasUpgrade)
            {
                list.AddRange(warband.playerWarbandManager.upgradeHolder.SelectedUpgrade.ExtraPawns(warband));
            }
            return list;
        }

        public static List<Pawn> GeneratePawnsFromTable(Warband warband, Dictionary<string, int> table)
        {
            List<Pawn> list = new List<Pawn>();
            foreach (var ele in table)
            {
                for (int i = 0; i < ele.Value; i++)
                {
                    Pawn pawn;
                    try
                    {
                        pawn = warband.Faction == Faction.OfPlayer ? GenerateWarbandPawnForPlayer(warband, ele.Key, out bool succeed) : GenerateWarbandPawnForNPC(warband, ele.Key);
                    }
                    catch
                    {
                        Log.Warning($"Tried to generate {ele.Value} but failed.");
                        continue;
                    }
                    if (pawn == null)
                    {
                        continue;
                    }
                    if (CEActive())
                    {
                        CE.GenerateAmmoFor(pawn);
                    }
                    if (SetMercenaryComp(pawn, warband, ele.Key, warband.Faction))
                        list.Add(pawn);
                }
            }
            return list;
        }

        public static List<Pawn> GenerateVassalPawnsFromTable(this WorldObject_VassalWarband warband)
        {
            List<Pawn> list = new List<Pawn>();
            foreach (var ele in warband.BandMembers)
            {
                for (int i = 0; i < ele.Value; i++)
                {
                    Pawn pawn;
                    try
                    {
                        pawn = GenerateVassalPawn(warband, ele.Key, out bool succeed);
                        if (pawn != null && GameComponent_Customization.Instance.CustomizationRequests.Any(x => x.defName == ele.Key))
                        {
                            var targetRequest = GameComponent_Customization.Instance.CustomizationRequests.First(x => x.defName == ele.Key);
                            targetRequest?.CustomizePawn(ref pawn);
                        }
                    }
                    catch
                    {
                        Log.Warning($"Tried to generate {ele.Value} but failed.");
                        continue;
                    }
                    if (pawn == null)
                    {
                        continue;
                    }
                    if (CEActive())
                    {
                        CE.GenerateAmmoFor(pawn);
                    }
                    if (SetMercenaryComp(pawn, ele.Key, Faction.OfPlayer))
                        list.Add(pawn);
                }
            }
            return list;
        }

        static bool CEActive()
        {
            return ModsConfig.IsActive("CETeam.CombatExtended");
        }

        static PawnGenerationRequest GetRequestFromWarband(Warband warband, string kindDefName, out bool succeed)
        {
            PawnKindDef kindDef = WarbandUtil.SoldierPawnKindsCache.Where(x => x.combatPower > 50).RandomElement();
            bool isCustom = false;
            if (GameComponent_Customization.Instance.CustomizationRequests.Any(x => x.defName == kindDefName))
            {
                kindDef = GameComponent_Customization.Instance.GeneratedKindDefs.First(x => x.defName == kindDefName);
                isCustom = true;
            }
            else if (WarbandUtil.SoldierPawnKindsCache.Any(x => x.defName == kindDefName))
            {
                kindDef = WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == kindDefName);
            }
            Faction pawnFaction = kindDef.defaultFactionType != null ? Find.FactionManager.FirstFactionOfDef(kindDef.defaultFactionType) : warband.Faction;
            if (warband.PawnKindFactionType != null && !isCustom)
            {
                var f = Find.FactionManager.FirstFactionOfDef(warband.PawnKindFactionType);
                if (f != null)
                {
                    pawnFaction = f;
                }
            }

            if(warband.Faction != Faction.OfPlayer)
            {
                pawnFaction = warband.Faction;
            }

            try
            {
                succeed = true;
                PawnGenerationRequest request = new PawnGenerationRequest(kindDef, pawnFaction, mustBeCapableOfViolence: true, developmentalStages: DevelopmentalStage.Adult);
                return request;
            }
            catch (Exception e)
            {
                succeed = false;
                warband.SetFactionType(null);
                Log.Error("WAW: Failed to generate pawn generation request: " + e);
                return new PawnGenerationRequest(kindDef, warband.Faction);
            }
        }

        static PawnGenerationRequest GetRequestFromVassal(WorldObject_VassalWarband warband, string kindDefName, out bool succeed)
        {
            PawnKindDef kindDef = WarbandUtil.SoldierPawnKindsCache.Where(x => x.combatPower > 50).RandomElement();
            if (GameComponent_Customization.Instance.CustomizationRequests.Any(x => x.defName == kindDefName))
            {
                kindDef = GameComponent_Customization.Instance.GeneratedKindDefs.First(x => x.defName == kindDefName);
            }
            else if (WarbandUtil.SoldierPawnKindsCache.Any(x => x.defName == kindDefName))
            {
                kindDef = WarbandUtil.SoldierPawnKindsCache.First(x => x.defName == kindDefName);
            }
            Faction faction = kindDef.defaultFactionType != null ? Find.FactionManager.FirstFactionOfDef(kindDef.defaultFactionType) : warband.Faction;

            try
            {
                succeed = true;
                PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, mustBeCapableOfViolence: true, developmentalStages: DevelopmentalStage.Adult);
                return request;
            }
            catch (Exception e)
            {
                succeed = false;
                Log.Message("WAW: Failed to generate pawn generation request: " + e);
                return new PawnGenerationRequest(kindDef, warband.Faction);
            }
        }

        static Pawn GenerateWarbandPawnForPlayer(Warband warband, string kindDefName, out bool succeed)
        {
            Pawn pawn;
            pawn = GenerateRequestedPawn(warband, kindDefName, out succeed);
            if(!succeed)
            {
                return null;
            }

            if (GameComponent_Customization.Instance.CustomizationRequests.Any(x => x.defName == kindDefName))
            {
                var targetRequest = GameComponent_Customization.Instance.CustomizationRequests.First(x => x.defName == kindDefName);
                targetRequest?.CustomizePawn(ref pawn);
            }

            var equipments = pawn.equipment.AllEquipmentListForReading;
            if (equipments != null)
                foreach (var equipment in equipments)
                {
                    if (equipment.def.IsWeapon && equipment.TryGetComp<CompBiocodable>() != null)
                    {
                        equipment.TryGetComp<CompBiocodable>().CodeFor(pawn);
                        equipment.TryGetComp<CompQuality>()?.SetQuality(warband.playerWarbandManager.upgradeHolder.GearQuality, ArtGenerationContext.Outsider);
                    }
                }
            TryToSetSkillFor(pawn, warband);
            if (pawn.apparel != null)
            {
                foreach(var apparel in pawn.apparel.WornApparel)
                {
                    apparel.TryGetComp<CompQuality>()?.SetQuality(warband.playerWarbandManager.upgradeHolder.GearQuality, ArtGenerationContext.Outsider);
                }
                pawn.apparel.SetColor(warband.playerWarbandManager.colorOverride.GetColorOverride());
                pawn.apparel.LockAll();
            }

            for (int i = 0; i < 24; i++)
            {
                pawn.timetable.SetAssignment(i, TimeAssignmentDefOf.Anything);
            }
            if (!pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            }
            return pawn;
        }

        static void TryToSetSkillFor(Pawn p, Warband warband)
        {
            if(warband == null)
            {
                return;
            }
            if (!warband.HasLeader())
            {
                return;
            }
            var skillSet = warband.playerWarbandManager.skillBonus.DefsAndBonus;
            if (skillSet.Count < 1)
            {
                return;
            }
            foreach (var ele in skillSet)
            {
                bool predicate(SkillRecord x) => x.def == ele.Key;
                if (p.skills.skills.Any(predicate))
                {
                    p.skills.skills.Find(predicate).levelInt += ele.Value;
                }
            }
        }

        static void SetColor(this Pawn_ApparelTracker apparel, Color color)
        {
            foreach (var item in apparel.WornApparel)
            {
                try
                {
                    if (item.HasComp<CompColorable>())
                        item.SetColor(color);
                }
                catch (Exception ex)
                {
                    Log.Warning("WAW: Failed to set color for apparel " + item + " " + ex);
                    continue;
                }
            }
        }

        static Pawn GenerateWarbandPawnForNPC(Warband warband, string kindefName)
        {
            Pawn pawn = GenerateRequestedPawn(warband, kindefName, out bool succeed);
            return pawn;
        }

        static Pawn GenerateRequestedPawn(Warband warband, string kindefName, out bool succeed)
        {
            PawnGenerationRequest request = GetRequestFromWarband(warband, kindefName, out succeed);
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);

            }
            catch (Exception ex)
            {
                Log.Message("WAW: Failed to generate pawn !" + ex);
                return null;
            }
            if (pawn.Faction != warband.Faction)
                pawn.SetFaction(warband.Faction);
            return pawn;
        }

        static Pawn GenerateVassalPawn(WorldObject_VassalWarband vassal, string kindefName, out bool succeed)
        {
            PawnGenerationRequest request = GetRequestFromVassal(vassal, kindefName, out succeed);
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);

            }
            catch (Exception ex)
            {
                Log.Message("WAW: Failed to generate pawn !" + ex);
                return null;
            }
            if (pawn.Faction != vassal.Faction)
                pawn.SetFaction(vassal.Faction);
            return pawn;
        }

        public static bool SetMercenaryComp(Pawn pawn, Warband warband, string pawnKindName, Faction faction = null)
        {
            CompMercenary comp = pawn.TryGetComp<CompMercenary>();
            if (comp == null)
            {
                return false;
            }
            comp.ResetAll();
            comp.ServesPlayerFaction = warband.Faction == Faction.OfPlayer;
            if (pawn.Faction != faction)
                pawn.SetFaction(faction);
            comp.SetWarband(warband);
            comp.SetServingFaction(warband.Faction);
            comp.SetRetreat(false);
            comp.ResetDuration();
            if (faction != null)
            {
                comp.SetServingFaction(faction);
            }
            comp.SetPawnKindName(pawnKindName);
            return true;
        }

        public static bool SetMercenaryComp(Pawn pawn, string pawnKindName, Faction faction)
        {
            CompMercenary comp = pawn.TryGetComp<CompMercenary>();
            if (comp == null)
            {
                return false;
            }
            comp.ResetAll();
            comp.ServesPlayerFaction = faction == Faction.OfPlayer;
            if (pawn.Faction != faction)
                pawn.SetFaction(faction);
            comp.SetRetreat(false);
            comp.ResetDuration();
            if (faction != null)
            {
                comp.SetServingFaction(faction);
            }
            comp.SetPawnKindName(pawnKindName);
            return true;
        }
    }
}
