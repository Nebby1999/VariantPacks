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
        public const string VERSION = "2.0.0";

        public static TO30Main Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            new Log(Logger);
            new TO30Config().Init();
            new TO30Assets().Init();
            new TO30Lang().Init();
            new TO30Content().Init();

            ConfigurableFieldManager.AddMod(this);
            TokenModifierManager.AddToManager();
        }
    }
}
