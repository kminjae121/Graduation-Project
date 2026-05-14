using System;
using Code.Core.Interfaces;
using Code.UnitSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    [CreateAssetMenu(fileName = "Input", menuName = "SO/Input/InputReader", order = 0)]
    public class InputReader : ScriptableObject, Controls.IPlayerActions
    {
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private LayerMask WhatIsEnemy;
        [SerializeField] private LayerMask WhatIsPlayer;
        
        public event Action OnAttackEvent;
        public event Action OnClickMoveEvent;
        public event Action OnClickEvent;
        public event Action OnSelectEvent;
        public event Action OnInteractionEvent;
        public event Action OnCancelEvent;

        public Vector2 MovementKey { get; private set; }
        public Vector2 MouseUpDownValue { get; private set; }
        public event Action OnSelectUnitEvent;

        public Controls _controls { get; set; }
        private Vector3 _gridPosition;

        public Vector2 MousePosition { get; private set; }

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public GameObject GetWorldPosition()
        {
            Camera mainCam = Camera.main;
            Debug.Assert(mainCam != null, "메인 카메라가 씬에 없습니다.");

            GameObject gameObj = null;
            
            Ray cameraRay = mainCam.ScreenPointToRay(MousePosition);
            
            if (Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane, whatIsGround))
                gameObj = hit.transform.gameObject;

            return gameObj;
        }
        
        public IMapTile GetSelectedTile()
        {
            Camera mainCam = Camera.main;
            Debug.Assert(mainCam != null, "메인 카메라가 씬에 없습니다.");
            
            IMapTile maptile = null; 
            
            Ray cameraRay = mainCam.ScreenPointToRay(MousePosition);
            
            if (Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane, whatIsGround))
                maptile = hit.transform.GetComponent<IMapTile>();

            return maptile;
        }

        public Unit GetUnit()
        {
            Camera mainCam = Camera.main;
            Debug.Assert(mainCam != null, "메인 카메라가 씬에 없습니다.");
            
            Ray cameraRay = mainCam.ScreenPointToRay(MousePosition);
            
            return Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane, WhatIsPlayer) ?
                hit.collider.gameObject.GetComponent<Unit>() : null;
        }

        public GameObject GetEnemy()
        {
            Camera mainCam = Camera.main;
            Debug.Assert(mainCam != null, "메인 카메라가 씬에 없습니다.");
            
            Ray cameraRay = mainCam.ScreenPointToRay(MousePosition);
            
            return Physics.Raycast(cameraRay, out RaycastHit hit, mainCam.farClipPlane, WhatIsEnemy) ?
                hit.collider.gameObject : null;
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnClickEvent?.Invoke();
                OnAttackEvent?.Invoke();
                OnSelectUnitEvent?.Invoke();
                
                if (GetSelectedTile() != null)
                    OnClickMoveEvent?.Invoke();
            }
        }

        public void OnPointer(InputAction.CallbackContext context)
        {
            MousePosition = context.ReadValue<Vector2>();
        }

        public void OnCamMove(InputAction.CallbackContext context)
        {
            MovementKey = context.ReadValue<Vector2>();
        }

        public void OnMouseUpDown(InputAction.CallbackContext context)
        {
            MouseUpDownValue = context.ReadValue<Vector2>();
        }

        public void OnSelectBtn(InputAction.CallbackContext context)
        {
            OnSelectEvent?.Invoke();
        }

        public void OnInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractionEvent?.Invoke();
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnCancelEvent?.Invoke();
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
                _controls.Player.Enable();
            else
                _controls.Player.Disable();
        }
    }
}