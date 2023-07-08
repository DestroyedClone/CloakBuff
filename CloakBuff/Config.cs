using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static CloakBuff.CloakBuffPlugin;
using static CloakBuff.Assets;

namespace CloakBuff
{
    public static class Config
    {
        public static ConfigEntry<bool> CfgShowDoppelgangerEffect { get; set; }
        public static ConfigEntry<bool> CfgEnableHealthbar { get; set; }
        public static ConfigEntry<bool> CfgEnablePinging { get; set; }
        public static ConfigEntry<bool> CfgEnableDamageNumbers { get; set; }
        public static ConfigEntry<bool> CfgEnableStunEffect { get; set; }
        public static ConfigEntry<bool> CfgEnableShockEffect { get; set; }
        public static ConfigEntry<bool> CfgHideBossIndicator { get; set; }
        public static ConfigEntry<bool> CfgHidePingOnCloaked { get; set; }
        public static ConfigEntry<TargetingType> CfgMissileIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> CfgMissileIncludesHarpoons { get; set; }

        public static ConfigEntry<bool> CfgMissileIncludesDMLATG { get; set; }
        public static ConfigEntry<TargetingType> CfgLightningOrbIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> CfgLightningOrbIncludesBFG { get; set; }

        public static ConfigEntry<bool> CfgLightningOrbIncludesGlaive { get; set; }
        public static ConfigEntry<bool> CfgLightningOrbIncludesUkulele { get; set; }
        public static ConfigEntry<bool> CfgLightningOrbIncludesRazorwire { get; set; }
        public static ConfigEntry<bool> CfgLightningOrbIncludesCrocoDisease { get; set; }
        public static ConfigEntry<bool> CfgLightningOrbIncludesTesla { get; set; }
        public static ConfigEntry<TargetingType> CfgDevilOrbIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> CfgDevilOrbIncludesSprintWisp { get; set; }

        public static ConfigEntry<bool> CfgDevilOrbIncludesNovaOnHeal { get; set; }
        public static ConfigEntry<TargetingType> CfgProjectileDirectionalTargetFinderFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> CfgProjectileDirectionalTargetFinderDagger { get; set; }

        public static ConfigEntry<TargetingType> CfgProjectileSphereTargetFinderFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist

        public static ConfigEntry<bool> CfgMiredUrn { get; set; }
        public static ConfigEntry<bool> CfgRoyalCap { get; set; }
        public static ConfigEntry<bool> CfgShockKillsCloak { get; set; }
        public static ConfigEntry<bool> CfgShockPausesCelestine { get; set; }
        public static ConfigEntry<ShouldShock> CfgSurvivorShockOverride { get; set; }
        public static ConfigEntry<bool> CfgHuntressCantAim { get; set; }
        public static ConfigEntry<bool> CfgMercCantFind { get; set; }
        public static ConfigEntry<bool> CfgEngiChargeMine { get; set; }
        public static ConfigEntry<bool> CfgEngiSpiderMine { get; set; }
        public static ConfigEntry<bool> CfgEngiSpiderMineCanExplodeOnImpaled { get; set; }
        public static ConfigEntry<bool> CfgRailgunnerPrimary { get; set; }
        public static ConfigEntry<float> CfgDecloakAlertRadius { get; set; }

        public static void SetupConfig(ConfigFile Config)
        {
            CfgShowDoppelgangerEffect = Config.Bind("Visual", "Disable Umbra Effect", true, "Enable to hide the Umbra's swirling particle effects on cloaked targets.");
            CfgEnableHealthbar = Config.Bind("Visual", "Disable Healthbar", true, "Enable to hide the healthbar on cloaked targets.");
            CfgEnablePinging = Config.Bind("Visual", "Mislead Pinging", true, "Attempts to mislead pinging by pinging the enemy behind the cloaked target. If things get messed up, this is the first option to likely disable.");
            CfgEnableDamageNumbers = Config.Bind("Visual", "Disable Damage Numbers", true, "Enable to hide damage numbers from appearing on cloaked targets.");
            CfgEnableStunEffect = Config.Bind("Visual", "Disable Stun Overhead Effect", true, "Enable to hide the overhead stun effect from appearing on cloaked targets.");
            CfgEnableShockEffect = Config.Bind("Visual", "Disable Shock Overhead Effect", true, "Enable to hide the overhead shock effects from appearing on cloaked targets.");
            CfgHideBossIndicator = Config.Bind("Visual", "Disable Boss Indicator", true, "Enable to hide the boss indicator from appearing on cloaked targets.");
            CfgHidePingOnCloaked = Config.Bind("Visual", "Hide Ping on Cloaked", true, "Enable to make the ping hidden once whatever you've pinged is cloaked.");

            CfgMissileIncludesDMLATG = Config.Bind("Items", "Disposable Missile Launcher and AtG Missile Mk. 1", true, "Enable to make missiles from these items to ignore cloaked targets.");
            CfgLightningOrbIncludesBFG = Config.Bind("Items", "Preon Accumulator", false, "Currently Broken. Enable to make Preon Accumulator's traveling tendrils ignore cloaked targets.");
            CfgLightningOrbIncludesUkulele = Config.Bind("Items", "Ukulele", false, "Enable to make Ukulele's electricity to no longer arc to cloaked targets.");
            CfgLightningOrbIncludesRazorwire = Config.Bind("Items", "Razorwire", false, "Currently Broken. Enable to make Razorwire unable to go to cloaked targets.");
            CfgLightningOrbIncludesTesla = Config.Bind("Items", "Unstable Tesla Coil", false, "Enable to make Tesla electricity to no longer arc to cloaked targets.");
            CfgDevilOrbIncludesNovaOnHeal = Config.Bind("Items", "Nkuhanas Opinion", false, "Enable to make the attack no longer seek out cloaked targets.");
            CfgDevilOrbIncludesSprintWisp = Config.Bind("Items", "Little Disciple", false, "Enable to make the attack no longer seek out cloaked targets.");
            CfgProjectileDirectionalTargetFinderDagger = Config.Bind("Items", "Ceremonial Dagger", false, "Enable to make the spawned daggers no longer seek out cloaked targets.");
            CfgMiredUrn = Config.Bind("Items", "Mired Urn", false, "Finnicky. Prioritizes noncloaked targets, but will choose a cloaked target if they are the only choice in range.");
            CfgRoyalCap = Config.Bind("Items", "Royal Capacitator", true, "Enable to prevent the aiming reticle from appearing on cloaked targets.");

            CfgLightningOrbIncludesCrocoDisease = Config.Bind("Survivors", "Acrid Epidemic", false, "Currently Broken. Affects Acrid's special Epidemic's spreading");
            CfgMissileIncludesHarpoons = Config.Bind("Survivors", "Engineer Harpoons+Targeting", false, "Affects the Engineer's Utility Thermal Harpoons. Also prevents the user from painting cloaked enemies as targets.");
            CfgEngiChargeMine = Config.Bind("Survivors", "Engineer Pressure Mines", false, "Finnicky. Affects the Engineer's Secondary Pressure Mines. Prevents exploding when cloaked enemies are in proximity.");
            CfgEngiSpiderMine = Config.Bind("Survivors", "Engineer Spider Mines", true, "Affects the Engineer's Secondary Spider Mines. Prevents exploding when cloaked enemies are in proximity.");
            CfgEngiSpiderMineCanExplodeOnImpaled = Config.Bind("Survivors", "Engineer Spider Mines Single Target", true, "Affects the Engineer's Secondary Spider Mines, requires the previous option to be enabled." +
                "\nIf enabled, then it will explode when armed if it is stuck on a cloaked target.");
            CfgHuntressCantAim = Config.Bind("Survivors", "Huntress Aiming", false, "This adjustment will make Huntress unable to target cloaked enemies with her primary and secondary abilities");
            CfgLightningOrbIncludesGlaive = Config.Bind("Survivors", "Huntress Glaive", true, "Affects the Huntress' Secondary Laser Glaive from bouncing to cloaked targets.");
            CfgMercCantFind = Config.Bind("Survivors", "Mercernary Eviscerate", false, "Finnicky. Fails if an invalid enemy is within the same range of a valid enemy. The adjustment will prevent Mercernary's Eviscerate from targeting cloaked enemies");

            CfgShockKillsCloak = Config.Bind("Extra", "Shocking disrupts cloak", true, "Setting this value to true will make shocked targets (usually via Captain's M2 and Shocking Beacon) to clear cloak on hit. Note that Survivors are immune to this damagetype, so umbras can't normally be shocked...");
            CfgShockPausesCelestine = Config.Bind("Extra", "Celestines cant buff shocked targets", true, "Enabling will make shocked targets unable to be cloaked via Celestine Elites.");
            CfgSurvivorShockOverride = Config.Bind("Extra", "Enable Shocking and Stunning for Survivors Or Umbras", ShouldShock.None, "0 = Disabled" +
                "\nUmbraOnly = Umbras can get shocked and stunned." +
                "\nSurvivorsAndUmbras = Both Survivors and Umbras can get shocked and stunned.");

            CfgDecloakAlertRadius = Config.Bind("AI", "Decloak Reaction Radius", 50f, "The radius, in meters, which monsters will immediately retarget if they have no target. Set to -1 to disable, or to 999 for unlimited range.");

            CfgMissileIncludesFilterType = Config.Bind("zFiltering", "MissileController", TargetingType.Whitelist, "Its safe to ignore the options in this category." +
                "\n 0 = Disabled," +
                "\n 1 = All missiles are affected" +
                "\n 2 = Only the following options");
            CfgLightningOrbIncludesFilterType = Config.Bind("zFiltering", "Lightning Orbs", TargetingType.Whitelist, "0 = Disabled," +
                "\n 1 = All Lightning Orbs are affected" +
                "\n 2 = Only the following options");
            CfgDevilOrbIncludesFilterType = Config.Bind("zFiltering", "Devil Orbs", TargetingType.Whitelist, "0 = Disabled," +
                "\n 1 = All Devil Orbs are affected" +
                "\n 2 = Only the following options");
            CfgProjectileDirectionalTargetFinderFilterType = Config.Bind("zFiltering", "ProjectileDirectionalTargetFinder", TargetingType.Whitelist, "0 = Disabled," +
                "\n 1 = All ProjectileDirectionalTargetFinderFilterType are affected" +
                "\n 2 = Only the following options");
            CfgProjectileSphereTargetFinderFilterType = Config.Bind("zFiltering", "ProjectileSphereTargetFinder", TargetingType.Whitelist, "#NG!M!N#S" +
                "\n 0 = Disabled," +
                "\n 1 = All ProjectileSphereTargetFinderFilterType are affected" +
                "\n 2 = Only the following options");
        }
    }
}
