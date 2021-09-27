/////////////////////////////////////////////////////////////////////////////////
//
//	JSON_MapContainer.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание: Генерация JSON сохраняемых данных.				
//			   
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[Serializable]
public class JSON_MapContainer
{
    public Vector2Int mapSize;
    public List<JSON_HexCell> cells = new List<JSON_HexCell>();
    public List<JSON_HexRoadData> roads = new List<JSON_HexRoadData>();
    public List<JSON_HexRiverData> rivers = new List<JSON_HexRiverData>();
    public List<JSON_HexAdvancedRiverData> advancedRivers = new List<JSON_HexAdvancedRiverData>();
    public List<JSON_Resource> resources = new List<JSON_Resource>();

    #region Публичные методы  
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="mapSize"></param>
    public JSON_MapContainer(IReadOnlyList<MG_HexCell> cells, Vector2Int mapSize)
    {
        //_cells = new List<JSON_HexCell>();

        this.mapSize = mapSize;
        if (cells.Any())
        {
            foreach (var cell in cells)
            {
                GenerateJSON_HexCell(cell);
                GenerateJSON_HexRoadData(cell);
                GenerateJSON_HexRiverData(cell);
                GenerateJSON_HexAdvancedRiverData(cell);
                GenerateJSON_Resource(cell);
            }
        }
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Сгенерировать JSON объект (хекс клетка)
    /// </summary>
    /// <param name="cell"></param>
    private void GenerateJSON_HexCell(MG_HexCell cell)
    {
        JSON_HexCell json_cell = new JSON_HexCell();

        json_cell.pos = (Vector2Int)cell.Pos;
        json_cell.type = cell.Type.Id;// id Тип клетки 
        json_cell.tile = CS_Extension.IndexOf(cell.Type.GroundVariants, cell.Tile);// id тайла
        json_cell.tile_Object = CS_Extension.IndexOf(cell.Type.ObjectVariants, cell.Tile_Object);// id тайла объекта
        json_cell.tileOrigin = cell.TileOrigin;

        cells.Add(json_cell);
    }

    /// <summary>
    ///Сгенерировать JSON объект (дорога)
    /// </summary>
    /// <param name="cell"></param>
    private void GenerateJSON_HexRoadData(MG_HexCell cell)
    {
        bool hasRoad = cell.HasAnyRoad();
        if (hasRoad)
        {
            JSON_HexRoadData json_hexRoadData = new JSON_HexRoadData();
            json_hexRoadData.pos = (Vector2Int)cell.Pos;
            roads.Add(json_hexRoadData);
        }
    }

    /// <summary>
    ///Сгенерировать JSON объект (реки)
    /// </summary>
    /// <param name="cell"></param>
    private void GenerateJSON_HexRiverData(MG_HexCell cell)
    {
        bool hasRiver = cell.HasAnyRiver();
        if (hasRiver)
        {
            JSON_HexRiverData json_hexRiverData = new JSON_HexRiverData();
            json_hexRiverData.pos = (Vector2Int)cell.Pos;
            rivers.Add(json_hexRiverData);
        }
    }

    /// <summary>
    /// Сгенерировать JSON объект (реки Advanced)
    /// </summary>
    /// <param name="cell"></param>
    private void GenerateJSON_HexAdvancedRiverData(MG_HexCell cell)
    {
        bool hasAdvancedRiver = cell.HasAnyAdvancedRiver();
        if (hasAdvancedRiver)
        {
            JSON_HexAdvancedRiverData json_hexAdvancedRiverData = new JSON_HexAdvancedRiverData();

            json_hexAdvancedRiverData.pos = (Vector2Int)cell.Pos;
            json_hexAdvancedRiverData.dirList = cell.GetDirectionsOfAdvancedRiver();

            advancedRivers.Add(json_hexAdvancedRiverData);
        }
    }

    /// <summary>
    /// Сгенерировать JSON объект (ресурсы)
    /// </summary>
    /// <param name="cell"></param>
    private void GenerateJSON_Resource(MG_HexCell cell)
    {
        bool hasResource = cell.HasResource();
        if (hasResource)
        {
            JSON_Resource json_Resource = new JSON_Resource();
            json_Resource.pos = (Vector2Int)cell.Pos;
            json_Resource.type = cell.Resource.Type.Id;
            json_Resource.tile = CS_Extension.IndexOf(cell.Type.GroundVariants, cell.Tile);
            json_Resource.tile_Object = CS_Extension.IndexOf(cell.Resource.Type.ObjectVariants, cell.Tile_Object);
            json_Resource.hexCellType = cell.Type.Id;
            json_Resource.amount = cell.Resource.Amount;

            resources.Add(json_Resource);
        }
    }
    #endregion Личные методы
}
}
