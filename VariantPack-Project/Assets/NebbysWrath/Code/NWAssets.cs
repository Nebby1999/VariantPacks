using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NW
{
    public class NWAssets : AssetsLoader<NWAssets>
    {
        public string AssemblyDir => Path.GetDirectoryName(NWMain.Instance.Info.Location);
        public override AssetBundle MainAssetBundle => _assetBundle;
        private AssetBundle _assetBundle;
        internal void Init()
        {
            var bundlePath = Path.Combine(AssemblyDir, "assetbundles", "nwassets");
            _assetBundle = AssetBundle.LoadFromFile(bundlePath);

            FinalizeMaterialsWithAddressableMaterialShader(_assetBundle);
            SwapShadersFromMaterialsInBundle(_assetBundle);
        }
    }
}
