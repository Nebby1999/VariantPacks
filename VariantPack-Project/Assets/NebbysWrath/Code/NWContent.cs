using R2API.ScriptableObjects;
using System;
using VAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates;
using NW.Modules;
using RoR2.ContentManagement;
using MSU;
using System.Collections;
using TO30;
using RoR2;
using UnityEngine;

namespace NW
{
    public class NWContent : IContentPackProvider
    {
        public string identifier => NWMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);

        internal static ContentPack contentPack { get; } = new ContentPack();

        internal static ParallelMultiStartCoroutine _parallelPreLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Func<IEnumerator>[] _loadDispatchers;

        internal static ParallelMultiStartCoroutine _parallelPostLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Action[] _fieldAssignDispatchers = Array.Empty<Action>();

        private bool _initialized;

        private static VariantPackDef _variantPack;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            if (_initialized)
                yield break;

            var enumerator = NWAssets.Initialize();
            while (!enumerator.IsDone())
            {
                yield return null;
            }

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.isDone)
                yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f)); //report progress
                enumerator = _loadDispatchers[i](); //call method

                while (enumerator?.MoveNext() ?? false) yield return null; //await
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.isDone)
                yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                _fieldAssignDispatchers[i]();
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield return null;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private static IEnumerator PopulateContentPackWithAssetCollection()
        {
            var request = NWAssets.LoadAssetAsync<AssetCollection>("acContentPack");

            while (!request.isDone)
                yield return null;

            contentPack.AddContentFromAssetCollection(request.asset);
        }

        private static IEnumerator AddVariantPack()
        {
            NWLog.Info("Adding Variants");

            var variantPackRequest = NWAssets.LoadAssetAsync<VariantPackDef>("NWVariantPack");
            var variantDefsRequest = NWAssets.LoadAssetsAsync<VariantDef>();

            var coroutine = new ParallelCoroutine();
            coroutine.Add(variantPackRequest);
            coroutine.Add(variantDefsRequest);

            while (!coroutine.IsDone())
                yield return null;

            _variantPack = variantPackRequest.asset;
            _variantPack.variants = variantDefsRequest.assets;
            VariantPackCatalog.AddVariantPack(_variantPack, TO30Main.instance.Config);
        }

        private static IEnumerator AddStates()
        {
            NWLog.Info("Adding EntityStates");
            yield return null;
            contentPack.entityStateTypes.Add(typeof(TO30Content).Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EntityState)) && !t.IsAbstract)
            .ToArray());
        }

        private IEnumerator CallAsyncLoadAttributes()
        {
            var routine = AsyncAssetLoadAttribute.CreateCoroutineForMod(NWMain.instance);

            routine.Start();
            while (!routine.isDone)
                yield return null;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public NWContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            NWAssets.assetsAvailability.CallWhenAvailable(() => _parallelPostLoadDispatchers.Add(CallAsyncLoadAttributes));
        }

        static NWContent()
        {
            var main = NWMain.instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {
                () => LanguageFileLoader.AddLanguageFilesFromModAsync(main, "languages"),
                PopulateContentPackWithAssetCollection,
                AddStates,
                () =>
                {
                    NWLog.Info($"Initializing Damage Types.");
                    IContentPieceProvider provider = ContentUtil.CreateContentPieceProvider<IDamageTypeContent>(main, contentPack);
                    return DamageTypeModule.Initialize(provider);
                },
                () =>
                {
                    NWLog.Info($"Initializing Cloned Prefabs.");

                    IContentPieceProvider<GameObject> provider = ContentUtil.CreateGameObjectGenericContentPieceProvider<ClonedPrefabBehaviour>(main, contentPack);

                    return PrefabCloneModule.Initialize(provider);
                },
                AddVariantPack,
            };
        }
    }
}