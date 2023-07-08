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
    public static class Visual
    {
        public static void Initialize()
        {
            if (!CfgShowDoppelgangerEffect.Value)
                ModifyDoppelGangerEffect();
            if (CfgEnableHealthbar.Value)
                On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += HideHealthbar;
            if (CfgEnablePinging.Value)
                On.RoR2.Util.HandleCharacterPhysicsCastResults += MisleadPinging;
            if (CfgEnableDamageNumbers.Value)
                IL.RoR2.HealthComponent.HandleDamageDealt += HideDamageNumbers;
            if (CfgHideBossIndicator.Value)
                On.RoR2.PositionIndicator.Start += BossIndicatorHiddenWhileCloaked;
            if (CfgHidePingOnCloaked.Value)
            {
                ModifyPingerPrefab();
            }
            //IL.RoR2.Util.HandleCharacterPhysicsCastResults += Util_HandleCharacterPhysicsCastResults1;
        }

        public static void ModifyPingerPrefab()
        {
            var component = pingIndicatorPrefab.AddComponent<KillPingerIfCloaked>();
            component.pingIndicator = pingIndicatorPrefab.GetComponent<RoR2.UI.PingIndicator>();
        }

        public static void HideDamageNumbers(ILContext il) //ty bubbet
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchCall<DamageNumberManager>("get_instance"),
                x => x.MatchLdloc(0),
                x => x.MatchLdfld<DamageDealtMessage>("damage"),
                x => x.MatchLdloc(0),
                x => x.MatchLdfld<DamageDealtMessage>("position")
            );
            //Debug.Log("Cursor before emit: \n" + c);
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<Func<DamageDealtMessage, bool>>(ddm =>
            {
                try
                {
                    if ((bool)ddm?.victim?.GetComponent<HealthComponent>()?.body?.hasCloakBuff)
                    {
                        //Debug.Log("body has cloak");
                        return false;
                    }
                }
                catch (InvalidOperationException) { }

                //Debug.Log("body does not have cloak");
                return true;
            });
            var ind = c.Index;
            c.GotoNext(
                x => x.MatchLdloc(0),
                x => x.MatchCall<GlobalEventManager>("ClientDamageNotified")
            );
            var br = c.Next; // Next was correct nre was caused by jumping to spawnDamagenumbers and it not having the arguments it needs
            c.Index = ind;
            c.Emit(OpCodes.Brfalse, br); // Brfalse is the correct behaviour given the return values from the delegate
                                         //Debug.Log("Cursor after emit: \n" + c);
        }

        public static bool MisleadPinging(On.RoR2.Util.orig_HandleCharacterPhysicsCastResults orig, GameObject bodyObject, Ray ray, RaycastHit[] hits, out RaycastHit hitInfo)
        {
            int num = -1;
            float num2 = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                float distance = hits[i].distance;
                if (distance < num2)
                {
                    HurtBox component = hits[i].collider.GetComponent<HurtBox>();
                    if (component)
                    {
                        HealthComponent healthComponent = component.healthComponent;
                        if (healthComponent)
                        {
                            if (healthComponent.gameObject == bodyObject)
                                goto IL_82;
                            else if (healthComponent.body.hasCloakBuff) // This is where you would put IL if you were smart (not me)
                            {
                                continue;
                            }
                        }
                    }
                    if (distance == 0f)
                    {
                        hitInfo = hits[i];
                        hitInfo.point = ray.origin;
                        return true;
                    }
                    num = i;
                    num2 = distance;
                }
            IL_82:;
            }
            if (num == -1)
            {
                hitInfo = default;
                return false;
            }
            hitInfo = hits[num];
            return true;
        }

        public static bool HideHealthbar(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
        {
            return orig(self, victim) && !victim.body.hasCloakBuff;
        }

        public static void ModifyDoppelGangerEffect()
        {
            if (!DoppelgangerEffect) return;

            var comp2 = DoppelgangerEffect.GetComponent<HideVfxIfCloaked>();
            if (!comp2)
            {
                comp2 = DoppelgangerEffect.AddComponent<HideVfxIfCloaked>();
            }
            comp2.obj1 = DoppelgangerEffect.transform.Find("Particles").gameObject;
            comp2.shadowVisEfx = DoppelgangerEffect.GetComponent<TemporaryVisualEffect>();
        }

        [RoR2.SystemInitializer(dependencies: typeof(RoR2.EntityStateCatalog))]
        public static void SetupStunAndShockStateVfx()
        {
            StunStateVfx = StunState.stunVfxPrefab;
            ShockStateVfx = ShockState.stunVfxPrefab;

            if (CfgEnableStunEffect.Value)
            {
                var comp = StunStateVfx.GetComponent<HideVfxIfCloaked>();
                if (!comp)
                {
                    comp = StunStateVfx.AddComponent<HideVfxIfCloaked>();
                }
                comp.obj1 = StunStateVfx.transform.Find("Ring").gameObject;
                comp.obj2 = StunStateVfx.transform.Find("Stars").gameObject;
            }

            if (CfgEnableShockEffect.Value)
            {
                var comp2 = ShockStateVfx.GetComponent<HideVfxIfCloaked>();
                if (!comp2)
                {
                    comp2 = ShockStateVfx.AddComponent<HideVfxIfCloaked>();
                }
                comp2.obj1 = ShockStateVfx.transform.Find("Stun").gameObject;
                comp2.obj2 = ShockStateVfx.transform.Find("SphereChainEffect").gameObject;
            }
        }
        public static void BossIndicatorHiddenWhileCloaked(On.RoR2.PositionIndicator.orig_Start orig, PositionIndicator self)
        {
            orig(self);
            if (self.name.StartsWith("BossPositionIndicator"))
            {
                var comp = self.gameObject.GetComponent<HideVfxIfCloaked>();
                if (!comp)
                    comp = self.gameObject.AddComponent<HideVfxIfCloaked>();
                comp.obj1 = self.gameObject.transform.Find("OutsideFrameArrow/Sprite").gameObject;
                comp.obj2 = self.gameObject.transform.Find("InsideFrameMarker/Sprite").gameObject;
            }
        }
    }
}
