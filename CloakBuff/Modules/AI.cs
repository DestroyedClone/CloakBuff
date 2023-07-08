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

namespace CloakBuff.Modules
{
    public static class AI
    {
        public static bool shouldPerformDistanceCheck = false;
        public static void Initialize()
        {
            shouldPerformDistanceCheck = CfgDecloakAlertRadius.Value != 999;
        }

        public static void AlertEnemiesUponUncloak(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);
            if (buffType == RoR2Content.Buffs.Cloak.buffIndex || buffType == RoR2Content.Buffs.AffixHauntedRecipient.buffIndex)
            {
                foreach (var baseAI in InstanceTracker.GetInstancesList<RoR2.CharacterAI.BaseAI>())
                {
                    if (!baseAI)
                        continue;

                    if (!shouldPerformDistanceCheck || Vector3.Distance(baseAI.body.corePosition, self.corePosition) <= CfgDecloakAlertRadius.Value)
                        baseAI.targetRefreshTimer = 0;
                }
            }
        }
    }
}
