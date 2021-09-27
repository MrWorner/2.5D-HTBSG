using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexRoadManager : MonoBehaviour
{
    private static MG_HexRoadManager _instance;
    [BoxGroup("Стоимость передвижения"), Required(InfoMessageType.Error), SerializeField] private float _roadMovementCost = 0.25f;
    public static float RoadMovementCost { get => _instance._roadMovementCost; }

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexRoadLibrary _roadTileLibrary;//[R] библиотека тайлов дорог
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _roadTileMap;//[R] объект стандартной TileMap (карта дорог) 
    #endregion Поля: необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexRoadManager Awake(): найден лишний MG_HexRoadManager!</color>");

        if (_roadTileLibrary == null) Debug.Log("<color=red>MG_HexRoadManager Awake(): 'roadTileLibrary' не задан!</color>");
        if (_map == null) Debug.Log("<color=red>MG_HexRoadManager Awake(): '_map' не задан!</color>");
        if (_roadTileMap == null) Debug.Log("<color=red>MG_HexRoadManager Awake(): 'roadTileMap' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Установить дорогу
    /// </summary>
    /// <param name="cell"></param>
    public void Place(MG_HexCell cell)
    {
        ConnectRoad(cell, true);
    }

    //public void UpdateRoadNeighbours(MG_HexCell _cell)
    //{

    //}

    /// <summary>
    /// Установить дорогу (круговую). ИСПОЛЬЗУЕТСЯ ЧТОБЫ ПОМЕТИТЬ КЛЕТКИ ЧТОБЫ ЗАТЕМ ПРИСОЕДИНИТЬ К ДРУГИМ ДОРОГАМ
    /// </summary>
    /// <param name="cell"></param>
    //public void PlaceCircle(MG_HexCell _cell)
    //{
    //    MG_HexRoad roadType = roadTileLibrary.GetRiver(RoadDirection.Circle);

    //    MG_HexRoadData roadData = _cell.RoadData;
    //    roadData.RiverType = roadType;

    //    DrawRoad(roadType, _cell);
    //}

    /// <summary>
    /// Удалить дорогу
    /// </summary>
    /// <param name="cell"></param>
    public void Remove(MG_HexCell cell)
    {
        MG_HexRoad roadType = MG_HexRoadLibrary.GetEmptyRoad();

        MG_HexRoadData roadData = cell.RoadData;
        roadData.RoadType = roadType;
        DrawRoad(roadType, cell, roadData);

        foreach (var cellN in cell.GetNeighbours())
        {
            MG_HexRoadData roadDataN = cellN.RoadData;
            MG_HexRoad roadTypeN = roadDataN.RoadType;
            if (!roadTypeN.Equals(MG_HexRoadLibrary.GetEmptyRoad()))
                ConnectRoad(cellN, false);
        }
    }

    /// <summary>
    /// Является ли пустой дорогой
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool IsEmptyRoad(MG_HexCell cell)
    {
        MG_HexRoad emptyRoadType = MG_HexRoadLibrary.GetEmptyRoad();
        MG_HexRoadData roadData = cell.RoadData;
        MG_HexRoad roadType = roadData.RoadType;
        return (emptyRoadType.Equals(roadType));
    }

    /// <summary>
    /// Очистить тайловую карту
    /// </summary>
    public void Clear()
    {
        _roadTileMap.ClearAllTiles();
    }
    #endregion Публичные методы

    #region Личные методы

    /// <summary>
    /// Присоединить дорогу на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="starter"></param>
    private void ConnectRoad(MG_HexCell cell, bool starter)
    {
        RoadDirection roadDirection = DefineRoadDirection(cell);
        MG_HexRoad roadType = _roadTileLibrary.GetRoad(roadDirection);

        //Debug.Log(roadDirection);
        //Debug.Log(roadType.Id);

        MG_HexRoadData roadData = cell.RoadData;
        roadData.RoadType = roadType;

        DrawRoad(roadType, cell, roadData);

        //Debug.Log("placed!: " + roadType.name + " | roadDirection: " + roadDirection + "| pos:" + _cell.Pos);
        if (starter)
        {
            foreach (var cellN in cell.GetNeighbours())
            {
                MG_HexRoadData roadDataN = cellN.RoadData;
                MG_HexRoad roadTypeN = roadDataN.RoadType;
                if (!roadTypeN.Equals(MG_HexRoadLibrary.GetEmptyRoad()))
                    ConnectRoad(cellN, false);
            }
        }
    }

    /// <summary>
    /// Нарисовать дорогу
    /// </summary>
    /// <param name="roadType"></param>
    /// <param name="cell"></param>
    /// <param name="roadData"></param>
    private void DrawRoad(MG_HexRoad roadType, MG_HexCell cell, MG_HexRoadData roadData)
    {
        var pos = cell.Pos;
        _roadTileMap.SetTile(pos, roadType.Tile);//устанавливаем тайл
        _roadTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла

        if (MG_VisibilityChecker.IsVisible(cell.CurrentVisibility) == false)
            roadData.SetVisibility(cell.CurrentVisibility);
    }

    /// <summary>
    /// Получить тип дороги
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <returns></returns>
    private RoadDirection DefineRoadDirection(MG_HexCell cell)
    {
        HashSet<DirectionHexPT> roadNeighbours = DefineRoadNeigbourds(cell);
        int count = roadNeighbours.Count;

        //Debug.Log(count);

        RoadDirection chosenDir = RoadDirection.None;
        switch (count)
        {
            case 0:
                chosenDir = RoadDirection.Circle;
                break;
            case 1:
                chosenDir = OneSide(roadNeighbours.First());
                break;
            case 2:
                chosenDir = TwoSides(roadNeighbours);
                break;
            case 3:
                chosenDir = ThreeSides(roadNeighbours);
                break;
            case 4:
                chosenDir = FourSides(roadNeighbours);
                break;
            case 5:
                chosenDir = FiveSides(roadNeighbours);
                break;
            case 6:
                chosenDir = RoadDirection.ABCDEF;
                break;
            default:
                Debug.Log("MG_HexRoadManager ERROR (DefineRoadType)!");
                break;
        }

        return chosenDir;
    }

    /// <summary>
    /// Определить стороны, где есть дороги
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private HashSet<DirectionHexPT> DefineRoadNeigbourds(MG_HexCell cell)
    {
        HashSet<DirectionHexPT> roadNeighbours = new HashSet<DirectionHexPT>();
        foreach (var item in cell.GetNeighboursWithDirection())
        {
            var directionN = item.Key;
            var cellN = item.Value;
            MG_HexRoad roadType = cellN.RoadData.RoadType;
            RoadDirection roadDir = roadType.Id;

            if (!roadDir.Equals(RoadDirection.None) || MG_SettlementManager.HasSettlement(cellN))//НЕ равняется бездорожному (нет дороги) типу клетки у соседа ИЛИ есть поселение
            {
                roadNeighbours.Add(directionN);
            }
        }

        return roadNeighbours;
    }

    /// <summary>
    /// Получить одну дорогу по одному направлению
    /// </summary>
    /// <param name="side">направление</param>
    /// <returns></returns>
    private RoadDirection OneSide(DirectionHexPT side)
    {
        switch (side)
        {
            case DirectionHexPT.NE:
                //A
                return RoadDirection.A;
            case DirectionHexPT.E:
                //B
                return RoadDirection.B;
            case DirectionHexPT.SE:
                //C
                return RoadDirection.C;
            case DirectionHexPT.SW:
                //D
                return RoadDirection.D;
            case DirectionHexPT.W:
                //E
                return RoadDirection.E;
            case DirectionHexPT.NW:
                //F
                return RoadDirection.F;
            default:
                Debug.Log("MG_HexRoadManager ERROR (OneSide)!");
                return RoadDirection.None;
        }
    }

    /// <summary>
    /// Получить две дороги по двум направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RoadDirection TwoSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RoadDirection> roads = new HashSet<RoadDirection>();
        foreach (var side in listOfSides)
        {
            RoadDirection border = OneSide(side);
            roads.Add(border);
        }

        RoadDirection correctRoad = RoadDirection.None;

        //----------A + (X)
        if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B))
            correctRoad = RoadDirection.AB;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C))
            correctRoad = RoadDirection.AC;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.AD;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.AE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.AF;

        //----------B + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C))
            correctRoad = RoadDirection.BC;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.BD;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.BE;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BF;

        //----------C + (X)
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.CD;
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.CE;
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.CF;

        //----------D + (X)
        else if (roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.DE;
        else if (roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.DF;

        //----------F + (X)
        else if (roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.EF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRoadManager ERROR (TwoSides)");

        return correctRoad;
    }

    /// <summary>
    /// Получить три дороги по трем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RoadDirection ThreeSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RoadDirection> roads = new HashSet<RoadDirection>();
        foreach (var side in listOfSides)
        {
            RoadDirection border = OneSide(side);
            roads.Add(border);
        }

        RoadDirection correctRoad = RoadDirection.None;

        //----------A + B + (X)
        if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C))
            correctRoad = RoadDirection.ABC;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.ABD;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ABE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ABF;

        //----------A + C + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.ACD;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ACE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ACF;

        //----------A + D + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ADE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ADF;

        //----------A + E + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.AEF;

        //----------B + C + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.BCD;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.BCE;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BCF;

        //----------B + D + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.BDE;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BDF;

        //----------B + E + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BEF;

        //----------C + D + (X)
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.CDE;
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.CDF;

        //----------C + E + (X)
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.CEF;

        //----------D + E + (X)
        else if (roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.DEF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRoadManager ERROR (ThreeSides)");

        return correctRoad;
    }

    /// <summary>
    /// Получить четыре дороги по четырем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RoadDirection FourSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RoadDirection> roads = new HashSet<RoadDirection>();
        foreach (var side in listOfSides)
        {
            RoadDirection border = OneSide(side);
            roads.Add(border);
        }

        RoadDirection correctRoad = RoadDirection.None;

        //----------A + B + C + (X)
        if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D))
            correctRoad = RoadDirection.ABCD;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ABCE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ABCF;

        //----------A + B + D + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ABDE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ABDF;

        //----------A + B + E + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ABEF;

        //----------A + C + D + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.ACDE;
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ACDF;

        //----------A + C + E + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ACEF;

        //----------A + D + E + (X)
        else if (roads.Contains(RoadDirection.A) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.ADEF;

        //----------B + C + D + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E))
            correctRoad = RoadDirection.BCDE;
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BCDF;

        //----------B + C + E + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BCEF;

        //----------B + D + E + (X)
        else if (roads.Contains(RoadDirection.B) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.BDEF;

        //----------C + D + E + (X)
        else if (roads.Contains(RoadDirection.C) && roads.Contains(RoadDirection.D) && roads.Contains(RoadDirection.E) && roads.Contains(RoadDirection.F))
            correctRoad = RoadDirection.CDEF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRoadManager ERROR (FourSides)");

        return correctRoad;
    }

    /// <summary>
    /// Получить пять дорог по пяти направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RoadDirection FiveSides(HashSet<DirectionHexPT> listOfSides)
    {
        RoadDirection correctRoad = RoadDirection.None;
        HashSet<RoadDirection> roads = new HashSet<RoadDirection>();

        foreach (var side in listOfSides)
        {
            RoadDirection border = OneSide(side);
            roads.Add(border);
        }

        if (//----------A + B + C + D + (X) variant 1
            roads.Contains(RoadDirection.A)
            && roads.Contains(RoadDirection.B)
            && roads.Contains(RoadDirection.C)
            && roads.Contains(RoadDirection.D)
            && roads.Contains(RoadDirection.E)
            )
            correctRoad = RoadDirection.ABCDE;

        else if
            (//----------A + B + C + D + (X) variant 2
            roads.Contains(RoadDirection.A)
            && roads.Contains(RoadDirection.B)
            && roads.Contains(RoadDirection.C)
            && roads.Contains(RoadDirection.D)
            && roads.Contains(RoadDirection.F)
            )
            correctRoad = RoadDirection.ABCDF;

        else if
            (//----------A + B + C + E +(X)
            roads.Contains(RoadDirection.A)
            && roads.Contains(RoadDirection.B)
            && roads.Contains(RoadDirection.C)
            && roads.Contains(RoadDirection.E)
            && roads.Contains(RoadDirection.F)
            )
            correctRoad = RoadDirection.ABCEF;

        else if
            (//----------A + B + D + E +(X)
            roads.Contains(RoadDirection.A)
            && roads.Contains(RoadDirection.B)
            && roads.Contains(RoadDirection.D)
            && roads.Contains(RoadDirection.E)
            && roads.Contains(RoadDirection.F)
            )
            correctRoad = RoadDirection.ABDEF;

        else if
            (//----------A + C + D + E +(X)
            roads.Contains(RoadDirection.A)
            && roads.Contains(RoadDirection.C)
            && roads.Contains(RoadDirection.D)
            && roads.Contains(RoadDirection.E)
            && roads.Contains(RoadDirection.F)
            )
            correctRoad = RoadDirection.ACDEF;

        else if
            (//----------B + C + D + E +(X)
            roads.Contains(RoadDirection.B)
            && roads.Contains(RoadDirection.C)
            && roads.Contains(RoadDirection.D)
            && roads.Contains(RoadDirection.E)
            && roads.Contains(RoadDirection.F)
            )
            correctRoad = RoadDirection.BCDEF;


        //----------ERROR!
        else
            Debug.Log("MG_HexRoadManager ERROR (FiveSides)");

        return correctRoad;
    }
    #endregion Личные методы

    #region Статический публичный класс GetRoadTileMap()
    /// <summary>
    /// Получить тайлмап дорог
    /// </summary>
    /// <returns></returns>
    public static Tilemap GetRoadTileMap()
    {
        return MG_HexRoadManager._instance._roadTileMap;
    }
    #endregion Статический публичный класс GetRoadTileMap()
}
}
