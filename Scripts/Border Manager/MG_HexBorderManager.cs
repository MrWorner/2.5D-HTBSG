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
public class MG_HexBorderManager : MonoBehaviour
{
    private static MG_HexBorderManager _instance;
    public static MG_HexBorderManager Instance { get => _instance; }

    #region Поля: требуемые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexBorderLibrary _borderTileLibrary;//[R] библиотека тайлов границ
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _borderTileMap;//[R] объект стандартной TileMap (карта границ)
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_LineOfSight _lineOfSight;//[R] зона видимости
    #endregion Поля: требуемые модули

    #region Свойства
    public Tilemap BorderTileMap { get => _borderTileMap; }
    #endregion

    #region Метод UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexBorderManager Awake(): найден лишний MG_HexBorderManager!</color>");

        if (_borderTileLibrary == null) Debug.Log("<color=red>MG_HexBorderManager Awake(): 'borderTileLibrary' не задан!</color>");
        if (_map == null) Debug.Log("<color=red>MG_HexBorderManager Awake(): '_map' не задан!</color>");
        if (_borderTileMap == null) Debug.Log("<color=red>MG_HexBorderManager Awake(): 'borderTileMap' не задан!</color>");
        if (_lineOfSight == null) Debug.Log("<color=red>MG_HexBorderManager Awake(): '_lineOfSight' не задан!</color>");
    }
    #endregion Метод UNITY

    #region Публичные методы
    /// <summary>
    /// Установить контроль над клеткой
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="side"></param>
    /// <param name="starter"></param>
    public void Place(MG_HexCell cell, ICellObserver observer, bool starter)
    {
        //Debug.Log("Show observer= " + observer.GetType() + " cell.pos= " + cell.Pos);
        MG_Player side = observer.GetSide();
        MG_HexBorderData borderData = cell.BorderData;
        borderData.SetOwner(observer);//задаем нового хозяина

        //if (!side.EqualsUID(borderData.Owner))
        //    borderData.Owner = side;//задаем нового хозяина

        MG_HexBorder border;
        if (side.Equals(MG_PlayerManager.GetEmptySide()))//если игровая сторона = пустая, то значит будем брать пустые границы
            border = MG_HexBorderLibrary.GetEmptyBorder();
        else
        {
            BorderDirection borderType = DefineBorderType(cell, side);//определить тип границ (с каких сторон начертить на клетке)
            border = _borderTileLibrary.GetBorder(borderType);//определить скриптовый объект границ по типу границы
        }

        borderData.BorderType = border;//задаем скриптовый объект границ
        DrawBorders(borderData, border, cell, side.Color);//начертить границы

        //Debug.Log("placed!: " + borderType + "| pos:" + _cell.Pos);

        //--Рекурсия, необходимо обновить всех соседей!
        if (starter)
        {
            foreach (var cellN in cell.GetNeighbours())
            {
                var regionOwner = cellN.BorderData.RegionOwner;
                Place(cellN, regionOwner, false);//Пересоздать 
            }
        }

        _lineOfSight.UpdateStaticFOV(new HashSet<MG_HexCell>() { cell }, side);
    }

    /// <summary>
    /// Очистить территорию от хозяина
    /// </summary>
    /// <param name="cell"></param>
    public void Erase(MG_HexCell cell)
    {
        //--BEGIN Логика очищения клетки от хозяина территории

        MG_HexBorderData borderData = cell.BorderData;
        ICellObserver regionOwner = borderData.RegionOwner;
        if (regionOwner is MG_Player)
        {
            MG_Player observer = regionOwner as MG_Player;
            _lineOfSight.RemoveStaticFOV(cell, observer);
        }
        else if (regionOwner is MG_Settlement)
        {
            MG_Settlement observer = regionOwner as MG_Settlement;
            MG_HexCell cellBase = observer.Cell;
            MG_Player side = observer.Side;

            if (cellBase.EqualsUID(cell))
            {
                _lineOfSight.RemoveVisionFromObserver(cell, observer);
            }
            _lineOfSight.RemoveStaticFOV(cell, side);
        }
   
        //--END Логика очищения клетки от хозяина территории

        //--BEGIN Визуальные границы
        MG_Player emptySide = MG_PlayerManager.GetEmptySide();
        Place(cell, emptySide, true);
        //--END Визуальные границы
        borderData.OnErase();// Очистить от владельца
    }

    /// <summary>
    /// Удаляем все тайлы
    /// </summary>
    public void Clear()
    {
        _borderTileMap.ClearAllTiles();
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Начертить границы
    /// </summary>
    /// <param name="borderData"></param>
    /// <param name="borderType"></param>
    /// <param name="cell"></param>
    /// <param name="color"></param>
    private void DrawBorders(MG_HexBorderData borderData, MG_HexBorder borderType, MG_HexCell cell, Color color)
    {
        var pos = cell.Pos;
        _borderTileMap.SetTile(pos, borderType.Tile);//устанавливаем тайл
        _borderTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла
        borderData.SetVisibility(cell.CurrentVisibility);
        //if (MG_VisibilityChecker.IsBlackFog(_cell.CurrentVisibility))
        //    _borderTileMap.UpdateColor(pos, Color.clear);
        //else
        //    _borderTileMap.UpdateColor(pos, color);
    }

    /// <summary>
    /// Получить тип границ
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <param name="side">игровая сторона</param>
    /// <returns></returns>
    private BorderDirection DefineBorderType(MG_HexCell cell, MG_Player side)
    {
        HashSet<DirectionHexPT> listOfEmptySides = DefineEmptyCellSides(cell, side);// Определить пустые стороны (с какой стороны нужна граница)
        int count = listOfEmptySides.Count;

        BorderDirection chosenBorder = BorderDirection.None;
        switch (count)
        {
            case 0:
                chosenBorder = BorderDirection.None;
                break;
            case 1:
                chosenBorder = OneSide(listOfEmptySides.First());
                break;
            case 2:
                chosenBorder = TwoSides(listOfEmptySides);
                break;
            case 3:
                chosenBorder = ThreeSides(listOfEmptySides);
                break;
            case 4:
                chosenBorder = FourSides(listOfEmptySides);
                break;
            case 5:
                chosenBorder = FiveSides(listOfEmptySides);
                break;
            case 6:
                chosenBorder = BorderDirection.ABCDEF;
                break;
            default:
                Debug.Log("ERROR MG_HexBorderManager (DefineBorderType)!");
                break;
        }

        return chosenBorder;
    }

    /// <summary>
    /// Определить пустые стороны (с какой стороны нужна граница)
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    private HashSet<DirectionHexPT> DefineEmptyCellSides(MG_HexCell cell, MG_Player side)
    {
        HashSet<DirectionHexPT> listOfEmptySides = new HashSet<DirectionHexPT>();
        foreach (var item in cell.GetNeighboursWithDirection())
        {
            var directionN = item.Key;
            var cellN = item.Value;
            var sideN = cellN.BorderData.Owner;

            if (!side.Equals(sideN))
            {
                listOfEmptySides.Add(directionN);
            }
        }

        var allEnums = Enum.GetValues(typeof(DirectionHexPT)).Cast<DirectionHexPT>().ToList();
        int enumMemberCount = allEnums.Count;
        if (cell.GetNeighboursWithDirection().Count < enumMemberCount)
        {
            //если кол-во соседей меньше чем существующих направлений, значит нужно также взять те направления, которые направлены за границы карты.
            //без этого не будут начерчены границы у пределов карты, так что исправляем это.
            foreach (var item in cell.GetNeighboursWithDirection())
            {
                var directionN = item.Key;
                if (allEnums.Contains(directionN))
                {
                    allEnums.Remove(directionN);
                }
            }

            if (allEnums.Count > 0)
            {
                listOfEmptySides.AddRange(allEnums);
            }
        }

        return listOfEmptySides;
    }
    #endregion Личные методы

    #region Private методы: ЛОГИКА ПОЛУЧЕНИЯ ВЕРНОГО 'BorderDirection'
    /// <summary>
    /// Получить одну границу по одному направлению
    /// </summary>
    /// <param name="side">направление</param>
    /// <returns></returns>
    private BorderDirection OneSide(DirectionHexPT side)
    {
        switch (side)
        {
            case DirectionHexPT.NE:
                //A
                return BorderDirection.A;
            case DirectionHexPT.E:
                //B
                return BorderDirection.B;
            case DirectionHexPT.SE:
                //C
                return BorderDirection.C;
            case DirectionHexPT.SW:
                //D
                return BorderDirection.D;
            case DirectionHexPT.W:
                //E
                return BorderDirection.E;
            case DirectionHexPT.NW:
                //F
                return BorderDirection.F;
            default:
                Debug.Log("ERROR MG_HexBorderManager (OneSide)!");
                return BorderDirection.None;
        }
    }

    /// <summary>
    /// Получить две границы по двум направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private BorderDirection TwoSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<BorderDirection> borders = new HashSet<BorderDirection>();
        foreach (var side in listOfSides)
        {
            BorderDirection border = OneSide(side);
            borders.Add(border);
        }

        BorderDirection correctBorder = BorderDirection.None;


        //----------A + (X)
        if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B))
            correctBorder = BorderDirection.AB;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C))
            correctBorder = BorderDirection.AC;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.AD;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.AE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.AF;

        //----------B + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C))
            correctBorder = BorderDirection.BC;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.BD;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.BE;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BF;

        //----------C + (X)
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.CD;
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.CE;
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.CF;

        //----------D + (X)
        else if (borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.DE;
        else if (borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.DF;

        //----------F + (X)
        else if (borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.EF;

        //----------ERROR!
        else
            Debug.Log("ERROR MG_HexBorderManager (TwoSides)");

        return correctBorder;
    }

    /// <summary>
    /// Получить три границы по трем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private BorderDirection ThreeSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<BorderDirection> borders = new HashSet<BorderDirection>();
        foreach (var side in listOfSides)
        {
            BorderDirection border = OneSide(side);
            borders.Add(border);
        }

        BorderDirection correctBorder = BorderDirection.None;

        //----------A + B + (X)
        if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C))
            correctBorder = BorderDirection.ABC;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.ABD;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ABE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ABF;

        //----------A + C + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.ACD;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ACE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ACF;

        //----------A + D + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ADE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ADF;

        //----------A + E + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.AEF;

        //----------B + C + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.BCD;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.BCE;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BCF;

        //----------B + D + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.BDE;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BDF;

        //----------B + E + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BEF;

        //----------C + D + (X)
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.CDE;
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.CDF;

        //----------C + E + (X)
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.CEF;

        //----------D + E + (X)
        else if (borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.DEF;

        //----------ERROR!
        else
            Debug.Log("ERROR MG_HexBorderManager (ThreeSides)");

        return correctBorder;
    }

    /// <summary>
    /// Получить четыре границы по четырем направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private BorderDirection FourSides(HashSet<DirectionHexPT> listOfSides)
    {
        HashSet<BorderDirection> borders = new HashSet<BorderDirection>();
        foreach (var side in listOfSides)
        {
            BorderDirection border = OneSide(side);
            borders.Add(border);
        }

        BorderDirection correctBorder = BorderDirection.None;

        //----------A + B + C + (X)
        if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D))
            correctBorder = BorderDirection.ABCD;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ABCE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ABCF;

        //----------A + B + D + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ABDE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ABDF;

        //----------A + B + E + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ABEF;

        //----------A + C + D + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.ACDE;
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ACDF;

        //----------A + C + E + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ACEF;

        //----------A + D + E + (X)
        else if (borders.Contains(BorderDirection.A) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.ADEF;

        //----------B + C + D + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E))
            correctBorder = BorderDirection.BCDE;
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BCDF;

        //----------B + C + E + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BCEF;

        //----------B + D + E + (X)
        else if (borders.Contains(BorderDirection.B) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.BDEF;

        //----------C + D + E + (X)
        else if (borders.Contains(BorderDirection.C) && borders.Contains(BorderDirection.D) && borders.Contains(BorderDirection.E) && borders.Contains(BorderDirection.F))
            correctBorder = BorderDirection.CDEF;

        //----------ERROR!
        else
            Debug.Log("ERROR MG_HexBorderManager (FourSides)");

        return correctBorder;
    }

    /// <summary>
    /// Получить пять границы по пяти направлениям
    /// </summary>
    /// <param name="listOfSides"></param>
    /// <returns></returns>
    private BorderDirection FiveSides(HashSet<DirectionHexPT> listOfSides)
    {
        BorderDirection correctBorder = BorderDirection.None;
        HashSet<BorderDirection> borders = new HashSet<BorderDirection>();

        foreach (var side in listOfSides)
        {
            BorderDirection border = OneSide(side);
            borders.Add(border);
        }

        if (
            borders.Contains(BorderDirection.A)
            && borders.Contains(BorderDirection.B)
            && borders.Contains(BorderDirection.C)
            && borders.Contains(BorderDirection.D)
            && borders.Contains(BorderDirection.E)
            )
            correctBorder = BorderDirection.ABCDE;

        else if
            (
            borders.Contains(BorderDirection.A)
            && borders.Contains(BorderDirection.B)
            && borders.Contains(BorderDirection.C)
            && borders.Contains(BorderDirection.D)
            && borders.Contains(BorderDirection.F)
            )
            correctBorder = BorderDirection.ABCDF;

        else if
            (
            borders.Contains(BorderDirection.A)
            && borders.Contains(BorderDirection.B)
            && borders.Contains(BorderDirection.C)
            && borders.Contains(BorderDirection.E)
            && borders.Contains(BorderDirection.F)
            )
            correctBorder = BorderDirection.ABCEF;

        else if
            (
            borders.Contains(BorderDirection.A)
            && borders.Contains(BorderDirection.B)
            && borders.Contains(BorderDirection.D)
            && borders.Contains(BorderDirection.E)
            && borders.Contains(BorderDirection.F)
            )
            correctBorder = BorderDirection.ABDEF;

        else if
            (
            borders.Contains(BorderDirection.A)
            && borders.Contains(BorderDirection.C)
            && borders.Contains(BorderDirection.D)
            && borders.Contains(BorderDirection.E)
            && borders.Contains(BorderDirection.F)
            )
            correctBorder = BorderDirection.ACDEF;

        else if
            (
            borders.Contains(BorderDirection.B)
            && borders.Contains(BorderDirection.C)
            && borders.Contains(BorderDirection.D)
            && borders.Contains(BorderDirection.E)
            && borders.Contains(BorderDirection.F)
            )
            correctBorder = BorderDirection.BCDEF;

        //----------ERROR!
        else
            Debug.Log("ERROR MG_HexBorderManager (FiveSides)");

        return correctBorder;
    }
    #endregion Private методы: ЛОГИКА ПОЛУЧЕНИЯ ВЕРНОГО 'BorderDirection'
}
}
