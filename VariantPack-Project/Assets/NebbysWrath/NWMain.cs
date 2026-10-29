using BepInEx;
using MSU;
using R2API.Utils;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace NW
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(VAPI.VAPIMain.GUID)]
    [BepInDependency(LoadingScreenFix.LoadingScreenFix.PluginGUID)]
    [BepInDependency(TO30.TO30Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class NWMain : BaseUnityPlugin
    {
        public const string GUID = "com.Nebby.NW";
        public const string MODNAME = "Nebbys Wrath";
        public const string VERSION = "2.1.2";

        public static NWMain instance { get; private set; }
        private void Awake()
        {
            instance = this;
            new NWLog(Logger);
            new NWConfig(this);

            new NWContent();

            LoadingScreenFix.LoadingScreenFix.AddSpriteAnimations(NWAssets.GetLoadingScreenBundle());
            RoR2Application.onLoad += OnLoad;
        }

        private void OnLoad()
        {
            if (MSUtil.IsModInstalled(TO30.TO30Main.GUID))
            {
                AddJellyfishSummons();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddJellyfishSummons()
        {

            if (MSUtil.IsModInstalled("com.Moffein.ClayMen"))
            {
               TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("MoffeinClayManMaster"));
            }
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("HermitCrabMaster"));
            TO30.Components.SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath.AddMaster(MasterCatalog.FindMasterIndex("RoboBallMiniMaster"));
        }
    }
}
