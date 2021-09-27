using DigitalRuby.Threading;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_MultiThreadPathExecuter : MonoBehaviour
{

    #region Поля 
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private bool _isRunning;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private bool _isReady;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private MG_Division _chosenUnit;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private Vector3Int _chosenCellPosition;
    [PropertyOrder(0), BoxGroup("ДЕБАГ"), SerializeField, ReadOnly] private int _resultSize = -1;
    private MG_HexCell _chosenDestination = null;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private EZThread _EZThread;
    private List<MG_HexCell> _result = new List<MG_HexCell>();
    #endregion Поля 

    #region Поля 
    public bool IsReady { get => _isReady; }
    public bool IsRunning { get => _isRunning; }
    #endregion Поля 

    #region Свойства
    private void Awake()
    {
        if (_EZThread == null) Debug.Log("<color=red>MG_MultiThreadPathExecuter Awake(): объект '_EZThread' не прикреплен!</color>");
    }
    #endregion Свойства

    #region Публичные методы 
    /// <summary>
    /// Запустить
    /// </summary>
    /// <param name="chosenUnit"></param>
    /// <param name="chosenDestination"></param>
    public void Execute(MG_Division chosenUnit, MG_HexCell chosenDestination)
    {
        if (_isRunning == false)
        {
            _isRunning = true;
        }

        if (_isReady)
        {
            _isReady = false;
        }

        _chosenUnit = chosenUnit;
        _chosenDestination = chosenDestination;
        _chosenCellPosition = chosenDestination.Pos;
        EZThread.ExecuteInBackground(StartThreadWork);

    }

    /// <summary>
    /// Получить клетку, которая использована для нахождения пути
    /// </summary>
    /// <returns></returns>
    public MG_HexCell GetLastUsedDestination()
    {
        return _chosenDestination;
    }

    /// <summary>
    /// Создать поток и сделать свои дела
    /// </summary>
    private void StartThreadWork()
    {
        //System.Threading.Thread.Sleep(2000);
        var path = _chosenUnit.FindPath(_chosenDestination);

        //результат
        _result = path.ToList();
        _resultSize = _result.Count();
        _isRunning = false;
        _isReady = true;
    }


    /// <summary>
    /// Получить результат
    /// </summary>
    /// <returns></returns>
    public List<MG_HexCell> GetResult()
    {
        _isReady = false;
        List<MG_HexCell> resultCopy = _result.ToList();

        return resultCopy;
    }
    #endregion Публичные методы  
}
}

