using KinematicCharacterController;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI;

namespace NW.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterMaster, useOnClient = false, useOnServer = true)]
    public sealed class EvilBuddyMasterModifier : VariantComponent
    {
        private CharacterMaster _master;
        
        private AISkillDriver _modifiedSkillDriver;
        private float _origDriverUpdateTimerOverride;
        private AISkillDriver.MovementType _origMovementType;

        private void Awake()
        {
            _master = GetComponent<CharacterMaster>();
        }

        private void Start()
        {
            if(_master && _master.TryGetComponent<BaseAI>(out var baseAI))
            {
                foreach(var skillDriver in baseAI.skillDrivers)
                {
                    if(skillDriver.skillSlot == SkillSlot.Primary)
                    {
                        _modifiedSkillDriver = skillDriver;

                        _origDriverUpdateTimerOverride = _modifiedSkillDriver.driverUpdateTimerOverride;
                        _origMovementType = _modifiedSkillDriver.movementType;

                        _modifiedSkillDriver.driverUpdateTimerOverride = 3f;
                        _modifiedSkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if(_modifiedSkillDriver)
            {
                _modifiedSkillDriver.driverUpdateTimerOverride = _origDriverUpdateTimerOverride;
                _modifiedSkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            }
        }
    }
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer  = true)]
    public sealed class EvilBuddyBodyModifier : VariantComponent
    {
        private CharacterBody _characterBody;
        private KinematicCharacterMotor _kinematicCharacterMotor;
        private Rigidbody _rigidbody;

        private float _origMass;
        private float _origDrag;
        private float _origAngularDrag;
        private RigidbodyInterpolation _origRigidbodyInterpolation;
        private CollisionDetectionMode _origCollisionDetectionMode;

        private PhysicMaterial _origPhysicMaterial;

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
            _rigidbody = GetComponent<Rigidbody>();
            _kinematicCharacterMotor = GetComponent<KinematicCharacterMotor>();
        }

        private void Start()
        {
            if(_rigidbody)
            {
                _origMass = _rigidbody.mass;
                _rigidbody.mass = 1000;

                _origDrag = _rigidbody.drag;
                _rigidbody.drag = 0.05f;

                _origAngularDrag = _rigidbody.angularDrag;
                _rigidbody.angularDrag = 0.05f;

                _origRigidbodyInterpolation = _rigidbody.interpolation;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                _origCollisionDetectionMode = _rigidbody.collisionDetectionMode;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if(_characterBody)
            {
                _characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                _characterBody.bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
                _characterBody.bodyFlags |= CharacterBody.BodyFlags.ResistantToAOE;
            }

            if(_kinematicCharacterMotor)
            {
                _origPhysicMaterial = _kinematicCharacterMotor.CapsulePhysicsMaterial;
                _kinematicCharacterMotor.CapsulePhysicsMaterial = Addressables.LoadAssetAsync<PhysicMaterial>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3.physMatPhysicsProjectile_physicMaterial).WaitForCompletion();
            }
        }

        private void OnDestroy()
        {
            if(_rigidbody)
            {
                _rigidbody.mass = _origMass;
                _rigidbody.drag = _origDrag;
                _rigidbody.angularDrag = _origAngularDrag;
                _rigidbody.interpolation = _origRigidbodyInterpolation;
                _rigidbody.collisionDetectionMode = _origCollisionDetectionMode;
            }

            if(_characterBody)
            {
                _characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                _characterBody.bodyFlags &= ~CharacterBody.BodyFlags.SprintAnyDirection;
                _characterBody.bodyFlags &= ~CharacterBody.BodyFlags.ResistantToAOE;
            }

            if(_kinematicCharacterMotor)
            {
                _kinematicCharacterMotor.CapsulePhysicsMaterial = _origPhysicMaterial;
            }
        }
    }

    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true, useOnServer = true)]
    public sealed class SetupEvilBuddyHitbox : VariantComponent
    {
        private CharacterModel characterModel;
        private ChildLocator childLocator;

        private HitBoxGroup _createdHitboxGroup;

        private void Awake()
        {
            characterModel = GetComponent<CharacterModel>();
            childLocator = GetComponent<ChildLocator>();
        }

        private void Start()
        {
            if(characterModel != null && childLocator != null)
            {
                _createdHitboxGroup = CreateHitboxGroup();
            }
        }

        private void OnDestroy()
        {
            if(_createdHitboxGroup != null)
            {
                Destroy(_createdHitboxGroup.hitBoxes[0].gameObject);
                Destroy(_createdHitboxGroup);
            }
        }

        private HitBoxGroup CreateHitboxGroup()
        {
            HitBoxGroup createdHitboxGroup = null;
            if(childLocator.TryFindChild("Body", out Transform parentForHitbox))
            {
                createdHitboxGroup = characterModel.gameObject.AddComponent<HitBoxGroup>();
                createdHitboxGroup.groupName = "Main";

                GameObject hitboxObject = new GameObject("EvilBuddyHitbox");
                hitboxObject.transform.parent = parentForHitbox;
                hitboxObject.transform.localPosition = Vector3.zero;
                hitboxObject.transform.localScale = Vector3.one * 3;
                HitBox hitbox = hitboxObject.AddComponent<HitBox>();
                hitbox.showAsSphere = true;

                createdHitboxGroup.hitBoxes = new HitBox[] { hitbox };
            }
            return createdHitboxGroup;
        }
    }
}