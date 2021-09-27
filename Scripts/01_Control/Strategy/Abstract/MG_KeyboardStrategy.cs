using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public abstract class MG_KeyboardStrategy : MonoBehaviour
{
    public abstract void Idling();//бездействие
    public abstract void AnyPressed();//любая клавиша нажата
    public abstract void AnyAfter();//любое событие после всех
}
}
