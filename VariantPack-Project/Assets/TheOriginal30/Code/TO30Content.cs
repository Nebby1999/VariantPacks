using EntityStates;
using MSU;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
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
        public static ReadOnlyVariantPack readOnlyVariantPack => _variantPack != null ? new ReadOnlyVariantPack(_variantPack) : default;

        private static ContentPack _contentPack { get; } = new ContentPack();
        private static VariantPack _variantPack { get; set; } //Created on demand during load static content async

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var initialization = InitializeAssetsAndLang();
            while (!initialization.IsDone())
            {
                yield return null;
            }

            var contentPackPopulation = PopulateContentPackWithAssetCollection();
            while(contentPackPopulation.MoveNext())
            {
                yield return null;
            }

            var variantPackPopulation = PopulateVariantPack();
            while(variantPackPopulation.MoveNext())
            {
                yield return null;
            }

            VariantPackManager.AddVariantPack(_variantPack);
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

            var asyncAssetLoadCoroutine = AsyncAssetLoadAttribute.CreateParallelCoroutineForMod(TO30Main.instance);
            while(!asyncAssetLoadCoroutine.isDone)
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
            var parallelProgressCoroutine = ParallelCoroutine.RequestFromPool();

            parallelProgressCoroutine.Add(TO30Assets.Initialize());
            parallelProgressCoroutine.Add(LanguageFileLoader.AddLanguageFilesFromModAsync(TO30Main.instance, "languages"));

            while(!parallelProgressCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelProgressCoroutine);
        }

        private IEnumerator PopulateContentPackWithAssetCollection()
        {
            ParallelCoroutine parallelLoadCoroutine = ParallelCoroutine.RequestFromPool();

            using var skillRequest = TO30Assets.LoadAllAssetsAsync<SkillDef>();
            using var entityStates = TO30Assets.LoadAssetAsync<EntityStateTypeCollection>("EntityStates");

            parallelLoadCoroutine.Add(skillRequest);
            parallelLoadCoroutine.Add(entityStates);

            while(!parallelLoadCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelLoadCoroutine);

            _contentPack.skillDefs.Add(skillRequest.assets);
            Type[] types = new Type[entityStates.asset.stateTypes.Length];
            for (int i = 0; i < entityStates.asset.stateTypes.Length; i++)
            {
                SerializableEntityStateType t = entityStates.asset.stateTypes[i];
                types[i] = t.stateType;
            }
        }

        private IEnumerator PopulateVariantPack()
        {
            TO30Log.Info($"Adding Variants");

            using var variantDefsRequest = TO30Assets.LoadAllAssetsAsync<CharacterVariantDef>();
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

            ConfiguredBool addAllyVariants = new ConfiguredBool(true)
                .WithModGUID(TO30Main.instance)
                .WithModName(TO30Main.instance)
                .WithConfigFile(TO30Main.instance.Config)
                .WithSection("General")
                .WithKey("Enable Ally Variants")
                .WithDescription("Enables \"Ally\" Variants for the main allied enemy bodies in the game. Devoted Lemurians may spawn as Flamethrower Lemurians for example.")
                .WithCheckBoxConfig(new CheckBoxConfig
                {
                    restartRequired = true
                })
                .DoConfigure();
            ConfiguredBool addPolymorphLemurianVariants = new ConfiguredBool(true)
                .WithModGUID(TO30Main.instance)
                .WithModName(TO30Main.instance)
                .WithConfigFile(TO30Main.instance.Config)
                .WithSection("General")
                .WithKey("Enable Polymorph Lemurian Variants")
                .WithDescription("Enables \"Polymorph Lemurian\" Variants. Starting the Polymorph Lemurian quest may spawn you as a Flamethrower Lemurian for example.")
                .WithCheckBoxConfig(new CheckBoxConfig
                {
                    restartRequired = true
                })
                .DoConfigure();

            parallelCoroutine = ParallelCoroutine.RequestFromPool();

            if(addAllyVariants)
            {
                parallelCoroutine.Add(AddAllyVariants());
            }
            if(addPolymorphLemurianVariants)
            {
                parallelCoroutine.Add(AddPolymorphLemurianVariants());
            }

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);
        }

        private static IEnumerator AddAllyVariants()
        {
            TO30Log.Info($"Adding Ally Variants");
            using var beetleGuardCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acBeetleGuardVariants");
            using var elderLemurianCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acElderLemurianVariants");
            using var lemurianCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acLemurianVariants");

            ParallelCoroutine parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(beetleGuardCollection);
            parallelCoroutine.Add(elderLemurianCollection);
            parallelCoroutine.Add(lemurianCollection);

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            foreach(var asset in beetleGuardCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoBeetleGuardGland(asset);
                if(!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a BeetleGuardAlly clone.");
                    continue;
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
            foreach (var asset in elderLemurianCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoDevotedLemurianBruiser(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a DevotedLemurianBruiser clone.");
                    continue;
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
            foreach (var asset in lemurianCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoDevotedLemurian(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a DevotedLemurian clone.");
                    continue;
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private static IEnumerator AddPolymorphLemurianVariants()
        {
            TO30Log.Info($"Adding Polymorph Lemurian Variants");

            using var lemurianCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acLemurianVariants");

            while(!lemurianCollection.isComplete)
            {
                yield return null;
            }

            foreach (var asset in lemurianCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VAPIUtils.CloneIntoLemurianPolymorph(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a LemurianPolymorph clone.");
                    continue;
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal TO30Content()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}