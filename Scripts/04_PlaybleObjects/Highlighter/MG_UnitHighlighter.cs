using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public class MG_UnitHighlighter : MonoBehaviour, IVisibility
{
    #region Поля
    private Coroutine _rotateCoroutine;//короутин поворачивания
    private Coroutine _glowCoroutine;//короутин свечения
    [SerializeField] private Visibility _currentVisibility = Visibility.Visible;//Текущая видимость
    #endregion Поля

    #region Поля: необходимые модули
    [Required("Должен быть задан в префабе!", InfoMessageType.Error), SerializeField] private SpriteRenderer _spriteRenderer;//[R] рендер спрайта
    [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private MG_UnitHighlighterSettings _settings;//настройки
    [Required("НЕ ИНИЦИАЛИЗИРОВАН В Init()!", InfoMessageType.Error), SerializeField] private MG_Player _side;//сторона
    #endregion Поля: необходимые модули

    #region Методы UNITY
    private void Awake()
    {
        if (_spriteRenderer == null)
            Debug.Log("<color=red>MG_UnitHighlighter Awake(): 'spriteRenderer' не прикреплен!</color>");
    }
    #endregion Методы UNITY

    #region Метод Init(MG_Player side)
    /// <summary>
    /// Инициализация
    /// </summary>
    public void Init(MG_Player side)
    {
        _settings = MG_UnitHighlighterSettings.GetInstance();
        if (_settings == null)
            Debug.Log("<color=red>MG_UnitHighlighter Init(): MG_UnitHighlighterSettings.Instance не найден!</color>");
        
        this._side = side;
        SetColor(side.Color);
    }
    #endregion Метод Init(MG_Player side)

    #region Публичные методы
    /// <summary>
    /// При отмены выделения
    /// </summary>
    public void OnDeselect()
    {
        if (_rotateCoroutine != null)
        {
            StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = null;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        if (_glowCoroutine != null)
        {
            StopCoroutine(_glowCoroutine);
            _glowCoroutine = null;
        }

        SetColor(_side.Color);
    }

    /// <summary>
    /// При выделении
    /// </summary>
    /// <param name="isClicked">кликнут</param>
    public void OnSelect(bool isClicked)
    {
        if (isClicked)
        {
            //pulseCoroutine = StartCoroutine(Pulse(1.0f, 0.5f, 0.75f));
            _rotateCoroutine = StartCoroutine(Rotate(3.0f));
            _glowCoroutine = StartCoroutine(Glow(_side.Color, Color.cyan, 1.0f));
            SetColor(_settings.Color_SelectedFriendly);
        }
        else
        {
            _rotateCoroutine = StartCoroutine(Rotate(3.0f));
            _glowCoroutine = StartCoroutine(Glow(_settings.Color_Enemy, Color.yellow, 0.15f));
            SetColor(_settings.Color_Enemy);
        }

    }

    /// <summary>
    /// Сменить сторону
    /// </summary>
    /// <param name="side">сторона</param>
    public void ChangeSide(MG_Player side)
    {
        this._side = side;
        SetColor(side.Color);
    }
    #endregion Публичные методы

    #region Личные методы
    /// <summary>
    /// Анимация: свещение
    /// </summary>
    /// <param name="color">цвет 1</param>
    /// <param name="color2">цвет 2</param>
    /// <param name="cooloutTime">время</param>
    /// <returns></returns>
    private IEnumerator Glow(Color color, Color color2, float cooloutTime)
    {
        float startTime = Time.time;
        bool reverse = false;

        while (true)
        {
            var currentTime = Time.time;
            if (startTime + cooloutTime < currentTime)
            {
                reverse = !reverse;
                startTime = Time.time;
            }

            if (reverse)
            {
                _spriteRenderer.color = Color.Lerp(color, color2, (startTime + cooloutTime) - currentTime);
            }
            else
            {
                _spriteRenderer.color = Color.Lerp(color2, color, (startTime + cooloutTime) - currentTime);
            }

            yield return 0;
        }

    }

    /// <summary>
    ///  Анимация: пулсировать
    /// </summary>
    /// <param name="breakTime">время остановки</param>
    /// <param name="delay">задержка</param>
    /// <param name="scaleFactor">коэфицент размера</param>
    /// <returns></returns>
    private IEnumerator Pulse(float breakTime, float delay, float scaleFactor)
    {
        var baseScale = transform.localScale;
        while (true)
        {
            float time1 = Time.time;
            while (time1 + delay > Time.time)
            {
                transform.localScale = Vector3.Lerp(baseScale * scaleFactor, baseScale, (time1 + delay) - Time.time);
                yield return 0;
            }

            float time2 = Time.time;
            while (time2 + delay > Time.time)
            {
                transform.localScale = Vector3.Lerp(baseScale, baseScale * scaleFactor, (time2 + delay) - Time.time);
                yield return 0;
            }

            yield return new WaitForSeconds(breakTime);
        }
    }

    /// <summary>
    /// Анимация: вращение
    /// </summary>
    /// <param name="duration">продолжительность</param>
    /// <returns></returns>
    private IEnumerator Rotate(float duration)
    {
        float startRotation = 0f;
        float yRotation = 0f;
        float saved = transform.eulerAngles.z;
        float t = 0.0f;
        while (true)
        {
            t += Time.deltaTime;
            yRotation = Mathf.Lerp(startRotation, 360, t / duration) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,
            yRotation);
            if (yRotation == 0)
                t = 0f;
            yield return new WaitForSeconds(0);
        }
    }

    /// <summary>
    /// Установить цвет
    /// </summary>
    /// <param name="color">цвет игрока</param>
    private void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }
    #endregion Личные методы

    #region МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
    /// <summary>
    /// Установить видимость (IVisibility)
    /// </summary>
    /// <param name="visibility">видимость</param>
    public void SetVisibility(Visibility visibility)
    {
        //Debug.Log("SetVisibility AFTER: " + visibility + " current: " + _currentVisibility + " _side:" + _side);
        switch (visibility)
        {
            case Visibility.BlackFog:
                if (MG_VisibilityChecker.IsGreyFog(_currentVisibility))
                {
                    //++++++++++++++++_spriteRenderer.color = new Color(1, 1, 1, 1);//Отбеливаем после серого. Иначе перейдем потом на видимость, а он будет серым.
                    //++++++++++++++++_spriteRenderer.enabled = false;//отключаем рендер спрайта, чтобы был невидим                   
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsVisible(_currentVisibility))
                {

                    _spriteRenderer.enabled = false;//отключаем рендер спрайта, чтобы был невидим
                    this._currentVisibility = visibility;
                }
                break;
            case Visibility.GreyFog:
                if (_currentVisibility.Equals(Visibility.BlackFog))
                {
                    //++++++++++_spriteRenderer.enabled = true;//включаем рендер спрайта, чтобы был видим
                    //++++++++++_spriteRenderer.color = MG_LineOfSight.Instance.GreyFogOfWar;
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsVisible(_currentVisibility))
                {
                    //++++++++++++++_spriteRenderer.color = MG_LineOfSight.Instance.GreyFogOfWar;
                    _spriteRenderer.enabled = false;//+++++++++++++++++отключаем рендер спрайта, чтобы был невидим
                    this._currentVisibility = visibility;
                }
                break;
            case Visibility.Visible:
                if (MG_VisibilityChecker.IsBlackFog(_currentVisibility))
                {
                    _spriteRenderer.enabled = true;//включаем рендер спрайта, чтобы был видим
                    this._currentVisibility = visibility;
                }
                else if (MG_VisibilityChecker.IsGreyFog(_currentVisibility))
                {
                    _spriteRenderer.enabled = true;//+++++++++++++++++включаем рендер спрайта, чтобы был видим
                    //+++++++++++++++++++++_spriteRenderer.color = new Color(1, 1, 1, 1);
                    this._currentVisibility = visibility;
                }
                break;
            default:
                Debug.Log("<color=orange>MG_UnitHighlighter SetVisibility(): switch DEFAULT.</color>");
                break;
        }

    }
    #endregion МЕТОДЫ ИНТЕРФЕЙСА "IVisibility"
}
}
