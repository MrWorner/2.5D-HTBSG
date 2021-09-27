using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexAdvancedRiverManager : MonoBehaviour
{
    private static MG_HexAdvancedRiverManager _instance;
    [BoxGroup("Стоимость передвижения"), Required(InfoMessageType.Error), SerializeField] private float _riverMovementCost = 1f;
    public static float RiverMovementCost { get => _instance._riverMovementCost; }

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexAdvancedRiverLibrary _advancedRriverTileLibrary;//[R] библиотека тайлов рек
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _advancedRiverTileMap;//[R] объект стандартной TileMap (карта рек)
    #endregion Поля: необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexAdvancedRiverManager Awake(): найден лишний MG_HexAdvancedRiverManager!</color>");

        if (_advancedRriverTileLibrary == null) Debug.Log("<color=red>MG_HexAdvancedRiverManager Awake(): '_advancedRriverTileLibrary' не задан!</color>");
        if (_map == null) Debug.Log("<color=red>MG_HexAdvancedRiverManager Awake(): '_map' не задан!</color>");
        if (_advancedRiverTileMap == null) Debug.Log("<color=red>MG_HexAdvancedRiverManager Awake(): '_advancedRiverTileMap' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Создать реку на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dir"></param>
    public void Place(MG_HexCell cell, DirectionHexPT dir)
    {
        ConnectRiver(cell, dir, true);
    }

    /// <summary>
    /// Удалить реку
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dir"></param>
    public void Remove(MG_HexCell cell, DirectionHexPT dir)
    {
        RemoveRiver(cell, dir, true);
    }

    /// <summary>
    /// Является ли пустой рекой
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    //public static bool IsEmptyRiver(MG_HexCell cell)
    //{
    //    MG_HexAdvancedRiver emptyRiverType = MG_HexAdvancedRiverLibrary.GetEmptyRiver();
    //    MG_HexAdvancedRiverData riverData = cell.AdvancedRiverData;
    //    MG_HexAdvancedRiver riverType = riverData.RiverType;
    //    return (emptyRiverType.Equals(riverType));
    //}

    /// <summary>
    /// Очистить тайловую карту
    /// </summary>
    public void Clear()
    {
        _advancedRiverTileMap.ClearAllTiles();
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Присоединить реку на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dir"></param>
    /// <param name="starter"></param>
    private void ConnectRiver(MG_HexCell cell, DirectionHexPT dir, bool starter)
    {
        cell.AdvancedRiverData.SetRiverByDirection(dir);
        AdvancedRiverDirection AdvancedRiverDirection = DefineRiverDirection(cell);

        MG_HexAdvancedRiver riverType = _advancedRriverTileLibrary.GetRiver(AdvancedRiverDirection);

        MG_HexAdvancedRiverData riverData = cell.AdvancedRiverData;
        riverData.RiverType = riverType;

        DrawRiver(riverType, cell, riverData);

        //Debug.Log("placed!: " + riverType.name + " | AdvancedRiverDirection: " + AdvancedRiverDirection + "| pos:" + cell.Pos);
        if (starter)
        {
            //---СОСЕДНЯЯ КЛЕТКА (ЗЕРКАЛЬНАЯ)
            MG_HexCell cellN = cell.GetNeighbourByDirection(dir);
            if (cellN != null)
            {
                DirectionHexPT dirN = MG_Direction.ReverseDir(dir);
                ConnectRiver(cellN, dirN, false);
            }
        }
    }

    /// <summary>
    /// удалить реку на указанной клетке
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="dir"></param>
    /// <param name="starter"></param>
    private void RemoveRiver(MG_HexCell cell, DirectionHexPT dir, bool starter)
    {
        cell.AdvancedRiverData.RemoveRiverByDirection(dir);
        AdvancedRiverDirection AdvancedRiverDirection = DefineRiverDirection(cell);

        MG_HexAdvancedRiver riverType = _advancedRriverTileLibrary.GetRiver(AdvancedRiverDirection);

        MG_HexAdvancedRiverData riverData = cell.AdvancedRiverData;
        riverData.RiverType = riverType;

        DrawRiver(riverType, cell, riverData);

        //Debug.Log("placed!: " + riverType.name + " | AdvancedRiverDirection: " + AdvancedRiverDirection + "| pos:" + cell.Pos);
        if (starter)
        {
            //---СОСЕДНЯЯ КЛЕТКА (ЗЕРКАЛЬНАЯ)
            MG_HexCell cellN = cell.GetNeighbourByDirection(dir);
            if (cellN != null)
            {
                DirectionHexPT dirN = MG_Direction.ReverseDir(dir);
                RemoveRiver(cellN, dirN, false);//Зеркальная клетка
            }
        }
    }

    /// <summary>
    /// Нарисовать реку
    /// </summary>
    /// <param name="riverType"></param>
    /// <param name="cell"></param>
    /// <param name="riverData"></param>
    private void DrawRiver(MG_HexAdvancedRiver riverType, MG_HexCell cell, MG_HexAdvancedRiverData riverData)
    {
        var pos = cell.Pos;
        _advancedRiverTileMap.SetTile(pos, riverType.Tile);//устанавливаем тайл
        _advancedRiverTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла

        if (MG_VisibilityChecker.IsVisible(cell.CurrentVisibility) == false)
            riverData.SetVisibility(cell.CurrentVisibility);
    }

    /// <summary>
    /// Получить тип реки
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <returns></returns>
    private AdvancedRiverDirection DefineRiverDirection(MG_HexCell cell)
    {
        HashSet<DirectionHexPT> riverNeighbours = DefineRiverNeigbourds(cell);
        int count = riverNeighbours.Count;
        AdvancedRiverDirection chosenDir = AdvancedRiverDirection.None;
        switch (count)
        {
            case 0:
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
                chosenDir = AdvancedRiverDirection.ABCDEF;
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
        MG_HexAdvancedRiverData riverData = cell.AdvancedRiverData;
        if (riverData.River_E) riverNeighbours.Add(DirectionHexPT.E);
        if (riverData.River_NE) riverNeighbours.Add(DirectionHexPT.NE);
        if (riverData.River_NW) riverNeighbours.Add(DirectionHexPT.NW);
        if (riverData.River_SE) riverNeighbours.Add(DirectionHexPT.SE);
        if (riverData.River_SW) riverNeighbours.Add(DirectionHexPT.SW);
        if (riverData.River_W) riverNeighbours.Add(DirectionHexPT.W);
        return riverNeighbours;
    }

    /// <summary>
    /// Получить одну реку по одному направлению
    /// </summary>
    /// <param name="side">направление</param>
    /// <returns></returns>
    private AdvancedRiverDirection OneSide(DirectionHexPT side)
    {
        switch (side)
        {
            case DirectionHexPT.NE:
                //A
                return AdvancedRiverDirection.A;
            case DirectionHexPT.E:
                //B
                return AdvancedRiverDirection.B;
            case DirectionHexPT.SE:
                //C
                return AdvancedRiverDirection.C;
            case DirectionHexPT.SW:
                //D
                return AdvancedRiverDirection.D;
            case DirectionHexPT.W:
                //E
                return AdvancedRiverDirection.E;
            case DirectionHexPT.NW:
                //F
                return AdvancedRiverDirection.F;
            default:
                Debug.Log("MG_HexRiverManager ERROR (OneSide)!");
                return AdvancedRiverDirection.None;
        }
    }

    /// <summary>
    /// Получить две реки по двум направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private AdvancedRiverDirection TwoSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<AdvancedRiverDirection> rivers = new HashSet<AdvancedRiverDirection>();
        foreach (var side in listOfSides)
        {
            AdvancedRiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        AdvancedRiverDirection correctRiver = AdvancedRiverDirection.None;

        //----------A + (X)
        if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B))
            correctRiver = AdvancedRiverDirection.AB;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C))
            correctRiver = AdvancedRiverDirection.AC;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.AD;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.AE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.AF;

        //----------B + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C))
            correctRiver = AdvancedRiverDirection.BC;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.BD;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.BE;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BF;

        //----------C + (X)
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.CD;
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.CE;
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.CF;

        //----------D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.DE;
        else if (rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.DF;

        //----------F + (X)
        else if (rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.EF;

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
    private AdvancedRiverDirection ThreeSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<AdvancedRiverDirection> rivers = new HashSet<AdvancedRiverDirection>();
        foreach (var side in listOfSides)
        {
            AdvancedRiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        AdvancedRiverDirection correctRiver = AdvancedRiverDirection.None;

        //----------A + B + (X)
        if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C))
            correctRiver = AdvancedRiverDirection.ABC;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.ABD;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ABE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ABF;

        //----------A + C + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.ACD;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ACE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ACF;

        //----------A + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ADE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ADF;

        //----------A + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.AEF;

        //----------B + C + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.BCD;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.BCE;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BCF;

        //----------B + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.BDE;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BDF;

        //----------B + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BEF;

        //----------C + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.CDE;
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.CDF;

        //----------C + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.CEF;

        //----------D + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.DEF;

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
    private AdvancedRiverDirection FourSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<AdvancedRiverDirection> rivers = new HashSet<AdvancedRiverDirection>();
        foreach (var side in listOfSides)
        {
            AdvancedRiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        AdvancedRiverDirection correctRiver = AdvancedRiverDirection.None;

        //----------A + B + C + (X)
        if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D))
            correctRiver = AdvancedRiverDirection.ABCD;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ABCE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ABCF;

        //----------A + B + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ABDE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ABDF;

        //----------A + B + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ABEF;

        //----------A + C + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.ACDE;
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ACDF;

        //----------A + C + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ACEF;

        //----------A + D + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.A) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.ADEF;

        //----------B + C + D + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E))
            correctRiver = AdvancedRiverDirection.BCDE;
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BCDF;

        //----------B + C + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BCEF;

        //----------B + D + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.B) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.BDEF;

        //----------C + D + E + (X)
        else if (rivers.Contains(AdvancedRiverDirection.C) && rivers.Contains(AdvancedRiverDirection.D) && rivers.Contains(AdvancedRiverDirection.E) && rivers.Contains(AdvancedRiverDirection.F))
            correctRiver = AdvancedRiverDirection.CDEF;

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
    private AdvancedRiverDirection FiveSides(HashSet<DirectionHexPT> listOfSides)
    {
        AdvancedRiverDirection correctRiver = AdvancedRiverDirection.None;
        HashSet<AdvancedRiverDirection> rivers = new HashSet<AdvancedRiverDirection>();

        foreach (var side in listOfSides)
        {
            AdvancedRiverDirection border = OneSide(side);
            rivers.Add(border);
        }

        if (//----------A + B + C + D + (X) variant 1
            rivers.Contains(AdvancedRiverDirection.A)
            && rivers.Contains(AdvancedRiverDirection.B)
            && rivers.Contains(AdvancedRiverDirection.C)
            && rivers.Contains(AdvancedRiverDirection.D)
            && rivers.Contains(AdvancedRiverDirection.E)
            )
            correctRiver = AdvancedRiverDirection.ABCDE;

        else if
            (//----------A + B + C + D + (X) variant 2
            rivers.Contains(AdvancedRiverDirection.A)
            && rivers.Contains(AdvancedRiverDirection.B)
            && rivers.Contains(AdvancedRiverDirection.C)
            && rivers.Contains(AdvancedRiverDirection.D)
            && rivers.Contains(AdvancedRiverDirection.F)
            )
            correctRiver = AdvancedRiverDirection.ABCDF;

        else if
            (//----------A + B + C + E +(X)
            rivers.Contains(AdvancedRiverDirection.A)
            && rivers.Contains(AdvancedRiverDirection.B)
            && rivers.Contains(AdvancedRiverDirection.C)
            && rivers.Contains(AdvancedRiverDirection.E)
            && rivers.Contains(AdvancedRiverDirection.F)
            )
            correctRiver = AdvancedRiverDirection.ABCEF;

        else if
            (//----------A + B + D + E +(X)
            rivers.Contains(AdvancedRiverDirection.A)
            && rivers.Contains(AdvancedRiverDirection.B)
            && rivers.Contains(AdvancedRiverDirection.D)
            && rivers.Contains(AdvancedRiverDirection.E)
            && rivers.Contains(AdvancedRiverDirection.F)
            )
            correctRiver = AdvancedRiverDirection.ABDEF;

        else if
            (//----------A + C + D + E +(X)
            rivers.Contains(AdvancedRiverDirection.A)
            && rivers.Contains(AdvancedRiverDirection.C)
            && rivers.Contains(AdvancedRiverDirection.D)
            && rivers.Contains(AdvancedRiverDirection.E)
            && rivers.Contains(AdvancedRiverDirection.F)
            )
            correctRiver = AdvancedRiverDirection.ACDEF;

        else if
            (//----------B + C + D + E +(X)
            rivers.Contains(AdvancedRiverDirection.B)
            && rivers.Contains(AdvancedRiverDirection.C)
            && rivers.Contains(AdvancedRiverDirection.D)
            && rivers.Contains(AdvancedRiverDirection.E)
            && rivers.Contains(AdvancedRiverDirection.F)
            )
            correctRiver = AdvancedRiverDirection.BCDEF;


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
        return MG_HexAdvancedRiverManager._instance._advancedRiverTileMap;
    }
    #endregion Статический публичный класс GetRiverTileMap()
}
}
