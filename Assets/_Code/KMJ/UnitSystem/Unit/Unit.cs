using System;
using System.Collections.Generic;
using System.Linq;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.UnitSystem.Combat;
using Code.UnitSystem.UnitComponent;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Code.UnitSystem
{
    public class Unit : MonoBehaviour, ITurnable, IPoolable
    {
        [field: Header("Settings")] 
        [field: SerializeField] public UnitSO unitSO { get; private set; }
        [field: SerializeField] public float TurnGauge { get; set; }
        
        [Header("Status")]
        public bool isMyTurn { get; private set; }
        public bool IsPlayerUnit { get; private set; }
        public int TurnSpeed { get; set; }
        public Sprite UnitImage { get; private set; }
        public bool IsReadyDoAct => TurnGauge >= 100f;

        public GameObject UnitObj { get; set; } 
        public string UnitName => unitSO != null ? unitSO.UnitName : "Unknown";
        
        protected Dictionary<Type, IUnitComponent> _components = new();
        public UnitManageRangeCompo RangeCompo { get; private set; }
        public UnitAnimation AnimationCompo { get; private set; }
        
        public UnitHealth HealthCompo { get; private set; }
        public StatusEffectCompo StatusEffectCompo { get; private set; }
        
        [Header("Events")]
        public Action OnDeathEvent;
        public Action OnHitEvent;
        
        public int AddDefensivePower { get; set; }
        
        public int AddAvoidProbability { get; set; }
        
        [field: SerializeField] public PoolingItemSO PoolingType { get; private set; }
        public GameObject GameObject => gameObject;
        
        private Pool _myPool;
        
        protected virtual void Awake()
        {
            AddUnitComponents();
            InitComponents();
            AfterInitComponents();
            UnitObj = gameObject;
        }
        
        protected virtual void OnEnable()
        {
            InitializeData();
            RegisterEvents();
            
            AnimationCompo.PlaySelectAnimation("IDLE");
        }
        
        protected virtual void OnDisable()
        {
        }
        
        protected virtual void OnDestroy()
        {
            UnregisterEvents();
        }
        
        private void InitializeData()
        {
            if (unitSO != null)
            {
                TurnSpeed = unitSO.Speed;
                IsPlayerUnit = unitSO.isPlayerUnit;
                UnitImage = unitSO.UnitImage;
            }
        
            TurnGauge = 0f;
        }
        
        private void RegisterEvents()
        {
            OnHitEvent -= Hit;
            OnDeathEvent -= Dead;
        
            OnHitEvent += Hit;
            OnDeathEvent += Dead;
        }
        
        private void UnregisterEvents()
        {
            OnHitEvent -= Hit;
            OnDeathEvent -= Dead;
        }
        
        public virtual void OnTurnStart()
        {
            isMyTurn = true;
            
            StatusEffectCompo.StartUpdateStatusEffects();
        }

        public void InitializeDefensivePower()
        {
            if (AddDefensivePower != 0)
                unitSO.DefensivePower -= AddDefensivePower;
        
            AddDefensivePower = 0;
        }

        public void InitializeAvoidProbability()
        {
            if (AddAvoidProbability != 0)
                unitSO.AvoidProbability -= AddAvoidProbability;
        
            AddDefensivePower = 0;
        }
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
        
        public void ResetItem()
        {
        }
        
        public virtual void OnTurnEnd()
        {
            StatusEffectCompo.EndUpdateStatusEffects();
            isMyTurn = false;
            Bus<UnitTurnEndEvent>.Raise(new UnitTurnEndEvent(this));
        }
        
        protected virtual void Hit()
        {
        }
        
        protected virtual void Dead()
        {
            Bus<UnitDeadEvent>.Raise(new UnitDeadEvent(this));
        }
        
        private void AddUnitComponents()
        {
            _components = GetComponentsInChildren<IUnitComponent>()
                .ToDictionary(compo => compo.GetType());
        
            RangeCompo = GetUnitCompo<UnitManageRangeCompo>();
            AnimationCompo = GetUnitCompo<UnitAnimation>();
            StatusEffectCompo = GetUnitCompo<StatusEffectCompo>();
            HealthCompo = GetUnitCompo<UnitHealth>();   
        }
        
        protected virtual void InitComponents()
        {
            foreach (var component in _components.Values)
                component.Initialize(this);
        }
        
        protected virtual void AfterInitComponents()
        {
            foreach (var component in _components.Values.OfType<IAfterInitialize>())
                component.AfterInitialize();
        }
        
        public T GetUnitCompo<T>() where T : class, IUnitComponent
        {
            return _components.GetValueOrDefault(typeof(T)) as T;
        }
        
        public IUnitComponent GetUnitCompo(Type type)
        {
            return _components.GetValueOrDefault(type);
        }
    }
}