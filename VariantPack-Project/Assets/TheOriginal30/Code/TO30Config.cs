using BepInEx;
using MSU.Config;

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