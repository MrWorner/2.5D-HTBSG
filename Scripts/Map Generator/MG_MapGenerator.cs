using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_MapGenerator : MonoBehaviour
{
    private static MG_MapGenerator _instance;

    #region Поля
    [SerializeField] private Vector2Int _size = new Vector2Int(30, 30);//Размер карты X/Y
    [SerializeField] private bool _useMaxMapSize;//Размер карты X/Y
    [SerializeField] private Vector2Int _maxSize = new Vector2Int(200, 160);//МАКСИМАЛЬНЫЙ Размер карты X/Y. 128x80 Civilization 5, 200x160 WinSPMBT
    #endregion Поля

    #region Поля: необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_HexCellTypeLibrary _cellDataLibrary;//[R] библиотека cellType
    [Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [Required(InfoMessageType.Error), SerializeField] private MG_GameCleaner _gameCleaner;//[R] модуль очистки карты
    [Required(InfoMessageType.Error), SerializeField] private MG_HexCellType _defaultLandType;//[R] стандартный тип местности при генерации чистой карты
    [Required(InfoMessageType.Error), SerializeField] private MG_MiniMap _miniMap;//[R]мини-карта
    #endregion Поля: необходимые модули

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_MapGenerator Awake(): найден лишний MG_MapGenerator!</color>");

        if (_map == null) Debug.Log("<color=red>MG_MapGenerator Awake(): '_map' не прикреплен!</color>");
        if (_cellDataLibrary == null) Debug.Log("<color=red>MG_MapGenerator Awake(): 'cellDataLibrary' не прикреплен!</color>");
        if (_gameCleaner == null) Debug.Log("<color=red>MG_MapGenerator Awake(): '_gameCleaner' не прикреплен!</color>");
        if (_miniMap == null) Debug.Log("<color=red>MG_MapGenerator Awake(): '_miniMap' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Сгенерировать пустую карту
    /// </summary>
    [Button("Generate Empty Map")]
    public void GenerateEmptyMap()
    {
        _gameCleaner.Clean();

        if (_useMaxMapSize)
            _size = _maxSize;

        int width = _size.x;
        int height = _size.y;

        IReadOnlyList<MG_HexCellType> cellDataList = _cellDataLibrary.Items;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                //MG_HexCellType cellData = cellDataList[Random.Range(0, (cellDataList.Count))];
                Tile tile = _defaultLandType.GroundVariants[Random.Range(0, (_defaultLandType.GroundVariants.Count))];
                Tile tile_obj = _defaultLandType.ObjectVariants[Random.Range(0, (_defaultLandType.GroundVariants.Count))];
                _map.CreateCell(pos, _defaultLandType, tile, tile_obj);
            }
        }
        //---Подправить видимость мини-карты
        _miniMap.Init();

    }

    /// <summary>
    /// Сгенерировать пустую карту
    /// </summary>
    /// <param name="size"></param>
    public void GenerateEmptyMap(Vector2Int size)
    {
        if (size.x == 0 || size.y == 0)
        {
            Debug.Log("<color=red>MG_MapGenerator GenerateEmptyMap(): 'size' имеет неверное значение!</color> size=" + size);
            return;
        }
        _size = size;
        if (_useMaxMapSize) _useMaxMapSize = false;
        GenerateEmptyMap();
    }

    /// <summary>
    /// Сгенерировать карту
    /// </summary>
    [Button("Generate Map")]
    public void GenerateMap()
    {
        _gameCleaner.Clean();

        if (_useMaxMapSize)
            _size = _maxSize;

        int width = _size.x;
        int height = _size.y;

        var cellDataList = _cellDataLibrary.Items;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                var cellData = cellDataList[Random.Range(0, (cellDataList.Count))];
                Tile tile = cellData.GroundVariants[Random.Range(0, (cellData.GroundVariants.Count))];
                Tile tile_obj = cellData.ObjectVariants[Random.Range(0, (cellData.GroundVariants.Count))];
                _map.CreateCell(pos, cellData, tile, tile_obj);
            }
        }
        //---Подправить видимость мини-карты
        _miniMap.Init();
    }


    /// <summary>
    /// Очистить карту
    /// </summary>
    [Button("Clear map")]
    public void ClearMap()
    {
        _gameCleaner.Clean();
    }
    #endregion Публичные методы
}
}
