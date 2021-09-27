////////////////////////////////////
//
//	MG_CameraDragger.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:   Перетаскивание камеры средней кнопкой мыши.
//			    
/////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public static class MG_CameraDragger// : MonoBehaviour
{
    private static Vector3 _draggMouseOriginP;//перетаскивание начальная позиция мыши
    private static Vector3 _draggMouseOffset;//перетаскивание разница перемещения мыши
    private static bool _isDraggingMouse;//активировано ли перетаскивание мыши

    #region Публичные методы
    
    /// <summary>
    /// Переносим камеру
    /// </summary>
    /// <param name="mainCamera"></param>
    /// <param name="cameraObject"></param>
    public static void Dragging(Camera mainCamera, GameObject cameraObject)
    {
        _draggMouseOffset = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - cameraObject.transform.position);
        if (!_isDraggingMouse)
        {
            _isDraggingMouse = true;
            _draggMouseOriginP = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            //mouseOriginPoint = _mainCameraGameObject.transform.position;
        }
        cameraObject.transform.position = _draggMouseOriginP - _draggMouseOffset;
    }

    /// <summary>
    /// Отключить перетаскивание
    /// </summary>
    public static void KeepDraggingDisabled()
    {
        if (_isDraggingMouse)
        {
            _isDraggingMouse = false;
        };

    }
    #endregion Публичные методы
}
}
