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

    internal static class WAWTex
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
        public static readonly Texture2D ScreenShot = ContentFinder<Texture2D>.Get("Misc/ScreenShot");


    }
}
