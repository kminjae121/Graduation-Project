using Code.Combat;
using Code.Core;
using Code.Core.Events.Bus;
using Code.UI;
using Code.UnitManaging;
using EntityComponent;
using GameEventChannel;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem.Combat
{
    public class UnitHealth : MonoBehaviour, IUnitComponent, IDamageable
    {
        [SerializeField] private StatSO hpStat;
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private TextInfo normalText, criticalText, healText;
        [SerializeField] private GameEventChannelSO textEventChannel;

        [field : SerializeField] public UnitStorageSO StorageSO;
        private UnitAnimation _unitAnimation;

        public delegate void DefenseHandler(ref int Damage);
        public event DefenseHandler OnDefenseEvent;
        
        private Unit _entity; 
        private UnitStatCompo _statCompo;
        private UnitState _unitStateCompo;
        private UnitShieldCompo _shieldCompo;
        
        private float _defensivePower;

        public bool IsInvincibility { get;  set; } = false;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public bool IsDead { get; private set; } = false;


        public UnityEvent<Unit,int> OnInteractionEvent;
        public delegate void OnHealthChanged(float current, float max);
        public event OnHealthChanged OnHealthChangedEvent;
        public void Initialize(Unit owner)
        {
            _entity = owner;
            _statCompo = owner.GetUnitCompo<UnitStatCompo>();
            if(_entity as CharacterUnit)
                _shieldCompo = owner.GetUnitCompo<UnitShieldCompo>();
        }
        
        private void Start()
        {
            _defensivePower = _statCompo.GetStat(StatInfo.DefensivePower);
            
            if (_entity as CharacterUnit)
            {
                foreach (var unitState in StorageSO.unitStates)
                {
                    if(unitState.Data == _entity.unitSO)
                        _unitStateCompo = unitState;
                }
            
                maxHealth = (int)_unitStateCompo.MaxHealth;
                currentHealth = (int)_unitStateCompo.CurrentHp.Value;   
            }
            else
            { 
                maxHealth = currentHealth = _entity.unitSO.Maxhealth;
            }

            _unitAnimation = _entity.GetUnitCompo<UnitAnimation>();
        }

        public void SetMaxHp(int value)
        {
            maxHealth = value;

            if (currentHealth >= maxHealth)
                currentHealth = maxHealth;
            
            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);
        }

        public void ResetMaxHp()
        {
            maxHealth = (int)_unitStateCompo.MaxHealth;
            currentHealth = (int)_unitStateCompo.CurrentHp.Value;  
            
            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);
        }

        public void HealHp(int amount)
        {
            currentHealth += amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
            
            if (_entity as CharacterUnit)
            {
                CharacterUnit characterUnit = _entity as CharacterUnit;
               
                Bus<SetUpUnitHealthBar>.Raise(new SetUpUnitHealthBar(characterUnit.PlayableUnitID,CurrentHealth
                    ,MaxHealth, characterUnit.UnitImage));
                
                _unitStateCompo.Heal(amount);
            }

            int healHash = healText.nameHash;
            
            Vector3 pos = _unitAnimation.gameObject.transform.position + new Vector3(0, 1.2f);;
            
            PopupTextEvent textEvt = TextEvent.PopupTextEvent.Initializer(amount.ToString(), healHash
                , pos, 0.5f);  
            
            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);
            
            textEventChannel.RaiseEvent(textEvt);
        }
        

        public void ApplyDamage(DamageData damageData, Vector3 hitPoint, Vector3 hitNormal,
            Unit dealer,bool isCritical, bool isPenetrate)
        {
            if (IsDead)
            {
                _entity.OnDeathEvent?.Invoke();
                SoundManager.Instance.PlayClip("HitSound");   
                return;
            }
            

            if (IsInvincibility)
                return;

            int damage = damageData.damage;
            
            OnDefenseEvent?.Invoke(ref damage);
            damage = ApplyDamageTakenModifiers(damage);
            
            if (isPenetrate != true)
            {
                if (_entity as CharacterUnit && _shieldCompo.GetShieldValue() > 0)
                {
                    _shieldCompo.BreakShield((int)damageData.damage);
                    return;
                }

                _defensivePower = _entity.unitSO.DefensivePower;
                
                int CalculateDamage = (int)(damage * (_defensivePower / 100));
                damage -= CalculateDamage;
            
                currentHealth = Mathf.Clamp(currentHealth - (int)damage, 0, maxHealth);   
            }

            OnHealthChangedEvent?.Invoke(currentHealth, maxHealth);
            
            int typeHash = isCritical ? criticalText.nameHash : normalText.nameHash;
            
            Vector3 position = _unitAnimation.gameObject.transform.position + new Vector3(0, 1.2f);
            PopupTextEvent textEvt = TextEvent.PopupTextEvent.Initializer(damage.ToString(), typeHash
                , position, 0.5f);  
            
            textEventChannel.RaiseEvent(textEvt);
           
           if (_entity as CharacterUnit)
           {
               CharacterUnit characterUnit = _entity as CharacterUnit;
               
               Bus<SetUpUnitHealthBar>.Raise(new SetUpUnitHealthBar(characterUnit.PlayableUnitID,CurrentHealth,
                   MaxHealth, characterUnit.UnitImage));

               _unitStateCompo.TakeDamage(damage);
           }
           
           if (currentHealth <= 0)
           {
               IsDead = true;
               
               if(_entity as CharacterUnit)
                   StorageSO.unitStates.Remove(_unitStateCompo);
               SoundManager.Instance.PlayClip("HitSound");   
               
               _entity.OnDeathEvent?.Invoke();
               return;
           }
           else
           {
               _entity.OnHitEvent?.Invoke();
               OnInteractionEvent?.Invoke(dealer, damage);
               SoundManager.Instance.PlayClip("HitSound");   
           }
        }

        private int ApplyDamageTakenModifiers(int damage)
        {
            if (damage <= 0 || _entity == null)
                return damage;

            foreach (var modifier in _entity.GetComponents<IDamageTakenModifier>())
                damage = Mathf.Max(0, modifier.ModifyDamageTaken(damage));

            return damage;
        }
    }
}