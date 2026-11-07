using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VAPI;
using UObject = UnityEngine.Object;

namespace NW
{
    public static class NWAssets
    {
        private const string ASSET_BUNDLE_NAME = "nwassets";
        private const string LOADING_SCREEN_SPRITES = "nwloadingscreen";
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";

        private static string assetBundleFolderPath => Path.Combine(Path.GetDirectoryName(NWMain.instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        public static ResourceAvailability assetsAvailability;

        private static AssetBundle _assetBundle;

        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAsset<TAsset>(name);
        }

        public static NWAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return NWAssetRequest<TAsset>.RequestFromPool(_assetBundle, name);        }

        public static TAsset[] LoadAllAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAllAssets<TAsset>();
        }

        public static NWAssetRequest<TAsset> LoadAllAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            return NWAssetRequest<TAsset>.RequestFromPool(_assetBundle);
        }

        internal static AssetBundle GetLoadingScreenBundle()
        {
            return AssetBundle.LoadFromFile(Path.Combine(assetBundleFolderPath, LOADING_SCREEN_SPRITES));
        }

        internal static IEnumerator Initialize()
        {
            if (assetsAvailability.available)
                yield break;

            NWLog.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundle();
            while (!loadRoutine.IsDone())
            {
                yield return null;
            }

            ParallelCoroutine coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(SwapShaders());
            coroutine.Add(SwapAddressableShaders());
            coroutine.Add(FinishMaterialVariants());

            while (!coroutine.isDone)
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            assetsAvailability.MakeAvailable();
        }

        private static IEnumerator FinishMaterialVariants()
        {
            var request = LoadAllAssetsAsync<MaterialVariant>();
            while (!request.isComplete)
                yield return null;

            ParallelCoroutine coroutine = ParallelCoroutine.RequestFromPool();
            foreach(var variant in request.assets)
            {
                coroutine.Add(variant.ApplyOverrides());
            }

            while (!coroutine.isDone)
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);
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

    public abstract class NWAssetRequest : IEnumerator
    {
        public abstract UObject boxedAsset { get; }
        public abstract UObject[] boxedAssets { get; }

        public bool isComplete
        {
            get
            {
                internalCoroutine ??= LoadAsset();

                return !internalCoroutine.MoveNext();
            }
        }

        object IEnumerator.Current => internalCoroutine?.Current;

        private IEnumerator internalCoroutine;

        protected abstract IEnumerator LoadAsset();

        bool IEnumerator.MoveNext()
        {
            return isComplete == false;
        }

        public virtual void Reset()
        {
            internalCoroutine = null;
        }
    }

    public sealed class NWAssetRequest<TAsset> : NWAssetRequest, IDisposable where TAsset : UObject
    {
        #region Pool implementation
        private static Stack<NWAssetRequest<TAsset>> _stack = new Stack<NWAssetRequest<TAsset>>();

        internal static NWAssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle);
            return request;
        }
        internal static NWAssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle, string name)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle, name);
            return request;
        }

        internal static void ReturnToPool(NWAssetRequest<TAsset> request)
        {
            _stack.Push(request);
        }

        private static NWAssetRequest<TAsset> GetExistingOrCreateRequest()
        {
            if (_stack.TryPop(out var assetRequest))
            {
                return assetRequest;
            }
            return new NWAssetRequest<TAsset>();
        }
        #endregion

        public override UObject boxedAsset => asset;

        public TAsset asset => _asset;
        private TAsset _asset;

        public override UObject[] boxedAssets => assets;

        public TAsset[] assets => _assets;
        private TAsset[] _assets;

        private bool isSingleAssetLoad => !string.IsNullOrWhiteSpace(_assetName);
        private string _assetName;
        private AssetBundle _assetBundle;
        private Type assetType => typeof(TAsset);

        protected override IEnumerator LoadAsset()
        {
            ThrowIfUninitialized();
            AssetBundleRequest request = null;

            if (isSingleAssetLoad)
            {
                request = _assetBundle.LoadAssetAsync<TAsset>(_assetName);
            }
            else
            {
                request = _assetBundle.LoadAllAssetsAsync<TAsset>();
            }

            while (!request.isDone)
                yield return null;

            if (isSingleAssetLoad)
            {
                _asset = (TAsset)request.asset;
#if DEBUG
                if (!_asset)
                {
                    NWLog.Warning($"Asset of type {typeof(TAsset).Name} with name {_assetName} was not found.");
                }
#endif
            }
            else
            {
                _assets = new TAsset[request.allAssets.Length];
                for (int i = 0; i < request.allAssets.Length; i++)
                {
                    _assets[i] = (TAsset)request.allAssets[i];
                }

#if DEBUG
                if (_assets == null || _assets.Length == 0)
                {
                    NWLog.Warning($"No Assets of type {typeof(TAsset).Name} exists within the VAPI Asset Bundle.");
                }
#endif
            }
        }

        /// <summary>
        /// Disposes this AssetRequest and returns it to the pool
        /// </summary>
        public void Dispose()
        {
            ThrowIfUninitialized();
            Reset();
            ReturnToPool(this);
        }

        public override void Reset()
        {
            ThrowIfUninitialized();
            base.Reset();

            _assetBundle = null;
            _assetName = null;

            _assets = null;
            _asset = null;
        }

        private void ThrowIfUninitialized()
        {
            if (_assetBundle == null || (_assetBundle == null && _assetName == null))
                throw new InvalidOperationException("You should not use a VAPIAssetRequest that has been disposed of.");
        }

        private void Init(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;
        }

        private void Init(AssetBundle assetBundle, string assetName)
        {
            _assetBundle = assetBundle;
            _assetName = assetName;
        }

        private NWAssetRequest() { }
    }
}
