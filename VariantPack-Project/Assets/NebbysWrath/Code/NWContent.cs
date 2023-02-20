using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using System;
using VAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates;
using NW.Modules;

namespace NW
{
    public class NWContent : ContentLoader<NWContent>
    {
        public override string identifier => NWMain.GUID;
        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = NWAssets.LoadAsset<R2APISerializableContentPack>("NWContentPack");
        public override Action[] LoadDispatchers { get; protected set; }
        private VariantPackDef variantPack;

        public override void Init()
        {
            base.Init();
            LoadDispatchers = new Action[]
            {
                () =>
                {
                    Log.Info("Adding Variants");
                    variantPack = NWAssets.LoadAsset<VariantPackDef>("NWVariantPack");
                    variantPack.variants = NWAssets.LoadAllAssetsOfType<VariantDef>();
                    VariantPackCatalog.AddVariantPack(variantPack, NWMain.Instance.Config);
                },
                () =>
                {
                    Log.Info("Adding EntityStates");
                    SerializableContentPack.entityStateTypes = GetType().Assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(EntityState)) && !t.IsAbstract)
                    .Select(t => new SerializableEntityStateType(t))
                    .ToArray();
                },
                () =>
                {
                    Log.Info("Adding SkillDefs");
                    SerializableContentPack.skillDefs = NWAssets.LoadAllAssetsOfType<RoR2.Skills.SkillDef>();
                },
                () =>
                {
                    Log.Info("Cloning Prefabs");
                    new PrefabCloneModule().Initialize();
                },
                () =>
                {
                    Log.Info("Adding DamageTypes");
                    new DamageTypeModule().Initialize();
                }
            };
        }
    }
}