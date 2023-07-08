using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static CloakBuff.Assets;
using static CloakBuff.Config;
using static CloakBuff.Modules.AI;
using static CloakBuff.Modules.Survivor;
using static CloakBuff.Modules.Visual;
using static CloakBuff.Modules.General;
using static CloakBuff.Modules.Tweaks;
using static CloakBuff.CloakBuffPlugin;
using CloakBuff.Modules;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace CloakBuff
{
    [BepInPlugin("com.DestroyedClone.CloakBuff", "CloakBuff", "1.1.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class CloakBuffPlugin : BaseUnityPlugin
    {

        public void Awake()
        {
            Assets.Initialize();
            SetupConfig(Config);
            Networking.RegisterMessages();

            AI.Initialize();
            Survivor.Initialize();
            Visual.Initialize();

            
            // Squid
            //On.EntityStates.Squid.SquidWeapon.FireSpine.
            if (CfgProjectileSphereTargetFinderFilterType.Value != 0)
            {
                if (CfgEngiChargeMine.Value || CfgEngiSpiderMine.Value)
                {
                    On.RoR2.Projectile.ProjectileSphereTargetFinder.PassesFilters += ModifyEngiMines;
                }
            }

            // Extra
            if (CfgShockKillsCloak.Value)
                On.EntityStates.ShockState.PlayShockAnimation += ShockState_StopCloak;
            if (CfgShockPausesCelestine.Value)
                On.RoR2.BuffWard.BuffTeam += BuffWard_BuffTeam;
            if (CfgSurvivorShockOverride.Value > ShouldShock.None)
                On.RoR2.SurvivorCatalog.Init += AddShockOrStunToSurvivors;

            // AI
            if (CfgDecloakAlertRadius.Value > 0)
                On.RoR2.CharacterBody.RemoveBuff_BuffIndex += AlertEnemiesUponUncloak;

            // Items
            // DML + ATG
            if (CfgMissileIncludesFilterType.Value != 0)
            {
                On.RoR2.Projectile.MissileController.FindTarget += MissileController_FindTarget;
                if (CfgMissileIncludesHarpoons.Value)
                    On.EntityStates.Engi.EngiMissilePainter.Paint.GetCurrentTargetInfo += Paint_GetCurrentTargetInfo;
            }
            // BFG / Huntress' Glaive / Ukulele / Razorwire / CrocoDisease / Tesla
            if (CfgLightningOrbIncludesFilterType.Value != 0)
                On.RoR2.Orbs.LightningOrb.PickNextTarget += LightningOrb_PickNextTarget;
            // Little Disciple / N'kuhana's Opinion
            if (CfgDevilOrbIncludesFilterType.Value != 0)
                On.RoR2.Orbs.DevilOrb.PickNextTarget += DevilOrb_PickNextTarget;
            if (CfgMiredUrn.Value)
                On.RoR2.SiphonNearbyController.SearchForTargets += SiphonNearbyController_SearchForTargets;
            // Ceremonial Dagger
            if (CfgProjectileDirectionalTargetFinderFilterType.Value != 0)
                On.RoR2.Projectile.ProjectileDirectionalTargetFinder.SearchForTarget += ProjectileDirectionalTargetFinder_SearchForTarget;
            if (CfgRoyalCap.Value)
                On.RoR2.EquipmentSlot.ConfigureTargetFinderForEnemies += EquipmentSlot_ConfigureTargetFinderForEnemies;

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;

        }

        private void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            orig(self, buffDef, duration, maxStacks);

        }
    }
}