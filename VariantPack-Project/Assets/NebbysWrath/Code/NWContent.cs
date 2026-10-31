using EntityStates;
using MSU;
using MSU.Config;
using NW.Modules;
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
            //Base
            public static CharacterVariantDef Adolescent;
            public static CharacterVariantDef ADShroom;
            public static CharacterVariantDef AlphaBison;
            public static CharacterVariantDef AlmostArchaicWisp;
            public static CharacterVariantDef AluminumContraption;
            public static CharacterVariantDef AmalgamatedGreatWisp;
            public static CharacterVariantDef AncientAurelionite;
            public static CharacterVariantDef AncientStoneTitan;
            public static CharacterVariantDef AncientStoneWisp;
            public static CharacterVariantDef BeetleEmpress;
            public static CharacterVariantDef BeetleMatriarch;
            public static CharacterVariantDef BerserkerOverlord;
            public static CharacterVariantDef BruiserImp;
            public static CharacterVariantDef Childish;
            public static CharacterVariantDef ColossalGolem;
            public static CharacterVariantDef DevourerDunestrider;
            public static CharacterVariantDef EnormousAurelionite;
            public static CharacterVariantDef GaussianProbe;
            public static CharacterVariantDef GhostOfKjaro;
            public static CharacterVariantDef GhostOfRunald;
            public static CharacterVariantDef GreatGrandParent;
            public static CharacterVariantDef GreaterStoneWisp;
            public static CharacterVariantDef HealerShroom;
            public static CharacterVariantDef IchorImp;
            public static CharacterVariantDef IchorOverlord;
            public static CharacterVariantDef IncineratingElderLemurian;
            public static CharacterVariantDef LeastestWisp;
            public static CharacterVariantDef LesserStoneWisp;
            public static CharacterVariantDef MalfunctioningAWU;
            public static CharacterVariantDef MalfunctioningSCU;
            public static CharacterVariantDef MammaShroom;
            public static CharacterVariantDef MoltenWorm;
            public static CharacterVariantDef MortarCrab;
            public static CharacterVariantDef MothershipVagrant;
            public static CharacterVariantDef OverflowingDunestrider;
            public static CharacterVariantDef OverflowingTemplar;
            public static CharacterVariantDef PygmyAurelionite;
            public static CharacterVariantDef SolusProbeMK2;
            public static CharacterVariantDef SomewhatGreatWisp;
            public static CharacterVariantDef StarvingDunestrider;
            public static CharacterVariantDef SteelContraption;
            public static CharacterVariantDef SwarmerProbe;
            public static CharacterVariantDef SwarmingAWU;
            public static CharacterVariantDef SwarmingSCU;
            public static CharacterVariantDef VampiricTemplar;

            //DLC1
            public static CharacterVariantDef ArmormelterPest;
            public static CharacterVariantDef ArmormelterVermin;
            public static CharacterVariantDef GammaConstruct;
            public static CharacterVariantDef Goop;
            public static CharacterVariantDef OmicronConstruct;
            public static CharacterVariantDef OverflowingApothecary;
            public static CharacterVariantDef ProtoConstruct;
            public static CharacterVariantDef SunWorshipper;
            public static CharacterVariantDef TarredGeep;
            public static CharacterVariantDef TarredGip;
            public static CharacterVariantDef TarredGup;
            public static CharacterVariantDef TinyPest;
            public static CharacterVariantDef TinyVermin;
            public static CharacterVariantDef TransplanarBarnacle;

            //DLC2
            public static CharacterVariantDef CollapsedChild;
            public static CharacterVariantDef DenseChild;
            public static CharacterVariantDef ElectricWurm;

            //Mods
            public static CharacterVariantDef AeonicWisp;
            public static CharacterVariantDef ArchaicStoneWisp;
            public static CharacterVariantDef ClayAssassin;
            public static CharacterVariantDef ClaySoldier;
            public static CharacterVariantDef EnragedWisp;
            public static CharacterVariantDef OverflowingClayman;
            public static CharacterVariantDef SomewhatAncientWisp;
            public static CharacterVariantDef SomewhatArchaicWisp;

            //Ally
            public static CharacterVariantDef EmpathyGreenGaussianProbe;
            public static CharacterVariantDef EmpathyGreenSolusProbeMK2;
            public static CharacterVariantDef EmpathyGreenSwarmerProbe;
            public static CharacterVariantDef EmpathyRedGaussianProbe;
            public static CharacterVariantDef EmpathyRedSolusProbeMK2;
            public static CharacterVariantDef EmpathyRedSwarmerProbe;
            public static CharacterVariantDef NucleusGammaConstruct;

            //Soul
            public static CharacterVariantDef SoulAlmostArchaicWisp;
            public static CharacterVariantDef SoulLeastestWisp;
            public static CharacterVariantDef SoulLesserStoneWisp;
        }

        public string identifier => NWMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(_contentPack);
        public static ReadOnlyVariantPack readOnlyVariantPack => _variantPack != null ? new ReadOnlyVariantPack(_variantPack) : default;

        private static ContentPack _contentPack = new ContentPack();
        private static VariantPack _variantPack;

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

            var initializeModules = InitializeModules();
            while(initializeModules.MoveNext())
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

            GupVariantHelper.AddVariantToGupBlacklist(CharacterVariants.Goop);
            GupVariantHelper.AddGupVariantRelation(CharacterVariants.TarredGup, CharacterVariants.TarredGeep, CharacterVariants.TarredGip);

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
            using var skillFamilyRequest = NWAssets.LoadAllAssetsAsync<SkillFamily>();

            parallelCoroutine.Add(skillRequest);
            parallelCoroutine.Add(skillFamilyRequest);

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            _contentPack.skillDefs.Add(skillRequest.assets);
            _contentPack.skillFamilies.Add(skillFamilyRequest.assets);

            Type[] allTypes = GetType().Assembly.GetTypes();
            List<Type> entityStateTypes = new List<Type>();
            foreach (Type type in allTypes)
            {
                if(type.IsSameOrSubclassOf(typeof(EntityState)) && !type.IsAbstract)
                {
                    entityStateTypes.Add(type);
                }
            }
            _contentPack.entityStateTypes.Add(entityStateTypes.ToArray());
        }

        private IEnumerator InitializeModules()
        {
            NWLog.Info("Initializing Modules");

            IContentPieceProvider damageTypeProvder = ContentUtil.CreateContentPieceProvider<IDamageTypeContent>(NWMain.instance, _contentPack);
            IContentPieceProvider<ItemDef> itemDefProvider = ContentUtil.CreateGenericContentPieceProvider<ItemDef>(NWMain.instance, _contentPack);
            ItemModule.AddProvider(NWMain.instance, itemDefProvider);

            ParallelCoroutine parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(DamageTypeModule.Initialize(damageTypeProvder));
            parallelCoroutine.Add(ItemModule.InitializeItems(NWMain.instance));

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            //Initialize prefabs after damage types
            IContentPieceProvider<GameObject> clonedPrefabProvider = ContentUtil.CreateGameObjectGenericContentPieceProvider<ClonedPrefabBehaviour>(NWMain.instance, _contentPack);
            var coroutine = PrefabCloneModule.Initialize(clonedPrefabProvider);

            while(coroutine.MoveNext())
            {
                yield return null;
            }

            yield break;
        }

        private static List<CharacterVariantDef> _dependantSpawnAllyDefs = new List<CharacterVariantDef>();
        private static List<CharacterVariantDef> _dependantSpawnSoulDefs = new List<CharacterVariantDef>();

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


            ConfiguredBool CreateConfiguredBoolForPackOption(string key, string desc, ConfiguredVariable<bool>.OnConfigChangedDelegate onConfigChanged)
            {
                return new ConfiguredBool(true)
                    .WithModGUID(NWMain.instance)
                    .WithModName(NWMain.instance)
                    .WithConfigFile(NWMain.instance.Config)
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

            ConfiguredBool addArtifactOfSoulWispVariants = CreateConfiguredBoolForPackOption("Enable Artifact of Soul Wisp Variants", "Enables \"Lesser Wisp Soul\" Variants. Playing with the Artifact of Soul enabled may spawn a Leastest Lesser Wisp Soul for example", b =>
            {
                foreach (var variantDef in _dependantSpawnSoulDefs)
                {
                    VariantDependantCondition condition = (VariantDependantCondition)variantDef.spawnCondition;
                    condition.cannotSpawn = b == false;
                    variantDef.spawnCondition = condition;
                }
            });

            parallelCoroutine = ParallelCoroutine.RequestFromPool();
            parallelCoroutine.Add(AddAllyVariants(addAllyVariants));
            parallelCoroutine.Add(AddArtifactOfSoulWispVariants(addArtifactOfSoulWispVariants));

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
            using var solusProbeCollection = NWAssets.LoadAssetAsync<AssetCollection>("acSolusProbeVariants");

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
                var cloned = VariantCloning.CloneIntoMinorConstructOnKill(asset);
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
                var cloned = VariantCloning.CloneIntoRoboBallGreenBuddy(asset);
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
                var cloned = VariantCloning.CloneIntoRoboBallRedBuddy(asset);
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

        private static IEnumerator AddArtifactOfSoulWispVariants(bool canSpawn)
        {
            NWLog.Info($"Adding Artifact of Soul Wisp Variants");

            using var lesserWispCollection = NWAssets.LoadAssetAsync<AssetCollection>("acLesserWispVariants");

            while (!lesserWispCollection.isComplete)
            {
                yield return null;
            }

            foreach (var asset in lesserWispCollection.asset.FindAssets<CharacterVariantDef>())
            {
                var cloned = VariantCloning.CloneIntoWispSoul(asset);
                if (!cloned)
                {
                    NWLog.Error($"Failed to clone {asset} into a Lesser Wisp Soul clone.");
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

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal NWContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}