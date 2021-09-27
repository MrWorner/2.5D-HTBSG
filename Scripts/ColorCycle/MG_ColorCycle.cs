//////////////////////////////////////////////////////////////////////////////
//
//	MG_ColorCycle.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание: Изменения цвета тайлмапа со временем
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
public class MG_ColorCycle : MonoBehaviour
{
    #region Личные поля + Модули и объекты для заполнения
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), InfoBox("Не должен быть пустым!", InfoMessageType.Warning), SerializeField] Color[] _colors;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Tilemap tileMap;
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] float _speed = 5;
    private int _currentIndex = 0;
    #endregion

    #region Методы UNITY
    private void Start()
    {
        _currentIndex = 0;
        SetColor(_colors[_currentIndex]);
    }
    #endregion

    #region Публичные методы
    /// <summary>
    /// Цикл изменения цвета
    /// </summary>
    /// <returns></returns>
    public IEnumerator Cycle()
    {
        while (true)
        {
            var startColor = tileMap.color;
            var endColor = _colors[0];
            if (_currentIndex + 1 < _colors.Length)
            {
                endColor = _colors[_currentIndex + 1];
            }

            var newColor = Color.Lerp(startColor, endColor, Time.deltaTime * _speed);
            SetColor(newColor);

            if (newColor == endColor)
            {

                if (_currentIndex + 1 < _colors.Length)
                {
                    _currentIndex++;
                }
                else
                {
                    _currentIndex = 0;
                }
            }
            yield return 0;
        };
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Установить цвет
    /// </summary>
    /// <param name="color"></param>
    private void SetColor(Color color)
    {
        tileMap.color = color;
    }

    #endregion

}
}
