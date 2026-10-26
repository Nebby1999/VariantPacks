using EntityStates;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VAPI;

namespace TO30
{
    public class TO30Content : IContentPackProvider
    {
        public string identifier => TO30Main.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(_contentPack);
        private static ContentPack _contentPack { get; } = new ContentPack();

        public static ReadOnlyVariantPack readOnlyVariantPack => new ReadOnlyVariantPack(_variantPack);
        private static VariantPack _variantPack;

        internal static ParallelCoroutine _parallelPreLoadDispatchers = ParallelCoroutine.RequestFromPool();

        private static Func<IEnumerator>[] _loadDispatchers;

        internal static ParallelCoroutine _parallelPostLoadDispatchers = ParallelCoroutine.RequestFromPool();

        private static Action[] _fieldAssignDispatchers = Array.Empty<Action>();

        private bool _initialized;


        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            if (_initialized)
                yield break;

            var enumerator = TO30Assets.Initialize();
            while (!enumerator.IsDone())
            {
                yield return null;
            }

            while (!_parallelPreLoadDispatchers.IsDone())
                yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f)); //report progress
                enumerator = _loadDispatchers[i](); //call method

                while (enumerator?.MoveNext() ?? false) yield return null; //await
            }

            while (!_parallelPostLoadDispatchers.IsDone())
                yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                _fieldAssignDispatchers[i]();
            }

            VariantPackManager.AddVariantPack(_variantPack);
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
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
            using var request = TO30Assets.LoadAssetAsync<AssetCollection>("acContentPack");

            while (!request.isComplete)
                yield return null;

            _contentPack.AddContentFromAssetCollection(request.asset);
        }

        private static IEnumerator AddVariantPack()
        {
            TO30Log.Info($"Adding Variants");

            using var variantDefsRequest = TO30Assets.LoadAssetsAsync<CharacterVariantDef>();
            using var spriteRequest = TO30Assets.LoadAssetAsync<Sprite>("texPackEnabledIcon");

            var parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(variantDefsRequest);
            parallelCoroutine.Add(spriteRequest);

            while (!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            _variantPack = new VariantPack(TO30Main.GUID, "TO30_VARIANTPACK_NAME", "TO30_VARIANPACK_TOOLTIP", "TO30_VARIANTPACK_DESC", spriteRequest.asset, TO30Main.instance, TO30Main.instance.Config, TO30Main.instance.Config);
            _variantPack.characterVariantDefs.Add(variantDefsRequest.assets);
        }

        private static IEnumerator AddStates()
        {
            TO30Log.Info("Adding EntityStates");
            yield return null;
            _contentPack.entityStateTypes.Add(typeof(TO30Content).Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EntityState)) && !t.IsAbstract)
            .ToArray());
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal TO30Content()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            TO30Assets.assetsAvailability.CallWhenAvailable(() => _parallelPostLoadDispatchers.Add(AsyncAssetLoadAttribute.CreateParallelCoroutineForMod(TO30Main.instance)));
        }

        static TO30Content()
        {
            _loadDispatchers = new Func<IEnumerator>[]
            {
                () => LanguageFileLoader.AddLanguageFilesFromModAsync(TO30Main.instance, "languages"),
                PopulateContentPackWithAssetCollection,
                AddStates,
                AddVariantPack,
            };
        }
    }
}