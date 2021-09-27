using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MG_StrategyGame
{
public class MG_GlobalHexMapSaver : MonoBehaviour
{
    private static MG_GlobalHexMapSaver _instance;

    #region Поля: необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMap _map;//[R] карта
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] string _JSON_filename = "JSON_MapContainer";
    #endregion Поля: необходимые модули

    #region Поля
    [SerializeField] bool _openFolder;
    #endregion Поля

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_GlobalHexMapSaver Awake(): найден лишний MG_GlobalHexMapSaver!</color>");

        if (_map == null) Debug.Log("<color=red>MG_GlobalHexMapSaver Awake(): '_map' не задан!</color>");
        if (_JSON_filename == null) Debug.Log("<color=red>MG_GlobalHexMapSaver Awake(): '_JSON_filename' не задан!</color>");
    }
    #endregion Методы UNITY

    /// <summary>
    /// Сохранить карту
    /// </summary>
    [Button]
    public void Save()
    {
        string dataPath = Path.Combine(Application.dataPath, _JSON_filename + ".json");

        Vector2Int mapSize = _map.Size;
        IReadOnlyList<MG_HexCell> cells = _map.GetCells();
        JSON_MapContainer _container = new JSON_MapContainer(cells, mapSize);//Обрабатываем информацию и получаем контейнер      

        string json = JsonUtility.ToJson(_container, false);
        StreamWriter sw = File.CreateText(dataPath);
        sw.Close();
        File.WriteAllText(dataPath, json);

        if (_openFolder) Process.Start(@Application.dataPath);
        //countSavedTiles = _container.GetSize();//получить кол-во сохраненных cells
        //Debug.Log("MG_SaveMap Save(): SAVED JSON tiles! Total tile count = " + countSavedTiles);
    }
}
}
