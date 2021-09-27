/////////////////////////////////////////////////////////////////////////////////
//
//	MG_Keyboard.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:   Основной класс управление клавиатурой    				
//			    
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_Keyboard : MonoBehaviour
{
    private static MG_Keyboard instance;//в редакторе только один объект должен быть создан

    #region Поля - необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_KeyboardStrategy _strategy;//[R] Выбранная стратегия
    #endregion Поля - необходимые модули

    #region Методы UNITY
    void Awake()
    {
        if (!instance)
            instance = this;
        else
            Debug.Log("<color=orange>MG_Keyboard Awake(): найдет лишний _instance класса MG_Keyboard.</color>");
        if (_strategy == null)
            Debug.Log("<color=red>MG_Keyboard Awake(): '_strategy' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Тик
    /// </summary>
    public void Tick()
    {
        if (Input.anyKey)//Хоть какая либо клавиша нажата
        {
            _strategy.AnyPressed();
        }
        else
        {
            _strategy.Idling();//простой
        }
        _strategy.AnyAfter();//после всех
    }

    /// <summary>
    /// Установить стратегию
    /// </summary>
    /// <param name="strategy">Стратегия</param>
    public void SetStrategy(MG_KeyboardStrategy strategy)
    {
        this._strategy = strategy;
    }
    #endregion Публичные методы
}
}
