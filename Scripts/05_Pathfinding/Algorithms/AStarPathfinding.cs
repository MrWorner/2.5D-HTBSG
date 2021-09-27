using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Можно поизучать, но там требования NET 5.0, который недоступен для UNITY https://habr.com/ru/post/513158/
//Можно попробовать getHash и сравнивать, а не ссылки. Чтобы ускорить производительность.

namespace MG_StrategyGame
{
class AStarPathfinding : IPathfinding<MG_HexCell>
{
    /// <summary>
    /// Найти путь
    /// </summary>
    /// <param name="originCell">начало</param>
    /// <param name="targetCell">конец</param>
    /// <param name="unwalkableGroundTypes">непроходимые типы клеток</param>
    /// <param name="side">сторона</param>
    /// <returns>путь к цели</returns>
    public override List<MG_HexCell> FindPath(MG_HexCell originCell, MG_HexCell targetCell, IReadOnlyCollection<GroundType> unwalkableGroundTypes, MG_Player side)
    {

        //--BEGIN Клетка цель = начало
        if (originCell.EqualsUID(targetCell))
            return new List<MG_HexCell>();//если с той клетки которой начинаем является целью, то возвращаем пустой лист
        //--END Клетка цель = начало

        //--BEGIN blockLastFriendlyTakenCell<- необходимо задать, данная переменная нужна если вдруг цель клетки занята вражеским юнитом, то мы перед ней остановимся, НО клетка остановки не должна быть также заблокирована кем либо! Иначе баг: два юнита на одной клетке
        bool blockLastFriendlyTakenCell = false;
        if (targetCell.HasAnyObserverBySide(side))
        {
            MG_Division unit = targetCell.Division;//юнит на данной клетке
            if (unit != null)//если какой либо юнит на данной клетке существует
            {
                if (!unit.Side.EqualsUID(side))//если на клетке юнит другой стороны
                {
                    blockLastFriendlyTakenCell = true;//нужен для: должны будем заблокировать клетку на которой остановимся на атаке, так как здесь наш юнит. Иначе баг: два юнита на одной клетке
                }
            }
        }
        //--END blockLastFriendlyTakenCell<- необходимо задать, данная переменная нужна если вдруг цель клетки занята вражеским юнитом, то мы перед ней остановимся, НО клетка остановки не должна быть также заблокирована кем либо! Иначе баг: два юнита на одной клетке

        IPriorityQueue<MG_HexCell> frontier = new HeapPriorityQueue<MG_HexCell>();
        frontier.Enqueue(originCell, 0);

        var cameFrom = new Dictionary<MG_HexCell, MG_HexCell>();
        cameFrom.Add(originCell, default(MG_HexCell));
        var costSoFar = new Dictionary<MG_HexCell, float>();
        costSoFar.Add(originCell, 0);
        while (frontier.Count != 0)
        {
            MG_HexCell current = frontier.Dequeue();
            if (current.EqualsUID(targetCell)) break;

            var neighbours = current.GetNeighbours();

            foreach (var cellN in neighbours)
            {
                bool isNotBlackFog = cellN.IsDiscoveredByPlayer(side);
                bool hasRoad = cellN.HasAnyRoad();
                bool hasRoad_currentCell = current.HasAnyRoad();
                bool hasRiver = cellN.HasAnyRiver();

                bool hasBridge = hasRoad_currentCell && hasRoad;
                bool hasAdvancedRiver = false;
                if (hasBridge == false)
                {
                    DirectionHexPT directionOfNeighbourCell = current.GetDirectionOfNeighbour(cellN);
                    hasAdvancedRiver = current.HasAnyAdvancedRiver(directionOfNeighbourCell);
                }


                if (!cellN.EqualsUID(targetCell))
                {
                    bool visible = cellN.IsVisibleForPlayer(side);
                    //---BEGIN РАБОТАЕТ. Если противник на клетке, то через эту клетку нельзя передвигаться!
                    if (visible)
                    {
                        if (cellN.IsTaken)
                        {
                            MG_Division unit = cellN.Division;
                            if (unit != null)
                            {
                                if (!unit.Side.EqualsUID(side))
                                    continue;//БЛОКИРУЕМ КЛЕТКУ ТАК КАК ЧЕРЕЗ НЕЁ НЕЛЬЗЯ ПЕРЕДВИГАТЬСЯ ИЗ-ЗА ЮНИТА ДРУГОЙ СТОРОНЫ
                                else
                                {
                                    if (blockLastFriendlyTakenCell)
                                    {
                                        if (targetCell.HasNeighbour(cellN))
                                        {
                                            continue;//БЛОКИРУЕМ СОСЕДНЮЮ КЛЕТКУ НА КОТОРЫЙ ЕСТЬ СВОЙ ЮНИТ, ЧТОБЫ НАЙТИ СВОБОДНУЮ ДЛЯ АТАКИ
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MG_Settlement settlement = cellN.Settlement;
                            if (settlement != null)
                            {
                                if (!settlement.Side.EqualsUID(side))
                                    continue;//Если город не принадлежит юниту, то через него лучше не проходить!
                            }
                        }
                    }
                    //---END РАБОТАЕТ. Если противник на клетке, то через эту клетку нельзя передвигаться!

                    if (isNotBlackFog)
                    {
                        //---BEGIN РАБОТАЕТ. Если гора, то через эту клетку нельзя передвигаться!
                        if (hasRoad == false)
                            if (unwalkableGroundTypes.Contains(cellN.Type.GroundType))
                                continue;
                        //---END РАБОТАЕТ. Если гора, то через эту клетку нельзя передвигаться!
                    }
                }

                if (isNotBlackFog)
                {
                    if (hasRoad)
                    {
                        cellN._temp_movementCost = MG_HexRoadManager.RoadMovementCost;//Road
                        if (hasBridge == false)//Если нет моста, то есть у текущей и следующей клетки нет дорог которые соединяли бы, то штраф через реку (ADVANCED)
                            if (hasAdvancedRiver)
                                cellN._temp_movementCost += MG_HexAdvancedRiverManager.RiverMovementCost;
                    }
                    else
                    {

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

                float newCost = costSoFar[current] + cellN._temp_movementCost * 10;//09.04.2021 <--- НЕОБХОДИМО для "ЛУЧШЕЕ РЕШЕНИЕ ВСЕХ ВРЕМЕН!", иначе баг: неверный патч через густой лес где в итоге -1 будет показывать AreaPath при наведении

                if (!costSoFar.ContainsKey(cellN) || newCost < costSoFar[cellN])
                {
                    costSoFar[cellN] = newCost;
                    cameFrom[cellN] = current;
                    //float priority = newCost + Heuristic(destinationNode, neighbour);-----OLD
                    //float priority = newCost + 1;//ДИСТАНЦИЯ ПО УМОЛЧАНИЮ РАВНА 1, НО ЭТО НЕ БУДЕТ  A*
                    float priority = newCost + Vector2.Distance(cellN.WorldPos, targetCell.WorldPos);//ЛУЧШЕЕ РЕШЕНИЕ ВСЕХ ВРЕМЕН!
                    //Debug.Log(Vector2.Distance(cellN.WorldPos, targetCell.WorldPos));
                    frontier.Enqueue(cellN, priority);
                }
            }
        }

        List<MG_HexCell> path = new List<MG_HexCell>();
        if (!cameFrom.ContainsKey(targetCell))
            return path;

        path.Add(targetCell);
        MG_HexCell temp = targetCell;

        while (!cameFrom[temp].EqualsUID(originCell))
        {
            MG_HexCell currentPathElement = cameFrom[temp];
            path.Add(currentPathElement);

            temp = currentPathElement;
        }

        return path;
    }
    }

}