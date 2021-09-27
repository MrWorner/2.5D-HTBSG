/////////////////////////////////////////////////////////////////////////////////
//
//	MG_MultipleTargetCamera.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//
//	Предназначение:	Правильное расположение всей карты на мини-карте.
//  Замечания: ---
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_MultipleTargetCamera : MonoBehaviour
{
    private static MG_MultipleTargetCamera _instance;

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Camera cam;
    #endregion Поля: Необходимые модул

    #region Поля
    [BoxGroup("Слежка за позициями"), SerializeField, ReadOnly] List<Transform> targets;
    [BoxGroup("Настройки"), SerializeField] Vector3 offset = new Vector3(0, 0, -14);
    [BoxGroup("Настройки"), SerializeField] float smoothTime = 0.5f;
    [BoxGroup("Настройки"), SerializeField] float minZoom = 500f;
    [BoxGroup("Настройки"), SerializeField] float maxZoom = 10f;
    [BoxGroup("Настройки"), SerializeField] float zoomLimiter = 50f;
    //[BoxGroup("Дебаг"), SerializeField, ReadOnly] bool initializated = false;
    [BoxGroup("Дебаг"), SerializeField, ReadOnly] Vector3 velocity;
    #endregion Поля

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_MultipleTargetCamera Awake(): найден лишний MG_MultipleTargetCamera!</color>");

        if (cam == null) Debug.Log("<color=red>MG_MultipleTargetCamera Awake(): 'cam' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Публичные методы
    /// <summary>
    /// Инициализация
    /// </summary>
    public void Init()
    {
        //if (initializated)
        //{
        //    Debug.Log("<color=red>MG_MultipleTargetCamera Init(): УЖЕ ИНИЦИАЛИЗИРОВАНО!</color>");
        //    return;
        //}
        //initializated = true;

        if (targets.Count == 0)
        {
            return;
        }

        Move();
        Zoom();
    }

    /// <summary>
    /// Добавить объект для слежки камерой
    /// </summary>
    /// <param name="obj"></param>
    public void AddObjectForWatch(GameObject obj)
    {
        targets.Add(obj.transform);
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Зум
    /// </summary>
    private void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        //cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
        cam.orthographicSize = newZoom;
    }

    /// <summary>
    /// Получить самую большую дистанцию
    /// </summary>
    /// <returns></returns>
    private float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        float result;
        var x = bounds.size.x;
        var y = bounds.size.y;

        if (x > y)
        {
            result = x;
        }
        else
        {
            result = y;
        }

        return result;
    }

    /// <summary>
    /// Переместить
    /// </summary>
    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        Vector3 newPosition = centerPoint + offset;

        //cam.transform.position = newPosition;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, newPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// Получить центр точки
    /// </summary>
    /// <returns></returns>
    private Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
    #endregion Личные методы
}
}
