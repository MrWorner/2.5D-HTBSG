using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_UnitObjectFactory : MonoBehaviour
{
    private static MG_UnitObjectFactory _instance;

    #region Личные поля: необходимые объекты
    [Required(InfoMessageType.Error), SerializeField] private GameObject _divisionPrefab;//[R] префаб Дивизии
    #endregion Личные поля: необходимые объекты

    #region Методы UNITY
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_UnitObjectFactory Awake(): найден лишний _instance!</color>");

        if (_divisionPrefab == null) Debug.Log("<color=red>MG_UnitObjectFactory Awake(): '_settlementPrefab' не задан!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Создать объект из префаба
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public GameObject Create(MG_HexCell cell, MG_Player side)
    {
        if (!cell.IsTaken)
        {
            GameObject newDivision = Instantiate(_divisionPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newDivision.GetComponent<MG_Division>().Init(cell, side);
            return newDivision;
        }

        Debug.Log("<color=red>MG_UnitObjectFactory Create():</color> объект не был создан! Клетка занята.");
        return null;     
    }
    #endregion Публичные методы

    #region TESTING
    public Vector3Int test_hexCellCoords;
    public bool test_randomCoords;
    public MG_Player test_side;
    [Button]
    private void TEST_Create()
    {
        MG_HexCell cell;
        if (test_randomCoords)
            cell = MG_GlobalHexMap.Instance.GetRandomCell(new List<GroundType>() { GroundType.Mountain, GroundType.Water, GroundType.HeavyForest, GroundType.Forest }, false, false, true);
        else
            cell = MG_GlobalHexMap.Instance.GetCell(test_hexCellCoords);

        if (cell != null)
        {
            if (!cell.IsTaken)
            {
                var divisionObj = Create(cell, test_side);
                Debug.Log("<color=green>MG_UnitObjectFactory TEST_Create()</color>: объект успешно создан!");
                return;
            }
        }
        Debug.Log("<color=red>MG_UnitObjectFactory TEST_Create():</color> объект не был создан!");
    }
    #endregion TESTING
}
}
