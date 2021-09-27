/////////////////////////////////////////////////////////////////////////////////
//
//	MG_Mouse.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:   Основной класс управление мышью    				
//			    
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MG_StrategyGame
{
public class MG_Mouse : MonoBehaviour
{
    private static MG_Mouse instance;//в редакторе только один объект должен быть создан

    #region Поля - необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_MouseStrategy _strategy;//[R] Выбранная стратегия
    #endregion Поля - необходимые модули

    #region Методы UNITY
    void Awake()
    {
        if (!instance)
            instance = this;
        else
            Debug.Log("<color=orange>MG_Mouse Awake(): найдет лишний _instance класса MG_Mouse.</color>");
        if (_strategy == null)
            Debug.Log("<color=red>MG_Mouse(): '_strategy' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Тик
    /// </summary>
    public void Tick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        _strategy.AnyBefore(mousePos);//Любое событие после всех

        if (Input.GetMouseButtonDown(0))//КЛИК МЫШИ C ОТПУСКАНИЕМ (ЛЕВЫЙ КЛИК)
        {
            _strategy.LeftClickDown(mousePos);
        }
        else if(Input.GetMouseButton(0))//КЛИК МЫШИ БЕЗ ОТПУСКАНИЯ (ЛЕВЫЙ КЛИК)
        {
            _strategy.LeftClick(mousePos);
        }
        else if (Input.GetMouseButton(1))//КЛИК МЫШИ БЕЗ ОТПУСКАНИЯ (ПРАВЫЙ КЛИК)
        {
            _strategy.RightClick(mousePos);
        }
        else if (Input.GetMouseButton(2))//КЛИК МЫШИ БЕЗ ОТПУСКАНИЯ (СРЕДНЯЯ КНОПКА)
        {
            _strategy.MiddleClick(mousePos);
        }
        else
            _strategy.Idling(mousePos);//Бездействие

        _strategy.AnyAfter(mousePos);//Любое событие после всех
    }

    /// <summary>
    /// Установить стратегию
    /// </summary>
    /// <param name="strategy">Стратегия</param>
    public void SetStrategy(MG_MouseStrategy strategy)
    {
        this._strategy = strategy;
    }
    #endregion Публичные методы
}
}
