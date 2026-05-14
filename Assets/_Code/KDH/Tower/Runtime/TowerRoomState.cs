using System;

namespace Code.Tower
{
    [Flags]
    public enum TowerRoomState
    {
        None = 0,
        Discovered = 1,
        Visited = 2,
        Cleared = 4
    }
}
