using Code.Core.Events.Bus;
using Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.UnitSystem
{
    public class SetUnitCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera unitCam;

        [SerializeField] private InputReader inputSO;

        private GameObject _ownCam;
        [SerializeField] private CharacterUnit _unit;
        
        private void Start()
        {
            inputSO.OnInteractionEvent += HandleCam;
        }

        private void OnEnable()
        {
            Bus<TopCamEvent>.Subscribe(HandleCamEvent);
        }

        private void HandleCamEvent(TopCamEvent obj)
        {
            _ownCam = obj.cam;
        }

        private void OnDisable()
        {
            inputSO.OnInteractionEvent -= HandleCam;
        }
        
        private void HandleCam()
        {
            if (_unit.isMyTurn)
            {
                Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(_unit.gameObject, false,new Vector3(1.5f,1.5f,1.5f)));
            }
        }

    }
}