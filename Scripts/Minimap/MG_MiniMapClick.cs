/////////////////////////////////////////////////////////////////////////////////
//
//	MG_MiniMapClick.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//
//	Предназначение:	Перемещение камеры по клику мини-карты.
//  Замечания: ---
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//https://forum.unity.com/threads/interactable-minimap-using-raw-image-render-texture-solved.525486/

namespace MG_StrategyGame
{
public class MG_MiniMapClick : MonoBehaviour, IPointerClickHandler
{
    private static MG_MiniMapClick _instance;

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Camera miniMapCam;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Camera mainCamera;
    #endregion Поля: Необходимые модул

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_MiniMapClick Awake(): найден лишний MG_MiniMapClick!</color>");

        if (miniMapCam == null) Debug.Log("<color=red>MG_MiniMapClick Awake(): 'miniMapCam' не прикреплен!</color>");
        if (mainCamera == null) Debug.Log("<color=red>MG_MiniMapClick Awake(): 'mainCamera' не прикреплен!</color>");
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform, eventData.pressPosition, eventData.pressEventCamera, out Vector2 localCursorPoint))
        {
            Rect imageRectSize = GetComponent<RawImage>().rectTransform.rect;
            localCursorPoint.x = (localCursorPoint.x - imageRectSize.x) / imageRectSize.width;
            localCursorPoint.y = (localCursorPoint.y - imageRectSize.y) / imageRectSize.height;
            SetCameraPosition(localCursorPoint);
        }
    }
    #endregion Методы UNITY

    #region Личные методы
    /// <summary>
    /// Установить позицию камеры
    /// </summary>
    /// <param name="localCursor"></param>
    private void SetCameraPosition(Vector2 localCursor)
    {
        Vector3 position = miniMapCam.ScreenToWorldPoint(new Vector2(localCursor.x * miniMapCam.pixelWidth, localCursor.y * miniMapCam.pixelHeight));
        mainCamera.transform.position = new Vector3(position.x, position.y, -14);
    }
    #endregion Личные методы
}
}