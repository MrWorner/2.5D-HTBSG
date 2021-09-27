using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_HexAdvancedRiverGrid : MonoBehaviour
{

    private static MG_HexAdvancedRiverGrid _instance;

    #region Поля
    //private Dictionary<Vector3, GameObject> _markers = new Dictionary<Vector3, GameObject>();//словарь созданных маркеров на сетке
    //private Dictionary<Vector3, MG_HexCell> _markerCells = new Dictionary<Vector3, MG_HexCell>();//словарь позиций и клеток, на котором создан маркер (маркер создан между двумя клетками и только одна клетка из двух будет записана как хозяин) 
    #endregion Поля

    #region Поля: необходимые модули
    //[PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] Карта
    //[PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private Tilemap _advancedRiverTileMap;//[R] карта тайлпама продвинутых рек
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private GameObject _markerPrefab;//[R] игровой объект со спрайтом (метка) 
    #endregion Поля: необходимые модули

    #region Свойства
    public GameObject MarkerPrefab { get => _markerPrefab; }//[R] игровой объект со спрайтом (метка)
    //public IReadOnlyDictionary<Vector3, GameObject> Markers { get => _markers; }//словарь созданных маркеров на сетке
    #endregion Свойства

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_HexAdvancedRiverGrid Awake(): найден лишний MG_HexAdvancedRiverGrid!</color> " + this.gameObject.name);

        //if (_map == null) Debug.Log("<color=red>MG_HexAdvancedRiverGrid Awake(): '_map' не задан!</color>");
        //if (_advancedRiverTileMap == null) Debug.Log("<color=red>MG_HexAdvancedRiverGrid Awake(): '_advancedRiverTileMap' не задан!</color>");
        if (_markerPrefab == null) Debug.Log("<color=red>MG_HexAdvancedRiverGrid Awake(): '_markerPrefab' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Создать маркер
    /// </summary>
    /// <param name="pos"></param>
    //public void CreateMarker(Vector3 pos, MG_HexCell cell)
    //{
    //    if (IsMarkerExist(pos))
    //        return;

    //    GameObject marker = Instantiate(_instance._markerPrefab, pos, Quaternion.identity);

    //    _markers.Add(pos, marker);
    //    _markerCells.Add(pos, cell);
    //    ChangeColorMarker( pos, Color.blue);
    //}

    /// <summary>
    /// Удалить маркер (уничтожить)
    /// </summary>
    /// <param name="pos"></param>
    //public void DeleteMarker(Vector3 pos)
    //{
    //    if (IsMarkerExist(pos))
    //    {
    //        GameObject marker;
    //        if (_markers.TryGetValue(pos, out marker))
    //        {
    //            _markers.Remove(pos);
    //            _markerCells.Remove(pos);
    //            Destroy(marker);
    //        }
    //        else
    //        {
    //            Debug.Log("<color=red>MG_HexAdvancedRiverGrid DeleteMarker(): Критическая ошибка! Не найден маркер.</color>");
    //        }
    //    }
    //}

    /// <summary>
    /// Существует ли маркер
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    //public bool IsMarkerExist(Vector3 pos)
    //{
    //    return _markers.ContainsKey(pos);
    //}

    /// <summary>
    /// Изменить цвет маркера по позиции
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    //public void ChangeColorMarker(Vector3 pos, Color color)
    //{
    //    GameObject marker;
    //    if (_markers.TryGetValue(pos, out marker))
    //    {
    //        SpriteRenderer spriteRenderer = marker.GetComponent<SpriteRenderer>();
    //        Color colorMarker = spriteRenderer.color;
    //        if (colorMarker.Equals(color) == false)
    //            spriteRenderer.color = color;
    //    }
    //}
    #endregion Публичные методы

    #region Личные методы

    #endregion Личные методы

}
}
