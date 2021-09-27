/////////////////////////////////////////////////////////////////////////////////
//
//	MG_MiniMap.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//
//	Предназначение:	Помощник по созданию 4 позиций на карте (4 угла) для правильного фокуса камеры мини-карты.
//  Замечания: ---
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_MiniMap : MonoBehaviour
{
    private static MG_MiniMap _instance;

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_GlobalHexMap map;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] MG_MultipleTargetCamera _multipleTargetCamera;
    #endregion Поля: Необходимые модули

    #region Поля
    [BoxGroup("Дебаг"), ReadOnly, SerializeField] GameObject _NE_obj;
    [BoxGroup("Дебаг"), ReadOnly, SerializeField] GameObject _SE_obj;
    [BoxGroup("Дебаг"), ReadOnly, SerializeField] GameObject _SW_obj;
    [BoxGroup("Дебаг"), ReadOnly, SerializeField] GameObject _NW_obj;
    [BoxGroup("Дебаг"), ReadOnly, SerializeField] bool initializated = false;
    #endregion Поля

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_MiniMap Awake(): найден лишний MG_MiniMap!</color>");

        if (map == null) Debug.Log("<color=red>MG_MiniMap Awake(): 'map' не прикреплен!</color>");
        if (_multipleTargetCamera == null) Debug.Log("<color=red>MG_MiniMap Awake(): '_multipleTargetCamera' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Инициализация
    /// </summary>
    public void Init()
    {
        //if (initializated)
        //{
        //    Debug.Log("<color=red>MG_MiniMap Init(): УЖЕ ИНИЦИАЛИЗИРОВАНО!</color>");
        //    return;
        //}

       
        CreateObjects();// Создать необходимые объекты
        SetObjectsPosition();// Установить позицию для каждого объекта
        AddObjsToCameraHelper();// Добавить объекты для слежки камерой
        _multipleTargetCamera.Init();

        if (!initializated) initializated = true;
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Создать необходимые объекты
    /// </summary>
    private void CreateObjects()
    {
        if (initializated) return;

        _NE_obj = new GameObject("_NE_obj (for minimap)");
        _SE_obj = new GameObject("_SE_obj (for minimap)");
        _SW_obj = new GameObject("_SW_obj (for minimap)");
        _NW_obj = new GameObject("_NW_obj (for minimap)");

        _NE_obj.transform.SetParent(this.transform);
        _SE_obj.transform.SetParent(this.transform);
        _SW_obj.transform.SetParent(this.transform);
        _NW_obj.transform.SetParent(this.transform);
    }

    /// <summary>
    /// Установить позицию для каждого объекта
    /// </summary>
    private void SetObjectsPosition()
    {
        MG_HexCell cell;

        cell = map.GetCell(new Vector3Int(0, 0, 0));
        _SW_obj.transform.position = cell.WorldPos;

        cell = map.GetCell(new Vector3Int(map.Size.x - 1, map.Size.y - 1, 0));
        _NE_obj.transform.position = cell.WorldPos;

        cell = map.GetCell(new Vector3Int(map.Size.x - 1, 0, 0));
        _SE_obj.transform.position = cell.WorldPos;

        cell = map.GetCell(new Vector3Int(0, map.Size.y - 1, 0));
        _NW_obj.transform.position = cell.WorldPos;
    }

    /// <summary>
    /// Добавить объекты для слежки камерой
    /// </summary>
    private void AddObjsToCameraHelper()
    {
        if (initializated) return;
        _multipleTargetCamera.AddObjectForWatch(_SW_obj);
        _multipleTargetCamera.AddObjectForWatch(_NE_obj);
        _multipleTargetCamera.AddObjectForWatch(_SE_obj);
        _multipleTargetCamera.AddObjectForWatch(_NW_obj);
    }
    #endregion Личные методы
}
}