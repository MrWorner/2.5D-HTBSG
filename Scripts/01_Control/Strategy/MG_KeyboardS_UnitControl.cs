using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_KeyboardS_UnitControl : MG_KeyboardStrategy
{
    #region Поля
    [SerializeField] private float _speedCamMovement = 20.0f;//скорость передвижения камеры
    [SerializeField] private float _zoomMin = 5f; //Минимальный зум
    [SerializeField] private float _zoomMax = 25f; //Максимальный зум
    [SerializeField] private float _defaultZoom = 10f;//стандартное значение зума

    private float _x = 0f;//для позиции камеры по Х
    private float _y = 0f;//для позиции камеры по Y
    private float _z = -10f;//для позиции камеры по Z
    #endregion Поля

    #region Поля: Необходимые модули
    private Camera _mainCamera;//[R] компонен главной камеры (ИНИЦИАЛИЗАЦИЯ В Awake())
    private GameObject _mainCameraGameObject;//[R] объект камеры (ИНИЦИАЛИЗАЦИЯ В Awake())
    [Required(InfoMessageType.Error), SerializeField] private BoxCollider2D _restrictionArea;//[R] зона по которой только можно передвигаться камерой
    #endregion Поля: Необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
            Debug.Log("<color=red>MG_KeyboardS_UnitControl Awake(): объект 'Camera.main' НЕ НАЙДЕН!</color>");

        _mainCameraGameObject = _mainCamera.gameObject;
        if (_mainCameraGameObject == null)
            Debug.Log("<color=red>MG_KeyboardS_UnitControl Awake(): объект '_mainCameraGameObject' не прикреплен!</color>");

        if (_restrictionArea == null)
            Debug.Log("<color=red>MG_KeyboardS_UnitControl Awake(): объект для _restrictionArea не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Родительские перегружаемые методы MG_KeyboardStrategy
    /// <summary>
    /// Любое событие после
    /// </summary>
    public override void AnyAfter()
    {

    }

    /// <summary>
    /// Любая клавиша нажата
    /// </summary>
    public override void AnyPressed()
    {
        CameraMovement();       
        CameraRestrictionArea();
        CameraZoom();
    }

    /// <summary>
    /// Событие во время застоя
    /// </summary>
    public override void Idling()
    {

    }
    #endregion #region Родительские методы MG_KeyboardStrategy

    #region Личные методы: CameraMovement(), CameraZoom(), CameraRestrictionArea()
    /// <summary>
    /// Передвижение камеры
    /// </summary>
    private void CameraMovement()
    {
        _x = _mainCameraGameObject.transform.position.x;
        _y = _mainCameraGameObject.transform.position.y;
        _z = _mainCameraGameObject.transform.position.z;
        if (Input.GetKey(KeyCode.D))
        {
            float bonusSpeed = 0;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                bonusSpeed = _speedCamMovement * 2;
            }
            _x = _x + ((_speedCamMovement + bonusSpeed) * Time.deltaTime);
            _mainCameraGameObject.transform.position = new Vector3(_x, _y, _z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            float bonusSpeed = 0;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                bonusSpeed = _speedCamMovement * 2;
            }
            _x = _x - ((_speedCamMovement + bonusSpeed) * Time.deltaTime);
            _mainCameraGameObject.transform.position = new Vector3(_x, _y, _z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            float bonusSpeed = 0;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                bonusSpeed = _speedCamMovement * 2;
            }
            _y = _y - ((_speedCamMovement + bonusSpeed) * Time.deltaTime);
            _mainCameraGameObject.transform.position = new Vector3(_x, _y, _z);
        }
        if (Input.GetKey(KeyCode.W))
        {
            float bonusSpeed = 0;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                bonusSpeed = _speedCamMovement * 2;
            }
            _y = _y + ((_speedCamMovement + bonusSpeed) * Time.deltaTime);
            _mainCameraGameObject.transform.position = new Vector3(_x, _y, _z);
        }
    }

    /// <summary>
    /// Зум камеры
    /// </summary>
    private void CameraZoom()
    {
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            _mainCamera.orthographicSize = _zoomMin;
        }

        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            _mainCamera.orthographicSize = _zoomMax;
        }

        if (Input.GetKey(KeyCode.Keypad5))
        {
            _mainCamera.orthographicSize = _defaultZoom;
        }
    }

    /// <summary>
    /// Ограничение камеры на передвижение
    /// </summary>
    private void CameraRestrictionArea()
    {
        float vertExtent = _mainCamera.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        vertExtent = 0f;
        horizExtent = 0f;

        Vector3 linkedCameraPos = _mainCamera.transform.position;
        Bounds areaBounds = _restrictionArea.bounds;

        _mainCameraGameObject.transform.position = new Vector3(
            Mathf.Clamp(linkedCameraPos.x, areaBounds.min.x + horizExtent, areaBounds.max.x - horizExtent),
            Mathf.Clamp(linkedCameraPos.y, areaBounds.min.y + vertExtent, areaBounds.max.y - vertExtent),
            linkedCameraPos.z);
    }
    #endregion Личные методы
}
}
