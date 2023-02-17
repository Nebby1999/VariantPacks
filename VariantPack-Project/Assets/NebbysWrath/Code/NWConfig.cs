using BepInEx;
using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW
{
    public class NWConfig : ConfigLoader<NWConfig>
    {
        public override BaseUnityPlugin MainClass => NWMain.Instance;
        public override bool CreateSubFolder => true;

        internal void Init()
        {

        }
    }
}