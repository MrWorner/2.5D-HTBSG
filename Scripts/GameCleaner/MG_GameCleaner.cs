//////////////////////////////////////////////////////////////////////////////
//
//	MG_GameCleaner.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание: Очистка игровой карты от всех объектов юнитов до самой карты. Может быть использовать для переформирования и перезагрузки карты.
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_GameCleaner : MonoBehaviour
{
    private static MG_GameCleaner _instance;

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexCellTypeLibrary _cellDataLibrary;//[R] библиотека cellType
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_ResourceManager _resourceManager;//[R]
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexBorderManager _hexBorderManager;//[R]

    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexRiverManager _hexRiverManager;//[R]
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexAdvancedRiverManager _hexAdvancedRiverManager;//[R]
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_HexRoadManager _hexRoadManager;//[R]
    #endregion Поля: необходимые модули

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_GameCleaner Awake(): найден лишний MG_GameCleaner!</color>");

        if (_map == null) Debug.Log("<color=red>MG_GameCleaner(): '_map' не прикреплен!</color>");
        if (_cellDataLibrary == null) Debug.Log("<color=red>MG_GameCleaner(): 'cellDataLibrary' не прикреплен!</color>");
        if (_resourceManager == null) Debug.Log("<color=red>MG_GameCleaner(): '_resourceManager' не прикреплен!</color>");
        if (_hexBorderManager == null) Debug.Log("<color=red>MG_GameCleaner(): '_hexBorderManager' не прикреплен!</color>");
        if (_hexRiverManager == null) Debug.Log("<color=red>MG_GameCleaner(): '_hexRiverManager' не прикреплен!</color>");
        if (_hexAdvancedRiverManager == null) Debug.Log("<color=red>MG_GameCleaner(): '_hexAdvancedRiverManager' не прикреплен!</color>");
        if (_hexRoadManager == null) Debug.Log("<color=red>MG_GameCleaner(): '_hexRoadManager' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Очистить карту
    /// </summary>
    [Button]
    public void Clean()
    {
        _map.Clear();//Удаляем все тайлы
        _hexBorderManager.Clear();//Удаляем все границы
        _resourceManager.Clear();//Удаляем все ресурсы
        _hexRiverManager.Clear();//удаляем все реки
        _hexAdvancedRiverManager.Clear();//удаляем все реки
        _hexRoadManager.Clear();//удаляем все дороги

        IReadOnlyList<MG_Player> playerList = MG_PlayerManager.Players;
        if (playerList.Any())
        {
            foreach (MG_Player player in playerList)
            {
                player.ClearAllDivisions();// Очистить от всех дивизий
                player.ClearAllSettlements();// Очистить от всех Поселений
            }
        }

    }
    #endregion Публичные методы
}
}
