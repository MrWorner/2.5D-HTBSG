using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface IObserverHandler
{
    void AddObserver(ICellObserver observer);// Добавить Наблюдателя
    bool HasObserver(ICellObserver observer);// Имеет ли заданного наблюдателя
    void RemoveObserver(ICellObserver observer);//Удалить наблюдателя
    bool HasAnyObserverBySide(MG_Player side);// Имеет ли какого нибудь наблюдателя заданной стороны
    void RecalculateObserversVisions();// Переформировать видимость для наблюдателей
}
}
