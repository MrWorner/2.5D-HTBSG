using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexRiverManager : MonoBehaviour
{
    private static MG_HexRiverManager _instance;
    [BoxGroup("Стоимость передвижения"), Required(InfoMessageType.Error), SerializeField] private float _riverMovementCost = 2f;
    public static float RiverMovementCost { get => _instance._riverMovementCost; }

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexRiverLibrary _riverTileLibrary;//[R] библиотека тайлов рек
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _riverTileMap;//[R] объект стандартной TileMap (карта рек)
    #endregion Поля: необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexRiverManager Awake(): найден лишний MG_HexRiverManager!</color>");

        if (_riverTileLibrary == null) Debug.Log("<color=red>MG_HexRiverManager Awake(): '_advancedRriverTileLibrary' не задан!</color>");
        if (_map == null) Debug.Log("<color=red>MG_HexRiverManager Awake(): '_map' не задан!</color>");
        if (_riverTileMap == null) Debug.Log("<color=red>MG_HexRiverManager Awake(): '_advancedRiverTileMap' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Создать реку на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    public void Place(MG_HexCell cell)
    {
        ConnectRiver(cell, true);      
    }

    //public void UpdateRiverNeighbours(MG_HexCell _cell)
    //{

    //}

    /// <summary>
    /// Установить реку (круговую). ИСПОЛЬЗУЕТСЯ ЧТОБЫ ПОМЕТИТЬ КЛЕТКИ ЧТОБЫ ЗАТЕМ ПРИСОЕДИНИТЬ К ДРУГИМ РЕКАМ
    /// </summary>
    /// <param name="cell"></param>
    //public void PlaceCircle(MG_HexCell _cell)
    //{
    //    MG_HexRiver riverType = riverTileLibrary.GetRiver(RiverDirection.Circle);

    //    MG_HexRiverData riverData = _cell.RiverData;
    //    riverData.RiverType = riverType;

    //    DrawRiver(riverType, _cell);
    //}

    /// <summary>
    /// Удалить реку
    /// </summary>
    /// <param name="cell"></param>
    public void Remove(MG_HexCell cell)
    {
        MG_HexRiver riverType = MG_HexRiverLibrary.GetEmptyRiver();

        MG_HexRiverData riverData = cell.RiverData;
        riverData.RiverType = riverType;
        DrawRiver(riverType, cell, riverData);

        foreach (var cellN in cell.GetNeighbours())
        {
            MG_HexRiverData riverDataN = cellN.RiverData;
            MG_HexRiver riverTypeN = riverDataN.RiverType;
            if (!riverTypeN.Equals(MG_HexRiverLibrary.GetEmptyRiver()))
                ConnectRiver(cellN, false);
        }
    }

    /// <summary>
    /// Является ли пустой рекой
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool IsEmptyRiver(MG_HexCell cell)
    {
        MG_HexRiver emptyRiverType = MG_HexRiverLibrary.GetEmptyRiver();
        MG_HexRiverData riverData = cell.RiverData;
        MG_HexRiver riverType = riverData.RiverType;
        return (emptyRiverType.Equals(riverType));
    }

    /// <summary>
    /// Очистить тайловую карту
    /// </summary>
    public void Clear()
    {
        _riverTileMap.ClearAllTiles();
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Присоединить дорогу на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="starter"></param>
    private void ConnectRiver(MG_HexCell cell, bool starter)
    {
        RiverDirection riverDirection = DefineRiverDirection(cell);
        MG_HexRiver riverType = _riverTileLibrary.GetRiver(riverDirection);

        MG_HexRiverData riverData = cell.RiverData;
        riverData.RiverType = riverType;

        DrawRiver(riverType, cell, riverData);

        //Debug.Log("placed!: " + riverType.name + " | riverDirection: " + riverDirection + "| pos:" + _cell.Pos);
        if (starter)
        {
            foreach (var cellN in cell.GetNeighbours())
            {
                MG_HexRiverData riverDataN = cellN.RiverData;
                MG_HexRiver riverTypeN = riverDataN.RiverType;
                if (!riverTypeN.Equals(MG_HexRiverLibrary.GetEmptyRiver()))
                    ConnectRiver(cellN, false);
            }
        }
    }

    /// <summary>
    /// Нарисовать реку
    /// </summary>
    /// <param name="riverType"></param>
    /// <param name="cell"></param>
    /// <param name="riverData"></param>
    private void DrawRiver(MG_HexRiver riverType, MG_HexCell cell, MG_HexRiverData riverData)
    {
        var pos = cell.Pos;
        _riverTileMap.SetTile(pos, riverType.Tile);//устанавливаем тайл
        _riverTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла

        if (MG_VisibilityChecker.IsVisible(cell.CurrentVisibility) == false)
            riverData.SetVisibility(cell.CurrentVisibility);
    }

    /// <summary>
    /// Получить тип реки
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <returns></returns>
    private RiverDirection DefineRiverDirection(MG_HexCell cell)
    {
        HashSet<DirectionHexPT> riverNeighbours = DefineRiverNeigbourds(cell);
        int count = riverNeighbours.Count;

        //Debug.Log(count);

        RiverDirection chosenDir = RiverDirection.None;
        switch (count)
        {
            case 0:
                chosenDir = RiverDirection.Circle;
                break;
            case 1:
                chosenDir = OneSide(riverNeighbours.First());
                break;
            case 2:
                chosenDir = TwoSides(riverNeighbours);
                break;
            case 3:
                chosenDir = ThreeSides(riverNeighbours);
                break;
            case 4:
                chosenDir = FourSides(riverNeighbours);
                break;
            case 5:
                chosenDir = FiveSides(riverNeighbours);
                break;
            case 6:
                chosenDir = RiverDirection.ABCDEF;
                break;
            default:
                Debug.Log("MG_HexRiverManager ERROR (DefineRiverType)!");
                break;
        }

        return chosenDir;
    }

    /// <summary>
    /// Определить стороны, где есть реки
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private HashSet<DirectionHexPT> DefineRiverNeigbourds(MG_HexCell cell)
    {
        HashSet<DirectionHexPT> riverNeighbours = new HashSet<DirectionHexPT>();
        foreach (var item in cell.GetNeighboursWithDirection())
        {
            var directionN = item.Key;
            var cellN = item.Value;
            MG_HexRiver riverType = cellN.RiverData.RiverType;
            RiverDirection riverDir = riverType.Id;

            if (!riverDir.Equals(RiverDirection.None))//НЕ равняется бездорожному (нет реки) типу клетки у соседа
            {
                riverNeighbours.Add(directionN);
            }
        }

        return riverNeighbours;
    }

    /// <summary>
    /// Получить одну реку по одному направлению
    /// </summary>
    /// <param name="side">направление</param>
    /// <returns></returns>
    private RiverDirection OneSide(DirectionHexPT side)
    {
        switch (side)
        {
            case DirectionHexPT.NE:
                //A
                return RiverDirection.A;
            case DirectionHexPT.E:
                //B
                return RiverDirection.B;
            case DirectionHexPT.SE:
                //C
                return RiverDirection.C;
            case DirectionHexPT.SW:
                //D
                return RiverDirection.D;
            case DirectionHexPT.W:
                //E
                return RiverDirection.E;
            case DirectionHexPT.NW:
                //F
                return RiverDirection.F;
            default:
                Debug.Log("MG_HexRiverManager ERROR (OneSide)!");
                return RiverDirection.None;
        }
    }

    /// <summary>
    /// Получить две реки по двум направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RiverDirection TwoSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RiverDirection> rivers = new HashSet<RiverDirection>();
        foreach (var side in listOfSides)
        {
            RiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        RiverDirection correctRiver = RiverDirection.None;

        //----------A + (X)
        if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B))
            correctRiver = RiverDirection.AB;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C))
            correctRiver = RiverDirection.AC;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.AD;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.AE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.AF;

        //----------B + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C))
            correctRiver = RiverDirection.BC;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.BD;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.BE;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BF;

        //----------C + (X)
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.CD;
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.CE;
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.CF;

        //----------D + (X)
        else if (rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.DE;
        else if (rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.DF;

        //----------F + (X)
        else if (rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.EF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRiverManager ERROR (TwoSides)");

        return correctRiver;
    }

    /// <summary>
    /// Получить три реки по трем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RiverDirection ThreeSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RiverDirection> rivers = new HashSet<RiverDirection>();
        foreach (var side in listOfSides)
        {
            RiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        RiverDirection correctRiver = RiverDirection.None;

        //----------A + B + (X)
        if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C))
            correctRiver = RiverDirection.ABC;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.ABD;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ABE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ABF;

        //----------A + C + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.ACD;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ACE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ACF;

        //----------A + D + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ADE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ADF;

        //----------A + E + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.AEF;

        //----------B + C + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.BCD;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.BCE;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BCF;

        //----------B + D + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.BDE;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BDF;

        //----------B + E + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BEF;

        //----------C + D + (X)
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.CDE;
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.CDF;

        //----------C + E + (X)
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.CEF;

        //----------D + E + (X)
        else if (rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.DEF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRiverManager ERROR (ThreeSides)");

        return correctRiver;
    }

    /// <summary>
    /// Получить четыре реки по четырем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RiverDirection FourSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<RiverDirection> rivers = new HashSet<RiverDirection>();
        foreach (var side in listOfSides)
        {
            RiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        RiverDirection correctRiver = RiverDirection.None;

        //----------A + B + C + (X)
        if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D))
            correctRiver = RiverDirection.ABCD;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ABCE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ABCF;

        //----------A + B + D + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ABDE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ABDF;

        //----------A + B + E + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ABEF;

        //----------A + C + D + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.ACDE;
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ACDF;

        //----------A + C + E + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ACEF;

        //----------A + D + E + (X)
        else if (rivers.Contains(RiverDirection.A) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.ADEF;

        //----------B + C + D + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E))
            correctRiver = RiverDirection.BCDE;
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BCDF;

        //----------B + C + E + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BCEF;

        //----------B + D + E + (X)
        else if (rivers.Contains(RiverDirection.B) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.BDEF;

        //----------C + D + E + (X)
        else if (rivers.Contains(RiverDirection.C) && rivers.Contains(RiverDirection.D) && rivers.Contains(RiverDirection.E) && rivers.Contains(RiverDirection.F))
            correctRiver = RiverDirection.CDEF;

        //----------ERROR!
        else
            Debug.Log("MG_HexRiverManager ERROR (FourSides)");

        return correctRiver;
    }

    /// <summary>
    /// Получить пять рек по пяти направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private RiverDirection FiveSides(HashSet<DirectionHexPT> listOfSides)
    {
        RiverDirection correctRiver = RiverDirection.None;
        HashSet<RiverDirection> rivers = new HashSet<RiverDirection>();

        foreach (var side in listOfSides)
        {
            RiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        if (//----------A + B + C + D + (X) variant 1
            rivers.Contains(RiverDirection.A)
            && rivers.Contains(RiverDirection.B)
            && rivers.Contains(RiverDirection.C)
            && rivers.Contains(RiverDirection.D)
            && rivers.Contains(RiverDirection.E)
            )
            correctRiver = RiverDirection.ABCDE;

        else if
            (//----------A + B + C + D + (X) variant 2
            rivers.Contains(RiverDirection.A)
            && rivers.Contains(RiverDirection.B)
            && rivers.Contains(RiverDirection.C)
            && rivers.Contains(RiverDirection.D)
            && rivers.Contains(RiverDirection.F)
            )
            correctRiver = RiverDirection.ABCDF;

        else if
            (//----------A + B + C + E +(X)
            rivers.Contains(RiverDirection.A)
            && rivers.Contains(RiverDirection.B)
            && rivers.Contains(RiverDirection.C)
            && rivers.Contains(RiverDirection.E)
            && rivers.Contains(RiverDirection.F)
            )
            correctRiver = RiverDirection.ABCEF;

        else if
            (//----------A + B + D + E +(X)
            rivers.Contains(RiverDirection.A)
            && rivers.Contains(RiverDirection.B)
            && rivers.Contains(RiverDirection.D)
            && rivers.Contains(RiverDirection.E)
            && rivers.Contains(RiverDirection.F)
            )
            correctRiver = RiverDirection.ABDEF;

        else if
            (//----------A + C + D + E +(X)
            rivers.Contains(RiverDirection.A)
            && rivers.Contains(RiverDirection.C)
            && rivers.Contains(RiverDirection.D)
            && rivers.Contains(RiverDirection.E)
            && rivers.Contains(RiverDirection.F)
            )
            correctRiver = RiverDirection.ACDEF;

        else if
            (//----------B + C + D + E +(X)
            rivers.Contains(RiverDirection.B)
            && rivers.Contains(RiverDirection.C)
            && rivers.Contains(RiverDirection.D)
            && rivers.Contains(RiverDirection.E)
            && rivers.Contains(RiverDirection.F)
            )
            correctRiver = RiverDirection.BCDEF;


        //----------ERROR!
        else
            Debug.Log("MG_HexRiverManager ERROR (FiveSides)");

        return correctRiver;
    }
    #endregion Личные методы

    #region Статический публичный класс GetRiverTileMap()
    /// <summary>
    /// Получить тайлмап рек
    /// </summary>
    /// <returns></returns>
    public static Tilemap GetRiverTileMap()
    {
        return MG_HexRiverManager._instance._riverTileMap;
    }
    #endregion Статический публичный класс GetRiverTileMap()
}
}
