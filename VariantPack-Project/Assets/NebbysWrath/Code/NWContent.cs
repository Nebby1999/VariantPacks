using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI;

namespace NW
{
    public class NWContent : IContentPackProvider
    {
        public static class CharacterVariants
        {
            public static CharacterVariantDef Goop;
            public static CharacterVariantDef TarredGup;
            public static CharacterVariantDef TarredGeep;
            public static CharacterVariantDef TarredGip;

            public static CharacterVariantDef GreaterStoneWisp;

            public static CharacterVariantDef SwarmerProbe;
        }

        public string identifier => NWMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(_contentPack);
        public static ReadOnlyVariantPack readOnlyVariantPack => _variantPack != null ? new ReadOnlyVariantPack(_variantPack) : default;

        private static ContentPack _contentPack { get; } = new ContentPack();
        private static VariantPack _variantPack { get; set; }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var initialization = InitializeAssetsAndLang();
            while (!initialization.IsDone())
            {
                yield return null;
            }

            var contentPackPopulation = PopulateContentPack();
            while (contentPackPopulation.MoveNext())
            {
                yield return null;
            }

            var variantPackPopulation = PopulateVariantPack();
            while (variantPackPopulation.MoveNext())
            {
                yield return null;
            }

            VariantPackManager.AddVariantPack(_variantPack);

            ContentLoadHelper.PopulateTypeFields(typeof(CharacterVariants), _variantPack.characterVariantDefs, null);
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            args.ReportProgress(0f);
            yield return null;

            ContentPack.Copy(_contentPack, args.output);

            yield return null;
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(0f);
            yield return null;

            var asyncAssetLoadCoroutine = AsyncAssetLoadAttribute.CreateParallelCoroutineForMod(NWMain.instance);
            while (!asyncAssetLoadCoroutine.isDone)
            {
                yield return null;
            }
            ParallelCoroutine.ReturnToPool(asyncAssetLoadCoroutine);

            yield return null;
            args.ReportProgress(1f);
            yield break;
        }

        private IEnumerator InitializeAssetsAndLang()
        {
            var parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(NWAssets.Initialize());
            parallelCoroutine.Add(LanguageFileLoader.AddLanguageFilesFromModAsync(NWMain.instance, "languages"));

            while(parallelCoroutine.isDone == false)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);
        }

        private IEnumerator PopulateContentPack()
        {
            ParallelCoroutine parallelCoroutine = ParallelCoroutine.RequestFromPool();

            using var skillRequest = NWAssets.LoadAllAssetsAsync<SkillDef>();
            using var entityStates = NWAssets.LoadAssetAsync<EntityStateTypeCollection>("EntityStates");

            parallelCoroutine.Add(entityStates);
            parallelCoroutine.Add(skillRequest);

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            _contentPack.skillDefs.Add(skillRequest.assets);
            Type[] entityStateTypes = new Type[entityStates.asset.stateTypes.Length];
            for(int i = 0; i < entityStateTypes.Length; i++)
            {
                entityStateTypes[i] = entityStates.asset.stateTypes[i].stateType;
            }
            _contentPack.entityStateTypes.Add(entityStateTypes);
        }


        private static List<CharacterVariantDef> _dependantSpawnAllyDefs = new List<CharacterVariantDef>();
        private static List<CharacterVariantDef> _dependantSpawnPolymorphDefs = new List<CharacterVariantDef>();

        private IEnumerator PopulateVariantPack()
        {
            NWLog.Info($"Adding Variants");

            using var variantDefsRequest = NWAssets.LoadAllAssetsAsync<CharacterVariantDef>();
            using var spriteRequest = NWAssets.LoadAssetAsync<Sprite>("texPackEnabledIcon");

            var parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(variantDefsRequest);
            parallelCoroutine.Add(spriteRequest);

            while (!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            _variantPack = new VariantPack(NWMain.GUID, "NW_VARIANTPACK_NAME", "NW_VARIANPACK_TOOLTIP", "NW_VARIANTPACK_DESC", spriteRequest.asset, NWMain.instance, NWMain.instance.Config, NWMain.instance.Config);
            _variantPack.characterVariantDefs.Add(variantDefsRequest.assets);

            ConfiguredBool addAllyVariants = new ConfiguredBool(true)
                .WithModGUID(NWMain.instance)
                .WithModName(NWMain.instance)
                .WithConfigFile(NWMain.instance.Config)
                .WithSection("General")
                .WithKey("Enable Ally Variants")
                .WithDescription("Enables \"Ally\" Variants for the main allied enemy bodies in the game. Quiet Probe may spawn as Gaussian Probes for example.")
                .WithConfigChange((b) =>
                {
                    foreach (var variantDef in _dependantSpawnAllyDefs)
                    {
                        VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                        condition.cannotSpawn = b == false;
                        variantDef.spawnCondition = condition;
                    }
                })
                .DoConfigure();

            parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(AddAllyVariants(addAllyVariants));

            while (!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);
        }

        private static IEnumerator AddAllyVariants(bool canSpawn)
        {
            NWLog.Info($"Adding Ally Variants");

            using var alphaConstructCollection = NWAssets.LoadAssetAsync<AssetCollection>("acAlphaConstructVariants");
            using var solusProbeCollection = NWAssets.LoadAssetAsync<AssetCollection>("acSolusProbeCollection");

            ParallelCoroutine parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(alphaConstructCollection);
            parallelCoroutine.Add(solusProbeCollection);

            while (!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            foreach (var asset in alphaConstructCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoMinorConstructOnKill(asset);
                if (!cloned)
                {
                    NWLog.Error($"Failed to clone {asset} into a MinorConstructOnKill clone.");
                    continue;
                }

                if (cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }

            //Green robo ball
            foreach (var asset in solusProbeCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoRoboBallGreenBuddy(asset);
                if (!cloned)
                {
                    NWLog.Error($"Failed to clone {asset} into a RoboBallGreenBuddy clone.");
                    continue;
                }

                if (cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }

            //Red robo ball
            foreach (var asset in solusProbeCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoRoboBallRedBuddy(asset);
                if (!cloned)
                {
                    NWLog.Error($"Failed to clone {asset} into a RoboBallRedBuddy clone.");
                    continue;
                }

                if (cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal NWContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
        /*public string identifier => NWMain.GUID;

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
        }*/
    }
}