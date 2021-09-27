using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_TurnManager : MonoBehaviour
{
    private static MG_TurnManager _instance;

    #region Поля
    [SerializeField] private int _turn = 0;//текущий ход
    private int _playerTurn = 0;//текущий номер игрока, чей ход
    [Required("Пустая сторона", InfoMessageType.Error), SerializeField] private MG_Player _currentPlayerTurn;//текущий ход игрока  
    [Required(InfoMessageType.Error), SerializeField] private GameObject _endTurnBtn;//[R] Кнопка окончания хода
    [Required("Пустая сторона", InfoMessageType.Error), SerializeField] private MG_Player _currentPlayerVisibleMap;//[R] карта с видимостью для какого именно игрока доступна
    private bool isEndButtonEnabled = true;
    #endregion Поля

    #region Требуемые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_LineOfSight _lineOfSight;//[R] Менеджер тумана войны
    #endregion Требуемые модули

    #region Свойства
    public int Turn { get => _turn; set => _turn = value; }//текущий ход
    public MG_Player CurrentPlayerVisibleMap { get => _currentPlayerVisibleMap; }//[R] карта с видимостью для какого именно игрока доступна
    #endregion

    #region ACTION
    public Action EndTurn { get; set; }// Действие после завершения хода
    #endregion ACTION

    #region Unity методы
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_TurnManager Awake(): найден лишний MG_TurnManager!</color>");

        if (_lineOfSight == null) Debug.Log("<color=red>MG_TurnManager Awake(): 'lineOfSight' не задан!</color>");
        if (_endTurnBtn == null) Debug.Log("<color=red>MG_TurnManager Awake(): 'EndTurnBtn' не задан!</color>");
    }
    #endregion

    #region Статические публичные методы
    /// <summary>
    /// Отключить кнопку Пропуск хода
    /// </summary>
    public static void DisableTurnButton()
    {
        _instance._endTurnBtn.SetActive(false);
        _instance.isEndButtonEnabled = false;
    }

    /// <summary>
    /// Включить кнопку Пропуск хода
    /// </summary>
    public static void EnableTurnButton()
    {
        _instance._endTurnBtn.SetActive(true);
        _instance.isEndButtonEnabled = true;
    }

    public static bool IsTurnButtonEnabled()
    {
        return _instance.isEndButtonEnabled;
    }


    /// <summary>
    /// Получить игрока текущего хода
    /// </summary>
    /// <returns></returns>
    public static MG_Player GetCurrentPlayerTurn()
    {
        return _instance._currentPlayerTurn;
    }

    /// <summary>
    /// Получить игрока для которого показана карта
    /// </summary>
    /// <returns></returns>
    public static MG_Player GetCurrentPlayerVisibleMap()
    {
        return _instance._currentPlayerVisibleMap;
    }

    /// <summary>
    /// Получить текущий ход
    /// </summary>
    /// <returns></returns>
    public static int GetTurn()
    {
        return _instance.Turn;
    }

    /// <summary>
    /// Видима ли карта для заданного игрока
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public static bool IsMapVisibleForSide(MG_Player side)
    {
        return side.Equals(_instance.CurrentPlayerVisibleMap);
    }

    /// <summary>
    /// Сбросить все показатели
    /// </summary>
    public static void Reset()
    {
        _instance._turn = 0;
        _instance._playerTurn = 0;
    }
    #endregion Статические публичные методы

    #region Публичные методы

    /// <summary>
    /// Активировать данный модуль
    /// </summary>
    public void Activate()
    {
        if (MG_PlayerManager.Players.Any())
            OnTurnStart();
        else
            Debug.Log("<color=red>MG_Game Start(): Лист 'Players' пустой!</color>");
    }

    /// <summary>
    /// При окончании хода
    /// </summary>
    [Button]
    public void OnTurnEnd()
    {
        if (_currentPlayerTurn != null)
        {
            foreach (var division in _currentPlayerTurn.GetDivisions())
            {
                division.OnTurnEnd();
            }

            foreach (var settlement in _currentPlayerTurn.GetSettlements())
            {
                //Поселение
                settlement.OnTurnEnd();
            }
        }

        EndTurn?.Invoke();
        OnTurnStart();
    }

    /// <summary>
    /// При старте каждого хода
    /// </summary>
    public void OnTurnStart()
    {
        int totalPlayers = MG_PlayerManager.Players.Count();

        if (_currentPlayerTurn.Equals(MG_PlayerManager.GetEmptySide()))//текущий игрок не задан, задаем
        {
            _currentPlayerTurn = MG_PlayerManager.Players[0];
            //Debug.Log("MG_Game: Начало раунда, первый игрок:" + currentPlayerTurn + " playerTurn= " + playerTurn + " totalPlayers=" + totalPlayers);
        }
        else//текущий игрок был задан и уже делал свой ход, нужно задать следующего игрока по списку
        {
            _playerTurn++;
            if (totalPlayers > _playerTurn)
            {
                //задаем нового игрока текущего хода
                _currentPlayerTurn = MG_PlayerManager.Players[_playerTurn];
                //Debug.Log("MG_Game: следующий игрок:" + currentPlayerTurn + " playerTurn= " + playerTurn + " totalPlayers=" + totalPlayers);
            }
            else
            {
                //если по кругу уже все ходили, то сбрасываем текущего игрока и запускаем метод снова
                _playerTurn = 0;
                _currentPlayerTurn = MG_PlayerManager.GetEmptySide();
                Turn++;
                //Debug.Log("MG_Game: Все игроки сделали свой ходы, начинаем новый раунт! round=" + round);
                OnTurnStart();
            }
        }

        if (!_currentPlayerTurn.Equals(MG_PlayerManager.GetEmptySide()))
        {
            //если не было сброса игрока, то обновляем ходы юнитов текущего игрока 

            foreach (var division in _currentPlayerTurn.GetDivisions())
            {
                //Обновляем очки действия для каждого юнита текущего игрока
                division.OnTurnStart();
            }

            //---ДЕЛАЕМ ВИДИМОСТЬ НА КАРТЕ ДЛЯ ДАННОГО ИГРОКА!
            if (_currentPlayerTurn.Type.Equals(PlayerType.Player))
            {
                if (!_currentPlayerTurn.Equals(_currentPlayerVisibleMap))
                {
                    //Debug.Log("Переключена карта видимости на: " + currentPlayerTurn.name + " | был: " + currentPlayerVisibleMap.name);
                    //Debug.Log("Карта видимости для: " + currentPlayerTurn.name);
                    _currentPlayerVisibleMap = _currentPlayerTurn;
                    _lineOfSight.SwitchVisibilityMapFor(_currentPlayerVisibleMap);
                }
            }
            else
            {
                //ЗДЕСЬ ДОЛЖНА БЫТЬ КОЕ КАКАЯ ЛОГИКА AI

                //AI и другие пропускают ход.
                Debug.Log(_currentPlayerTurn.name + " пропускает свой ход: " + Turn);
                OnTurnEnd();
            }
        }
    }


    /// <summary>
    /// Заполнен ли лист игроков
    /// </summary>
    /// <returns></returns>
    //public bool IsPlayerListNotEmpty()
    //{
    //    return _players.Any();
    //}
    #endregion Публичные методы
}
}
