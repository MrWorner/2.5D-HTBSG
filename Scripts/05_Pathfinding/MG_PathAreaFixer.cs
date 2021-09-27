using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public static class MG_PathAreaFixer
{
    /// <summary>
    /// Починить зону доступности после передвижения
    /// </summary>
    /// <param name="unit">юнит</param>
    public static void FindAndFixAfterMove(MG_Division unit)
    {
        //Debug.Log("FindAndFixAfterMove() ЗАПУСТИЛ ДАННЫЙ МЕТОД: "+ unit.gameObject.name);
        var takenCell = unit.Cell;
        foreach (var player in MG_PlayerManager.Players)
        {
            foreach (var unitN in player.GetDivisions())
            {
                if (!unit.Equals(unitN))
                {

                    if (unitN.BlockedPathsInRange.Any())
                    {
                        foreach (var blockedCell in unitN.BlockedPathsInRange.ToList())
                        {
                            if (!blockedCell.IsTaken)//РАЗБЛОКИРОВАТЬ ОСВОБОДИВШЕЮСЯ КЛЕТКУ ДЛЯ СОСЕДНЕГО ЮНИТА
                            {
                                if (!unit.Side.Equals(unitN.Side))
                                {
                                    unitN.CalculatePathArea();//ВОЗМОЖНО ЗДЕСЬ ПОТРЕБУЕТСЯ ПЕРЕСЧИТАТЬ ЕСЛИ ЭТО ПРОТИВНИК ЗАНИМАЛ КЛЕТКУ! А ТО НА ОСТАЛЬНЫХ ЗАМЕТИЛ НЕ ДОТЯГИВАЕТСЯ ЗА ЭТОЙ КЛЕТКОЙ!
                                }
                                else
                                {
                                    unitN.RemoveBlockedCell(blockedCell);
                                    unitN.AddAvailableCell(blockedCell);
                                }

                                //Debug.Log("РАЗБЛОКИРОВАТЬ for " + unitN.name + " pos=" + blockedCell.Pos);


                            }
                        }
                    }

                    if (unitN.PathsInRange.Contains(takenCell))//ЗАБЛОКИРОВАТЬ ЗАНЯТУЮ КЛЕТКУ ДЛЯ СОСЕДНЕГО ЮНИТА
                    {

                        if (!unit.Side.Equals(unitN.Side))
                        {
                            //Debug.Log("CELL CANT BE REMOVED! RESETTING! CalculateAvailablePaths()" + takenCell.Pos + " for " + unitN.gameObject.name);
                            unitN.CalculatePathArea();
                        }
                        else
                        {
                            //Debug.Log("CELL REMOVED! " + takenCell.Pos + " for " + unitN.gameObject.name);
                            unitN.AddBlockedCell(takenCell);
                            unitN.RemoveAvailableCell(takenCell);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Починить зону доступности после уничтожение юнита
    /// </summary>
    /// <param name="defeatedSide">проигравшая сторона</param>
    public static void FindAndFixAfterDestroyedUnit(MG_Player defeatedSide)
    {
        foreach (var player in MG_PlayerManager.Players)
        {
            //Debug.Log("step0 " + player.name);
            foreach (var unit in player.GetDivisions())
            {
                if (unit.MovementPoints == 0)//09.04.2021
                    continue;

                //Debug.Log("step1 " + unit.name + " countBlocks:" + unit.BlockedPathsInRange.Count);
                if (unit.BlockedPathsInRange.Any())
                {
                    //Debug.Log("step2 " + unit.name);
                    foreach (var blockedCell in unit.BlockedPathsInRange.ToList())
                    {
                        //Debug.Log("step3 " + unit.name + " pos=" + blockedCell.Pos);
                        if (!blockedCell.IsTaken)//РАЗБЛОКИРОВАТЬ ОСВОБОДИВШЕЮСЯ КЛЕТКУ ДЛЯ СОСЕДНЕГО ЮНИТА
                        {
                            if (!unit.Side.Equals(defeatedSide))
                            {
                                //Debug.Log("CELL CANT BE REMOVED! RESETTING! CalculateAvailablePaths()" + takenCell.Pos + " for " + unitN.gameObject.name);
                                unit.CalculatePathArea();
                            }
                            else
                            {
                                //Debug.Log("step4 РАЗБЛОКИРОВАТЬ for " + unit.name + " pos=" + blockedCell.Pos);
                                unit.RemoveBlockedCell(blockedCell);
                                unit.AddAvailableCell(blockedCell);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Обновить зону доступности у юнитов после обновления клетки
    /// </summary>
    /// <param name="cell"></param>
    public static void FixAfterCellUpdate(MG_HexCell cell)
    {
        float roadMovementCost = 1;
        bool hasRoad = cell.HasAnyRoad();
        if (hasRoad)
        {
            roadMovementCost = MG_HexRoadManager.RoadMovementCost;
        }

        //НА БУДУЩЕЕ: возможно лучше будет хранить данные в самой клетке кто может до нее дотянуться чем так каджый юнит проверять дистанцией, а то очень затратно если их будет много!

        foreach (var player in MG_PlayerManager.Players)
        {
            foreach (var unit in player.GetDivisions())
            {
                float movementPoints = unit.MovementPoints;
                if (movementPoints == 0)
                    continue;

                if (hasRoad)
                    movementPoints = movementPoints * (1 / roadMovementCost);//EXPERIMENTAL: Для того чтобы подпавить после того как на этой клетке была установлена новая дорога. ИНАЧЕ не будет правильно отображать стоимость и дотягивания юнитом! 

                int distance = MG_GeometryFuncs.Cube_distance(cell.HexPos, unit.Cell.HexPos);
                if (movementPoints >= distance)//новый способ найти юнита для которого необходимо повторно вычислить доступные клетки
                    unit.CalculatePathArea();
            }
        }
    }

    /// <summary>
    /// Обновить зону доступности у юнитов после раскрытия карты
    /// </summary>
    public static void FixAfterMapReveal()
    {
        foreach (var player in MG_PlayerManager.Players)
        {
            //Debug.Log("step0 " + player.name);
            foreach (var unit in player.GetDivisions())
            {
                if (unit.MovementPoints == 0)//09.04.2021
                    continue;

                unit.CalculatePathArea();
            }
        }
    }
}
}