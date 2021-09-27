using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public abstract class MG_BasicCell<mapType, cellType>
{
    #region Поля
    protected readonly mapType _map;//Принадлежность к карте
    protected readonly Vector3Int _pos;//локальные координаты
    protected readonly Vector3 _worldPos;//реальные координаты в игровом пространстве
    protected Tile _tile;//текущий используемый вариант тайла из всех доступных вариантов тайлов cellType (LEVEL 1: GROUND)
    
    protected cellType _type;//Тип клетки
    #endregion Поля

    #region Свойства
    public Vector3Int Pos { get => _pos; }//координаты на карте
    public Vector3 WorldPos { get => _worldPos; }//координаты в игровом пространстве
    public mapType Map { get => _map; }//принадлежность к карте
    public Tile Tile { get => _tile; }//используемый тайл из всех доступных вариантов тайлов cellType (LEVEL 1: GROUND)
    public cellType Type { get => _type; }//Тип клетки
    #endregion Свойства

    #region Конструктор
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="cellType">Тип клетки</param>
    /// <param name="tile">Используемый вариант тайла (LEVEL 1: GROUND)</param>
    /// <param name="pos">Позиция</param>
    /// <param name="worldPos">Глобальная позиция</param>
    /// <param name="map">Принадлежность к карте</param>
    public MG_BasicCell(cellType cellType, Tile tile, Vector3Int pos, Vector3 worldPos, mapType map)
    {
        this._type = cellType;
        this._tile = tile;
        this._pos = pos;
        this._worldPos = worldPos;
        this._map = map;
    }
    #endregion
}
}
