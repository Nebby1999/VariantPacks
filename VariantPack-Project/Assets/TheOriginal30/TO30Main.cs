using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

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

            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimations(TO30Assets.GetLoadingScreenBundle());
            RoR2Application.onLoad += AddSpectralSummons;
        }

        private void AddSpectralSummons()
        {
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("BeetleMaster"));
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("ImpMaster"));
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("LemurianMaster"));
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("WispMaster"));
        }
    }
}
