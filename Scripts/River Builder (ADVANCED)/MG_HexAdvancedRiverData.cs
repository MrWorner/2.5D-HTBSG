using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexAdvancedRiverData : IVisibility, IMarkable
{
    #region Поля
    private MG_HexAdvancedRiver _riverType;//[R CONSTRUCTOR] тип реки
    private Tilemap _advancedRiverTileMap;//[R CONSTRUCTOR] объект стандартной TileMap (карта рек)
    private Vector3Int _pos;//[R CONSTRUCTOR]
    private MG_HexCell _cell;//[R CONSTRUCTOR]
    private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
    private Dictionary<DirectionHexPT, Vector3> _centersOfSides = new Dictionary<DirectionHexPT, Vector3>();//[R] данные о позициях ребр хекса. Необходим для визуальных маркеров

    private bool _river_NE = false;//Есть ли река на направлении Север-Восток
    private bool _river_E = false;//Есть ли река на направлении Восток
    private bool _river_SE = false;//Есть ли река на направлении Юг-Восток
    private bool _river_SW = false;//Есть ли река на направлении Юг-Запад
    private bool _river_W = false;//Есть ли река на направлении Запад
    private bool _river_NW = false;//Есть ли река на направлении Север-Запад
    #endregion Поля

    #region Свойства
    public MG_HexAdvancedRiver RiverType { get => _riverType; set => _riverType = value; }//[R CONSTRUCTOR] Тип реки
    public IReadOnlyDictionary<DirectionHexPT, Vector3> CentersOfSides { get => _centersOfSides; }//данные о позициях ребр хекса. Необходим для визуальных маркеров

    public bool River_NE { get => _river_NE; }//Есть ли река на направлении Север-Восток
    public bool River_E { get => _river_E; }//Есть ли река на направлении Восток
    public bool River_SE { get => _river_SE; }//Есть ли река на направлении Юг-Восток
    public bool River_SW { get => _river_SW; }//Есть ли река на направлении Юг-Запад
    public bool River_W { get => _river_W; }//Есть ли река на направлении Запад
    public bool River_NW { get => _river_NW; }//Есть ли река на направлении Север-Запад
    #endregion Свойства

    #region Конструктор
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pos">позиция</param>
    public MG_HexAdvancedRiverData(MG_HexCell cell)
    {
        _advancedRiverTileMap = MG_HexAdvancedRiverManager.GetRiverTileMap();
        _riverType = MG_HexAdvancedRiverLibrary.GetEmptyRiver();
        _lineOfSight = MG_LineOfSight.Instance;
        _pos = cell.Pos;
        _cell = cell;

        if (_advancedRiverTileMap == null) Debug.Log("<color=red>MG_HexAdvancedRiverData MG_HexAdvancedRiverData(): '_advancedRiverTileMap' не задан!</color>");
        if (_riverType == null) Debug.Log("<color=red>MG_HexAdvancedRiverData MG_HexAdvancedRiverData(): '_riverType' не задан!</color>");
        if (_lineOfSight == null) Debug.Log("<color=red>MG_HexAdvancedRiverData MG_HexAdvancedRiverData(): '_lineOfSight' не задан!</color>");

        FillCentersOfSidesDIctionary();// Заполнить словарик
    }
    #endregion Конструктор

    #region Публичные методы
    /// <summary>
    /// Установить реку по направлению
    /// </summary>
    /// <param name="dir"></param>
    public void SetRiverByDirection(DirectionHexPT dir)
    {
        switch (dir)
        {
            case DirectionHexPT.NE:
                if (_river_NE == false)
                {
                    _river_NE = true;
                }
                break;
            case DirectionHexPT.E:
                if (_river_E == false)
                {
                    _river_E = true;
                }
                break;
            case DirectionHexPT.SE:
                if (_river_SE == false)
                {

                }
                _river_SE = true;
                break;
            case DirectionHexPT.SW:
                if (_river_SW == false)
                {
                    _river_SW = true;
                }
                break;
            case DirectionHexPT.W:
                if (_river_W == false)
                {
                    _river_W = true;
                }
                break;
            case DirectionHexPT.NW:
                if (_river_NW == false)
                {
                    _river_NW = true;
                }
                break;
            default:
                Debug.Log("<color=red>MG_HexAdvancedRiverData SetRiverByDirection(): DEFAULT!</color>");
                break;
        }
    }

    /// <summary>
    /// Удалить реку по направлению
    /// </summary>
    /// <param name="dir"></param>
    public void RemoveRiverByDirection(DirectionHexPT dir)
    {
        switch (dir)
        {
            case DirectionHexPT.NE:
                if (_river_NE)
                {
                    _river_NE = false;
                }
                break;
            case DirectionHexPT.E:
                if (_river_E)
                {
                    _river_E = false;
                }
                break;
            case DirectionHexPT.SE:
                if (_river_SE)
                {
                    _river_SE = false;
                }
                break;
            case DirectionHexPT.SW:
                if (_river_SW)
                {
                    _river_SW = false;
                }
                break;
            case DirectionHexPT.W:
                if (_river_W)
                {
                    _river_W = false;
                }
                break;
            case DirectionHexPT.NW:
                if (_river_NW)
                {
                    _river_NW = false;
                }
                break;
            default:
                Debug.Log("<color=red>MG_HexAdvancedRiverData RemoveRiverByDirection(): DEFAULT!</color>");
                break;
        }
    }

    /// <summary>
    /// Есть ли река по заданному направлению
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool HasRiver(DirectionHexPT dir)
    {
        switch (dir)
        {
            case DirectionHexPT.NE:
                return _river_NE;
            case DirectionHexPT.E:
                return _river_E;
            case DirectionHexPT.SE:
                return _river_SE;
            case DirectionHexPT.SW:
                return _river_SW;
            case DirectionHexPT.W:
                return _river_W;
            case DirectionHexPT.NW:
                return _river_NW;
            default:
                Debug.Log("<color=red>MG_HexAdvancedRiverData HasRiver(): DEFAULT!</color>");
                break;
        }
        return false;
    }
    #endregion Публичные методы

    #region Личные методы

    /// <summary>
    /// Заполнить словарик стороны хекса с позициями центра
    /// </summary>
    private void FillCentersOfSidesDIctionary()
    {

        Tilemap tileMap = _cell.Map.TileMap;
        Vector3 posCenter = _cell.WorldPos;//Центральная клетка

        int x = _cell.Pos.x;
        int y = _cell.Pos.y;

        //--Находим глобальную позицию хекс клеток соседей
        Vector3 posN_1;//Сосед по направлению: DirectionHexPT.NE
        Vector3 posN_2;//Сосед по направлению: DirectionHexPT.E
        Vector3 posN_3;//Сосед по направлению: DirectionHexPT.SE
        Vector3 posN_4;//Сосед по направлению: DirectionHexPT.SW
        Vector3 posN_5;//Сосед по направлению: DirectionHexPT.W
        Vector3 posN_6;//Сосед по направлению: DirectionHexPT.NW
        if (_cell.IsEvenRow)
        {
            posN_1 = tileMap.CellToWorld(new Vector3Int(x, y + 1, 0));//Сосед по направлению: DirectionHexPT.NE
            posN_2 = tileMap.CellToWorld(new Vector3Int(x + 1, y, 0));//Сосед по направлению: DirectionHexPT.E
            posN_3 = tileMap.CellToWorld(new Vector3Int(x, y - 1, 0));//Сосед по направлению: DirectionHexPT.SE
            posN_4 = tileMap.CellToWorld(new Vector3Int(x - 1, y - 1, 0));//Сосед по направлению: DirectionHexPT.SW
            posN_5 = tileMap.CellToWorld(new Vector3Int(x - 1, y, 0));//Сосед по направлению: DirectionHexPT.W
            posN_6 = tileMap.CellToWorld(new Vector3Int(x - 1, y + 1, 0));//Сосед по направлению: DirectionHexPT.NW
        }
        else
        {
            posN_1 = tileMap.CellToWorld(new Vector3Int(x + 1, y + 1, 0));//Сосед по направлению: DirectionHexPT.NE
            posN_2 = tileMap.CellToWorld(new Vector3Int(x + 1, y, 0));//Сосед по направлению: DirectionHexPT.E
            posN_3 = tileMap.CellToWorld(new Vector3Int(x + 1, y - 1, 0));//Сосед по направлению: DirectionHexPT.SE
            posN_4 = tileMap.CellToWorld(new Vector3Int(x, y - 1, 0));//Сосед по направлению: DirectionHexPT.SW
            posN_5 = tileMap.CellToWorld(new Vector3Int(x - 1, y, 0));//Сосед по направлению: DirectionHexPT.W
            posN_6 = tileMap.CellToWorld(new Vector3Int(x, y + 1, 0));//Сосед по направлению: DirectionHexPT.NW
        }
        //--Находим глобальную позицию центров (сторон) хекс клеток соседей
        Vector3 pos_NE = (posCenter + posN_1) / 2;//Позиция на ребре по направлению: DirectionHexPT.NE
        Vector3 pos_E = (posCenter + posN_2) / 2;//Позиция на ребре по направлению: DirectionHexPT.E
        Vector3 pos_SE = (posCenter + posN_3) / 2;//Позиция на ребре по направлению: DirectionHexPT.SE
        Vector3 pos_SW = (posCenter + posN_4) / 2;//Позиция на ребре по направлению: DirectionHexPT.SW
        Vector3 pos_W = (posCenter + posN_5) / 2;//Позиция на ребре по направлению: DirectionHexPT.W
        Vector3 pos_NW = (posCenter + posN_6) / 2;//Позиция на ребре по направлению: DirectionHexPT.NW

        //--Добавляем данные в справочник
        _centersOfSides.Add(DirectionHexPT.NE, pos_NE);
        _centersOfSides.Add(DirectionHexPT.E, pos_E);
        _centersOfSides.Add(DirectionHexPT.SE, pos_SE);
        _centersOfSides.Add(DirectionHexPT.SW, pos_SW);
        _centersOfSides.Add(DirectionHexPT.W, pos_W);
        _centersOfSides.Add(DirectionHexPT.NW, pos_NW);

        //--Test
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_NE);
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_E);
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_SE);
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_SW);
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_W);
        //MG_HexAdvancedRiverGrid.CreateCursorMarker(pos_NW);
    }
    #endregion Личные методы


    #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
    /// <summary>
    /// Установить видимость (IVisibility)
    /// </summary>
    /// <param name="visibility">видимость</param>
    public void SetVisibility(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.BlackFog:
                Mark(Color.clear);
                break;

            case Visibility.GreyFog:
                Mark(_lineOfSight.GreyFogOfWar);
                break;

            case Visibility.Visible:
                if (_cell.Settlement != null)
                {
                    Mark(Color.clear);//прячем реку если на данной клетке есть поселение
                    break;
                }
                UnMark();
                break;

            default:
                Debug.Log("<color=orange>MG_HexAdvancedRiverData SetVisibility(): switch DEFAULT.</color>");
                break;
        }
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"

    #region МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
    /// <summary>
    /// Пометить цветом (IMarkable)
    /// </summary>
    /// <param name="color">Цвет</param>
    public void Mark(Color color)
    {
        _advancedRiverTileMap.SetColor(_pos, color);
    }

    /// <summary>
    /// Сбросить выделение (IMarkable)
    /// </summary>
    public void UnMark()
    {
        _advancedRiverTileMap.SetColor(_pos, Color.white);
        //throw new System.NotImplementedException();
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
}

[Serializable]
public class JSON_HexAdvancedRiverData
{
    public Vector2Int pos;
    public List<DirectionHexPT> dirList;
}
}