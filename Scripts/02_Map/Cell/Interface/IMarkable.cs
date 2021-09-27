using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public interface IMarkable
{
    void Mark(Color color);//пометить
    void UnMark();//убрать отметку
}
}
