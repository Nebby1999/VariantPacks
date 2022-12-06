using BepInEx;
using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TO30
{
    public class TO30Config : ConfigLoader<TO30Config>
    {
        public override BaseUnityPlugin MainClass => TO30Main.Instance;
        public override bool CreateSubFolder => true;

        internal void Init()
        {

        }
    }
}