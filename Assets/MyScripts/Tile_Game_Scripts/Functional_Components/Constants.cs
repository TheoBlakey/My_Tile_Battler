using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static readonly List<Color> ColorList = new()
    {
       new (0, 0, 0, 0), //NOT TAKE
       new (0, 0, 1, 0.5f), //blue
       new (1, 0, 0, 0.5f), //red
       new (0, 0.25f, 0, 0.5f), //dark green
       new (1, 0, 1, 0.5f), //magenta
       new (0, 1, 0, 1), //green
       new(1, 1, 1, 1) //white
    };

    public static readonly int ConstructionTime = 10;


    //public static readonly Dictionary<TilePathFindingComponent.Direction, (int, int)> DirectionOffsetsOdd = new()
    //{
    //    { TilePathFindingComponent.Direction.N, (0, 1) },
    //    { TilePathFindingComponent.Direction.NE, (1, 0) },
    //    { TilePathFindingComponent.Direction.NW, (-1, 0) },
    //    { TilePathFindingComponent.Direction.S, (0, -1) },
    //    { TilePathFindingComponent.Direction.SE, (1, -1) },
    //    { TilePathFindingComponent.Direction.SW, (-1, -1) }
    //};

    //public static readonly Dictionary<TilePathFindingComponent.Direction, (int, int)> DirectionOffsetsEven = new()
    //{
    //    { TilePathFindingComponent.Direction.N, (0, 1) },
    //    { TilePathFindingComponent.Direction.NE, (1, 1) },
    //    { TilePathFindingComponent.Direction.NW, (-1, 1) },
    //    { TilePathFindingComponent.Direction.S, (0, -1) },
    //    { TilePathFindingComponent.Direction.SE, (1, 0) },
    //    { TilePathFindingComponent.Direction.SW, (-1, 0) }
    //};
}