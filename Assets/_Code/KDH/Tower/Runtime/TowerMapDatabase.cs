using System.Collections.Generic;
using UnityEngine;

namespace Code.Tower
{
    [CreateAssetMenu(menuName = "Tower/Tower Map Database", fileName = "TowerMapDatabase")]
    public sealed class TowerMapDatabase : ScriptableObject
    {
        public const string DefaultResourcesPath = "TowerMapDatabase";

        [SerializeField] private List<TowerFloorMapDefinition> floorMaps = new();

        public bool TryCreateMap(TowerFloorKey floorKey, out TowerFloorMap map)
        {
            map = null;

            if (floorMaps == null)
                return false;

            foreach (TowerFloorMapDefinition floorMap in floorMaps)
            {
                if (floorMap == null || !floorMap.Matches(floorKey))
                    continue;

                map = floorMap.BuildMap();
                return map != null;
            }

            return false;
        }

        public static TowerMapDatabase LoadDefault()
        {
            return Resources.Load<TowerMapDatabase>(DefaultResourcesPath);
        }
    }
}
