using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_CellNeighbourManager : MonoBehaviour
{
    private static MG_CellNeighbourManager _instance;//в редакторе только один объект должен быть создан

    #region Методы UNITY
    private void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Debug.Log("<color=orange>MG_NeighbourManager Awake(): найдет лишний _instance класса MG_NeighbourManager.</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Инициализация клетки (знакомим со всеми близжайщими соседями)
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="map"></param>
    public void DefineNeighbours(MG_HexCell cell, MG_GlobalHexMap map)//Для определения соседей (клетки)
    {
        bool isEvenRow = cell.IsEvenRow;
        int x = cell.Pos.x;
        int y = cell.Pos.y;

        int[][] coord;//координаты для суммирования (массив координат, для нахождения соседей через цикл For)
        if (isEvenRow)//данная клетка находиться в четном ряде? (Point Flopped)
            coord = MG_Direction.Coord_EvenRow;//присваиваем координаты для суммирования для четного ряда
        else
            coord = MG_Direction.Coord_OddRow;//присваиваем координаты для суммирования для нечетного ряда

        for (int i = 0; i < coord.Length; i++)
        {
            int dirX = coord[i][0];
            int dirY = coord[i][1];
            int xn = x + dirX;//координата соседа по X
            int yn = y + dirY;//координата соседа по Y

            Vector3Int posN = new Vector3Int(xn, yn, 0);

            if (map.IsCellExist(posN))
            {
                MG_HexCell cellN = map.GetCell(posN);//получаем соседнию клетку

                DirectionHexPT dir = MG_Direction.GetDirectionByXY(dirX, dirY, isEvenRow);//получаем направление соседа              
                DirectionHexPT dirN = MG_Direction.ReverseDir(dir);//зеркалим направление

                AcceptNeighbour(cell, cellN, dir);//устанавливаем соседа у клетки
                AcceptNeighbour(cellN, cell, dirN);//устанавливаем также и у клетки соседа нового соседа (Обратная связь)
            }
        }
    }

    #endregion Публичные методы

    #region Закрытые методы
    /// <summary>
    /// Добавляем клетку в список соседа (ОДНОСТОРОНЕ).
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="cellN"></param>
    /// <param name="dir"></param>
    private void AcceptNeighbour(MG_HexCell cell, MG_HexCell cellN, DirectionHexPT dir)
    {
        cell.AddNeighbour(cellN, dir);//добавляем клетку в список соседа
    }
    #endregion Закрытые методы

}
}
