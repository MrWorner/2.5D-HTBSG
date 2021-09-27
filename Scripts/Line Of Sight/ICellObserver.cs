using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface ICellObserver
{
    void AddVisibleCell(MG_HexCell cell);// Добавить клетку в список видимых клеток
    void RemoveVisibleCell(MG_HexCell cell);// Убрать клетку из списка видимых клеток
    MG_Player GetSide();// Получить сторону
    void RecalculateVision();//Обновить видимость
    IReadOnlyCollection<MG_HexCell> GetVisibleCells();//получить все видимые клетки
}
}