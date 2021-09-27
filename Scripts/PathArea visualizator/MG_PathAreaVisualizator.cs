/////////////////////////////////////////////////////////////////////////////////
//
//	MG_PathAreaVisualizator.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание: Для визуализации доступных путей для передвижения	    				
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_PathAreaVisualizator: MonoBehaviour
{
    private static MG_PathAreaVisualizator _instance;

    #region Поля + необходимые модули
    private Coroutine _colorCoroutine;//короутин изменения цвета Тайлмапа зоны
    //[PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tile _hexPathTile;//[R] тайл патча
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _pathAreaTileMap;//[R] объект стандартной TileMap (карта дорог)
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_AnimatedTile _animatedPathTile;//[R] Анимированный тайл
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_ColorCycle _сolorCycle;//[R] для изменения цвета Тайлмапа
    #endregion Поля + необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): найден лишний MG_PathAreaVisualizator!</color>");

        //if (_hexPathTile == null) Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): '_hexPathTile' не задан!</color>");
        if (_map == null) Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): '_map' не задан!</color>");
        if (_pathAreaTileMap == null) Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): 'roadTileMap' не задан!</color>");
        if (_animatedPathTile == null) Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): '_animatedPathTile' не задан!</color>");
        if (_сolorCycle == null) Debug.Log("<color=red>MG_PathAreaVisualizator Awake(): '_сolorCycle' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Показать зону
    /// </summary>
    /// <param name="cells"></param>
    public void Show(IReadOnlyCollection<MG_HexCell> cells)
    {
        foreach (var cell in cells)
        {
            Place(cell);
        }
        _colorCoroutine = StartCoroutine(_сolorCycle.Cycle());
    }

    /// <summary>
    /// Очистить карту
    /// </summary>
    public void Clear()
    {
        StopCoroutine(_colorCoroutine);
        _pathAreaTileMap.ClearAllTiles();
    }
    #endregion Публичные методы

    #region MyRegion
    /// <summary>
    /// Установить тайл
    /// </summary>
    /// <param name="cell"></param>
    private void Place(MG_HexCell cell)
    {
        var pos = cell.Pos;
        //_pathAreaTileMap.SetTile(pos, _hexPathTile);//устанавливаем тайл
        _pathAreaTileMap.SetTile(pos, _animatedPathTile);
        //_pathAreaTileMap.SetTileFlags(pos, TileFlags.None);//сбрасываем флаги тайла


    }
    #endregion

}
}
