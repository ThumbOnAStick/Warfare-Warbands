using RimWorld.Planet;
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

namespace WAWLeadership
{
    public static class InteractionUtility
    {

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

        static void DoInteractWithWorkingSite(ref bool usedSkill, Pawn leader, Site site)
        {
            if (usedSkill)
            {
                return;
            }
            bool predicateWorkingSite(SitePart x) => x.def.defName.Contains("WorkSite_");
            if (site.parts.Any(predicateWorkingSite))
            {
                DropAtHome(MakeDropPodInfo(GetLoots(predicateWorkingSite, site)));
                site.Destroy();
                SendCompleteTradingMessage(leader);
                usedSkill = true;
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

        static ActiveDropPodInfo MakeDropPodInfo(List<Thing> loots)
        {
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(loots, true, false);
            activeDropPodInfo.spawnWipeMode = new WipeMode?(WipeMode.Vanish);
            return activeDropPodInfo;   

        }

        static void DropAtHome(ActiveDropPodInfo activeDropPodInfo)
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


        #region WorkingSite
        static void TryToInteractWithWorkingSites(WorldObject o, ref bool usedSkill, Pawn leader)
        {
            usedSkill = ValidateLeader<Attribute_Diplomacy>(leader, 2) && !o.Faction.def.PermanentlyHostileTo(FactionDefOf.PlayerColony);
            if (usedSkill)
            {
                InteractWithWorkingSite(o);
                GameComponent_WAW.Instance.OnLeaderAbilityUsed(leader);
            }
        }

        static void InteractWithWorkingSite(WorldObject o)
        {
            FactionRelationKind playerRelationKind = o.Faction.PlayerRelationKind;
            int goodWill = DiplomacyTuning.Goodwill_MemberExitedMapHealthy;
            Faction.OfPlayer.TryAffectGoodwillWith(o.Faction, goodWill, canSendMessage: false);
            TaggedString ltterText = "WAW.GoodWillGained.Desc".Translate(o.Faction.NameColored, goodWill);
            Find.LetterStack.ReceiveLetter("WAW.GoodWillGained".Translate(), ltterText, LetterDefOf.PositiveEvent);
        }
        #endregion


    }
}
