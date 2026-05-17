using R2API.ScriptableObjects;
using System;
using VAPI.Legacy;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates;
using NW.Modules;
using RoR2.ContentManagement;
using MSU;
using System.Collections;
using RoR2;
using UnityEngine;

namespace NW
{
    public class NWContent : IContentPackProvider
    {
        public string identifier => NWMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);

        internal static ContentPack contentPack { get; } = new ContentPack();

        internal static ParallelCoroutine _parallelPreLoadDispatchers = new ParallelCoroutine();

        private static Func<IEnumerator>[] _loadDispatchers;

        internal static ParallelCoroutine _parallelPostLoadDispatchers = new ParallelCoroutine();

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

            while (!_parallelPreLoadDispatchers.isDone)
                yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f)); //report progress
                enumerator = _loadDispatchers[i](); //call method

                while (enumerator?.MoveNext() ?? false) yield return null; //await
            }

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
            VariantPackCatalog.AddVariantPack(_variantPack, NWMain.instance.Config);
        }

        private static IEnumerator AddGupDefsToGupVariantHandler()
        {
            var goopDef = NWAssets.LoadAssetAsync<VariantDef>("Goop");
            var tarredGupDef = NWAssets.LoadAssetAsync<VariantDef>("TarredGup");
            var tarredGeepDef = NWAssets.LoadAssetAsync<VariantDef>("TarredGeep");
            var tarredGipDef = NWAssets.LoadAssetAsync<VariantDef>("TarredGip");

            ParallelCoroutine coroutine = new ParallelCoroutine();
            coroutine.Add(goopDef);
            coroutine.Add(tarredGupDef);
            coroutine.Add(tarredGeepDef);
            coroutine.Add(tarredGipDef);

            while (!coroutine.IsDone())
                yield return null;

            var goop = goopDef.asset;
            var tarredGup = tarredGupDef.asset;
            var tarredGeep = tarredGeepDef.asset;
            var tarredGip = tarredGipDef.asset;

            GupVariantHelper.AddToBlacklist(goop);
            GupVariantHelper.AddGupProgression(tarredGup, tarredGeep, tarredGip);
        }

        private static IEnumerator AddStates()
        {
            NWLog.Info("Adding EntityStates");
            yield return null;
            contentPack.entityStateTypes.Add(typeof(NWMain).Assembly
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
            NWAssets.assetsAvailability.CallWhenAvailable(() =>
            {
                _parallelPostLoadDispatchers.Add(AsyncAssetLoadAttribute.CreateParallelCoroutineForMod(NWMain.instance));
                _parallelPostLoadDispatchers.Add(AddGupDefsToGupVariantHandler());
            });
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
                () =>
                {
                    NWLog.Info($"Initializing Variant Items");

                    IContentPieceProvider<ItemDef> provider = ContentUtil.CreateGenericContentPieceProvider<ItemDef>(main, contentPack);
                    ItemModule.AddProvider(main, provider);
                    return ItemModule.InitializeItems(main);
                },
                AddVariantPack,
            };
        }
    }
}