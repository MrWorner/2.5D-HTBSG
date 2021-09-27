using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexRoadData : IVisibility, IMarkable
{
    #region Поля
    private MG_HexRoad _roadType;//[R CONSTRUCTOR] тип дороги
    private Tilemap _roadTileMap;//[R CONSTRUCTOR] объект стандартной TileMap (карта дорог)
    private Vector3Int _pos;//[R CONSTRUCTOR]
    private MG_HexCell _cell;//[R CONSTRUCTOR]
    private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
    #endregion Поля

    #region Свойства
    public MG_HexRoad RoadType { get => _roadType; set => _roadType = value; }//[R CONSTRUCTOR] Тип дороги
    #endregion Свойства

    #region Конструктор
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pos">позиция</param>
    public MG_HexRoadData(MG_HexCell cell)
    {
        _roadTileMap = MG_HexRoadManager.GetRoadTileMap();
        _roadType = MG_HexRoadLibrary.GetEmptyRoad();
        _lineOfSight = MG_LineOfSight.Instance;
        _pos = cell.Pos;
        _cell = cell;

        if (_roadTileMap == null) Debug.Log("<color=red>MG_HexRoadData MG_HexRoadData(): 'roadTileMap' не задан!</color>");
        if (_roadType == null) Debug.Log("<color=red>MG_HexRoadData MG_HexRoadData(): 'roadType' не задан!</color>");
        if (_lineOfSight == null) Debug.Log("<color=red>MG_HexRoadData MG_HexRoadData(): '_lineOfSight' не задан!</color>");
    }
    #endregion Конструктор

    #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
    /// <summary>
    /// Установить видимость (IVisibility)
    /// </summary>
    /// <param name="visibility">видимость</param>
    public void SetVisibility(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.BlackFog:
                Mark(Color.clear);               
                break;

            case Visibility.GreyFog:
                Mark(_lineOfSight.GreyFogOfWar);             
                break;

            case Visibility.Visible:
                if (_cell.Settlement != null)
                {
                    Mark(Color.clear);//прячем дорогу если на данной клетке есть поселение, так как у поселений уже по стандарту есть дорога
                    break;
                }
                UnMark();              
                break;

            default:
                Debug.Log("<color=orange>MG_HexBorderData SetVisibility(): switch DEFAULT.</color>");
                break;
        }
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"

    #region МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
    /// <summary>
    /// Пометить цветом (IMarkable)
    /// </summary>
    /// <param name="color">Цвет</param>
    public void Mark(Color color)
    {
        _roadTileMap.SetColor(_pos, color);
    }

    /// <summary>
    /// Сбросить выделение (IMarkable)
    /// </summary>
    public void UnMark()
    {
        _roadTileMap.SetColor(_pos, Color.white);
        //throw new System.NotImplementedException();
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
}

[Serializable]
public class JSON_HexRoadData
{
    public Vector2Int pos;
    //public string roadType;
}
}