using System;
using System.Collections.Generic;
using Code.Core.Debugs;
using Code.UnitSystem;
using UnityEngine;

namespace _Code.UnitSystem
{
    enum EffectType
    {
        
    }
    public class UnitEffectCompo : MonoBehaviour, IUnitComponent
    {
        private Dictionary<string, UnitEffect> _effectDict = new Dictionary<string, UnitEffect>();

        public void Initialize(Unit owner)
        {
            UnitEffect[] atkEffect = GetComponentsInChildren<UnitEffect>(true);

            foreach (UnitEffect effect in atkEffect)
            {
                if (string.IsNullOrWhiteSpace(effect.EffectName))
                {
                    UnityLogger.LogWarning($"EffectName이 비어있음: {effect.name}");
                    continue;
                }

                if (_effectDict.ContainsKey(effect.EffectName))
                {
                    UnityLogger.LogWarning($"{effect.EffectName}이 딕셔너리에 이미 존재함");
                    continue;
                }

                _effectDict.Add(effect.EffectName, effect);
            }   
        }

        public void PlayTargetEffect(string effectName, Vector3 position = default(Vector3))
        {
            if (string.IsNullOrWhiteSpace(effectName)) 
                    return;
            
            if (_effectDict.TryGetValue(effectName, out var effect)) 
                effect.PlayEffect();

            if (position != default(Vector3))
            {
                effect.transform.position = position;
            }
        }

        public void StopTargetEffect(string effectName)
        {
            if (string.IsNullOrWhiteSpace(effectName)) 
                return;
            
            if (_effectDict.TryGetValue(effectName, out var effect)) 
                effect.StopEffect();
        }

    }
}