using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using System;
using VAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates;

namespace TO30
{
    public class TO30Content : ContentLoader<TO30Content>
    {
        public override string identifier => TO30Main.GUID;
        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = TO30Assets.LoadAsset<R2APISerializableContentPack>("TO30ContentPack");
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
                    variantPack = TO30Assets.LoadAsset<VariantPackDef>("TO30VariantPack");
                    variantPack.variants = TO30Assets.LoadAllAssetsOfType<VariantDef>();
                    VariantPackCatalog.AddVariantPack(variantPack, TO30Main.Instance.Config);
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
                    SerializableContentPack.skillDefs = TO30Assets.LoadAllAssetsOfType<RoR2.Skills.SkillDef>();
                }
            };
        }
    }
}