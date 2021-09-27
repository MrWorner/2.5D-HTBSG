using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexRiverData : IVisibility, IMarkable
{
    #region Поля
    private MG_HexRiver _riverType;//[R CONSTRUCTOR] тип реки
    private Tilemap _riverTileMap;//[R CONSTRUCTOR] объект стандартной TileMap (карта рек)
    private Vector3Int _pos;//[R CONSTRUCTOR]
    private MG_HexCell _cell;//[R CONSTRUCTOR]
    private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
    #endregion Поля

    #region Свойства
    public MG_HexRiver RiverType { get => _riverType; set => _riverType = value; }//[R CONSTRUCTOR] Тип реки
    #endregion Свойства

    #region Конструктор
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pos">позиция</param>
    public MG_HexRiverData(MG_HexCell cell)
    {
        _riverTileMap = MG_HexRiverManager.GetRiverTileMap();
        _riverType = MG_HexRiverLibrary.GetEmptyRiver();
        _lineOfSight = MG_LineOfSight.Instance;
        _pos = cell.Pos;
        _cell = cell;

        if (_riverTileMap == null) Debug.Log("<color=red>MG_HexRoadData MG_HexRoadData(): 'roadTileMap' не задан!</color>");
        if (_riverType == null) Debug.Log("<color=red>MG_HexRoadData MG_HexRoadData(): 'roadType' не задан!</color>");
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
                    Mark(Color.clear);//прячем реку если на данной клетке есть поселение
                    break;
                }
                UnMark();
                break;

            default:
                Debug.Log("<color=orange>MG_HexRiverData SetVisibility(): switch DEFAULT.</color>");
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
        _riverTileMap.SetColor(_pos, color);
    }

    /// <summary>
    /// Сбросить выделение (IMarkable)
    /// </summary>
    public void UnMark()
    {
        _riverTileMap.SetColor(_pos, Color.white);
        //throw new System.NotImplementedException();
    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
}


[Serializable]
public class JSON_HexRiverData
{
    public Vector2Int pos;
    //public string roadType;
}
}
