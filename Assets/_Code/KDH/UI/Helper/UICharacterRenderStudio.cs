using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.UnitSystem;
using UnityEngine;

namespace Code.UI
{
    public class UICharacterRenderStudio : MonoBehaviour
    {
        [Serializable]
        public struct UnitModelMapping
        {
            public UnitType unitType;
            public GameObject modelPrefab;
        }

        [Header("Render Settings")]
        [SerializeField] private Camera renderCamera;
        [SerializeField] private Transform modelSpawnPoint;
        [SerializeField] private int renderLayer;
        [SerializeField] private List<UnitModelMapping> modelMappings;

        public Texture TargetTexture => renderCamera != null ? renderCamera.targetTexture : null;

        private GameObject _currentRenderModel;
        private readonly Dictionary<UnitType, GameObject> _modelDict = new();

        private void Awake()
        {
            foreach (var mapping in modelMappings)
            {
                if (!_modelDict.ContainsKey(mapping.unitType))
                {
                    _modelDict.Add(mapping.unitType, mapping.modelPrefab);
                }
            }

            Bus<CharacterInfoEvent>.Subscribe(HandleCharacterInfo);
        }

        private void OnDestroy()
        {
            Bus<CharacterInfoEvent>.Unsubscribe(HandleCharacterInfo);
        }

        private void HandleCharacterInfo(CharacterInfoEvent evt)
        {
            if (evt.Unit == null)
            {
                ClearCurrentModel();
                return;
            }

            SpawnRenderModel(evt.Unit.Data.UnitType);
        }

        public void Setup(UnitType unitType)
        {
            SpawnRenderModel(unitType);
        }

        private void ClearCurrentModel()
        {
            if (_currentRenderModel != null)
            {
                Destroy(_currentRenderModel);
                _currentRenderModel = null;
            }
        }

        private void SpawnRenderModel(UnitType type)
        {
            ClearCurrentModel();

            if (!_modelDict.TryGetValue(type, out var prefab) || prefab == null)
            {
                Debug.LogWarning("해당 직업의 렌더링용 모델 프리팹이 등록되지 않았습니다.");
                return;
            }

            _currentRenderModel = Instantiate(prefab, modelSpawnPoint);
            _currentRenderModel.transform.localPosition = Vector3.zero;
            _currentRenderModel.transform.localRotation = Quaternion.identity;

            SetLayerRecursively(_currentRenderModel, renderLayer);
            DisableLogicComponents(_currentRenderModel);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private void DisableLogicComponents(GameObject obj)
        {
            var monoBehaviours = obj.GetComponents<MonoBehaviour>();
            foreach (var comp in monoBehaviours)
            {
                comp.enabled = false;
            }

            var colliders = obj.GetComponents<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            var animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
            }
        }
    }
}