using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public abstract class MG_MouseStrategy : MonoBehaviour
{
    public abstract void Idling(Vector3 mousePos);//бездействие
    public abstract void LeftClick(Vector3 mousePos);//Левый клик мыши
    public abstract void LeftClickDown(Vector3 mousePos);//Левый клик мыши c отпусканием
    public abstract void RightClick(Vector3 mousePos);//Правый клик мыши
    public abstract void MiddleClick(Vector3 mousePos);//Средний клик мыши
    public abstract void AnyAfter(Vector3 mousePos);//Любое событие после всех
    public abstract void AnyBefore(Vector3 mousePos);//Любое событие перед всех
}
}
