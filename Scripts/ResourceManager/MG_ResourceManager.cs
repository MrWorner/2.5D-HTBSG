using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_ResourceManager : MonoBehaviour
{
    private static MG_ResourceManager _instance;
    private static int count = 0;


    #region Поля: Необходимые модули + объекты
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_GlobalHexMap _map;//карта
    #endregion Поля: Необходимые модули + объекты

    #region Поля
    private GameObject _resourceContainer;//Текущий GameObject, где будут созданы ресурсы
    [InfoBox("Все созданные ресурсы на карте", InfoMessageType.Info), SerializeField] private List<MG_Resource> _resources;//Все созданные ресурсы на карте    
    #endregion Поля

    #region Методы UNITY
    void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Debug.Log("<color=orange>MG_ResourceManager Awake(): найдет лишний _instance класса MG_ResourceManager.</color>");
        if (_map == null) Debug.Log("<color=red>MG_ResourceManager Awake(): объект '_map' не прикреплен!</color>");
        _resourceContainer = this.gameObject;
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Создать ресурс на карте
    /// </summary>
    /// <param name="amount">кол-во запасов</param>
    /// <param name="type">тип</param>
    /// <param name="cell">клетка</param>
    public void Create(int amount, MG_ResourceType type, MG_HexCell cell)
    {
        count++;
        GameObject gameObj = new GameObject("" + type.ResourceName + " " + count + " [" + cell.Pos.x + "," + cell.Pos.y + "]");
        gameObj.transform.SetParent(_instance._resourceContainer.transform);
        gameObj.transform.position = cell.WorldPos;

        MG_Resource resource = gameObj.AddComponent<MG_Resource>() as MG_Resource;
        resource.Init(amount, type, cell);
        _resources.Add(resource);
    }

    /// <summary>
    /// Удалить ресурс
    /// </summary>
    /// <param name="cell"></param>
    public void Remove(MG_HexCell cell)
    {
        if (HasResource(cell))
        {
            MG_Resource resource = cell.Resource;
            cell.SetResource(null);
            //cell.Tile = cell.Type.Variants[0];
            MG_HexCellType cellType = cell.Type;
            Tile tile = cellType.GroundVariants[0];
            Tile tile_obj = cellType.ObjectVariants[0];
            _map.UpdateCell(cell, cellType, tile, tile_obj, TileOrigin.cellType);
            _resources.Remove(resource);
            Destroy(resource.gameObject);
        }
    }

    /// <summary>
    /// Очистить карту от ресурсов
    /// </summary>
    public void Clear()
    {
        if (_resources.Any())
        {
            foreach (var resource in _resources.ToList())
            {
                MG_HexCell cell = resource.Cell;
                cell.SetResource(null);
                MG_HexCellType cellType = cell.Type;
                Tile tile = cellType.GroundVariants[0];
                Tile tile_obj = cellType.ObjectVariants[0];
                cell.SetTypeAndTiles(cellType, tile, tile_obj, TileOrigin.cellType);
                _resources.Remove(resource);
                Destroy(resource.gameObject);
            }
        }
    }

    /// <summary>
    /// Имеется ли ресурс
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool HasResource(MG_HexCell cell)
    {
        MG_Resource resource = cell.Resource;
        return (resource != null);
    }
    #endregion Публичные методы
}
}