using System.Collections.Generic;
using RoR2;
using RoR2.AsyncManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EntityStates.MinePod.Roller
{

    public class RollerPlant : BaseState
    {
        public static float mineSpawnDelay;

        public static float baseDuration;

        public static SpawnCard mineSpawnCard;

        public static EntityStateGameObject minePlantVFX;

        public static int spawnCount;

        public static string spawnPointChildName1;

        public static string spawnPointChildName2;

        public static string spawnPointChildName3;

        public static string emergeSoundEvent;

        public static string deploySoundEvent;

        public static string retractSoundEvent;

        private List<string> spawnPointChildNames;

        private float duration;

        private bool _mineSpawned;

        private float storedMoveSpeed;

        private ChildLocator childLocator;

        private bool opened;

        private bool _initFromOrig;

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_initFromOrig)
            {
                _initFromOrig = true;
                mineSpawnDelay = MinePlant.mineSpawnDelay;
                baseDuration = MinePlant.baseDuration;
                mineSpawnCard = Addressables.LoadAssetAsync<SpawnCard>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_SolusAmalgamator.cscSolusAmalgamatorThruster_asset).WaitForCompletion();
                minePlantVFX = MinePlant.minePlantVFX;
                spawnCount = MinePlant.spawnCount;
                spawnPointChildName1 = MinePlant.spawnPointChildName1;
                spawnPointChildName2 = MinePlant.spawnPointChildName2;
                spawnPointChildName3 = MinePlant.spawnPointChildName3;
                emergeSoundEvent = MinePlant.emergeSoundEvent;
                deploySoundEvent = MinePlant.deploySoundEvent;
                retractSoundEvent = MinePlant.retractSoundEvent;
            }

            if (base.characterBody.master.IsDeployableLimited(DeployableSlot.MinePodMine))
            {
                outer.SetNextStateToMain();
                return;
            }
            spawnPointChildNames = new List<string>();
            if (!string.IsNullOrEmpty(spawnPointChildName1))
            {
                spawnPointChildNames.Add(spawnPointChildName1);
            }
            if (!string.IsNullOrEmpty(spawnPointChildName2))
            {
                spawnPointChildNames.Add(spawnPointChildName2);
            }
            if (!string.IsNullOrEmpty(spawnPointChildName3))
            {
                spawnPointChildNames.Add(spawnPointChildName3);
            }
            duration = baseDuration / attackSpeedStat;
            childLocator = GetModelChildLocator();
            StartAimMode(0.3f);
            PlayAnimation("Body", "OpenPod", "OpenPod.playbackRate", mineSpawnDelay);
            opened = true;
            Util.PlayAttackSpeedSound(emergeSoundEvent, base.gameObject, attackSpeedStat);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (opened)
            {
                PlayAnimation("Body", "ClosePod", "OpenPod.playbackRate", 1f);
            }
            Util.PlayAttackSpeedSound(retractSoundEvent, base.gameObject, attackSpeedStat);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > mineSpawnDelay && !_mineSpawned)
            {
                _mineSpawned = true;
                SpawnMines();
            }
            if (base.fixedAge > duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void SpawnMines()
        {
            if ((bool)base.characterBody && (bool)base.characterBody.master && NetworkServer.active)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnMine(spawnPointChildNames[i % spawnPointChildNames.Count]);
                }
            }
        }

        private void SpawnMine(string spawnPointChildName)
        {
            if (base.characterBody.master.GetDeployableCount(DeployableSlot.MinePodMine) < base.characterBody.master.GetDeployableSameSlotLimit(DeployableSlot.MinePodMine))
            {
                Transform transform = childLocator.FindChild(spawnPointChildName);
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(mineSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    minDistance = 0f,
                    maxDistance = 0f,
                    position = transform.position
                }, RoR2Application.rng);
                directorSpawnRequest.summonerBodyObject = base.gameObject;
                directorSpawnRequest.onSpawnedServer = OnCardSpawned;
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                if ((bool)gameObject)
                {
                    CharacterMaster component = gameObject.GetComponent<CharacterMaster>();
                    Deployable deployable = gameObject.AddComponent<Deployable>();
                    deployable.onUndeploy = new UnityEvent();
                    deployable.onUndeploy.AddListener(component.TrueKill);
                    base.characterBody.master.AddDeployable(deployable, DeployableSlot.MinePodMine);
                    EffectManager.SimpleEffect(minePlantVFX, gameObject.transform.position, Quaternion.identity, transmit: true);
                }
                Util.PlayAttackSpeedSound(deploySoundEvent, base.gameObject, attackSpeedStat);
            }
            void OnCardSpawned(SpawnCard.SpawnResult result)
            {
                if (result.success)
                {
                    CharacterMaster characterMaster = base.characterBody.master;
                    if ((bool)characterMaster && (bool)characterMaster.minionOwnership.ownerMaster)
                    {
                        characterMaster = characterMaster.minionOwnership.ownerMaster;
                    }
                    CharacterMaster component2 = result.spawnedInstance.GetComponent<CharacterMaster>();
                    component2.minionOwnership.SetOwner(characterMaster);
                    Util.CopyEliteStatusParams copyEliteStatusParams = default(Util.CopyEliteStatusParams);
                    copyEliteStatusParams.ownerMaster = base.characterBody.masterObject;
                    copyEliteStatusParams.minionMaster = component2.gameObject;
                    copyEliteStatusParams.copyFrom = Util.EliteUpgradeSource.EliteDef;
                    copyEliteStatusParams.limitTo = Util.EliteUpgradeSource.Owner;
                    Util.CopyEliteStatus(copyEliteStatusParams);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}