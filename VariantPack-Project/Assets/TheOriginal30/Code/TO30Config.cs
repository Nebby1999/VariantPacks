using BepInEx;
using MSU;
using MSU.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TO30
{
    public class TO30Config
    {
        public const string PREFIX = "TO30.";

        internal static ConfigFactory configFactory { get; private set; }

        internal TO30Config(BaseUnityPlugin plugin)
        {
            configFactory = new ConfigFactory(plugin);
        }
    }
}