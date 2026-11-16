using BepInEx;
using MSU;
using MSU.Config;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace TO30
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(VAPI.VAPIMain.GUID)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class TO30Main : BaseUnityPlugin
    {
        public const string GUID = "com.Nebby.TO30";
        public const string MODNAME = "The Original 30";
        public const string VERSION = "2.2.1";

        public static TO30Main instance { get; private set; }
        private void Awake()
        {
            instance = this;

            new TO30Log(Logger);
            new TO30Config(this);

            new TO30Content();

            bool configuredBoolUseRoRRSprites = new ConfiguredBool(true)
                .WithModName(this)
                .WithModGUID(this)
                .WithConfigFile(Config)
                .WithSection("General")
                .WithKey("Use RoRR loading sprites")
                .WithDescription("Uses the Risk of Rain Returns style of the Flamethrower Lemurian on the loading screens.")
                .DoConfigure();

            Vector2? sizeDelta = configuredBoolUseRoRRSprites ? new Vector2(48, 48) : null;

            var loadingScreenBundle = TO30Assets.GetLoadingScreenBundle();

            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimation(configuredBoolUseRoRRSprites ? loadingScreenBundle.LoadAsset<SimpleSpriteAnimation>("ssaFlamethrowerLemurianWalkReturns") : loadingScreenBundle.LoadAsset<SimpleSpriteAnimation>("ssaFlamethrowerLemurianWalk"), null, sizeDelta);

            RoR2Application.onLoad += AddSpectralSummons;
        }

        private void AddSpectralSummons()
        {
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddWeightedMaster(MasterCatalog.FindMasterIndex("BeetleMaster"), 1);
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddWeightedMaster(MasterCatalog.FindMasterIndex("ImpMaster"), 1);
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddWeightedMaster(MasterCatalog.FindMasterIndex("LemurianMaster"), 1);
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddWeightedMaster(MasterCatalog.FindMasterIndex("WispMaster"), 1);
        }
    }
}
