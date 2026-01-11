using MoreSlugcats;
using RWCustom;
using SlugBase.DataTypes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScavSlug;

public class ScavSlugPlayerData(Player player)
{
    public readonly bool IsScavSlug = player.slugcatStats.name == Enums.scavslug;

    public Spear spearOnBack2;
    public Player.AbstractOnBackStick abstractStickOnBack2;
}

public static class PlayerExtension
{
    private static readonly ConditionalWeakTable<Player, ScavSlugPlayerData> cwt = new();

    public static ScavSlugPlayerData ScavSlug(this Player player) => cwt.GetValue(player, _ => new ScavSlugPlayerData(player));

    public static Player Get(this WeakReference<Player> weakRef)
    {
        weakRef.TryGetTarget(out var result);
        return result;
    }

    public static PlayerGraphics PlayerGraphics(this Player player) => (PlayerGraphics)player.graphicsModule;

    public static bool IsScavSlug(this Player player) => player.ScavSlug().IsScavSlug;

    public static bool IsScavSlug(this Player player, out ScavSlugPlayerData ScavSlug)
    {
        ScavSlug = player.ScavSlug();
        return ScavSlug.IsScavSlug;
    }
}