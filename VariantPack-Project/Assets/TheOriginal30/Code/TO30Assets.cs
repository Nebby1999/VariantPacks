using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VAPI;
using UObject = UnityEngine.Object;

namespace TO30
{
    public static class TO30Assets
    {
        private const string ASSET_BUNDLE_NAME = "to30assets";
        private const string LOADING_SCREEN_SPRITES = "to30loadingscreen";
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
            return TO30AssetRequest<TAsset>.RequestFromPool(_assetBundle, name);
        }

        public static TAsset[] LoadAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAllAssets<TAsset>();
        }

        public static TO30AssetRequest<TAsset> LoadAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            return TO30AssetRequest<TAsset>.RequestFromPool(_assetBundle);
        }

        internal static AssetBundle GetLoadingScreenBundle()
        {
            return AssetBundle.LoadFromFile(Path.Combine(assetBundleFolderPath, LOADING_SCREEN_SPRITES));
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

            ParallelCoroutine coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(SwapShaders());
            coroutine.Add(SwapAddressableShaders());
            coroutine.Add(FinishMaterialVariants());

            while (!coroutine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            assetsAvailability.MakeAvailable();
        }

        private static IEnumerator LoadAssetBundle()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(assetBundleFolderPath, ASSET_BUNDLE_NAME));

            while (!request.isDone)
                yield return null;

            _assetBundle = request.assetBundle;
        }

        private static IEnumerator FinishMaterialVariants()
        {
            using var request = LoadAssetsAsync<MaterialVariant>();

            while (!request.isComplete)
                yield return null;

            ParallelCoroutine routine = ParallelCoroutine.RequestFromPool();

            foreach (var variant in request.assets)
            {
                routine.Add(variant.ApplyOverrides());
            }

            while (!routine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(routine);
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

    /// <summary>
    /// An abstract class that represents an Async request for a VarianceAPI Asset.
    /// <br></br>
    /// Look at <see cref="VAPIAssetRequest{TAsset}"/> for a generic version
    /// </summary>
    public abstract class TO30AssetRequest : IEnumerator
    {
        /// <summary>
        /// The loaded asset, boxed in a <see cref="UnityEngine.Object"/>
        /// </summary>
        public abstract UObject boxedAsset { get; }
        /// <summary>
        /// The Loaded assets, boxed in an array of <see cref="UnityEngine.Object"/>
        /// </summary>
        public abstract UObject[] boxedAssets { get; }

        /// <summary>
        /// Returns true if the AssetRequest has finished loading
        /// </summary>
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

        /// <summary>
        /// Implement the asset loading here
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator LoadAsset();

        bool IEnumerator.MoveNext()
        {
            return isComplete == false;
        }

        /// <summary>
        /// Resets the asset request, you should not call this yourself.
        /// </summary>
        public virtual void Reset()
        {
            internalCoroutine = null;
        }
    }

    /// <summary>
    /// A class that represents an async asset request from VarianceAPI's assetbundle.
    /// <para></para>
    /// This class is pooled internally, to return the request to the pool, call <see cref="Dispose"/>.
    /// </summary>
    /// <typeparam name="TAsset">The type of asset being loaded</typeparam>
    public sealed class TO30AssetRequest<TAsset> : TO30AssetRequest, IDisposable where TAsset : UObject
    {
        #region Pool implementation
        private static Stack<TO30AssetRequest<TAsset>> _stack = new Stack<TO30AssetRequest<TAsset>>();

        internal static TO30AssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle);
            return request;
        }
        internal static TO30AssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle, string name)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle, name);
            return request;
        }

        internal static void ReturnToPool(TO30AssetRequest<TAsset> request)
        {
            _stack.Push(request);
        }

        private static TO30AssetRequest<TAsset> GetExistingOrCreateRequest()
        {
            if (_stack.TryPop(out var assetRequest))
            {
                return assetRequest;
            }
            return new TO30AssetRequest<TAsset>();
        }
        #endregion

        public override UObject boxedAsset => asset;

        /// <summary>
        /// The loaded asset
        /// </summary>
        public TAsset asset => _asset;
        private TAsset _asset;

        public override UObject[] boxedAssets => assets;
        /// <summary>
        /// The array of loaded assets
        /// </summary>
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
                    TO30Log.Warning($"Asset of type {typeof(TAsset).Name} with name {_assetName} was not found.");
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
                    TO30Log.Warning($"No Assets of type {typeof(TAsset).Name} exists within the VAPI Asset Bundle.");
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

        private TO30AssetRequest() { }
    }
}
