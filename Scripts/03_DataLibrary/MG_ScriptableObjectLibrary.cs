using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public abstract class MG_ScriptableObjectLibrary<I> : MonoBehaviour
{
    [InfoBox("Не должен быть пустым!", InfoMessageType.Warning), SerializeField] protected List<I> _items;//элемент
    public IReadOnlyList<I> Items { get => _items.ToList(); }//получить лист элементов (копию)

    /// <summary>
    /// Добавить в список элемент
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(I item)
    {
        _items.Add(item);
    }

}
}