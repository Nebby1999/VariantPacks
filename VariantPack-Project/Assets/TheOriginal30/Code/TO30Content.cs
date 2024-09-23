using R2API.ScriptableObjects;
using System;
using VAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates;
using RoR2.ContentManagement;
using System.Collections;
using MSU;
using RoR2;

namespace TO30
{
    public class TO30Content : IContentPackProvider
    {
        public string identifier => TO30Main.GUID;

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

            var enumerator = TO30Assets.Initialize();
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
            var request = TO30Assets.LoadAssetAsync<AssetCollection>("acContentPack");

            while (!request.isDone)
                yield return null;

            contentPack.AddContentFromAssetCollection(request.asset);
        }

        private static IEnumerator AddVariantPack()
        {
            TO30Log.Info("Adding Variants");

            var variantPackRequest = TO30Assets.LoadAssetAsync<VariantPackDef>("TO30VariantPack");
            var variantDefsRequest = TO30Assets.LoadAssetsAsync<VariantDef>();

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
            TO30Log.Info("Adding EntityStates");
            yield return null;
            contentPack.entityStateTypes.Add(typeof(TO30Content).Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EntityState)) && !t.IsAbstract)
            .ToArray());
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        private IEnumerator CallAsyncLoadAttributes()
        {
            var routine = AsyncAssetLoadAttribute.CreateCoroutineForMod(TO30Main.instance);

            routine.Start();
            while (!routine.isDone)
                yield return null;
        }

        internal TO30Content()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            TO30Assets.assetsAvailability.CallWhenAvailable(() => _parallelPostLoadDispatchers.Add(CallAsyncLoadAttributes));
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