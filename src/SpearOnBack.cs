using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using RWCustom;

namespace ScavSlug;

public static partial class Hooks
{
    public static void ApplySpearOnBackHooks()
    {
        On.Player.SpearOnBack.Update += SpearOnBack_Update;
        On.Player.SpearOnBack.SpearToHand += SpearOnBack_SpearToHand;
        On.Player.SpearOnBack.SpearToBack += SpearOnBack_SpearToBack;
        On.Player.SpearOnBack.DropSpear += SpearOnBack_DropSpear;
        On.Player.SpearOnBack.GraphicsModuleUpdated += SpearOnBack_GraphicsModuleUpdated;
    }

    private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack self, bool eu)
    {
        // Should only execute for ScavSlug
        if (!self.owner.IsScavSlug(out ScavSlugPlayerData s))
        {
            orig(self, eu);
            return;
        }

        if (self.increment && self.spear != null && s.spearOnBack2 == null && self.counter >= 20)
        {
            for (int i = 0; i < 2; i++)
            {
                if (self.owner.grasps[i] != null && self.owner.grasps[i].grabbed is Spear)
                {
                    Plugin.SLogger.LogInfo("About to put spear on back slot 2");
                    self.owner.bodyChunks[0].pos += Custom.DirVec(self.owner.grasps[i].grabbed.firstChunk.pos, self.owner.bodyChunks[0].pos) * 2f;
                    self.SpearToBack(self.owner.grasps[i].grabbed as Spear);
                    self.counter = 0;
                    self.increment = false;
                    return;
                }
            }
        }

        orig(self, eu);
    }

    private static void SpearOnBack_SpearToHand(On.Player.SpearOnBack.orig_SpearToHand orig, Player.SpearOnBack self, bool eu)
    {
        orig(self, eu);

        // Should only execute for ScavSlug
        if (!self.owner.IsScavSlug(out ScavSlugPlayerData s))
        {
            return;
        }

        Plugin.SLogger.LogInfo("Put spear 1 on hand");
        if (s.spearOnBack2 != null)
        {
            Plugin.SLogger.LogInfo("Moved spear 2 to spear 1 slot");
            self.spear = s.spearOnBack2;
            self.abstractStick = s.abstractStickOnBack2;
            s.spearOnBack2 = null;
            s.abstractStickOnBack2.Deactivate();
            s.abstractStickOnBack2 = null;
        }
    }

    private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, Player.SpearOnBack self, Spear spr)
    {
        // Should only execute for ScavSlug and if there's only an empty slot for spear 2
        if (!self.owner.IsScavSlug(out ScavSlugPlayerData s) || self.spear == null)
        {
            Plugin.SLogger.LogInfo("Putting spear on back slot 1");
            orig(self, spr);
            return;
        }

        if (s.spearOnBack2 != null)
        {
            Plugin.SLogger.LogInfo("Back slots are full!");
            return;
        }
        Plugin.SLogger.LogInfo("Putting spear on back slot 2");
        for (int i = 0; i < 2; i++)
        {
            if (self.owner.grasps[i] != null && self.owner.grasps[i].grabbed == spr)
            {
                self.owner.ReleaseGrasp(i);
                break;
            }
        }
        s.spearOnBack2 = spr;
        s.spearOnBack2.ChangeMode(Weapon.Mode.OnBack);
        self.interactionLocked = true;
        self.owner.noPickUpOnRelease = 20;
        self.owner.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, self.owner.mainBodyChunk);
        s.abstractStickOnBack2?.Deactivate();
        s.abstractStickOnBack2 = new Player.AbstractOnBackStick(self.owner.abstractPhysicalObject, spr.abstractPhysicalObject);
    }

    private static void SpearOnBack_DropSpear(On.Player.SpearOnBack.orig_DropSpear orig, Player.SpearOnBack self)
    {
        orig(self);

        // Should only execute for ScavSlug
        if (!self.owner.IsScavSlug(out ScavSlugPlayerData s))
        {
            return;
        }

        if (s.spearOnBack2 != null)
        {
            Plugin.SLogger.LogInfo("Moved spear 2 to spear 1 slot");
            self.spear = s.spearOnBack2;
            self.abstractStick = s.abstractStickOnBack2;
            s.spearOnBack2 = null;
            s.abstractStickOnBack2.Deactivate();
            s.abstractStickOnBack2 = null;
        }
    }

    private static void SpearOnBack_GraphicsModuleUpdated(On.Player.SpearOnBack.orig_GraphicsModuleUpdated orig, Player.SpearOnBack self, bool actuallyViewed, bool eu)
    {
        orig(self, actuallyViewed, eu);

        // Should only execute for ScavSlug and if this is the second spear slot on the back
        if (!self.owner.IsScavSlug(out ScavSlugPlayerData s))
        {
            return;
        }

        if (s.spearOnBack2 == null)
        {
            return;
        }
        if (s.spearOnBack2.slatedForDeletetion || s.spearOnBack2.grabbedBy.Count > 0)
        {
            s.abstractStickOnBack2?.Deactivate();
            s.spearOnBack2 = null;
            return;
        }
        Vector2 vector = self.owner.mainBodyChunk.pos;
        Vector2 vector2 = self.owner.bodyChunks[1].pos;
        if (self.owner.graphicsModule != null)
        {
            vector = Vector2.Lerp((self.owner.graphicsModule as PlayerGraphics).drawPositions[0, 0], (self.owner.graphicsModule as PlayerGraphics).head.pos, 0.2f);
            vector2 = (self.owner.graphicsModule as PlayerGraphics).drawPositions[1, 0];
        }
        Vector2 vector3 = Custom.DirVec(vector2, vector);
        if (self.owner.Consious && self.owner.bodyMode != Player.BodyModeIndex.ZeroG && self.owner.EffectiveRoomGravity > 0f)
        {
            if (self.counter > 12 && !self.interactionLocked && self.owner.input[0].x != 0 && self.owner.standing)
            {
                float num = 0f;
                for (int i = 0; i < self.owner.grasps.Length; i++)
                {
                    if (self.owner.grasps[i] == null)
                    {
                        num = ((i == 0) ? -1f : 1f);
                        break;
                    }
                }
                s.spearOnBack2.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector2, vector) + Custom.LerpMap((float)self.counter, 12f, 20f, 0f, 360f * num) + 90f));
            }
            else
            {
                s.spearOnBack2.setRotation = new Vector2?((vector3 - Custom.PerpendicularVector(vector3) * (0.9f * (1f - Mathf.Abs(self.flip)))).normalized);
            }
            s.spearOnBack2.ChangeOverlap(vector3.y < -0.1f && self.owner.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
        }
        else
        {
            s.spearOnBack2.setRotation = new Vector2?(vector3 - Custom.PerpendicularVector(vector3) * 0.9f);
            s.spearOnBack2.ChangeOverlap(false);
        }
        s.spearOnBack2.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(vector2, vector, 0.6f) - Custom.PerpendicularVector(vector2, vector) * (7.5f * self.flip));
        s.spearOnBack2.firstChunk.vel = self.owner.mainBodyChunk.vel;
        s.spearOnBack2.rotationSpeed = 0f;
    }
}
