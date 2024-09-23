using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VAPI;

namespace TO30
{
    public static class TO30Assets
    {
        private const string ASSET_BUNDLE_NAME = "to30assets";
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";

        private static string assetBundleFolderPath => Path.Combine(Path.GetDirectoryName(TO30Main.instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        public static ResourceAvailability assetsAvailability;

        private static AssetBundle _assetBundle;

        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAsset<TAsset>(name);
        }

        public static TO30AssetRequest<TAsset> LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return new TO30AssetRequest<TAsset>(_assetBundle.LoadAssetAsync(name));
        }

        public static TAsset[] LoadAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAllAssets<TAsset>();
        }

        public static TO30AssetRequest<TAsset> LoadAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            return new TO30AssetRequest<TAsset>(_assetBundle.LoadAllAssetsAsync<TAsset>());
        }

        internal static IEnumerator Initialize()
        {
            if (assetsAvailability.available)
                yield break;

            TO30Log.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundle();
            while (!loadRoutine.IsDone())
            {
                yield return null;
            }

            ParallelMultiStartCoroutine coroutine = new ParallelMultiStartCoroutine();

            coroutine.Add(SwapShaders);
            coroutine.Add(SwapAddressableShaders);

            coroutine.Start();
            while (!coroutine.isDone)
                yield return null;

            assetsAvailability.MakeAvailable();
        }

        private static IEnumerator LoadAssetBundle()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(assetBundleFolderPath, ASSET_BUNDLE_NAME));

            while (!request.isDone)
                yield return null;

            _assetBundle = request.assetBundle;
        }

        private static IEnumerator SwapShaders()
        {
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundle);
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundle);
        }
    }

    public class TO30AssetRequest<TAsset> : IEnumerator where TAsset : UnityEngine.Object
    {
        public TAsset asset => (TAsset)_request.asset;
        public TAsset[] assets => _request.allAssets.OfType<TAsset>().ToArray();
        public bool isDone => _request.isDone;
        public float progress => _request.progress;

        object IEnumerator.Current => _request.asset;

        private AssetBundleRequest _request;
        internal TO30AssetRequest(AssetBundleRequest request)
        {
            _request = request;
        }

        bool IEnumerator.MoveNext()
        {
            return !_request.isDone;
        }

        void IEnumerator.Reset()
        {
            throw new System.NotSupportedException();
        }
    }
}
