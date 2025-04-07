using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    [StaticConstructorOnStartup]
    public static class WAWTex
    {
        public static readonly Texture2D ModIcon = ContentFinder<Texture2D>.Get("UI/Icon/ModIcon", true);
        public static readonly Texture2D ResettleIcon = ContentFinder<Texture2D>.Get("UI/Commands/Resettle", true);
        public static readonly Texture2D ReArrangeIcon = ContentFinder<Texture2D>.Get("UI/Commands/ConfigureWarband", true);
        public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);
        public static readonly Texture2D OpenTex = ContentFinder<Texture2D>.Get("UI/Designators/Open");
        public static readonly Texture2D DismissTex = ContentFinder<Texture2D>.Get("UI/Commands/WarbandDismiss");
        public static readonly Texture2D WarbandWithdrawTex = ContentFinder<Texture2D>.Get("UI/Commands/WarbandWithdraw");
        public static readonly Texture2D HeadTex = ContentFinder<Texture2D>.Get("Things/Pawn/HumanLike/Heads/Male/Male_Average_Normal_south");
        public static readonly Texture2D BodyTex = ContentFinder<Texture2D>.Get("Things/Pawn/HumanLike/Bodies/Naked_Male_south");
        public static readonly Texture2D CreateWarbandTex = ContentFinder<Texture2D>.Get("UI/Commands/CreateWarband");
        public static readonly Texture2D WarbandWorldObjectTex = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Warband");
        public static readonly Texture2D PromoteSoldier = ContentFinder<Texture2D>.Get("UI/Commands/Promote");
        public static readonly Texture2D WarbandTex = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Warband");
        public static readonly Texture2D WarbandOutpost = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/WarbandOutpost");
        public static readonly Texture2D WarbandEliteTex = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/WarbandElite");
        public static readonly Texture2D WarbandVehicleTex = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/BrigadeCombatTeam");
        public static readonly Texture2D WarbandPsycasterTex = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Psycasters");
        public static readonly Texture2D PurchaseVehicleTex = ContentFinder<Texture2D>.Get("UI/Commands/PurchaseVehicle");
        public static readonly Texture2D Town = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/MinorFaction_Town");
        public static readonly Texture2D Village = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/MinorFaction_Village");
        public static readonly Texture2D QuickLoadTex = ContentFinder<Texture2D>.Get("UI/Commands/WAWQuickLoad");
        public static readonly Texture2D BankAccount = ContentFinder<Texture2D>.Get("UI/Icon/BankAccount");
        public static readonly Texture2D DevelopmentPoints = ContentFinder<Texture2D>.Get("UI/Icon/DevelopmentPoints");
        public static readonly Texture2D Cohesion = ContentFinder<Texture2D>.Get("UI/Icon/Cohesion");

             
    }
}
