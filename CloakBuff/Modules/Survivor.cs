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
using static CloakBuff.CBUtil;

namespace CloakBuff.Modules
{
    public static class Survivor
    {
        public static void Initialize()
        {// Character Specific
            if (CfgHuntressCantAim.Value)
                On.RoR2.HuntressTracker.SearchForTarget += HuntressTracker_SearchForTarget;
            if (CfgMercCantFind.Value)
            {
                On.EntityStates.Merc.Evis.SearchForTarget += Evis_SearchForTarget;
            }
        }

        public static void Paint_GetCurrentTargetInfo(On.EntityStates.Engi.EngiMissilePainter.Paint.orig_GetCurrentTargetInfo orig, EntityStates.Engi.EngiMissilePainter.Paint self, out HurtBox currentTargetHurtBox, out HealthComponent currentTargetHealthComponent)
        {
            orig(self, out currentTargetHurtBox, out currentTargetHealthComponent);
            foreach (HurtBox hurtBox in self.search.GetResults())
            {
                if ((bool)hurtBox.healthComponent?.alive && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.hasCloakBuff)
                {
                    currentTargetHurtBox = hurtBox;
                    currentTargetHealthComponent = hurtBox.healthComponent;
                    return;
                }
            }
            currentTargetHurtBox = null;
            currentTargetHealthComponent = null;
        }

        public static void HuntressTracker_SearchForTarget(On.RoR2.HuntressTracker.orig_SearchForTarget orig, HuntressTracker self, Ray aimRay)
        {
            orig(self, aimRay);
            self.trackingTarget = FilterMethod(self.search.GetResults());
        }

        public static HurtBox Evis_SearchForTarget(On.EntityStates.Merc.Evis.orig_SearchForTarget orig, EntityStates.Merc.Evis self)
        {
            var original = orig(self);
            if (!original.healthComponent.body.hasCloakBuff)
            {
                return original;
            }
            BullseyeSearch bullseyeSearch = new BullseyeSearch
            {
                searchOrigin = self.transform.position,
                searchDirection = UnityEngine.Random.onUnitSphere,
                maxDistanceFilter = evisMaxRange,
                teamMaskFilter = TeamMask.GetUnprotectedTeams(self.GetTeam()),
                sortMode = BullseyeSearch.SortMode.Distance
            };
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(self.gameObject);
            return FilterMethod(bullseyeSearch.GetResults());
        }

    }
}
