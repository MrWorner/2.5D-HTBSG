////////////////////////////////////
//
//	MG_CameraWheelZoomer.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:   Зум камеры с помощью прокручиванием колесиком мыши.
//			    
/////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public static class MG_CameraWheelZoomer
{
    private static float _zoomMin = 3f; //Минимальный зум
    private static float _zoomMax = 35f; //Максимальный зум

    #region Публичные методы
    /// <summary>
    /// Зум с помощью колесика мыши
    /// </summary>
    public static void ZoomByMouseWeel(Camera mainCamera)
    {
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * (10f * mainCamera.orthographicSize * 0.1f), _zoomMin, _zoomMax);
    }

    /// <summary>
    /// Установить значение зума
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public static void SetWheelZoom(float min, float max)
    {
        if (min <= 0) min = 1f;
        if (max <= 0) max = 1f;
        if (min > max) max = min;

        _zoomMin = min;
        _zoomMax = max;
    }
    #endregion Публичные методы
}
}
