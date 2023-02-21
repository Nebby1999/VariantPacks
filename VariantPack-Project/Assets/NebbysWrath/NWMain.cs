using BepInEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using R2API.Utils;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
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
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class NWMain : BaseUnityPlugin
    {
        public const string GUID = "com.Nebby.NW";
        public const string MODNAME = "Nebbys Wrath";
        public const string VERSION = "2.0.1";

        public static NWMain Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            new Log(Logger);
            new NWConfig().Init();
            new NWAssets().Init();
            new NWLang().Init();
            new NWContent().Init();

            ConfigurableFieldManager.AddMod(this);
            RoR2Application.onLoad += AddSpectralSummons;
        }

        private void AddSpectralSummons()
        {
            var validMasters = EntityStates.JellyfishMonster.Spectral.SpawnRandomLesserEnemyVariant.validMasters;
            if(MSUtil.IsModInstalled("com.Moffein.ClayMen"))
            {
                validMasters.Add(MasterCatalog.FindMasterIndex("MoffeinClayManMaster"));
            }
            validMasters.Add(MasterCatalog.FindMasterIndex("HermitCrabMaster"));
            validMasters.Add(MasterCatalog.FindMasterIndex("RoboBallMiniMaster"));
        }
    }
}
