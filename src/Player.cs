using BepInEx;
using MonoMod.RuntimeDetour;
using SlugBase.Features;
using System;
using System.Linq;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;

namespace ScavSlug;

public static partial class Hooks
{
    public static void ApplyPlayerHooks()
    {
        new Hook(typeof(Player).GetProperty(nameof(Player.CanPutSpearToBack), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetGetMethod(), Player_get_CanPutSpearToBack);
    }

    public delegate bool orig_CanPutSpearToBack(Player self);
    private static bool Player_get_CanPutSpearToBack(orig_CanPutSpearToBack orig, Player self)
    {
        return orig(self) || (self.IsScavSlug(out ScavSlugPlayerData s) && s.spearOnBack2 == null);
    }
}
