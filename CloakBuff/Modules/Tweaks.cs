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
    public static class Tweaks
    {
        public static void AddShockOrStunToSurvivors(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();
            foreach (var survivor in SurvivorCatalog.allSurvivorDefs)
            {
                var bodyPrefab = survivor.bodyPrefab;
                if (bodyPrefab)
                {
                    var comp = bodyPrefab.GetComponent<SetStateOnHurt>();
                    if (comp)
                    {
                        if (CfgSurvivorShockOverride.Value == ShouldShock.SurvivorsAndUmbras)
                            comp.canBeStunned = true;
                        if (CfgSurvivorShockOverride.Value == ShouldShock.UmbraOnly)
                        {
                            var umbraComp = bodyPrefab.GetComponent<IfDoppelgangerThenAllowShock>();
                            if (!umbraComp)
                            {
                                umbraComp = bodyPrefab.AddComponent<IfDoppelgangerThenAllowShock>();
                                umbraComp.setStateOnHurt = comp;
                                umbraComp.characterBody = bodyPrefab.GetComponent<CharacterBody>();
                            }
                        }
                    }
                }
            }
        }

        public static void BuffWard_BuffTeam(On.RoR2.BuffWard.orig_BuffTeam orig, BuffWard self, IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
        {
            if (self.buffDef == RoR2Content.Buffs.AffixHauntedRecipient || self.buffDef == RoR2Content.Buffs.Cloak)
            {
                var newList = recipients.ToList();
                foreach (var recipient in recipients)
                {
                    var comp = recipient.GetComponent<SetStateOnHurt>();
                    if (comp && comp.targetStateMachine && comp.targetStateMachine.state is ShockState)
                    {
                        newList.Remove(recipient);
                    }
                }
                recipients = newList;
            }
            orig(self, recipients, radiusSqr, currentPosition);
        }

        public static void ShockState_StopCloak(On.EntityStates.ShockState.orig_PlayShockAnimation orig, ShockState self)
        {
            orig(self);
            if (self.characterBody)
            {
                self.characterBody.ClearTimedBuffs(RoR2Content.Buffs.Cloak);
                self.characterBody.ClearTimedBuffs(RoR2Content.Buffs.AffixHauntedRecipient);
            }
        }
    }
}
