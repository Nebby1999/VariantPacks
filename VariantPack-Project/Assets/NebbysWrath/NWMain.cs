using BepInEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSU;
using R2API.Utils;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using VAPI;

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
        public const string VERSION = "2.0.2";

        public static NWMain instance { get; private set; }
        private void Awake()
        {
            instance = this;
            new NWLog(Logger);
            new NWConfig(this);

            new NWContent();

            RoR2Application.onLoad += OnLoad;
        }

        private void OnLoad()
        {
            if (MSUtil.IsModInstalled("com.Nebby.TO30"))
            {
                AddJellyfishSummons();
            }
        }

        private void AddJellyfishSummons()
        {
            var validMasters = EntityStates.JellyfishMonster.Spectral.SpawnRandomLesserEnemyVariant.validMasters;
            if (MSUtil.IsModInstalled("com.Moffein.ClayMen"))
            {
                validMasters.Add(MasterCatalog.FindMasterIndex("MoffeinClayManMaster"));
            }
            validMasters.Add(MasterCatalog.FindMasterIndex("HermitCrabMaster"));
            validMasters.Add(MasterCatalog.FindMasterIndex("RoboBallMiniMaster"));
        }
    }
}
