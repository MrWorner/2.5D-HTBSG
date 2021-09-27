using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public abstract class MG_BasicMap<c> : MonoBehaviour
{
    #region Поля _size, cells
    [SerializeField] protected Vector2Int _size = new Vector2Int (0,0);//Размер карты X/Y
    protected readonly Dictionary<Vector3Int, c> cells = new Dictionary<Vector3Int, c>();//Словарик со всеми клетками данной карты 
    #endregion Поля

    #region Необходимые объекты _tileMap
    [Required(InfoMessageType.Error), SerializeField] protected Tilemap _tileMap;//[R] объект стандартной TileMap (Сама карта)
    #endregion

    #region Свойства Size, TileMap
    public Vector2Int Size { get => _size; }//получить размер карты
    public Tilemap TileMap { get => _tileMap; }//получить Тайлмап данной карты
    #endregion

    #region Абстрактный класс Clear() 
    public abstract void Clear();// Очистить карту
    #endregion

    #region Публичный метод ChangeCellColor(Vector3Int pos, Color color)
    /// <summary>
    /// Изменить цвет тайла клетки
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    public virtual void ChangeCellColor(Vector3Int pos, Color color)
    {
        _tileMap.SetColor(pos, color);
    }
    #endregion
}
}
