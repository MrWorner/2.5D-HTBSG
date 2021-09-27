using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public interface ICellControl<c>
{
    c CreateCell(Vector3Int pos, MG_HexCellType cellType, Tile tile, Tile tile_obj);// Создать клетку
    bool IsCellExist(Vector3Int pos);// Существует ли клетка
    c GetCell(Vector3Int pos);// Получить клетку по игровым координатам
    c GetCell(Vector3 pos);// Получить клетку по реальным координатам
    void RemoveCell(c cell);// Удалить клетку
    IReadOnlyList<c> GetCells();// Получить список всех клеток
}
}
