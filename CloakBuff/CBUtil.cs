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

namespace CloakBuff
{
    public static class CBUtil
    {


        // Plugin
        public static HurtBox FilterMethod(IEnumerable<HurtBox> listOfTargets)
        {
            HurtBox hurtBox = listOfTargets.FirstOrDefault<HurtBox>();
            if (hurtBox == null)
            {
                //Debug.Log("Evis chose target: None");
            }
            else
            {
                //Debug.Log("Evis chose target: " + hurtBox.healthComponent.body.GetDisplayName());
            }
            //Debug.Log("Attempting Iteration with list of length: "+listOfTargets.Count());
            int index = 0;
            while (hurtBox != null)
            {
                if ((bool)hurtBox.healthComponent?.body?.hasCloakBuff)
                {
                    //Debug.Log("Target was cloaked, moving on to");
                    index++;
                    hurtBox = listOfTargets.ElementAtOrDefault(index);
                    //Debug.Log("NEW Target: " + hurtBox.healthComponent.body.GetDisplayName());
                    continue;
                }
                //Debug.Log("Chosen target works!");
                break;
            }
            return hurtBox;
        }
    }
}
