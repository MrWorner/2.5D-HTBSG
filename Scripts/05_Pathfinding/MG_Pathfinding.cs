using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Данный класс для запуска нахождения пути
/// </summary>

namespace MG_StrategyGame
{
public static class MG_Pathfinding
{
    private readonly static DijkstraPathfinding _dijkstar = new DijkstraPathfinding();
    private readonly static IPathfinding<MG_HexCell> _astar = new AStarPathfinding();

    public static IPathfinding<MG_HexCell> Astar { get => _astar; }
    public static DijkstraPathfinding Dijkstar { get => _dijkstar; }
}
}
