////////////////////////////////////
//
//	MG_PlayerController.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:   Основной класс тика управления периферийными устройствами.
//			    
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_PlayerController : MonoBehaviour
{
    #region Поля - необходимые модули
    [Required(InfoMessageType.Error), SerializeField] private MG_Mouse _mouse;//[R] Мышь
    [Required(InfoMessageType.Error), SerializeField] private MG_Keyboard _keyboard;//[R] Клавиатура
    #endregion Поля - необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_mouse == null)
            Debug.Log("<color=red>MG_PlayerController Awake(): '_mouse' не прикреплен!</color>");
        if (_keyboard == null)
            Debug.Log("<color=red>MG_PlayerController Awake(): '_keyboard' не прикреплен!</color>");
    }

    private void LateUpdate()
    {
        _mouse.Tick();// тик мыши
        _keyboard.Tick();// тик клавы
    }
    #endregion Методы UNITY
}
}
