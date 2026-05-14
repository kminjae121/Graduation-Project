using System.Collections;
using _Code.Passive;
using Code.Core.Managers;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.Managers;
using Code.Map;
using Code.SkillSystem;
using Code.UI;
using Code.UnitSystem.Combat;
using GondrLib.Dependencies;
using Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.UnitSystem
{
    public class CharacterUnit : Unit
    {
        [Header("Basic Unit Refs")]
        [field: SerializeField] public InputReader InputSO { get; private set; }
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Image unitImage;
        [SerializeField] private UnitSpawnSO unitSpawnSO;

        #region UnitCompo
        public UnitMoveCompo MoveCompo { get; private set; }
        [field:SerializeField] public SkillComponent SkillCompo { get; private set; }
        public UnitAnimationTrigger TriggerCompo { get; private set; }
        public UnitManageRangeCompo UnitRangeCompo { get; private set; }
        public UnitStatCompo UnitStatCompo { get; private set; }
        public UnitSkillCost SkillCostCompo { get; private set; }
        public PassiveComponent PassiveCompo { get; private set; }
        public UnitOutLineCompo OutLineCompo { get; private set; }

        #endregion
        
        public int PlayableUnitID { get; set; } = -1;
        
        public GameObject _startTile;
        
        private readonly Vector3 _dampingSpeed = new(1.5f,1.5f,1.5f);

        public UnityEvent OnTurnStartEvent;
        public UnityEvent OnTurnEndEvent;

        private void Start()
        {
            TriggerCompo = GetUnitCompo<UnitAnimationTrigger>();
            MoveCompo = GetUnitCompo<UnitMoveCompo>();
            UnitRangeCompo =  GetUnitCompo<UnitManageRangeCompo>();
            UnitStatCompo = GetUnitCompo<UnitStatCompo>();
            SkillCostCompo =  GetUnitCompo<UnitSkillCost>();
            OutLineCompo =  GetUnitCompo<UnitOutLineCompo>();
            PassiveCompo = GetUnitCompo<PassiveComponent>();
            
            if (unitSO != null && SkillSendManager.Instance != null)
            {
                SkillSendManager.Instance.SyncEquippedSkills(unitSO);
            }

            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(false));

            if (TriggerCompo != null)
                TriggerCompo.OnDeadEvent += HandleDieAnimationEnd;

            MoveCompo.CurrentMapTile = _startTile.GetComponent<IMapTile>();
            MoveCompo.CurrentMapTile.SetTileUnit(this);
            
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(true));
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (TriggerCompo != null)
                TriggerCompo.OnDeadEvent -= HandleDieAnimationEnd;
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            
            Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(gameObject, false,_dampingSpeed));
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(true));
            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
            

            SkillCompo.ResetSkillsCount();
            SkillCostCompo.AddSkillCost();
            SkillCompo.UpdateSkillUI();
            PassiveCompo.StartAllTurnPassives();

            if (MoveCompo != null)
            {
                UnitRangeCompo.RemoveAllRange(); 
                MoveCompo.FindObjectInRange(unitSO.MoveRange);
                MoveCompo.MoveCount = 0;
            }
            
            OnTurnStartEvent?.Invoke();
            
            Bus<WhatUnitTurnEvent>.Raise(new WhatUnitTurnEvent(unitSO.UnitType));
        }

        public void SetMoveTile()
        {
            if (MoveCompo != null && isMyTurn)
            {
                if (MoveCompo.MoveCount < 1)
                {
                    UnitRangeCompo.RemoveAllRange(); 
                    MoveCompo.FindObjectInRange(unitSO.MoveRange);
                }
                else
                    return;
            }
        }

        public override void OnTurnEnd()
        {
            if (isMyTurn)
            {
                base.OnTurnEnd();
                UnitRangeCompo.RemoveAllRange(); 
                OnTurnEndEvent?.Invoke();
                PassiveCompo.StopAllTurnPassives();
                Bus<UnitMoveControlEvent>.Raise(new UnitMoveControlEvent(true));
                Bus<UnitAttackControlEvent>.Raise(new UnitAttackControlEvent(true));      
            }
        }

        protected override void Hit()
        {
            if (AnimationCompo != null)
            {
                AnimationCompo.RestartFromEntry();
                AnimationCompo.PlaySelectAnimation("HIT");
                StartCoroutine(ReturnIdleAnimation());
            }
            base.Hit();
        }
        

        public void HandleDieAnimationEnd()
        {
            MoveCompo.CurrentMapTile.SetState(TileState.Obstacle,false);
            
            if (StageManager.Instance != null)
                StageManager.Instance.PlayerDie();

            HealthCompo.StorageSO.units.Remove(unitSpawnSO);
            
            gameObject.SetActive(false);
        }

        public void Die()
        {
            if (AnimationCompo != null)
                AnimationCompo.PlaySelectAnimation("DEAD");
        }

        protected override void Dead()
        {
            base.Dead();
            Die();
        }
        
        private IEnumerator ReturnIdleAnimation()
        {
            yield return new WaitForSeconds(1.5f);
            AnimationCompo.ReturnIdleAnimation();
        }
    }
}