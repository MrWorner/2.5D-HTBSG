using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_LineOfSight : MonoBehaviour
{
    private static MG_LineOfSight _instance;//в редакторе только один объект должен быть создан
    public static MG_LineOfSight Instance { get => _instance; }

    #region Поля
    [SerializeField] private Color _greyFogOfWar;//Цвет серого тумана войны (разведан, 6F6D6D)
    [SerializeField] private List<GroundType> _penalityGroundTypes = new List<GroundType>() { GroundType.HeavyForest, GroundType.Hill };//Типы GroundType, которые могут блокировать видимость дальше при суммировании
    [SerializeField] private List<GroundType> _blockGroundTypes = new List<GroundType>() { GroundType.Mountain };//Типы GroundType, которые блокируют видимость

    #endregion Поля

    #region Поля: необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_FogOfWar _fogOfWar;//[R] Менеджер Тумана войны    
    [Required(InfoMessageType.Error), SerializeField] MG_GlobalHexMap _map;//[R] Карта
    //private MG_HexCell _cellForDrawLineTest;
    //[SerializeField] private Vector3Int _staticCellPosForDrawLineTest;
    //[SerializeField] private GameObject _testUnitPrefab;
    #endregion Поля: необходимые модули

    #region Свойства   
    public Color GreyFogOfWar { get => _greyFogOfWar; }//Цвет серого тумана войны (разведан, 6F6D6D)
    #endregion Свойства

    #region Методы UNITY
    void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Debug.Log("<color=orange>MG_LineOfSight Awake(): найдет лишний _instance.</color>");

        if (_fogOfWar == null)
            Debug.Log("<color=red>MG_LineOfSight Awake(): 'fogOfWarManager' не задан!</color>");

        if (_map == null)
            Debug.Log("<color=red>MG_LineOfSight Awake(): '_map' не задан!</color>");

    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Удалить видимость у наблюдателя
    /// </summary>
    /// <param name="visibleCells">видимые клетки</param>
    /// <param name="cellObserver">наблюдатель</param>
    public void RemoveVisionFromObserver(HashSet<MG_HexCell> visibleCells, ICellObserver cellObserver)
    {
        MG_Player side = cellObserver.GetSide();
        var list = visibleCells.ToList();
        foreach (var cell in list)
            cell.RemoveObserver(cellObserver);
        if (side.EqualsUID(MG_TurnManager.GetCurrentPlayerVisibleMap()))
            foreach (var cell in list)
            {
                if (!cell.HasAnyObserverBySide(side))
                    cell.SetVisibility(Visibility.GreyFog);
            }
    }
    /// <summary>
    /// Удалить видимость у наблюдателя
    /// </summary>
    /// <param name="visibleCells">видимые клетки</param>
    /// <param name="cellObserver">наблюдатель</param>
    public void RemoveVisionFromObserver(MG_HexCell cell, ICellObserver cellObserver)
    {
        MG_Player side = cellObserver.GetSide();
        cell.RemoveObserver(cellObserver);
        if (side.EqualsUID(MG_TurnManager.GetCurrentPlayerVisibleMap()))
            if (!cell.HasAnyObserverBySide(side))
                cell.SetVisibility(Visibility.GreyFog);
    }

    /// <summary>
    /// Переключить карту видимости на заданного игрока
    /// </summary>
    /// <param name="side">игрок</param>
    public void SwitchVisibilityMapFor(MG_Player side)
    {
        _fogOfWar.PlaceVisualFogOfWar();
        SetGreyFogOnAllVisitedCells(side);

        foreach (var unit in side.GetDivisions())
            foreach (var cell in unit.VisibleCells)
                cell.SetVisibility(Visibility.Visible);

        foreach (var settlement in side.GetSettlements())
            foreach (var cell in settlement.CellsUnderControl)
                cell.SetVisibility(Visibility.Visible);

        foreach (var cell in side.VisibleCells)
            cell.SetVisibility(Visibility.Visible);
    }

    /// <summary>
    /// Обновить Зону Видимости
    /// </summary>
    /// <param name="starterCell">начальная клетка</param>
    /// <param name="radius">радиус видимости</param>
    /// <param name="visibleCells">текущие видимые клетки</param>
    /// <param name="cellObserver">наблюдатель</param>
    /// <returns>обновленный список видимых клеток</returns>
    public HashSet<MG_HexCell> UpdateFOV(MG_HexCell starterCell, int radius, IReadOnlyCollection<MG_HexCell> visibleCells, ICellObserver cellObserver)
    {
        MG_Player side = cellObserver.GetSide();
        HashSet<MG_HexCell> updatedVisibleCells = DefineVisibleCells(starterCell, radius);

        if (updatedVisibleCells.Any())
        {
            //Есть хотя бы одна новая видимая клетка
            FindAndUnsubscribeFromUnusedCells(visibleCells, updatedVisibleCells, cellObserver);// Проверить старые клетки, если они уже не в зоне, то отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'
            SubscribeToCells(updatedVisibleCells, cellObserver);// Подписаться ко все новым клеткам
        }
        else
        {
            // нет ни одной видимой клетки
            UnsubscribeFromCells(visibleCells, cellObserver);// Проверить старые клетки, если они уже не в зоне, то отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'    
        }

        return updatedVisibleCells;
    }

    /// <summary>
    /// Обновить Зону Видимости для статических вещей (Поселения, территория которой владеет игрок и тд.)
    /// </summary>
    /// <param name="visibleCells"></param>
    /// <param name="cellObserver"></param>
    public void UpdateStaticFOV(IReadOnlyCollection<MG_HexCell> updatedVisibleCells, ICellObserver cellObserver)
    {
        if (updatedVisibleCells.Any())
        {
            SubscribeToCells(updatedVisibleCells, cellObserver);// Подписаться ко все новым клеткам
        }
    }

    /// <summary>
    /// Удалить зону видимости
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="cellObserver"></param>
    public void RemoveStaticFOV(IReadOnlyCollection<MG_HexCell> cells, ICellObserver cellObserver)
    {
        if (cells.Any())
        {
            UnsubscribeFromCells(cells, cellObserver);// Отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'
        }
    }

    /// <summary>
    /// Удалить зону видимости
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="cellObserver"></param>
    public void RemoveStaticFOV(MG_HexCell cell, ICellObserver cellObserver)
    {
        if (cell != null)
        {
            UnsubscribeFromCells(new HashSet<MG_HexCell>() { cell }, cellObserver);// Отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'
        }
    }

    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Установить серый туман у всех посещенных клеток
    /// </summary>
    /// <param name="side">сторона</param>
    private void SetGreyFogOnAllVisitedCells(MG_Player side)
    {
        foreach (var cell in side.DiscoveredCells)
        {
            cell.SetVisibility(Visibility.GreyFog);
        }
    }

    /// <summary>
    /// Подписаться к клеткам
    /// </summary>
    /// <param name="list"></param>
    /// <param name="cellObserver"></param>
    private void SubscribeToCells(IReadOnlyCollection<MG_HexCell> list, ICellObserver cellObserver)
    {
        foreach (var cell in list)
        {
            //if (!_cell.HasObserver(_side, cellObserver))
            cell.AddObserver(cellObserver);//запоминаем клетку, что данный юнит видет ее 
        }
    }

    /// <summary>
    /// Проверить старые клетки, если они уже не в зоне, то отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'
    /// </summary>
    /// <param name="visibleCells"></param>
    /// <param name="newVisibleCells"></param>
    /// <param name="cellObserver"></param>
    private void FindAndUnsubscribeFromUnusedCells(IReadOnlyCollection<MG_HexCell> visibleCells, HashSet<MG_HexCell> newVisibleCells, ICellObserver cellObserver)
    {
        if (visibleCells.Any())
        {
            foreach (var oldCell in visibleCells.ToHashSet())
            {
                if (!newVisibleCells.Contains(oldCell))
                {
                    oldCell.RemoveObserver(cellObserver);//удаляем старую клетку из списка, так как ее данный юнит не видит.
                }
            }
        }

    }

    /// <summary>
    /// Отписаться. Также накрыть туманом, если нет ни одного 'наблюдателя'
    /// </summary>
    /// <param name="visibleCells"></param>
    /// <param name="cellObserver"></param>
    private void UnsubscribeFromCells(IReadOnlyCollection<MG_HexCell> visibleCells, ICellObserver cellObserver)
    {
        if (visibleCells.Any())
        {
            foreach (var oldCell in visibleCells.ToArray())
            {
                oldCell.RemoveObserver(cellObserver);//удаляем старую клетку из списка, так как ее данный юнит не видит.
            }
        }
    }

    /// <summary>
    /// Получить список видимых клеток
    /// </summary>
    /// <param name="starterCell"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    private HashSet<MG_HexCell> DefineVisibleCells(MG_HexCell starterCell, int radius)
    {
        HashSet<MG_HexCell> newVisibleCells = new HashSet<MG_HexCell>();//Здесь будут храниться все новые+старые видимые клетки
        HashSet<Vector3Int> ringCells = MG_GeometryFuncs.Cube_ring(starterCell.HexPos, radius);//Получить координаты кольца
        //BEGIN Проводим линии по всем направлениям и запоминаем видимые клетки в список
        foreach (var resultPos in ringCells)
        {
            MG_HexCell cellN = _map.GetCellByHexPos(resultPos);//Получить клетку по Хекс координатам

            var line = MG_GeometryFuncs.Cube_Line(starterCell.HexPos, resultPos);//Нарисовать линию
            bool first = true;
            foreach (var pos in line)
            {
                cellN = _map.GetCellByHexPos(pos);//Получить клетку по Хекс координатам
                int penality = 0;//штраф к видимости следующей клетки
                if (cellN != null)
                {
                    if (first)
                    {
                        newVisibleCells.Add(cellN);
                        first = false;
                        continue;
                    }

                    if (_blockGroundTypes.Contains(cellN.Type.GroundType))
                    {
                        newVisibleCells.Add(cellN);
                        break;
                    }
                    else if (_penalityGroundTypes.Contains(cellN.Type.GroundType))
                    {
                        penality++;
                        newVisibleCells.Add(cellN);
                        if (penality > 2)
                            break;
                    }
                    else
                        newVisibleCells.Add(cellN);
                }
            }
        }
        //END Проводим линии по всем направлениям и запоминаем видимые клетки в список

        return newVisibleCells;
    }
    #endregion Личные методы
}
}

