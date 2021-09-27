using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public abstract class Test_MG_SObjLibrary<T>
{

    public abstract void Add(T item);
    public abstract T Get();
}

public class Test_MS_CellDataLibrary : Test_MG_SObjLibrary<MG_HexCellType>
{
    public override void Add(MG_HexCellType item)
    {
        throw new System.NotImplementedException();
    }

    public override MG_HexCellType Get()
    {
        throw new System.NotImplementedException();
    }
}
}