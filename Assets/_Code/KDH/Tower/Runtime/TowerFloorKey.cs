using System;

namespace Code.Tower
{
    [Serializable]
    public readonly struct TowerFloorKey
    {
        public readonly int TowerFloor;
        public readonly int StageFloor;

        public TowerFloorKey(int towerFloor, int stageFloor)
        {
            TowerFloor = Math.Max(1, towerFloor);
            StageFloor = Math.Max(1, Math.Min(5, stageFloor));
        }

        public bool IsBossStage => StageFloor == 5;
        public string DisplayName => $"{TowerFloor}-{StageFloor}";

        public TowerFloorKey Next()
        {
            if (StageFloor < 5)
                return new TowerFloorKey(TowerFloor, StageFloor + 1);

            return new TowerFloorKey(TowerFloor + 1, 1);
        }
    }
}
