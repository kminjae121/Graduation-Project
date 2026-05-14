using System.Collections.Generic;
using Code.Core.Events.Bus;
using Code.Core.Interfaces;
using Code.Map;
using Code.UnitSystem.UnitComponent;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UnitSystem
{
    public class UnitMoveCompo : RangeComponent
    {
        [SerializeField] private UnitAnimation animationCompo;
        [SerializeField] private UnitRotator rotatorCompo;
        [SerializeField] private UnitAnimationTrigger triggerCompo;

        private PathMover _pathMoverCompo;
        [field: SerializeField] public GameObject VisualPrefabs { get; set; }

        public IMapTile CurrentMapTile { get; set; }

        private IMapTile _targetMapTile;
        public UnitManageRangeCompo UnitRangeCompo { get; private set; }

        private readonly List<IMapTile> _movingTiles = new();

        private CharacterUnit _unit;
        private bool _isMoving;

        public float MoveCount { get; set; }
        
        private IMapTile _nextTile;

        protected override void Start()
        {
            base.Start();

            _unit = _owner as CharacterUnit;
            UnitRangeCompo = _unit.GetUnitCompo<UnitManageRangeCompo>();
            _pathMoverCompo = _unit.GetUnitCompo<PathMover>();
            
            Bus<UnitSetMoveEvent>.Subscribe(StartWalk);
            
            _unit.InputSO.OnClickMoveEvent += Move;
            _pathMoverCompo.OnMoveEnd += HandleMoveEnd;
            _isMoving = false;
        }

        protected override void OnDestroy()
        {
            _nextTile?.SetState(TileState.Enemy,false);
            Bus<UnitSetMoveEvent>.Unsubscribe(StartWalk);

            if (_unit == null)
                return;
            _unit.InputSO.OnClickMoveEvent -= Move;
            
            _pathMoverCompo.OnMoveEnd -= HandleMoveEnd;
        }

        private void EndTargeting()
        {
            _nextTile?.SetState(TileState.Enemy,false);
            VisualPrefabs.SetActive(false);
        }

        private void SetTargetTile(IMapTile tile)
        {
            _nextTile?.SetState(TileState.Enemy,false);

            VisualPrefabs.SetActive(true);
            VisualPrefabs.transform.rotation = _unit.transform.rotation;
            VisualPrefabs.transform.position = tile.WorldPos;

            _nextTile = tile;
        }

        private void CheckTilesCanMoving()
        {
            _movingTiles.Clear();

            foreach (var tile in TilesInRange)
                if (!tile.HasState(TileState.Obstacle) && !tile.HasState(TileState.Enemy))
                    _movingTiles.Add(tile);
        }

        private void Update()
        {
            if (_unit.isMyTurn && IsActive && !_isMoving)
            {
                CheckTilesCanMoving();

                IMapTile tile = _unit.InputSO.GetSelectedTile();

                if (tile != null && _movingTiles.Contains(tile))
                    SetTargetTile(tile);
                else
                    EndTargeting();
            }
            else if (!IsActive)
                EndTargeting();
        }

        public void StartWalk(UnitSetMoveEvent evt)
        {
            if (_unit.isMyTurn && !evt.isStart)
                ResetTile();
            else if (_unit.isMyTurn && evt.isStart)
                ReCheckInRange();
        }


        private void HandleResetTile()
        {
            UnitRangeCompo.RemoveAllRange();
            Bus<UnitSetMoveEvent>.Raise(new UnitSetMoveEvent(true));
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(true));
        }

        private void Move()
        {
            if (!_unit.isMyTurn)
                return;
            
            if (!IsActive)
                return;

            if (_isMoving)
                return;

            if (MoveCount >= 1)
                return;
            
            IMapTile tile = _unit.InputSO.GetSelectedTile();
            
            VisualPrefabs.SetActive(false);

            if (!_movingTiles.Contains(tile))
                return;
            
            Move(tile);
        }
        
        private void MoveStart(IMapTile tile)
        {
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(false));
            Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(_unit.gameObject,
                true, new Vector3(0.1f, 0.1f, 0.1f)));
            
            GridMap.Instance.SetGridVisible(false);
            _targetMapTile = tile;
            IsActive = false;
            _isMoving = true;

            rotatorCompo.SetDir(tile.WorldPos);
            
            animationCompo.PlaySelectAnimation("MOVE");
        }

        private void HandleMoveEnd()
        {
            GridMap.Instance.SetGridVisible(true);
            _isMoving = false;
            IsActive = true;
            
            CurrentMapTile = _targetMapTile;
            CurrentMapTile.SetTileUnit(_unit);

            Bus<TurnEndUIEvent>.Raise(new TurnEndUIEvent(false));
            Bus<UnitCamSettingEvent>.Raise(new UnitCamSettingEvent(null,
                false, new Vector3(0.1f, 0.1f, 0.1f)));
            Bus<SetAtkUIEvent>.Raise(new SetAtkUIEvent(true));
            
            animationCompo.PlaySelectAnimation("IDLE");
            UnitRangeCompo.RemoveAllRange();
        }

        private void Move(IMapTile tileInfo)
        {
            if (tileInfo == null)
                return;

            if (!tileInfo.HasState(TileState.Walkable))
                return;
            
            MoveStart(tileInfo);
            
            _pathMoverCompo.SetPathAndMove(CurrentMapTile.GridPos, tileInfo.GridPos);
            MoveCount++;
        }
    }
}