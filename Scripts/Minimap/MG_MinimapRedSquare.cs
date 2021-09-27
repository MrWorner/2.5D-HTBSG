/////////////////////////////////////////////////////////////////////////////////
//
//	MG_MinimapRedSquare.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//
//	Предназначение:	Красный квадрат на мини-карте, обозначающий текущее положение основной камеры на карте.
//  Замечания: ---
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
//https://medium.com/@alessandrovalcepina/creating-a-rts-like-minimap-with-unity-9cd578dc4522
public class MG_MinimapRedSquare : MonoBehaviour
{
    private static MG_MinimapRedSquare _instance;

    #region Поля: Необходимые модули
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Material cameraBoxMaterial;//For the cameraBoxMaterial field, just create a material and change its shader to the Sprites/Default shader, or any another vertex shader, in order to be able to set the color through GL.Color.
    [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] Camera minimap;
    #endregion Поля: Необходимые модули

    #region Поля
    [ BoxGroup("Настройки"), SerializeField] float lineWidth = 0.012f;
    #endregion Поля

    #region Методы UNITY
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Debug.Log("<color=red>MG_MinimapRedSquare Awake(): найден лишний MG_MinimapRedSquare!</color>");

        if (cameraBoxMaterial == null) Debug.Log("<color=red>MG_MinimapRedSquare Awake(): 'cameraBoxMaterial' не прикреплен!</color>");
        if (minimap == null) Debug.Log("<color=red>MG_MinimapRedSquare Awake(): 'minimap' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Методы UNITY OnPostRender()
    public void OnPostRender()
    {
        CreateSquare();
    }
    #endregion Методы UNITY OnPostRender()

    #region Личные методы
    /// <summary>
    /// Получить точку усечения камеры
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private Vector3 GetCameraFrustumPoint(Vector3 position)
    {
        Vector3 result = Camera.main.ScreenToWorldPoint(position);
        return result;
    }

    /// <summary>
    /// Создать квадрат, который указывает границы основной камеры на мини-карте
    /// </summary>
    private void CreateSquare()
    {
        Vector3 minViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(0f, 0f)));
        Vector3 maxViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(Screen.width, Screen.height)));

        float minX = minViewportPoint.x;
        float minY = minViewportPoint.y;

        float maxX = maxViewportPoint.x;
        float maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            cameraBoxMaterial.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(Color.red);
            {
                GL.Vertex(new Vector3(minX, minY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + lineWidth, 0));

                GL.Vertex(new Vector3(minX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + lineWidth, maxY, 0));

                GL.Vertex(new Vector3(minX, maxY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + lineWidth, 0));

                GL.Vertex(new Vector3(maxX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + lineWidth, maxY, 0));
            }
            GL.End();
        }
        GL.PopMatrix();
    }
    #endregion Личные методы

}
}