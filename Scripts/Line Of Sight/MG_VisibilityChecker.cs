using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MG_StrategyGame
{
public static class MG_VisibilityChecker
{
    #region Публичные статические методы
    /// <summary>
    /// Является ли текущая видимость видимой
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <returns></returns>
    public static bool IsVisible(Visibility visibility)
    {
        return visibility.Equals(Visibility.Visible);
    }

    /// <summary>
    /// Является ли текущая видимость серым туманом
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <returns></returns>
    public static bool IsGreyFog(Visibility visibility)
    {
        return visibility.Equals(Visibility.GreyFog);
    }

    /// <summary>
    /// Является ли текущая видимость черным туманом
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <returns></returns>
    public static bool IsBlackFog(Visibility visibility)
    {
        return visibility.Equals(Visibility.BlackFog);
    }

    #endregion Публичные статические методы

    /// <summary>
    /// Является ли текущая видимость видимой для данной стороны
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <param name="side"></param>
    /// <returns></returns>
    //public static bool IsVisibleFor(Visibility visibility, MG_Player _side)
    //{

    //    return visibility.Equals(Visibility.Visible);
    //}

    /// <summary>
    /// Является ли текущая видимость серым туманом для данной стороны
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <param name="side"></param>
    /// <returns></returns>
    //public static bool IsGreyFogFor(Visibility visibility, MG_Player _side)
    //{
    //    return visibility.Equals(Visibility.GreyFog);
    //}

    /// <summary>
    /// Является ли текущая видимость черным туманом для данной стороны
    /// </summary>
    /// <param name="visibility">видимость</param>
    /// <param name="side"></param>
    /// <returns></returns>
    //public static bool IsBlackFogFor(Visibility visibility, MG_Player _side)
    //{
    //    return visibility.Equals(Visibility.BlackFog);
    //}

}
}
