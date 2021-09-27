using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_GlobalHexMapLoader : MonoBehaviour
{
    private static MG_GlobalHexMapLoader _instance;

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_MapGenerator _mapGenerator;//[R] генератор карты
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] string _JSON_filename = "JSON_MapContainer";
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_HexCellTypeLibrary _hexCellTypeLibrary;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_HexRoadManager _roadBuilder;//менеджер дорог
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_HexRiverManager _riverBuilder;//менеджер рек
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_HexAdvancedRiverManager _hexAdvancedRiverManager;//[R] Менеджер Advanced River
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_ResourceManager _resourceManager;//[R] Менеджер Ресурсов
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_ResourceLibrary _resourceLibrary;
    #endregion Поля: необходимые модули

    #region Поля
    private HashSet<Vector2Int> resourceList;
    #endregion Поля

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): найден лишний MG_GlobalHexMapLoader!</color>");

        if (_map == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_map' не задан!</color>");
        if (_JSON_filename == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_JSON_filename' не задан!</color>");
        if (_hexCellTypeLibrary == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_hexCellTypeLibrary' не задан!</color>");
        if (_roadBuilder == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_roadBuilder' не задан!</color>");
        if (_riverBuilder == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_riverBuilder' не задан!</color>");
        if (_hexAdvancedRiverManager == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_hexAdvancedRiverManager' не задан!</color>");
        if (_resourceManager == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_resourceManager' не задан!</color>");
        if (_resourceLibrary == null) Debug.Log("<color=red>MG_GlobalHexMapLoader Awake(): '_resourceLibrary' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Загрузить карту
    /// </summary>
    [Button]
    public void Load()
    {
        string dataPath = Path.Combine(Application.dataPath, _JSON_filename + ".json");
        string json = File.ReadAllText(dataPath);
        JSON_MapContainer сontainer = JsonUtility.FromJson<JSON_MapContainer>(json);

        GenerateMap(сontainer);
        LoadCells(сontainer);
        LoadRoads(сontainer);
        LoadRivers(сontainer);
        LoadAdvancedRivers(сontainer);
        LoadResources(сontainer);
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Сгенерировать карту
    /// </summary>
    /// <param name="сontainer"></param>
    private void GenerateMap(JSON_MapContainer сontainer)
    {
        Vector2Int size = сontainer.mapSize;
        _mapGenerator.GenerateEmptyMap(size);

    }
    #endregion Личные методы

    #region Личные методы (хекс клетки)
    /// <summary>
    /// Загрузить клетки
    /// </summary>
    /// <param name="сontainer"></param>
    private void LoadCells(JSON_MapContainer сontainer)
    {
        List<JSON_HexCell> json_cells = сontainer.cells;
        if (json_cells.Any())
        {
            foreach (JSON_HexCell json_cell in json_cells)
            {
                PlaceCell(json_cell);
            }
        }
    }

    /// <summary>
    /// Установить клетку
    /// </summary>
    /// <param name="json_cell"></param>
    private void PlaceCell(JSON_HexCell json_cell)
    {
        
        Vector3Int pos = (Vector3Int)json_cell.pos;
        MG_HexCell cell = _map.GetCell(pos);
        //Debug.Log("type="+json_cell.type + " tile="+ json_cell.tile + " tile_Object=" + json_cell.tile_Object);

        TileOrigin tileOrigin = json_cell.tileOrigin;
        MG_HexCellType hexCellType = _hexCellTypeLibrary.GetElement(json_cell.type);
        Tile tile;
        Tile tile_Object;
        if (tileOrigin.Equals(TileOrigin.cellType))
        {
            tile = hexCellType.GroundVariants[json_cell.tile];
            tile_Object = hexCellType.ObjectVariants[json_cell.tile_Object];
        }
        else
        {
            tile = hexCellType.GroundVariants[0];
            tile_Object = hexCellType.ObjectVariants[0];
        }
              
        _map.UpdateCell(cell, hexCellType, tile, tile_Object, tileOrigin);
        MG_PathAreaFixer.FixAfterCellUpdate(cell);
    }

    #endregion Личные методы (хекс клетки)

    #region Личные методы (дорога)
    /// <summary>
    /// Загрузить дороги
    /// </summary>
    /// <param name="сontainer"></param>
    private void LoadRoads(JSON_MapContainer сontainer)
    {
        List<JSON_HexRoadData> json_roads = сontainer.roads;
        if (json_roads.Any())
        {
            foreach (JSON_HexRoadData json_road in json_roads)
            {
                PlaceRoad(json_road);
            }
        }
    }

    /// <summary>
    /// Установить дорогу
    /// </summary>
    /// <param name="json_road"></param>
    private void PlaceRoad(JSON_HexRoadData json_road)
    {
        Vector2Int pos = json_road.pos;
        MG_HexCell cell = _map.GetCell((Vector3Int)pos);
        _roadBuilder.Place(cell);
        MG_PathAreaFixer.FixAfterCellUpdate(cell);
    }
    #endregion Личные методы (дорога)

    #region Личные методы (реки)
    /// <summary>
    /// Загрузить реки
    /// </summary>
    /// <param name="сontainer"></param>
    private void LoadRivers(JSON_MapContainer сontainer)
    {
        List<JSON_HexRiverData> json_rivers = сontainer.rivers;
        if (json_rivers.Any())
        {
            foreach (JSON_HexRiverData json_river in json_rivers)
            {
                PlaceRiver(json_river);
            }
        }
    }

    /// <summary>
    /// Установить реку
    /// </summary>
    /// <param name="json_road"></param>
    private void PlaceRiver(JSON_HexRiverData json_road)
    {
        Vector2Int pos = json_road.pos;
        MG_HexCell cell = _map.GetCell((Vector3Int)pos);
        _riverBuilder.Place(cell);
        MG_PathAreaFixer.FixAfterCellUpdate(cell);
    }
    #endregion Личные методы (реки)

    #region Личные методы (реки Advanced)
    /// <summary>
    /// Загрузить реки (Advanced)
    /// </summary>
    /// <param name="сontainer"></param>
    private void LoadAdvancedRivers(JSON_MapContainer сontainer)
    {
        List<JSON_HexAdvancedRiverData> json_advancmedRivers = сontainer.advancedRivers;
        if (json_advancmedRivers.Any())
        {
            foreach (JSON_HexAdvancedRiverData json_advancedRiver in json_advancmedRivers)
            {
                Vector2Int pos = json_advancedRiver.pos;
                List<DirectionHexPT> dirList = json_advancedRiver.dirList;
                if (dirList.Any())
                {
                    foreach (DirectionHexPT dir in dirList)
                    {
                        PlaceAdvancedRiver(pos, dir);
                    }
                }
                else
                {
                    Debug.Log("<color=red>MG_GlobalHexMapLoader LoadAdvancedRivers(): Error! Empty list!</color>");
                }

            }
        }
    }

    /// <summary>
    /// Установить реку (Advanced)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    private void PlaceAdvancedRiver(Vector2Int pos, DirectionHexPT dir)
    {
        MG_HexCell cell = _map.GetCell((Vector3Int)pos);
        _hexAdvancedRiverManager.Place(cell, dir);
        MG_PathAreaFixer.FixAfterCellUpdate(cell);
    }
    #endregion Личные методы (реки Advanced)

    #region Личные методы (ресурсы)
    /// <summary>
    /// Загрузить ресурсы
    /// </summary>
    /// <param name="сontainer"></param>
    private void LoadResources(JSON_MapContainer сontainer)
    {
        List<JSON_Resource> json_resources = сontainer.resources;
        if (json_resources.Any())
        {
            foreach (JSON_Resource json_resource in json_resources)
            {
                PlaceResource(json_resource);
            }
        }
    }

    /// <summary>
    /// Установить клетку
    /// </summary>
    /// <param name="json_cell"></param>
    private void PlaceResource(JSON_Resource json_resource)
    {
        Vector3Int pos = (Vector3Int)json_resource.pos;
        MG_HexCell cell = _map.GetCell(pos);

        MG_ResourceType resourceType = _resourceLibrary.GetElement(json_resource.type);     
        int amount = json_resource.amount;
       
        _resourceManager.Create(amount, resourceType, cell);
        MG_PathAreaFixer.FixAfterCellUpdate(cell);

        TileOrigin tileOrigin = cell.TileOrigin;
        if (tileOrigin.Equals(TileOrigin.resource) == false)
            return;

        Tile tile = resourceType.GroundVariants[json_resource.tile];
        Tile tile_Object = resourceType.ObjectVariants[json_resource.tile_Object];

        MG_HexCellType hexCellType = _hexCellTypeLibrary.GetElement(json_resource.hexCellType);
        _map.UpdateCell(cell, hexCellType, tile, tile_Object, TileOrigin.resource);
        //MG_PathAreaFixer.FixAfterCellUpdate(cell);
    }
    #endregion Личные методы (хекс клетки)

}
}
