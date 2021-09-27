using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_PlayerManager : MonoBehaviour
{
    private static MG_PlayerManager _instance;

    #region Поля: необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_Player _emptySide;//[R] пустая сторона
    #endregion Поля: необходимые модули

    #region Поля
    [InfoBox("Должно быть заполнено перед стартом игры!", InfoMessageType.Warning), SerializeField] private List<MG_Player> _players;//[R] лист текущих игроков
    #endregion Поля

    #region Свойства
    public static IReadOnlyList<MG_Player> Players { get => _instance._players; }//[R] лист текущих игроков
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_PlayerManager Awake(): найден лишний MG_PlayerManager!</color>");

        if (_emptySide == null) Debug.Log("<color=red>MG_PlayerManager Awake(): 'EmptySide' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Статический метод GetEmptySide()
    /// <summary>
    /// Получить пустую сторону
    /// </summary>
    /// <returns></returns>
    public static MG_Player GetEmptySide()
    {
        return _instance._emptySide;
    }
    #endregion Статический метод GetEmptySide()
}
}
