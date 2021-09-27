using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_Resource : MonoBehaviour//, IVisibility
{
    #region Поля
    [BoxGroup("Настройки"), SerializeField, ReadOnly] private MG_ResourceType _type;//тип
    [BoxGroup("Настройки"), SerializeField] private int _amount = 100;//кол-во
    [BoxGroup("Для дебагинга"), SerializeField, ReadOnly] private Vector3Int _pos;//позиция, на которой находиться ресурс
    //private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
    private MG_HexCell _cell;//клетка, на которой находиться ресурс
    #endregion Поля

    #region Свойства
    public MG_HexCell Cell { get => _cell; }
    public MG_ResourceType Type { get => _type; }
    public int Amount { get => _amount; }
    #endregion Свойства

    #region Публичный метод
    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="amount">кол-во</param>
    /// <param name="resourceType">тип</param>
    /// <param name="cell">клетка</param>
    public void Init(int amount, MG_ResourceType resourceType, MG_HexCell cell)
    {
        this._amount = amount;
        this._type = resourceType;
        this._cell = cell;
        _pos = cell.Pos;
        cell.SetResource(this);


        MG_HexCellType cellType = resourceType.CellType;
        Tile tile = resourceType.GroundVariants[0];
        Tile tile_obj = resourceType.ObjectVariants[0];

        MG_GlobalHexMap map = cell.Map;
        map.UpdateCell(cell, cellType, tile, tile_obj, TileOrigin.resource);

        //_lineOfSight = MG_LineOfSight.Instance;
        //InheritCellVisibility();// Унаследовать видимость от клетки
    }
    #endregion Публичный метод

    #region Личный метод 
    /// <summary>
    /// Унаследовать видимость от клетки
    /// </summary>
    //private void InheritCellVisibility()
    //{
    //    switch (_cell.CurrentVisibility)
    //    {
    //        case Visibility.BlackFog:
    //            _cell.Map.TileMap.SetTile(_cell.Pos, MG_FogOfWar.Instance.FogTile);//устанавливаем тайл
    //            _cell.Map.TileMap.SetTileFlags(_cell.Pos, TileFlags.None);//сбрасываем флаги тайла
    //            break;
    //        case Visibility.GreyFog:
    //            MG_GlobalHexMap.Instance.UpdateCell(_cell, _type.CellType, _cell.Tile);
    //            _cell.Mark(_lineOfSight.GreyFogOfWar);
    //            break;
    //        case Visibility.Visible:
    //            MG_GlobalHexMap.Instance.UpdateCell(_cell, _type.CellType, _cell.Tile);
    //            break;
    //        default:
    //            Debug.Log("<color=orange>MG_Resource InheritCellVisibility(): switch DEFAULT.</color>");
    //            break;
    //    }
    //}
    #endregion Личный метод
}

[Serializable]
public class JSON_Resource
{
    public Vector2Int pos;
    public int amount;//кол-во

    public string type;// id
    public string hexCellType;// id
    public int tile;// id тайла
    public int tile_Object;// id тайла объекта
}
}
