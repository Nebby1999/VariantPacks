using BepInEx;
using MSU;
using MSU.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW
{
    public class NWConfig
    {
        public const string PREFIX = "NW.";

        internal static ConfigFactory configFactory { get; private set; }

        internal NWConfig(BaseUnityPlugin plugin)
        {
            configFactory = new ConfigFactory(plugin);
        }
    }
}