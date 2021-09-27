using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface IPathContainer<c>
{
    void AddBlockedCell(c cell);//Добавить заблокированную клетку
    void RemoveBlockedCell(c cell);//Убрать заблокированную клетку
    void AddAvailableCell(c cell);//Добавить доступную клетку
    void RemoveAvailableCell(c cell);//Убрать доступную клетку
    bool IsWalkableCell(c cell);//Является ли текущая клетка проходимой
}
}
