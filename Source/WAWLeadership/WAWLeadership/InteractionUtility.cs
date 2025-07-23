﻿using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;
using WAWLeadership.LeadershipAttributes;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.UI;

namespace WAWLeadership
{
    public static class InteractionUtility
    {

        static int tileTemp = 1;

        public static void TryToInteract(WorldObject o, ref bool usedSkill, Pawn leader, Warband warband = null)
        {
            switch (o.GetType().Name)
            {
                
                case "PeaceTalks":
                    
                    TryToInteractWithPeaceTalks(o, ref usedSkill, leader);
                    break;

                case "Settlement":
                    TryToInteractWithSettlements(o, ref usedSkill, leader);
                    break;
                case "Warband":
                    DoInteractWithWarbands(ref usedSkill, leader, (Warband)o);
                    break;
                case "DestroyedSettlement":
                    DoInteractWithDestroyedSettlement(leader, o);
                    break;
                default:
                    TryToInteractWithSite(o, ref usedSkill, leader, warband);
                    break;
            }
        }



        static bool ValidateLeader<T>(Pawn leader, int requiredLevel) where T : LeadershipAttribute
        {
            if (leader == null)
            {
                return true;
            }
            var leadership = leader.TryGetComp<CompLeadership>();
            if (leadership == null)
            {
                return false;
            }

            bool invalidAttribute = leadership.Leadership.AttributeSet.GetAttribute<T>().GetLevel() < requiredLevel;
            if (invalidAttribute)
            {
                string attributeTooLow = "WAW.InsufficientAttribute".Translate
                    (leadership.Leadership.AttributeSet.GetAttribute<T>().GetLabel(), requiredLevel);
                Message message = new Message(attributeTooLow, MessageTypeDefOf.RejectInput);
                Messages.Message(message);
                return false;
            }
            return true;
        }


        static void TryToInteractWithSite(WorldObject o, ref bool usedSkill, Pawn leader, Warband warband)
        {
            if ((o as Site) != null)
            {
                var site = (Site)o;
                    DoInteractWithWorkingSite(ref usedSkill, leader, site);
            }
        }
   

        static List<Thing> GetLoots(Predicate<SitePart> predicate, Site site)
        {
            var part = site.parts.Find(predicate);
            var lootsDefAndCount = part.lootThings;
            List<Thing> loots = new List<Thing>();
            foreach (var ele in lootsDefAndCount)
            {
                Thing thing = ThingMaker.MakeThing(ele.ThingDef);
                thing.stackCount = ele.Count;
                loots.Add(thing);
            }
            return loots;
        }

        static ActiveTransporterInfo MakeDropPodInfo(List<Thing> loots)
        {
            ActiveTransporterInfo activeDropPodInfo = new ActiveTransporterInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(loots, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            return activeDropPodInfo;   

        }

        static void DropAtHome(ActiveTransporterInfo activeDropPodInfo)
        {
            Map playerMap = Find.AnyPlayerHomeMap;
            if (playerMap == null)
            {
                return;
            }
            Current.Game.CurrentMap = playerMap;
            var cell = CellFinder.StandableCellNear(playerMap.Center, playerMap, 50);
            if (cell == IntVec3.Invalid)
            {
                cell = DropCellFinder.RandomDropSpot(playerMap);
            }
            CameraJumper.TryJump(cell, playerMap);
            DropPodUtility.MakeDropPodAt(cell, playerMap, activeDropPodInfo);
        }

        static void SendCompleteTradingMessage(Pawn leader)
        {
            string name = leader != null ? leader.NameShortColored.ToString() : "";
            Messages.Message("WAW.WorksiteTradeComplete".Translate(name), MessageTypeDefOf.PositiveEvent);
        }

        #region Peace Talks
        static void TryToInteractWithPeaceTalks(WorldObject o, ref bool usedSkill, Pawn leader)
        {
            usedSkill = ValidateLeader<Attribute_Diplomacy>(leader, 3);
            if (usedSkill)
            {
                InteractWithPeaceTalk(o);
                QuestUtility.SendQuestTargetSignals(o.questTags, "Resolved", o.Named("SUBJECT"));
                GameComponent_WAW.Instance.OnLeaderAbilityUsed(leader);
            }
        }

        static void InteractWithPeaceTalk(WorldObject o)
        {
            FactionRelationKind playerRelationKind = o.Faction.PlayerRelationKind;
            int randomInRange = DiplomacyTuning.Goodwill_PeaceTalksSuccessRange.RandomInRange;
            Faction.OfPlayer.TryAffectGoodwillWith(o.Faction, randomInRange, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
            TaggedString ltterText = GetLetterTextPeaceTalk("LetterPeaceTalks_Success".Translate(o.Faction.NameColored, randomInRange), playerRelationKind, o.Faction);
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Success".Translate(), ltterText, LetterDefOf.PositiveEvent);
            o.Destroy();
        }

        static TaggedString GetLetterTextPeaceTalk(string baseText, FactionRelationKind previousRelationKind, Faction f)
        {
            TaggedString text = baseText;
            f.TryAppendRelationKindChangedInfo(ref text, previousRelationKind, f.PlayerRelationKind);
            return text;
        }
        #endregion


        #region Settlements
        static void TryToInteractWithSettlements(WorldObject o, ref bool usedSkill, Pawn leader)
        {
            usedSkill = ValidateLeader<Attribute_Diplomacy>(leader, 2) && !o.Faction.def.PermanentlyHostileTo(FactionDefOf.PlayerColony);
            if (usedSkill)
            {
                InteractWithSettlement(o);
                GameComponent_WAW.Instance.OnLeaderAbilityUsed(leader);
            }
        }

        static void InteractWithSettlement(WorldObject o)
        {
            FactionRelationKind playerRelationKind = o.Faction.PlayerRelationKind;
            int goodWill = DiplomacyTuning.Goodwill_MemberExitedMapHealthy;
            Faction.OfPlayer.TryAffectGoodwillWith(o.Faction, goodWill, canSendMessage: false);
            TaggedString ltterText = "WAW.GoodWillGained.Desc".Translate(o.Faction.NameColored, goodWill);
            Find.LetterStack.ReceiveLetter("WAW.GoodWillGained".Translate(), ltterText, LetterDefOf.PositiveEvent);
        }
        #endregion

        #region DestroyedSettlements
        static void DoInteractWithDestroyedSettlement(Pawn leader, WorldObject o)
        {
            if (!ValidateLeader<Attribute_Engineering>(leader, 3))
            {
                return;
            }
            if(o == null)
            {
                return;
            }
            SetTileTemp(o.Tile);
            TaggedString text = "WAW.ConfirmBuildTown".Translate(WAWSettings.townConstructionCost, WAWSettings.townConstructionDuration);
            WindowStack windowStack = Find.WindowStack;
            windowStack.Add(Dialog_MessageBox.CreateConfirmation(text, ConfirmedAct, true, null, WindowLayer.Dialog));
        }

        static void SetTileTemp(int val)
        {
            tileTemp = val;
        }

        static void ConfirmedAct()
        {
            if (CheckFund())
            {
                GenerateTownConstructionAround(tileTemp);
                UseSkill();
                SendSuccessLetter();
            }
        }

        static bool CheckFund()
        {
            if (!WarbandUtil.TryToSpendSilverFromColonyOrBank(null, WAWSettings.townConstructionCost))
            {
                return false;
            }
            return true;
        }

        static void UseSkill()
        {
            LeadershipUtility.LeaderCompCache?.ResetLastUsedTick();
        }

        static void SendSuccessLetter()
        {
            Letter l = LetterMaker.MakeLetter("WAW.TownConstructionPlanned".Translate(), "WAW.TownConstructionPlanned.Desc".Translate(), LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(l);
        }

        static WorldObject GenerateTownConstructionAround(int tile)
        {
            TileFinder.TryFindPassableTileWithTraversalDistance(tile, 5, 10, out PlanetTile randomTile);
            return GenerateTownConstruction(randomTile);
        }

        static WorldObject GenerateTownConstruction(int tile)
        {
            var result = WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_SettlementConstruction);
            result.Tile = tile;
            Find.WorldObjects.Add(result);
            return result;
        }
        #endregion

        #region WorkingSite
        static void DoInteractWithWorkingSite(ref bool usedSkill, Pawn leader, Site site)
        {
            if (usedSkill)
            {
                return;
            }
            bool predicateWorkingSite(SitePart x) => x.def.defName.Contains("WorkSite_");
            if (site.parts.Any(predicateWorkingSite) && ValidateLeader<Attribute_Economy>(leader, 2))
            {
                DropAtHome(MakeDropPodInfo(GetLoots(predicateWorkingSite, site)));
                site.Destroy();
                SendCompleteTradingMessage(leader);
                usedSkill = true;
            }
        }
        #endregion

        #region Warbands
        static void DoInteractWithWarbands(ref bool usedSkill, Pawn leader, Warband warband)
        {
            if (usedSkill)
            {
                return;
            }
            if (warband.Faction == Faction.OfPlayer && ValidateLeader<Attribute_Medic>(leader, 3))
            {
                warband.playerWarbandManager.injuriesManager.RecoverAll();
                Messages.Message("WAW.AllRecovered".Translate(leader.NameShortColored), MessageTypeDefOf.PositiveEvent);
                usedSkill = true;
            }
        }
        #endregion
    }
}
