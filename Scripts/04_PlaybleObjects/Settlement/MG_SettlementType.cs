using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
[CreateAssetMenu(fileName = "settlementType_", menuName = "--->MG/MG_SettlementType ", order = 51)]
public class MG_SettlementType : ScriptableObject
{
    [SerializeField] private string id;//уникальный номер
    [SerializeField] private string settlementName = "noname";//имя
    [SerializeField] private string description;//описание
    [SerializeField] private MG_HexCellType cellType;//тип

    public string Id { get => id; }//уникальный номер
    public string SettlementName { get => settlementName; }//имя
    public string Description { get => description; }//описание
    public MG_HexCellType CellType { get => cellType; }//тип
}
}
