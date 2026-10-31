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
            public static CharacterVariantDef BeetleGuardSharpshooter;
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

            public static CharacterVariantDef SoulAlmostGreatWisp;
            public static CharacterVariantDef SoulInfernalWisp;

            public static CharacterVariantDef GuraguraDreamScavenger;
            public static CharacterVariantDef KipkipDreamScavenger;
            public static CharacterVariantDef TwiptwipDreamScavenger;
            public static CharacterVariantDef WipwipDreamScavenger;

            public static CharacterVariantDef Sanctum1DreamScavenger;
            public static CharacterVariantDef Sanctum2DreamScavenger;
            public static CharacterVariantDef Sanctum3DreamScavenger;
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

            var contentPackPopulation = PopulateContentPack();
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

        private IEnumerator PopulateContentPack()
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
        private static List<CharacterVariantDef> _dependantSpawnSoulDefs = new List<CharacterVariantDef>();
        private static List<CharacterVariantDef> _dependantSpawnTwistedScavDefs = new List<CharacterVariantDef>();
        private static List<CharacterVariantDef> _dependantSpawnSanctumScavDefs = new List<CharacterVariantDef>();

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

            ConfiguredBool CreateConfiguredBoolForPackOption(string key, string desc, ConfiguredVariable<bool>.OnConfigChangedDelegate onConfigChanged)
            {
                return new ConfiguredBool(true)
                    .WithModGUID(TO30Main.instance)
                    .WithModName(TO30Main.instance)
                    .WithConfigFile(TO30Main.instance.Config)
                    .WithSection("General")
                    .WithKey(key)
                    .WithDescription(desc)
                    .WithConfigChange(onConfigChanged)
                    .DoConfigure();
            }

            ConfiguredBool addAllyVariants = CreateConfiguredBoolForPackOption("Enable Ally Variants", "Enables \"Ally\" Variants for the main allied enemy bodies in the game. Devoted Lemurians may spawn as Flamethrower Devoted Lemurians for example.", b =>
            {
                foreach (var variantDef in _dependantSpawnAllyDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            ConfiguredBool addPolymorphLemurianVariants = CreateConfiguredBoolForPackOption("Enable Polymorph Lemurian Variants", "Enables \"Polymorph Lemurian\" Variants. Starting the Polymoprh Lemurian quest may spawn you as a Flamethrower Lemurian for example", b =>
            {
                foreach (var variantDef in _dependantSpawnPolymorphDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            ConfiguredBool addArtifactOfSoulWispVariants = CreateConfiguredBoolForPackOption("Enable Artifact of Soul Wisp Variants", "Enables \"Lesser Wisp Soul\" Variants. Playing with the Artifact of Soul enabled may spawn an Infernal Lesser Wisp Soul for example", b =>
            {
                foreach (var variantDef in _dependantSpawnSoulDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            ConfiguredBool addTwistedScavVariants = CreateConfiguredBoolForPackOption("Enable Twisted Scavenger Variants", "Enables \"Twisted Scavenger\" Variants. Reaching \"A Moment, Whole\" may spawn a Dream Twisted Scavenger for example", b =>
            {
                foreach (var variantDef in _dependantSpawnTwistedScavDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            ConfiguredBool addSanctumScavVariants = CreateConfiguredBoolForPackOption("Enable Sanctum Scavenger Variants", "Enables \"Sanctum Scavenger\" Variants. Hitting all Sconces in \"Lurid Basilica\" may spawn a Dream Sanctum Scavenger for example", b =>
            {
                foreach (var variantDef in _dependantSpawnSanctumScavDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(AddAllyVariants(addAllyVariants));
            parallelCoroutine.Add(AddPolymorphLemurianVariants(addPolymorphLemurianVariants));
            parallelCoroutine.Add(AddArtifactOfSoulWispVariants(addArtifactOfSoulWispVariants));
            parallelCoroutine.Add(AddTwistedScavVariants(addTwistedScavVariants));
            parallelCoroutine.Add(AddSanctumScavVariants(addSanctumScavVariants));

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
                var cloned = VariantCloning.CloneIntoBeetleGuardGland(asset);
                if(!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a BeetleGuardAlly clone.");
                    continue;
                }

                if(cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnAllyDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
            foreach (var asset in elderLemurianCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VariantCloning.CloneIntoDevotedLemurianBruiser(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a DevotedLemurianBruiser clone.");
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
            foreach (var asset in lemurianCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VariantCloning.CloneIntoDevotedLemurian(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a DevotedLemurian clone.");
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
                var cloned = VariantCloning.CloneIntoLemurianPolymorph(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a LemurianPolymorph clone.");
                    continue;
                }

                if (cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnPolymorphDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private static IEnumerator AddArtifactOfSoulWispVariants(bool canSpawn)
        {
            TO30Log.Info($"Adding Artifact of Soul Wisp Variants");

            using var lesserWispCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acLesserWispVariants");

            while (!lesserWispCollection.isComplete)
            {
                yield return null;
            }

            foreach (var asset in lesserWispCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VariantCloning.CloneIntoWispSoul(asset);
                if (!cloned)
                {
                    TO30Log.Error($"Failed to clone {asset} into a Lesser Wisp Soul clone.");
                    continue;
                }

                if (cloned.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    dependantCondition.cannotSpawn = canSpawn == false;
                    cloned.spawnCondition = dependantCondition;
                    _dependantSpawnSoulDefs.Add(cloned);
                }

                _variantPack.characterVariantDefs.AddSingle(cloned);
            }
        }

        private static IEnumerator AddTwistedScavVariants(bool canSpawn)
        {
            TO30Log.Info($"Adding Twisted Scavenger Variants");

            using var scavengerCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acScavengerVariants");

            while (!scavengerCollection.isComplete)
            {
                yield return null;
            }

            foreach (var asset in scavengerCollection.asset.FindAssets<CharacterVariantDef>())
            {
                CharacterVariantDef[] clones = VariantCloning.CloneIntoLunarScavs(asset, true, true);

                if(clones.Length == 0)
                {
                    TO30Log.Error($"Failed to clone {asset} into twisted scavenger clones.");
                    continue;
                }

                foreach(var clone in clones)
                {
                    if(clone.spawnCondition is VariantDependantCondition dependantCondition)
                    {
                        dependantCondition.cannotSpawn = canSpawn == false;
                        clone.spawnCondition = dependantCondition;
                        _dependantSpawnTwistedScavDefs.Add(clone);
                    }
                }

                _variantPack.characterVariantDefs.Add(clones);
            }
        }

        private static IEnumerator AddSanctumScavVariants(bool canSpawn)
        {
            TO30Log.Info($"Adding Sanctum Scavenger Variants");

            using var scavengerCollection = TO30Assets.LoadAssetAsync<AssetCollection>("acScavengerVariants");

            while (!scavengerCollection.isComplete)
            {
                yield return null;
            }

            foreach (var asset in scavengerCollection.asset.FindAssets<CharacterVariantDef>())
            {
                CharacterVariantDef[] clones = VariantCloning.CloneIntoSanctumScavs(asset, true, true);

                if (clones.Length == 0)
                {
                    TO30Log.Error($"Failed to clone {asset} into sanctum scavenger clones.");
                    continue;
                }

                foreach (var clone in clones)
                {
                    if (clone.spawnCondition is VariantDependantCondition dependantCondition)
                    {
                        dependantCondition.cannotSpawn = canSpawn == false;
                        clone.spawnCondition = dependantCondition;
                        _dependantSpawnTwistedScavDefs.Add(clone);
                    }
                }

                _variantPack.characterVariantDefs.Add(clones);
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