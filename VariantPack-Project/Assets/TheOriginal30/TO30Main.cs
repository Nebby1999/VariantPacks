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

[assembly: HG.Reflection.SearchableAttribute.OptIn]

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace TO30
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(VAPI.Legacy.VAPIMain.GUID)]
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
            var validMasters = EntityStates.JellyfishMonster.Spectral.SpawnRandomLesserEnemyVariant.validMasters;
            validMasters.AddRange(new List<MasterCatalog.MasterIndex>
            {
                MasterCatalog.FindMasterIndex("BeetleMaster"),
                MasterCatalog.FindMasterIndex("ImpMaster"),
                MasterCatalog.FindMasterIndex("LemurianMaster"),
                MasterCatalog.FindMasterIndex("WispMaster")
            });
        }
    }
}
