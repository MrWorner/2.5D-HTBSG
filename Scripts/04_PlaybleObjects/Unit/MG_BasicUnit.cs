using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MG_StrategyGame
{
public abstract class MG_BasicUnit<cell,unitType> : MonoBehaviour
{
    
    #region Поля: необходимые модули (INIT + заданные в префабе)
    [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] protected MG_Player _side;//[R INIT] Игровая сторона
    [Required("Должен быть задан в префабе!", InfoMessageType.Error), SerializeField] protected SpriteRenderer _spriteRenderer;//[R DEV] Рендер спрайта
    [Required("Должен быть задан в префабе!", InfoMessageType.Error), SerializeField] protected MG_UnitHighlighter _unitHighlighter;//[R DEV PREFAB] Подсветка юнита
    #endregion Поля: необходимые модули (INIT + заданные в префабе)

    #region Поля
    protected cell _cell;//местонахождение
    #endregion Поля

    #region Свойства
    public cell Cell { get => _cell; set => _cell = value; }//местонахождение
    public MG_Player Side { get => _side; set => _side = value; }//[R INIT] игровая сторона
    public SpriteRenderer SpriteR { get => _spriteRenderer; }//[R PREFAB] рендер спрайта
    #endregion Свойства

    #region Публичные абстрактные методы
    public abstract void RemoveFromGame();//убрать с игрового поля
    public abstract void OnTurnStart();//При начало хода
    public abstract void OnTurnEnd();//При завершении хода
    public abstract void OnSelect();//При выделении
    public abstract void OnSelectUnderMouse();//При выделении как противника/нейтрального оппонента
    public abstract void OnDeselect();//При отмена выделении
    public abstract void OnDeselectUnderMouse();//При отмена выделении как противника/нейтрального оппонента
    public abstract void OnMoveFinished();// При завершении движения
    #endregion Публичные абстрактные методы

    public Action Moved { get; set; }// Действие при завершении движения
    public Action<unitType> Deleted { get; set; }//действие при удалении из игры

    #region Метод INIT
    public virtual void Init(cell cell, MG_Player side)
    {
        _cell = cell;// Местонахождение
        _side = side;// Игровая сторона

        if (_spriteRenderer == null) Debug.Log("<color=red>MG_Division_NEW Init(): компонент '_spriteRenderer' не найден!</color>");//[R DEV] рендер спрайта
        if (_unitHighlighter == null) Debug.Log("<color=red>MG_Division_NEW Init(): префаб '_unitHighlighter' не найден!</color>");//[R DEV PREFAB] Подсветка юнита
    }
    #endregion Метод INIT
}
}
