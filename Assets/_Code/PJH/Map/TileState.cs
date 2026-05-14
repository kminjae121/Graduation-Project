using System;

namespace Code.Map
{
    [Flags]
    public enum TileState
    {
        None = 0,
        Walkable = 1 << 0,
        Obstacle = 1 << 1,
        Enemy = 1 << 2
    }
}