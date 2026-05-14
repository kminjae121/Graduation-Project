using System;
using System.Collections.Generic;
using Code.Core.Events.Bus;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    [Serializable]
    public struct UnitModelMapping
    {
        public UnitType unitType;
        public GameObject modelPrefab;
    }

    public class PartyCharacterInfoUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject lobbyStatPanel;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterClassText;

        [Header("3D Model Settings")]
        [SerializeField] private Transform[] modelSpawnPoints;
        [SerializeField] private List<UnitModelMapping> unitModelMappings;

        [Header("Stat TMPs")]
        [SerializeField] private TextMeshProUGUI maxHealthText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI moveSpeedText;
        [SerializeField] private TextMeshProUGUI turnSpeedText;
        [SerializeField] private TextMeshProUGUI criticalProbabilityText;
        [SerializeField] private TextMeshProUGUI criticalDamageIncreaseText;
        [SerializeField] private TextMeshProUGUI maxSkillCostText;
        [SerializeField] private TextMeshProUGUI recoverySkillCostText;

        private GameObject[] _spawnedModels = new GameObject[3];
        private UnitSO[] _assignedUnits = new UnitSO[3];
        private GameObject _previewModel;

        private void Awake()
        {
            Bus<PartyCharacterHoverEvent>.Subscribe(HandleHover);
            Bus<PartyCharacterSelectEvent>.Subscribe(HandleSelect);
            Bus<PartyCharacterDeselectEvent>.Subscribe(HandleDeselect);

            if (lobbyStatPanel != null) lobbyStatPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            Bus<PartyCharacterHoverEvent>.Unsubscribe(HandleHover);
            Bus<PartyCharacterSelectEvent>.Unsubscribe(HandleSelect);
            Bus<PartyCharacterDeselectEvent>.Unsubscribe(HandleDeselect);
            
            CleanupAllModels();
        }

        private void HandleHover(PartyCharacterHoverEvent evt)
        {
            CleanupPreviewModel();

            bool isHovering = evt.Unit != null;
            if (lobbyStatPanel != null) lobbyStatPanel.SetActive(isHovering);
            
            if (isHovering)
            {
                UpdateStats(evt.Unit);

                bool isAlreadyAssigned = false;
                for (int i = 0; i < _assignedUnits.Length; i++)
                {
                    if (_assignedUnits[i] == evt.Unit)
                    {
                        isAlreadyAssigned = true;
                        break;
                    }
                }

                if (!isAlreadyAssigned)
                {
                    for (int i = 0; i < _assignedUnits.Length; i++)
                    {
                        if (_assignedUnits[i] == null)
                        {
                            _previewModel = CreateModelInstance(i, evt.Unit);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleSelect(PartyCharacterSelectEvent evt)
        {
            if (evt.Unit == null) return;

            for (int i = 0; i < _assignedUnits.Length; i++)
            {
                if (_assignedUnits[i] == evt.Unit) return;
            }

            CleanupPreviewModel();

            for (int i = 0; i < _assignedUnits.Length; i++)
            {
                if (_assignedUnits[i] == null)
                {
                    _assignedUnits[i] = evt.Unit;
                    _spawnedModels[i] = CreateModelInstance(i, evt.Unit);
                    break;
                }
            }
        }

        private void HandleDeselect(PartyCharacterDeselectEvent evt)
        {
            if (evt.Unit == null) return;

            for (int i = 0; i < _assignedUnits.Length; i++)
            {
                if (_assignedUnits[i] == evt.Unit)
                {
                    _assignedUnits[i] = null;
                    if (_spawnedModels[i] != null)
                    {
                        Destroy(_spawnedModels[i]);
                        _spawnedModels[i] = null;
                    }
                    break;
                }
            }
        }

        private GameObject CreateModelInstance(int index, UnitSO unit)
        {
            if (index >= modelSpawnPoints.Length || modelSpawnPoints[index] == null) return null;

            GameObject prefabToSpawn = GetModelPrefab(unit.UnitType);
            if (prefabToSpawn != null)
            {
                GameObject instance = Instantiate(prefabToSpawn, modelSpawnPoints[index]);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                return instance;
            }
            return null;
        }

        private GameObject GetModelPrefab(UnitType unitType)
        {
            if (unitModelMappings == null) return null;
            foreach (var mapping in unitModelMappings)
            {
                if (mapping.unitType == unitType) return mapping.modelPrefab;
            }
            return null;
        }

        private void CleanupPreviewModel()
        {
            if (_previewModel != null)
            {
                Destroy(_previewModel);
                _previewModel = null;
            }
        }

        private void CleanupAllModels()
        {
            CleanupPreviewModel();
            for (int i = 0; i < _spawnedModels.Length; i++)
            {
                if (_spawnedModels[i] != null)
                {
                    Destroy(_spawnedModels[i]);
                    _spawnedModels[i] = null;
                }
                _assignedUnits[i] = null;
            }
        }

        private void UpdateStats(UnitSO data)
        {
            if (data == null) return;
            if (characterNameText != null) characterNameText.text = data.UnitName ?? string.Empty;
            if (characterClassText != null) characterClassText.text = data.UnitClass ?? string.Empty;
            if (maxHealthText != null) maxHealthText.text = data.Maxhealth.ToString();
            if (atkText != null) atkText.text = data.AttackDamage.ToString();
            if (defText != null) defText.text = data.DefensivePower.ToString();
            if (moveSpeedText != null) moveSpeedText.text = data.MoveRange.ToString();
            if (turnSpeedText != null) turnSpeedText.text = data.Speed.ToString();
            if (criticalProbabilityText != null) criticalProbabilityText.text = $"{data.CriticalProbability:F1}%";
            if (criticalDamageIncreaseText != null) criticalDamageIncreaseText.text = data.CriticalDamageIncrease.ToString("F1");
            if (maxSkillCostText != null) maxSkillCostText.text = data.MaxManaCost.ToString();
            if (recoverySkillCostText != null) recoverySkillCostText.text = data.RecoveryManaCost.ToString();
        }
    }
}