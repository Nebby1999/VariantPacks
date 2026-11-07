using BepInEx;
using MSU.Config;

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