using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_HexCellTypeLibrary : MG_ScriptableObjectLibrary<MG_HexCellType>
{
    private static MG_HexCellTypeLibrary _instance;

    private void Awake()
    {
        if (!_instance)
            _instance = this;
        else
            Debug.Log("<color=orange>MG_HexCellTypeLibrary Awake(): найдет лишний _instance класса MG_HexCellTypeLibrary.</color>");

    }

    /// <summary>
    /// Найти элемент по ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MG_HexCellType GetElement(string id)
    {
        MG_HexCellType result = null;
        if (_items.Any())
        {
            result = _items.First(s => s.Id == id);
            return result;
        }
        return result;
    }
}
}
