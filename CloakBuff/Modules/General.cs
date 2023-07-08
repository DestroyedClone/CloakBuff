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
    public static class General
    {
        public static void ProjectileDirectionalTargetFinder_SearchForTarget(On.RoR2.Projectile.ProjectileDirectionalTargetFinder.orig_SearchForTarget orig, RoR2.Projectile.ProjectileDirectionalTargetFinder self)
        {
            orig(self);
            var type = self.gameObject.name;
            if (cfgProjectileDirectionalTargetFinderFilterType.Value == TargetingType.Whitelist)
            {
                var daggerCheck = (type == "DaggerProjectile(Clone)" && cfgProjectileDirectionalTargetFinderDagger.Value);
                if (!daggerCheck)
                {
                    IEnumerable<HurtBox> source = self.bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(self.PassesFilters));
                    self.SetTarget(FilterMethod(source));
                }
            }
        }

        public static void SiphonNearbyController_SearchForTargets(On.RoR2.SiphonNearbyController.orig_SearchForTargets orig, SiphonNearbyController self, List<HurtBox> dest)
        {
            orig(self, dest);
            self.sphereSearch.ClearCandidates();
            self.sphereSearch.RefreshCandidates();
            self.sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(self.networkedBodyAttachment.attachedBody.teamComponent.teamIndex));
            var destCopy = new List<HurtBox>(dest);
            foreach (var hurtBox in destCopy)
            {
                //Debug.Log("Mired Urn: Checking " + hurtBox.healthComponent.body.GetDisplayName());
                if ((bool)hurtBox.healthComponent?.body?.hasCloakBuff)
                {
                    dest.Remove(hurtBox);
                    //Debug.Log("Removed");
                }
                else
                {
                    //Debug.Log("Kept");
                }
            }
            self.sphereSearch.OrderCandidatesByDistance();
            self.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            self.sphereSearch.GetHurtBoxes(dest);
            self.sphereSearch.ClearCandidates();
        }

        public static HurtBox DevilOrb_PickNextTarget(On.RoR2.Orbs.DevilOrb.orig_PickNextTarget orig, RoR2.Orbs.DevilOrb self, Vector3 position, float range)
        {
            var type = self.effectType;
            //Debug.Log("Devil Orb: "+type.ToString());
            if (DevilOrbIncludesFilterType.Value == TargetingType.Whitelist)
            {
                var novaOnHealCheck = (type == RoR2.Orbs.DevilOrb.EffectType.Skull && DevilOrbIncludesNovaOnHeal.Value);
                var sprintWispCheck = (type == RoR2.Orbs.DevilOrb.EffectType.Wisp && DevilOrbIncludesSprintWisp.Value);
                if (!(novaOnHealCheck || sprintWispCheck))
                {
                    return orig(self, position, range);
                }
            }

            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = position;
            bullseyeSearch.searchDirection = Vector3.zero;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.teamMaskFilter.RemoveTeam(self.teamIndex);
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.maxDistanceFilter = range;
            bullseyeSearch.RefreshCandidates();
            List<HurtBox> list = bullseyeSearch.GetResults().ToList<HurtBox>();
            if (list.Count <= 0)
            {
                return null;
            }

            HurtBox hurtBox = list[UnityEngine.Random.Range(0, list.Count)];

            while ((bool)hurtBox.healthComponent?.body.hasCloakBuff && list.Count > 0)
            {
                list.Remove(hurtBox);
                hurtBox = list[UnityEngine.Random.Range(0, list.Count)];
            }
            return hurtBox;
        }

        public static HurtBox LightningOrb_PickNextTarget(On.RoR2.Orbs.LightningOrb.orig_PickNextTarget orig, RoR2.Orbs.LightningOrb self, Vector3 position)
        {
            var type = self.lightningType;
            var original = orig(self, position);
            //Debug.Log("Lightning Orb: "+type.ToString());

            if (LightningOrbIncludesFilterType.Value == TargetingType.Whitelist)
            {
                var bfgCheck = (type == RoR2.Orbs.LightningOrb.LightningType.BFG && LightningOrbIncludesBFG.Value);
                var glaiveCheck = (type == RoR2.Orbs.LightningOrb.LightningType.HuntressGlaive && LightningOrbIncludesGlaive.Value);
                var ukuleleCheck = (type == RoR2.Orbs.LightningOrb.LightningType.Ukulele && LightningOrbIncludesUkulele.Value);
                var razorwireCheck = (type == RoR2.Orbs.LightningOrb.LightningType.RazorWire && LightningOrbIncludesRazorwire.Value);
                var crocoDiseaseCheck = (type == RoR2.Orbs.LightningOrb.LightningType.CrocoDisease && LightningOrbIncludesCrocoDisease.Value);
                var teslaCheck = (type == RoR2.Orbs.LightningOrb.LightningType.Tesla && LightningOrbIncludesTesla.Value);
                if (!(bfgCheck || glaiveCheck || ukuleleCheck || razorwireCheck || crocoDiseaseCheck || teslaCheck))
                {
                    return original;
                }
            }
            if (self.search == null)
                self.search = new BullseyeSearch();
            self.search.searchOrigin = position;
            self.search.searchDirection = Vector3.zero;
            self.search.teamMaskFilter = TeamMask.allButNeutral;
            self.search.teamMaskFilter.RemoveTeam(self.teamIndex);
            self.search.filterByLoS = false;
            self.search.sortMode = BullseyeSearch.SortMode.Distance;
            self.search.maxDistanceFilter = self.range;
            self.search.RefreshCandidates();
            HurtBox hurtBox = (from v in self.search.GetResults()
                               where (!self.bouncedObjects.Contains(v.healthComponent) && !v.healthComponent.body.hasCloakBuff)
                               select v).FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                self.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }

        public static Transform MissileController_FindTarget(On.RoR2.Projectile.MissileController.orig_FindTarget orig, RoR2.Projectile.MissileController self)
        {
            var objName = self.gameObject.name;
            if (MissileIncludesFilterType.Value == TargetingType.Whitelist)
            {
                var harpoonCheck = (objName == "EngiHarpoon(Clone)" && MissileIncludesHarpoons.Value);
                var dmrAtgCheck = (objName == "MissileProjectile(Clone)" && MissileIncludesDMLATG.Value);
                if (!(harpoonCheck || dmrAtgCheck))
                {
                    return orig(self);
                }
            }
            self.search.searchOrigin = self.transform.position;
            self.search.searchDirection = self.transform.forward;
            self.search.teamMaskFilter.RemoveTeam(self.teamFilter.teamIndex);
            self.search.RefreshCandidates();
            HurtBox hurtBox = FilterMethod(self.search.GetResults());

            if (hurtBox == null)
            {
                return null;
            }
            return hurtBox.transform;
        }

        public static void EquipmentSlot_ConfigureTargetFinderForEnemies(On.RoR2.EquipmentSlot.orig_ConfigureTargetFinderForEnemies orig, EquipmentSlot self)
        {
            orig(self);
            foreach (var target in self.targetFinder.GetResults())
            {
                if ((bool)target.healthComponent?.body?.hasCloakBuff)
                    self.targetFinder.FilterOutGameObject(target.gameObject);
            }
        }

        public static void Util_HandleCharacterPhysicsCastResults1(ILContext il) //harb help wip
        {
            ILCursor c = new ILCursor(il);
            int healthComponentLocal = -1;
            c.GotoNext(
              x => x.MatchLdfld("HealthComponent", "healthComponent"),

              x => x.MatchLdloc(out healthComponentLocal)
            );
            ILLabel continLabel = null;
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                //x => x.MatchCall("op_Equality"),
                x => x.MatchBrtrue(out continLabel)
            );
            c.Emit(OpCodes.Ldloc, healthComponentLocal);
            c.EmitDelegate<Func<HealthComponent, bool>>(
                (HealthComponent hc) =>
                { return hc.body.hasCloakBuff; }
                );
            c.Emit(OpCodes.Brtrue, continLabel);

            c.Emit(OpCodes.Add);
        }

        public static bool ModifyEngiMines(On.RoR2.Projectile.ProjectileSphereTargetFinder.orig_PassesFilters orig, RoR2.Projectile.ProjectileSphereTargetFinder self, HurtBox result)
        {
            var original = orig(self, result);
            var objName = self.gameObject.name;
            var spiderCheck = (objName == "SpiderMine(Clone)" && cfgEngiSpiderMine.Value);
            CharacterBody body = result.healthComponent.body;
            if (cfgProjectileSphereTargetFinderFilterType.Value == TargetingType.Whitelist)
            {
                var mineCheck = (objName == "EngiMine(Clone)" && cfgEngiChargeMine.Value);
                if (!(mineCheck || spiderCheck))
                {
                    //Debug.Log("Neither type of mine");
                    return original;
                }
                if (spiderCheck)
                {
                    if (cfgEngiSpiderMineCanExplodeOnImpaled.Value)
                    {
                        var stickOnImpact = self.gameObject.GetComponent<RoR2.Projectile.ProjectileStickOnImpact>();
                        if (stickOnImpact?.victim == body.gameObject)
                        {
                            //Debug.Log("Spidermine is attached, so we're exploding it");
                            return original;
                        }
                    }
                }
            }
            return original && !body.hasCloakBuff;
        }
    }
}
