using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface IMovable<cell>
{
    HashSet<cell> GetAvailableDestinations();// Получить доступные клетки (зависит от movementPoints)
    List<cell> FindPath(cell destination);// Найти путь
    void Move();// Передвинуться
    void CalculatePathArea();// Вычислить клетки до которым можно "дотянуться"
}
}
