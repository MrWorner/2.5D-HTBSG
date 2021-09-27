using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface ICellNeighbours<C,D>
{
    bool HasNeighbour(C cell);//есть ли сосед C
    void AddNeighbour(C cell, D dir);//добавить соседа по направлению
    void RemoveNeighbour(C cell);//удалить соседа
    void RemoveNeighbour(D dir);//удалить соседа по направлению
    void ClearAllNeighbours();//удалить всех соседей
    IReadOnlyList<C> GetNeighbours();//получить лист всех соседей
    IReadOnlyDictionary<D, C> GetNeighboursWithDirection();//получить словарик, который содержит соседей по направлениям
    C GetNeighbourByDirection(D dir);//получить соседа по направлению
    D GetDirectionOfNeighbour(C cellN);//получить направление заданного соседа
}
}
