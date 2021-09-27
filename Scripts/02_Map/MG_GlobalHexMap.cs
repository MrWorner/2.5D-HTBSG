using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_GlobalHexMap : MG_BasicMap<MG_HexCell>, ICellControl<MG_HexCell>//, IClearEverything
{
    #region поля
    private static MG_GlobalHexMap instance;//синглетон
    [SerializeField] protected MapType mapType = MapType.Hexagonal2DPT;//тип карты
    #endregion поля

    #region Необходимые модули
    [Required(InfoMessageType.Error), SerializeField] protected Tilemap _tileMap_objects;//[R] объект TileMap LEVEL 2: Objects
    [Required(InfoMessageType.Error), SerializeField] private MG_CellNeighbourManager neighbourManager;//[R]Менеджер соседей
    #endregion Необходимые модули

    #region Свойства
    public static MG_GlobalHexMap Instance { get => instance; }
    public Tilemap TileMap_objects { get => _tileMap_objects; }//получить Тайлмап LEVEL 2: Objects
    #endregion Свойства

    #region Методы UNITY
    void Awake()
    {
        if (neighbourManager == null)
            Debug.Log("<color=red>MG_GlobalHexMap Awake(): 'neighbourManager' не прикреплен!</color>");
        if (_tileMap == null)
            Debug.Log("<color=red>MG_BasicMap Awake(): 'tileMap' не прикреплен!</color>");
        else
            _tileMap.ClearAllTiles();
        if (!Instance)
            instance = this;
        else
            Debug.Log("<color=red>MG_BasicMap Awake(): '_instance' найден дубликат!</color>");
    }
    #endregion

    #region Методы родительского класса (override абстрактных методов) 

    /// <summary>
    /// Очистить карту
    /// </summary>
    public override void Clear()
    {
        foreach (var cell in cells.Values.ToList())
            RemoveCell(cell);
    }
    #endregion Методы родительского класса (override абстрактных методов) 

    #region Публичный метод
    /// <summary>
    /// Получить клетку по Хекс координатам
    /// </summary>
    /// <param name="pos">хекс координаты</param>
    /// <returns></returns>
    public MG_HexCell GetCellByHexPos(Vector3Int pos)
    {
        Vector3Int convertedLocalPos = MG_HexCell.Cube_to_oddr(pos);
        return GetCell(convertedLocalPos);
    }

    /// <summary>
    /// Изменить цвет тайла клетки
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    public override void ChangeCellColor(Vector3Int pos, Color color)
    {
        _tileMap.SetColor(pos, color);
        _tileMap_objects.SetColor(pos, color);
    }
    #endregion Публичный метод

    #region Private методы
    /// <summary>
    /// Обновить информацию о размере карты
    /// </summary>
    private void UpdateMapSizeInfo()
    {
        int x_min = 0;
        int y_min = 0;
        int x_max = 0;
        int y_max = 0;
        int x_final = 0;
        int y_final = 0;

        if (cells.Count > 0)
        {
            foreach (var cell in cells)
            {
                if (cell.Key.x > x_max)
                    x_max = cell.Key.x;
                if (cell.Key.y > y_max)
                    y_max = cell.Key.y;
                {
                    if (cell.Key.x < x_min)
                        x_min = cell.Key.x;
                    if (cell.Key.y < y_min)
                        y_min = cell.Key.y;
                }
            }
            x_final = x_max - x_min + 1;
            y_final = y_max - y_min + 1;
        }
        else
        {
            x_final = 0;
            y_final = 0;
        }

        _size = new Vector2Int(x_final, y_final);
    }
    #endregion Private методы

    #region Методы интерфейса "ICellControl"
    /// <summary>
    /// Создать клетку (ICellControl)
    /// </summary>
    /// <param name="pos">позиция</param>
    /// <param name="cellType">тип</param>
    /// <param name="tile">тайл</param>
    public MG_HexCell CreateCell(Vector3Int pos, MG_HexCellType cellType, Tile tile, Tile tile_obj)
    {
        //Debug.Log("<color=orange>MG_GlobalHexMap CreateCell(): pos=" + pos + " _tile=" + _tile + " </orange>");

        TileMap.SetTile(pos, tile);//устанавливаем тайл
        TileMap_objects.SetTile(pos, tile_obj);//устанавливаем тайл обьект
        TileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла

        MG_HexCell cell;
        if (IsCellExist(pos))
        {//Клетка существует
            cell = cells[pos];//берем существующую клетку
        }
        else
        {
            bool isEven = (pos.y % 2) == 0;//Четный ли ряд.
            Vector3 worldPos = _tileMap.CellToWorld(pos);//переводим в Vector3
            cell = new MG_HexCell(cellType, tile, tile_obj, pos, worldPos, this, isEven);//создаем новую клетку
            cells.Add(pos, cell);//Добавляем в справочник
            //_cell.NodePro = nodeConstructor.CreatePointNode(worldPos);//Создаем нод
            neighbourManager.DefineNeighbours(cell, this);//знакомим со всеми близжайщими соседями
            UpdateMapSizeInfo();// Обновить информацию о размере карты
        }

        return cell;
    }

    /// <summary>
    /// Обновить клетку (ICellControl)
    /// </summary>
    /// <param name="cell">клетка</param>
    /// <param name="type">тип</param>
    /// <param name="tile">текущий используемый вариант тайла из всех доступных вариантов тайлов cellType (LEVEL 1: LAND)</param>
    /// <param name="tile_objects">текущий используемый вариант тайла из всех доступных вариантов тайлов cellType (LEVEL 2: OBJECTS)</param>
    /// <param name="tileUsedFrom"></param>
    public void UpdateCell(MG_HexCell cell, MG_HexCellType type, Tile tile, Tile tile_objects, TileOrigin tileUsedFrom)
    {
        var pos = cell.Pos;
        TileMap.SetTile(pos, tile);//устанавливаем тайл
        TileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла
        cell.SetTypeAndTiles(type, tile, tile_objects, tileUsedFrom);
        cell.SetVisibility(cell.CurrentVisibility);
    }

    /// <summary>
    /// Существует ли клетка (ICellControl)
    /// </summary>
    /// <param name="pos">позиция</param>
    /// <returns></returns>
    public bool IsCellExist(Vector3Int pos)
    {
        return cells.ContainsKey(pos);
    }

    /// <summary>
    /// Получить клетку (ICellControl)
    /// </summary>
    /// <param name="pos">позиция</param>
    /// <returns></returns>
    public MG_HexCell GetCell(Vector3Int pos)
    {
        MG_HexCell cell;
        cells.TryGetValue(pos, out cell);
        //if (!cells.TryGetValue(pos, out _cell))
        //{
        //    //Debug.Log("<color=red>MG_GlobalHexMap GetCell(): клетка не найдена!</color>");
        //}
        return cell;
    }

    /// <summary>
    /// Получить клетку (ICellControl)
    /// </summary>
    /// <param name="pos">позиция</param>
    /// <returns></returns>
    public MG_HexCell GetCell(Vector3 pos)
    {
        Vector3Int cellPos = _tileMap.WorldToCell(pos);
        ///Debug.Log("cellPos= " + cellPos + " pos" + pos);
        return GetCell(cellPos);
    }

    /// <summary>
    /// Получить случайную клетку по заданным параметрам
    /// </summary>
    /// <param name="ExceptionType">НЕ типа</param>
    /// <param name="ignoreSettlement">игнорировать поселения</param>
    /// <param name="ignoreUnit">игнорировать юнитов</param>
    /// <param name="ignoreResource">игнорировать ресурсы</param>
    /// <returns></returns>
    public MG_HexCell GetRandomCell(List<GroundType> ExceptionType, bool ignoreSettlement, bool ignoreUnit, bool ignoreResource)
    {
        var bookOfCell = cells.Where(pair => !ExceptionType.Contains(pair.Value.Type.GroundType)).ToDictionary(pair => pair.Key, pair => pair.Value);

        if (!ignoreSettlement)
            bookOfCell = bookOfCell.Where(pair => pair.Value.Settlement == null).ToDictionary(pair => pair.Key, pair => pair.Value);

        if (!ignoreUnit)
            bookOfCell = bookOfCell.Where(pair => pair.Value.Division == null).ToDictionary(pair => pair.Key, pair => pair.Value);

        if (!ignoreResource)
            bookOfCell = bookOfCell.Where(pair => pair.Value.Resource == null).ToDictionary(pair => pair.Key, pair => pair.Value);

        List<MG_HexCell> newList = bookOfCell.Values.ToList();
        //MG_HexCell randomCell = newList.ElementAt(new System.Random(DateTime.Now.Millisecond).Next(newList.Count()));

        if (newList.Count == 0)
            return null;

        if (newList.Count == 1)
            return newList[0];

        MG_HexCell randomCell = newList[UnityEngine.Random.Range(0, newList.Count)];
        return randomCell;
    }

    /// <summary>
    /// Удалить клетку (ICellControl)
    /// </summary>
    /// <param name="cell">клетка</param>
    public void RemoveCell(MG_HexCell cell)
    {
        if (cell == null)
        {
            Debug.Log("<color=red>MG_GlobalHexMap RemoveCell(): клетка не найдена!</color>");
            return;
        }

        cell.ClearAllNeighbours();//Очищаем информацию у соседях
        Vector3Int pos = cell.Pos;//позиция клетки
        _tileMap.SetTile(pos, null);
        _tileMap_objects.SetTile(pos, null);
        cells.Remove(pos);
        UpdateMapSizeInfo();// Обновить информацию о размере карты
    }

    /// <summary>
    /// Получить список всех клеток (ICellControl)
    /// </summary>
    public IReadOnlyList<MG_HexCell> GetCells()
    {
        return cells.Values.ToList();
    }
    #endregion Методы интерфейса "ICellControl"

    //#region Методы интерфейса "IClearEverything"
    //public void ClearEverything()
    //{
    //    Clear();//Удаляем все тайлы
    //}
    //#endregion Методы интерфейса "IClearEverything"
}
}