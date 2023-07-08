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
using UnityEngine.AddressableAssets;

namespace CloakBuff
{
    public static class Assets
    {
        public static GameObject DoppelgangerEffect;
        public static float evisMaxRange = EntityStates.Merc.Evis.maxRadius;
        public static GameObject pingIndicatorPrefab = Resources.Load<GameObject>("Prefabs/PingIndicator");

        public static GameObject StunStateVfx;
        public static GameObject ShockStateVfx;

        public static T LoadAddressable<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }

        public static void Initialize()
        {
            DoppelgangerEffect = LoadAddressable<GameObject>("RoR2/Base/InvadingDoppelganger/DoppelgangerEffect.prefab");
            pingIndicatorPrefab = LoadAddressable<GameObject>("RoR2/Base/Common/PingIndicator.prefab");
        }

        public enum TargetingType
        {
            Disabled,
            All,
            Whitelist
        }

        public enum ShouldShock
        {
            None,
            UmbraOnly,
            SurvivorsAndUmbras
        }

        public class HideVfxIfCloaked : MonoBehaviour
        {
            public CharacterBody body;
            public GameObject obj1;
            public GameObject obj2;

            public TemporaryVisualEffect shadowVisEfx = null;

            public void Start()
            {
                if (shadowVisEfx)
                {
                    body = shadowVisEfx.healthComponent.body;
                }
                else
                {
                    body = gameObject.transform.parent.gameObject.GetComponent<CharacterBody>();
                }
            }

            public void FixedUpdate()
            {
                if (body)
                {
                    var isVisible = !body.hasCloakBuff;
                    if (obj1) obj1.SetActive(isVisible);
                    if (obj2) obj2.SetActive(isVisible);
                }
            }
        }

        public class IfDoppelgangerThenAllowShock : MonoBehaviour
        {
            public SetStateOnHurt setStateOnHurt;
            public CharacterBody characterBody;

            public void Start()
            {
                if (setStateOnHurt && characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
                {
                    setStateOnHurt.canBeStunned = true;
                }
            }
        }


        public class KillPingerIfCloaked : MonoBehaviour
        {
            public RoR2.UI.PingIndicator pingIndicator;
            public bool shouldDestroy = false;

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDestroy()
            {
                InstanceTracker.Remove(this);
            }

            public void FixedUpdate()
            {
                if (shouldDestroy)
                {
                    return;
                }
                if (UnityEngine.Networking.NetworkServer.active)
                {
                    if (pingIndicator)
                    {
                        if (pingIndicator.pingTarget
                            && pingIndicator.pingTarget.GetComponent<CharacterBody>()
                            && pingIndicator.pingTarget.GetComponent<CharacterBody>().hasCloakBuff)
                        {
                            var index = InstanceTracker.GetInstancesList<KillPingerIfCloaked>().IndexOf(this);
                            new Networking.SendToClientsToDeleteIndicator(index).Send(NetworkDestination.Clients);
                            shouldDestroy = true;
                        }
                    }
                }
            }

            public void LateUpdate()
            {
                if (shouldDestroy)
                {
                    pingIndicator.fixedTimer = 0f;
                }
            }
        }
    }
}
