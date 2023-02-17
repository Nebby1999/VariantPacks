using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TO30
{
    public class TO30Assets : AssetsLoader<TO30Assets>
    {
        public string AssemblyDir => Path.GetDirectoryName(TO30Main.Instance.Info.Location);
        public override AssetBundle MainAssetBundle => _assetBundle;
        private AssetBundle _assetBundle;

        private static List<Material> modifiedAddressableMaterials = new List<Material>();
        internal void Init()
        {
            var bundlePath = Path.Combine(AssemblyDir, "assetbundles", "to30assets");
            _assetBundle = AssetBundle.LoadFromFile(bundlePath);

            FinalizeMaterialsWithAddressableMaterialShader(_assetBundle);
        }
    }
}
