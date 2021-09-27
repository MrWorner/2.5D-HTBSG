using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_ResourceLibrary : MG_ScriptableObjectLibrary<MG_ResourceType>
{
    private static MG_ResourceLibrary _instance;

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_ResourceLibrary Awake(): найден лишний MG_ResourceLibrary!</color>");

        if (_items.Count == 0)
            Debug.Log("<color=red>MG_ResourceLibrary Awake(): '_items' пуст! Должен быть заполнен всеми возможными вариантами</color>");
        else if (_items.Count != 4)
            Debug.Log("<color=red>MG_ResourceLibrary Awake(): '_items' заполнен не до конца! (А также должен включать: Empty River, Cirlce river)</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Получить объект ресурс
    /// </summary>
    /// <param name="roadDirection">направление дороги</param>
    /// <returns></returns>
    public MG_ResourceType Get(int index)
    {
        if (index > _items.Count)
        {
            Debug.Log("<color=red>MG_ResourceLibrary Get(): index > _items.Count </color>");
            return null;
        }
            
        return _items[index];
    }

    /// <summary>
    /// Найти элемент по ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MG_ResourceType GetElement(string id)
    {
        MG_ResourceType result = null;
        if (_items.Any())
        {
            result = _items.First(s => s.Id == id);
            return result;
        }
        return result;
    }
    #endregion Публичные методы

}
}
