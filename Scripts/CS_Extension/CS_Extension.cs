/////////////////////////////////////////////////////////////////////////////////
//
//	CS_Extension.cs
//	Автор: MaximGodyna
//  GitHub: https://github.com/MrWorner
//	
//	Описание: IndexOf отсутстует у списка, который доступен только для чтения.
//	          Поэтому необходимо данное решение.				
//			   
//
/////////////////////////////////////////////////////////////////////////////////

//https://stackoverflow.com/questions/37431844/why-ireadonlycollection-has-elementat-but-not-indexof
using Collections = System.Collections.Generic;

namespace MG_StrategyGame
{
public static class CS_Extension
{
    /// <summary>
    /// Получить номер под которым храниться элемент в списке
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="elementToFind"></param>
    /// <returns></returns>
    public static int IndexOf<T>(this Collections.IReadOnlyList<T> self, T elementToFind)
    {
        int i = 0;
        foreach (T element in self)
        {
            if (Equals(element, elementToFind))
                return i;
            i++;
        }
        return -1;
    }
}
}
