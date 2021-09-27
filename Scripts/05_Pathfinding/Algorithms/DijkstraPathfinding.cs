using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
/// <summary>
/// Implementation of Dijkstra pathfinding algorithm.
/// </summary>
public class DijkstraPathfinding
{
    /// <summary>
    /// Найти зону
    /// </summary>
    /// <param name="originCell">начало</param>
    /// <param name="unwalkableGroundTypes">непроходимые типы клеток</param>
    /// <param name="movementPoints">очки движения</param>
    /// <param name="side">сторона</param>
    /// <param name="blockedNodes">заблокированные клетки</param>
    /// <returns>все свободные клетки по которым можно пройтись</returns>
    public Dictionary<MG_HexCell, List<MG_HexCell>> FindArea(MG_HexCell originCell, IReadOnlyCollection<GroundType> unwalkableGroundTypes, float movementPoints, MG_Player side, HashSet<MG_HexCell> blockedNodes)
    {

        IPriorityQueue<MG_HexCell> frontier = new HeapPriorityQueue<MG_HexCell>();
        frontier.Enqueue(originCell, 0);

        var cameFrom = new Dictionary<MG_HexCell, MG_HexCell>();
        cameFrom.Add(originCell, default(MG_HexCell));
        var costSoFar = new Dictionary<MG_HexCell, float>();
        costSoFar.Add(originCell, 0);

        //bool isWorkDone = false;
        //while (isWorkDone == false)
        while (frontier.Count != 0)
        {
            MG_HexCell current = frontier.Dequeue();

            //var neighbours = GetNeigbours(walkableCells, current);
            var neighbours = current.GetNeighbours();
            foreach (var cellN in neighbours)
            {
                bool isNotBlackFog = cellN.IsDiscoveredByPlayer(side);
                if (isNotBlackFog)
                {

                    //-bool visible = cellN.CurrentVisibility.EqualsUID(Visibility.Visible);
                    bool visible = cellN.IsVisibleForPlayer(side);
                    if (visible)
                    {
                        if (cellN.IsTaken)
                        {
                            MG_Division unit = cellN.Division;
                            if (unit != null)
                            {
                                if (!unit.Side.EqualsUID(side))
                                {
                                    blockedNodes.Add(cellN);// Если противник на клетке, то через эту клетку нельзя передвигаться!
                                    continue;
                                }
                            }
                        }
                    }

                    bool hasRoad = cellN.HasAnyRoad();
                    bool hasRiver = cellN.HasAnyRiver();

                    bool hasRoad_currentCell = current.HasAnyRoad();
                    bool hasBridge = hasRoad_currentCell && hasRoad;
                    bool hasAdvancedRiver = false;
                    if (hasBridge == false)
                    {
                        DirectionHexPT directionOfNeighbourCell = current.GetDirectionOfNeighbour(cellN);
                        hasAdvancedRiver = current.HasAnyAdvancedRiver(directionOfNeighbourCell);
                    }

                    if (hasRoad)
                    {

                        cellN._temp_movementCost = MG_HexRoadManager.RoadMovementCost;//Road3
                        if (hasBridge == false)//Если нет моста, то есть у текущей и следующей клетки нет дорог которые соединяли бы, то штраф через реку (ADVANCED)
                            if (hasAdvancedRiver)
                                cellN._temp_movementCost += MG_HexAdvancedRiverManager.RiverMovementCost;

                    }
                    else
                    {
                        if (unwalkableGroundTypes.Contains(cellN.Type.GroundType))
                            continue;//Если например гора или вода, то через эту клетку нельзя передвигаться!

                        cellN._temp_movementCost = cellN.Type.MovementCost;

                        if (hasAdvancedRiver)
                        {
                            cellN._temp_movementCost += MG_HexAdvancedRiverManager.RiverMovementCost;
                        }
                        else if (hasRiver)//штраф через реку (ADVANCED)
                        {
                            cellN._temp_movementCost += MG_HexRiverManager.RiverMovementCost;
                        }
                    }
                }
                else
                    cellN._temp_movementCost = 1f;

                float newCost = costSoFar[current] + cellN._temp_movementCost;

                if (movementPoints >= newCost)
                {
                    if (!costSoFar.ContainsKey(cellN) || newCost < costSoFar[cellN])
                    {
                        costSoFar[cellN] = newCost;
                        cameFrom[cellN] = current;
                        frontier.Enqueue(cellN, newCost);
                    }
                }
            }
        }

        var paths = new Dictionary<MG_HexCell, List<MG_HexCell>>();
        foreach (var destination in cameFrom.Keys)
        {
            var path = new List<MG_HexCell>();
            MG_HexCell current = destination;
            //current.Mark(Color.red);
            while (!current.EqualsUID(originCell))
            {
                path.Add(current);
                current = cameFrom[current];
            }
            paths.Add(destination, path);
        }

        return paths;
    }
}
}
