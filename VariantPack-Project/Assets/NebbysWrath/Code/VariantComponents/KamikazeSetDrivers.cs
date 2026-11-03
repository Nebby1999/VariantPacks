using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterMaster, useOnServer = true)]
    public sealed class KamikazeSetDrivers : VariantComponent
    {
        private static GameObject _customSkillDrivers = NWAssets.LoadAsset<GameObject>("KamikazeSkillDrivers");

        private CharacterMaster _characterMaster;

        private AISkillDriver[] _origSkillDrivers;
        private BaseAI _modifiedBaseAI;

        private GameObject _customSkillDriversContainer;
        private bool _assignedDrivers;

        private void Awake()
        {
            _characterMaster = GetComponent<CharacterMaster>();
        }

        private void Start()
        {
            if(_characterMaster && _characterMaster.TryGetComponent<BaseAI>(out var baseAI) && _customSkillDrivers)
            {
                AssignCustomSkillDrivers(baseAI);
            }
        }

        private void AssignCustomSkillDrivers(BaseAI baseAI)
        {
            if (_assignedDrivers == true)
                return;

            _assignedDrivers = true;
            _customSkillDriversContainer = Instantiate(_customSkillDrivers);
            _customSkillDriversContainer.transform.SetParent(transform);

            _modifiedBaseAI = baseAI;
            _origSkillDrivers = baseAI.skillDrivers;

            _modifiedBaseAI.ReplaceSkillDrivers(_customSkillDriversContainer.GetComponents<AISkillDriver>());
        }

        private void OnDestroy()
        {
            if (_assignedDrivers && _modifiedBaseAI)
            {
                _modifiedBaseAI.ReplaceSkillDrivers(_origSkillDrivers);
                if(_customSkillDriversContainer)
                {
                    Destroy(_customSkillDriversContainer);
                }
            }
        }
    }
}