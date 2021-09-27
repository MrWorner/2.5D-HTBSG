using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_FogOfWar : MonoBehaviour
{
    private static MG_FogOfWar _instance;//в редакторе только один объект должен быть создан

    #region Поля + необходимые модули
    [SerializeField] bool _disableFogOfWarOnStart = false;//отключить туман войны для текущего игрока при старте
    [SerializeField] bool _revealMapOnStart = false;//Раскрыть карту для текущего игрока при старте
    //[SerializeField] bool _disableFOW = false;
    //[SerializeField] bool revealMapOnStart = false;
    [Required(InfoMessageType.Error), SerializeField] Tile _fogTile;//[R] Тайл тумана
    [Required(InfoMessageType.Error), SerializeField] MG_GlobalHexMap _map;//[R] Карта
    [Required(InfoMessageType.Error), SerializeField] MG_LineOfSight _lineOfSight;//[R] модуль зоны видимости
    #endregion Поля + необходимые модули

    #region Свойства
    public static MG_FogOfWar Instance { get => _instance; }//в редакторе только один объект должен быть создан
    public Tile FogTile { get => _fogTile; }
    #endregion Свойства

    #region Методы UNITY
    void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Debug.Log("<color=orange>MG_FogOfWar Awake(): найдет лишний _instance.</color>");
        if (_fogTile == null)
            Debug.Log("<color=red>MG_FogOfWar Awake(): '_fogTile' не задан!</color>");
        if (_map == null)
            Debug.Log("<color=red>MG_FogOfWar Awake(): '_map' не задан!</color>");
        if (_lineOfSight == null)
            Debug.Log("<color=red>MG_FogOfWar Awake(): '_lineOfSight' не задан!</color>");
    }

    private void Start()
    {
        if (_disableFogOfWarOnStart)
            DisableFogOfWar();
        else if (_revealMapOnStart)
        {
            DisableFogOfWar();
            EnableFogOfWar();
        }

    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Активировать туман
    /// </summary>
    public void PlaceVisualFogOfWar()
    {
        var cells = _map.GetCells();
        foreach (var cell in cells)
        {
            cell.SetVisibility(Visibility.BlackFog);
        }
    }

    /// <summary>
    /// Отключить туман
    /// </summary>
    [Button]
    public void DisableFogOfWar()
    {
        MG_Player side = MG_TurnManager.GetCurrentPlayerVisibleMap();
        bool revealed = side.IsFogDeactivated;
        if (!revealed)
        {

            side.IsFogDeactivated = true;
            var allCells = MG_GlobalHexMap.Instance.GetCells();
            var allCellsHashSet = new HashSet<MG_HexCell>(allCells);
            _lineOfSight.UpdateStaticFOV(allCellsHashSet, side);// Обновить Зону Видимости для статических вещей (Поселения, территория которой владеет игрок и тд.)
            MG_PathAreaFixer.FixAfterMapReveal();
        }
    }

    /// <summary>
    /// Включить туман
    /// </summary>
    [Button]
    public void EnableFogOfWar()
    {
        MG_Player side = MG_TurnManager.GetCurrentPlayerVisibleMap();
        bool revealed = side.IsFogDeactivated;
        if (revealed)
        {
            side.IsFogDeactivated = false;
            var allCells = MG_GlobalHexMap.Instance.GetCells();
            var allCellsHashSet = new HashSet<MG_HexCell>(allCells);
            _lineOfSight.RemoveStaticFOV(allCellsHashSet, side);// Обновить Зону Видимости для статических вещей (Поселения, территория которой владеет игрок и тд.)
        }
    }

    /// <summary>
    /// Сбросить Туман Войны до начального видения
    /// </summary>
    [Button]
    public void ResetFogOfWar()
    {
        MG_Player side = MG_TurnManager.GetCurrentPlayerVisibleMap();

        bool revealed = side.IsFogDeactivated;
        if (revealed) EnableFogOfWar();

        var cells = _map.GetCells();

        foreach (var cell in cells)
        {
            cell.SetVisibility(Visibility.BlackFog);
        }

        side.ForgetAllDiscoveredCells();

        _lineOfSight.SwitchVisibilityMapFor(side);//RESET видимости всех клеток у юнитов и других объектов текущего игрока
        MG_PathAreaFixer.FixAfterMapReveal();

    }
    #endregion Публичные методы

}
}
