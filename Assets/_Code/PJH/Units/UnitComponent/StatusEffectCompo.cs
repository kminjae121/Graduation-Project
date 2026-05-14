using System;
using System.Collections.Generic;
using System.Linq;
using Code.Combat.StatusEffect;
using Code.Core.Debugs;
using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.UnitSystem.UnitComponent
{
    public class StatusEffectCompo : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private StatusEffectStorageSO statusEffectList;
        
        private static readonly Dictionary<string, Type> _typeCacheDict = new();
        private readonly List<EffectType> _removeBuffer = new();

        private Dictionary<EffectType, StatusEffectSO> _statusEffectDict;
        private Dictionary<EffectType, StatusEffect> _activeEffectDict;
        
        private int _statusEffectBit;
        private Unit _owner;

        public void Initialize(Unit owner)
        {
            _activeEffectDict = new Dictionary<EffectType, StatusEffect>();
            _statusEffectDict = new Dictionary<EffectType, StatusEffectSO>();
            
            _statusEffectBit = 0;
            _owner = owner;

            RegisterStatusEffects();
        }

        private void OnEnable()
        {
            Bus<ApplyStatusEffectEvent>.Subscribe(HandleApplyStatusEffect);
        }

        private void OnDisable()
        {
            Bus<ApplyStatusEffectEvent>.Unsubscribe(HandleApplyStatusEffect);
        }

        public void StartUpdateStatusEffects()
        {
            if (_activeEffectDict == null || _activeEffectDict.Count == 0)
                return;

            _removeBuffer.Clear();

            foreach (var (effectType, effect) in _activeEffectDict)
            {
                if (effect.TriggerTiming != EffectTriggerTiming.TurnStart)
                    continue;
                
                effect.StartUpdateEffect();

                if (effect.IsCompleted)
                    _removeBuffer.Add(effectType);
            }

            foreach (var effectType in _removeBuffer)
                RemoveStatusEffect(effectType);
        }
        
        public void EndUpdateStatusEffects()
        {
            if (_activeEffectDict == null || _activeEffectDict.Count == 0)
                return;

            _removeBuffer.Clear();

            foreach (var (effectType, effect) in _activeEffectDict)
            {
                if (effect.TriggerTiming != EffectTriggerTiming.TurnEnd)
                    continue;
                
                effect.EndUpdateEffect();

                if (effect.IsCompleted)
                    _removeBuffer.Add(effectType);
            }

            foreach (var effectType in _removeBuffer)
                RemoveStatusEffect(effectType);
        }

        private void AddStatusEffect(EffectType effectType, StatusEffectApplyData data)
        {
            if (_owner == null)
                return;

            if (_activeEffectDict.TryGetValue(effectType, out var activeEffect))
            {
                activeEffect.Merge(data);
                return;
            }

            StatusEffect effect = CreateEffect(effectType);

            if (effect == null)
                return;

            effect.SetEffect(_owner, data);
            effect.ApplyEffect();
            
            _activeEffectDict[effectType] = effect;
            _statusEffectBit |= (int)effectType;
        }

        private void RemoveStatusEffect(EffectType effectType)
        {
            if (_activeEffectDict != null && _activeEffectDict.TryGetValue(effectType, out var effect))
            {
                effect.EndEffect();
                _activeEffectDict.Remove(effectType);
            }

            _statusEffectBit &= ~(int)effectType;
        }

        public bool HasState(EffectType effectType)
            => (_statusEffectBit & (int)effectType) == (int)effectType;

        public bool HasAnyState(EffectType effectType)
            => (_statusEffectBit & (int)effectType) != 0;

        private void HandleApplyStatusEffect(ApplyStatusEffectEvent evt)
        {
            if (_owner == null || evt.Target != _owner)
                return;

            AddStatusEffect(evt.EffectType, evt.ApplyData);
        }

        private void RegisterStatusEffects()
        {
            if (statusEffectList == null || statusEffectList.statusEffects.Count == 0)
                return;

            foreach (var statusEffectSO in statusEffectList.statusEffects)
            {
                if (statusEffectSO == null)
                    continue;

                if (!_statusEffectDict.TryAdd(statusEffectSO.effectType, statusEffectSO))
                    Debug.LogWarning($"[{nameof(StatusEffectCompo)}] Duplicate StatusEffectSO for {statusEffectSO.effectType} on {name}.");
            }
        }

        private StatusEffect CreateEffect(EffectType effectType)
        {
            if (_statusEffectDict == null || !_statusEffectDict.TryGetValue(effectType, out var statusEffectSO))
            {
                UnityLogger.LogWarning($"[{nameof(StatusEffectCompo)}] {effectType} StatusEffectSO is not assigned on {name}.");
                return null;
            }

            Type effectTypeClass = GetTypeByName(statusEffectSO.className);

            if (effectTypeClass == null)
            {
                UnityLogger.LogWarning($"[{nameof(StatusEffectCompo)}] Could not find class '{statusEffectSO.className}' for {effectType}.");
                return null;
            }

            if (Activator.CreateInstance(effectTypeClass) is not StatusEffect effect)
            {
                UnityLogger.LogWarning($"[{nameof(StatusEffectCompo)}] Failed to create {effectTypeClass.Name} instance.");
                return null;
            }

            effect.Initialize(statusEffectSO);
            return effect;
        }

        private static Type GetTypeByName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            if (_typeCacheDict.TryGetValue(className, out Type cachedType))
                return cachedType;

            Type type = Type.GetType(className);

            if (type == null)
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetTypes().FirstOrDefault(t =>
                        t.Name == className || t.FullName == className || t.FullName.EndsWith($".{className}"));

                    if (type != null)
                        break;
                }

            if (type != null)
                _typeCacheDict[className] = type;

            return type;
        }
    }
}