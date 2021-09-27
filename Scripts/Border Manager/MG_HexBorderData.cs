using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MG_StrategyGame
{
    public class MG_HexBorderData : IVisibility, IMarkable
    {
        #region Поля
        private MG_HexCell _cell;//[R CONSTRUCTOR]
        private MG_HexBorder _borderType;//[R CONSTRUCTOR] тип границ
        private MG_Player _owner;//[R CONSTRUCTOR] владелец (сторона)
        private ICellObserver _regionOwner;//[R CONSTRUCTOR] владелец (регион). Кому именно территория принадлежить: городу, мобильному посту и тд.

        private Tilemap _borderTileMap;//[R CONSTRUCTOR] объект стандартной TileMap (карта границ)
        private Vector3Int _pos;//[R CONSTRUCTOR]
        private MG_LineOfSight _lineOfSight;//[R CONSTRUCTOR] модуль зоны видимости
        #endregion Поля

        #region Свойства
        public MG_HexBorder BorderType { get => _borderType; set => _borderType = value; }//[R CONSTRUCTOR] тип границ
        public MG_Player Owner { get => _owner; }//[R CONSTRUCTOR] владелец
        public ICellObserver RegionOwner { get => _regionOwner; }//[R CONSTRUCTOR] владелец (регион). Кому именно территория принадлежить: городу, мобильному посту и тд.

        #endregion Свойства

        #region ACTIONS
        public Action<MG_HexCell> Erase { get; set; }//Очищение от текущего владельца
        #endregion ACTIONS

        #region Конструктор
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="pos">позиция</param>
        public MG_HexBorderData(MG_HexCell cell)
        {
            MG_HexBorderManager borderManager = MG_HexBorderManager.Instance;
            if (borderManager == null) Debug.Log("<color=red>MG_HexBorderData MG_HexBorderData(): '_borderManager' не задан!</color>");

            this._borderTileMap = borderManager.BorderTileMap;
            this._pos = cell.Pos;
            this._cell = cell;
            _borderType = MG_HexBorderLibrary.GetEmptyBorder();
            _lineOfSight = MG_LineOfSight.Instance;

            SetOwner(MG_PlayerManager.GetEmptySide());//// Установить хозяина.

            if (_borderTileMap == null) Debug.Log("<color=red>MG_HexBorderData MG_HexBorderData(): 'borderTileMap' не задан!</color>");
            if (_borderType == null) Debug.Log("<color=red>MG_HexBorderData MG_HexBorderData(): 'borderType' не задан!</color>");
            if (_owner == null) Debug.Log("<color=red>MG_HexBorderData MG_HexBorderData(): 'owner' не задан!</color>");
        }
        #endregion Конструктор

        #region Публичные методы
        /// <summary>
        /// Установить хозяина.
        /// </summary>
        /// <param name="owner"></param>
        public void SetOwner(ICellObserver owner)
        {
            _regionOwner = owner;// владелец (регион). Кому именно территория принадлежить: городу, мобильному посту и тд.
            _owner = owner.GetSide();//владелец (сторона)
        }

        /// <summary>
        /// Очистить от владельца
        /// </summary>
        public void OnErase()
        {
            MG_Player emptySide = MG_PlayerManager.GetEmptySide();
            SetOwner(emptySide);
            Erase?.Invoke(_cell);//вызвать Действие после удаления клетки
        }

        /// <summary>
        /// Имеет ли владельца
        /// </summary>
        /// <returns></returns>
        public bool HasOwner()
        {
            MG_Player emptySide = MG_PlayerManager.GetEmptySide();
            return (_owner == null || emptySide.Equals(_owner)) ? false : true;
        }
        #endregion Публичные методы

        #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
        /// <summary>
        /// Установить видимость (IVisibility)
        /// </summary>
        /// <param name="visibility">видимость</param>
        public void SetVisibility(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.BlackFog:
                    Mark(Color.clear);
                    break;
                case Visibility.GreyFog:
                    Mark(_lineOfSight.GreyFogOfWar);
                    break;
                case Visibility.Visible:
                    UnMark();
                    break;
                default:
                    Debug.Log("<color=orange>MG_HexBorderData SetVisibility(): switch DEFAULT.</color>");
                    break;
            }
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"

        #region МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
        /// <summary>
        /// пометить клетку цветом (IMarkable)
        /// </summary>
        /// <param name="color">Цвет</param>
        public void Mark(Color color)
        {
            _borderTileMap.SetColor(_pos, color);
        }

        /// <summary>
        /// Сбросить выделение (IMarkable)
        /// </summary>
        public void UnMark()
        {
            _borderTileMap.SetColor(_pos, Owner.Color);
            //throw new System.NotImplementedException();
        }
        #endregion МЕТОДЫ ИНТЕРФЕЙСА "IMarkable"
    }
}
