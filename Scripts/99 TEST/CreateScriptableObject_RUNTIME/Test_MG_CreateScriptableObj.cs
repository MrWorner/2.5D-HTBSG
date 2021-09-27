using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class Test_MG_CreateScriptableObj : MonoBehaviour
{
    public MG_HexCellType template;
    public MG_HexCellType created;

    private void Start()
    {
        Create();
    }

    public void Create()
    {
        created = Instantiate(template);
        created.name = "AAAAAAAAA";
        //-------- created.CellName = "AAAAAAAAA"; <----- Метод должен быть переименовки думаю, а не Свойства открывать
    }
}
}
