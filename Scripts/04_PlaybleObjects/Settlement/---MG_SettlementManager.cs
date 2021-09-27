//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class MG_SettlementManager : MonoBehaviour
//{
//    private static MG_SettlementManager instance;

//    #region Поля
//    [SerializeField] private List<MG_Settlement> settlements;//поселение
//    #endregion Поля

//    #region Свойства
//    public List<MG_Settlement> Settlements { get => settlements; }//поселение
//    #endregion Свойства

//    #region Методы UNITY
//    void Awake()
//    {
//        if (!instance)
//            instance = this;
//        else
//            Debug.Log("<color=orange>MG_SettlementManager Awake(): найдет лишний _instance класса MG_SettlementManager.</color>");
//    }
//    #endregion Методы UNITY

//    #region Публичный метод Show(MG_HexCell _cell, MG_Player _side, MG_SettlementType _type, GameObject settlementPrefab)
//    /// <summary>
//    /// Установить постройку на клетке
//    /// </summary>
//    /// <param name="_cell">клетка</param>
//    /// <param name="_side">сторона</param>
//    /// <param name="settlementPrefab">префаб поселения</param>
//    public MG_Settlement Show(MG_HexCell _cell, MG_Player _side, MG_SettlementType _type, GameObject settlementPrefab)//, GameObject buildingPrefab
//    {
//        GameObject gameObj = Instantiate(settlementPrefab);        
//        MG_Settlement settlement = gameObj.GetComponent<MG_Settlement>();
//        settlement.Init(_cell, _side, _type);
//        //Debug.Log("MG_BuildingManager Show(): Construction completed!");
//        Settlements.Add(settlement);
//        return settlement;
//    }
//    #endregion Публичный метод Show(MG_HexCell _cell, MG_Player _side, MG_SettlementType _type, GameObject settlementPrefab)
//}
