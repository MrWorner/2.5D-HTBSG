/////////////////////////////////////////////////////////////////////////////////
//
//	MG_Game.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание:	Главный класс, который запускает все необходимы модули в 	    				
//			    правильной последовательности.
//
/////////////////////////////////////////////////////////////////////////////////

using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MG_StrategyGame
{
    public class MG_Game : MonoBehaviour
    {
        private static MG_Game _instance;

        #region Поля: необходимые модули
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_MapGenerator _mapGenerator;//[R] генератор карты
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_TurnManager _turnManager;//[R] менеджер ходов
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_FogOfWar _fogOfWar;//[R] менеджер тумана
        [PropertyOrder(-1), BoxGroup("ТРЕБОВАНИЯ"), Required(InfoMessageType.Error), SerializeField] private MG_GlobalHexMapLoader _globalHexMapLoader;//[R] загрузчик карты
        #endregion Поля: необходимые модули

        #region Поля
        [BoxGroup("Настройки"), Required(InfoMessageType.Error), SerializeField] private bool _loadExistingMap = false;
        #endregion Поля

        #region Свойства

        #endregion Свойства

        #region Методы UNITY
        private void Awake()
        {
#if UNITY_EDITOR//https://youtu.be/KErkmxbkBs8?t=404    5 ways to make your unity3d code faster (Jason Weimann)
            Debug.unityLogger.logEnabled = true;
#else
        //ОТКЛЮЧИТЬ 
        Debug.unityLogger.logEnabled = false;
#endif

            if (_instance == null)
                _instance = this;
            else
                Debug.Log("<color=red>MG_Game Awake(): найден лишний MG_Game!</color>");

            if (_mapGenerator == null) Debug.Log("<color=red>MG_Game Awake(): '_mapGenerator' не задан!</color>");
            if (_turnManager == null) Debug.Log("<color=red>MG_Game Awake(): '_turnManager' не задан!</color>");
            if (_fogOfWar == null) Debug.Log("<color=red>MG_Game Awake(): '_fogOfWar' не задан!</color>");
            if (_globalHexMapLoader == null) Debug.Log("<color=red>MG_Game Awake(): '_globalHexMapLoader' не задан!</color>");
        }

        private void Start()
        {
            //---ШАГ 1: Сгенерировать карту ИЛИ загрузить карту
            if (_loadExistingMap)
                _globalHexMapLoader.Load();
            else
                _mapGenerator.GenerateEmptyMap();

            //---ШАГ 2: Активировать туман войны
            _fogOfWar.PlaceVisualFogOfWar();

            //---ШАГ 3: Заполнить тестовыми объектами карту (ВРЕМЕННЫЙ ТЕСТ)
            MG_TestTBSframework._instance.StartNow();

            //---ШАГ 4: Активировать менеджер ходов
            _turnManager.Activate();
        }
        #endregion Методы UNITY
    }
}