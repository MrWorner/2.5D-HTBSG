/////////////////////////////////////////////////////////////////////////////////
//
//	MG_GameSettings.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:	Игровые настройки	    				
//			    
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
    public class MG_GameSettings : MonoBehaviour
    {
        private static MG_GameSettings _instance;
        public static MG_GameSettings Instance()
        {
            return _instance;
        }

        #region Поля
        [BoxGroup("Анимация"), SerializeField] private bool _skipMovementAnimation = false;//Пропустить анимацию движения
                                                                                           //[BoxGroup("MG_CameraWheelZoomer"), SerializeField, DisableInPlayMode] private float _minZoom = 3f;//минимальный зум мыши
                                                                                           //[BoxGroup("MG_CameraWheelZoomer"), SerializeField, DisableInPlayMode] private float _maxZoom = 35f;//максимальный зум мыши
        #endregion Поля

        #region Свойства
        public bool SkipMovementAnimation { get => _skipMovementAnimation; set => _skipMovementAnimation = value; }//Пропустить анимацию движения
        #endregion Свойства

        #region Методы UNITY
        void Awake()
        {
            if (!_instance)
                _instance = this;
            else
                Debug.Log("<color=orange>MG_GameSettings Awake(): найдет лишний _instance класса MG_GameSettings.</color>");
        }

        void Start()
        {
            Application.targetFrameRate = 120; // Make the game run as fast as possible       
            QualitySettings.vSyncCount = 0;// Turn off v-sync
        }

        #endregion Методы UNITY


        #region Личные методы
        [BoxGroup("MG_CameraWheelZoomer"), Button]
        /// <summary>
        /// Обновить значения зум камеры
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void UpdateCameraWheelZoom(float min = 3f, float max = 35f)
        {
            MG_CameraWheelZoomer.SetWheelZoom(min, max);
        }

        #endregion Личные методы
    }
}