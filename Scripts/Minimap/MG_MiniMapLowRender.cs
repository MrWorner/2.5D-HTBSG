using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
//[RequireComponent(typeof(Camera))]
public class MG_MiniMapLowRender : MonoBehaviour
{
    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Camera renderCam;
    #endregion Поля: Необходимые модули

    #region Поля
    [BoxGroup("Настройки (установить перед запуском!)"), SerializeField] float FPS = 5f;
    #endregion Поля

    #region Методы UNITY
    void Start()
    {
        InvokeRepeating("Render", 0f, 1f / FPS);
    }
    void OnPostRender()
    {
        renderCam.enabled = false;
    }
    #endregion Методы UNITY

    #region Личные методы
    void Render()
    {
        renderCam.enabled = true;
    }
    #endregion Личные методы
}
}
