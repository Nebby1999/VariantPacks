using EntityStates;
using MSU;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VAPI;

namespace TO30
{
    public class TO30Content : IContentPackProvider
    {
        public static class CharacterVariants
        {
            public static CharacterVariantDef AlbinoBison;
            public static CharacterVariantDef AlloyDeagle;
            public static CharacterVariantDef AlmostGreatWisp;
            public static CharacterVariantDef ArmoredBeetle;
            public static CharacterVariantDef ArtilleryVulture;
            public static CharacterVariantDef BadassLemurian;
            public static CharacterVariantDef BattleBeetle;
            public static CharacterVariantDef BeetleGuardBrute;
            public static CharacterVariantDef BeetleGuardShaprshooter;
            public static CharacterVariantDef ClottedImp;
            public static CharacterVariantDef CursedJellyfish;
            public static CharacterVariantDef DreamScavenger;
            public static CharacterVariantDef EnormousStoneTitan;
            public static CharacterVariantDef FlamethrowerLemurian;
            public static CharacterVariantDef FuckingTemplar;
            public static CharacterVariantDef FullAutoGolem;
            public static CharacterVariantDef GreatestWisp;
            public static CharacterVariantDef InfernalWisp;
            public static CharacterVariantDef MOAJ;
            public static CharacterVariantDef MoltenElderLemurian;
            public static CharacterVariantDef NuclearJellyfish;
            public static CharacterVariantDef OverchargedGolem;
            public static CharacterVariantDef PygmyStoneTitan;
            public static CharacterVariantDef RushGolem;
            public static CharacterVariantDef SpectralJellyfish;
            public static CharacterVariantDef SpeedyBeetle;
            public static CharacterVariantDef SpeedyBison;
            public static CharacterVariantDef TitanishGolem;
            public static CharacterVariantDef ToxicBeetle;
            public static CharacterVariantDef UnstableVagrant;

            public static CharacterVariantDef DevotedBadassLemurian;
            public static CharacterVariantDef DevotedFlamethrowerLemurian;

            public static CharacterVariantDef DevotedMoltenElderLemurian;

            public static CharacterVariantDef GlandBeetleGuardBrute;
            public static CharacterVariantDef GlandBeetleGuardSharpshooter;

            public static CharacterVariantDef PolymorphBadassLemurian;
            public static CharacterVariantDef PolymorphFlamethrowerLemurian;
        }
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
            _contentPack.entityStateTypes.Add(types);
        }

        private static List<CharacterVariantDef> _dependantSpawnAllyDefs = new List<CharacterVariantDef>();
        private static List<CharacterVariantDef> _dependantSpawnPolymorphDefs = new List<CharacterVariantDef>();

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
                .WithConfigChange((b) =>
                {
                    foreach(var variantDef in _dependantSpawnAllyDefs)
                    {
                        VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                        condition.cannotSpawn = b == false; 
                        variantDef.spawnCondition = condition;
                    }
                })
                .DoConfigure();
            ConfiguredBool addPolymorphLemurianVariants = new ConfiguredBool(true)
                .WithModGUID(TO30Main.instance)
                .WithModName(TO30Main.instance)
                .WithConfigFile(TO30Main.instance.Config)
                .WithSection("General")
                .WithKey("Enable Polymorph Lemurian Variants")
                .WithDescription("Enables \"Polymorph Lemurian\" Variants. Starting the Polymorph Lemurian quest may spawn you as a Flamethrower Lemurian for example.")
                .WithConfigChange((b) =>
                {
                    foreach(var variantDef in _dependantSpawnPolymorphDefs)
                    {
                        VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                        condition.cannotSpawn = b == false;
                        variantDef.spawnCondition = condition;
                    }
                })
                .DoConfigure();

            parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(AddAllyVariants(addAllyVariants));
            parallelCoroutine.Add(AddPolymorphLemurianVariants(addPolymorphLemurianVariants));

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);
        }

        private static IEnumerator AddAllyVariants(bool canSpawn)
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

                if(asset.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    asset.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(asset);
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

                if (asset.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    asset.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(asset);
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

                if (asset.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    asset.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(asset);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private static IEnumerator AddPolymorphLemurianVariants(bool canSpawn)
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

                if (asset.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    asset.spawnCondition = dependantCondition;
                    _dependantSpawnPolymorphDefs.Add(asset);
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