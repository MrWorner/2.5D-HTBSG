using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----СОЗДАЛ ДЛЯ ТОГО ЧТОБЫ БЫСТРО СРАВНИВАТЬ УНИКАЛЬНЫЕ ОБЪЕКТЫ, а не по ссылку объектов стандартным Equals(). ДЛЯ УЛУЧШЕНИЯ ПРОИЗВОДИТЕЛЬНОСТИ!
//-----Либо HashCode использовать, чтобы не было что за предел вышел Тип uint. https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
//-----https://www.dotnetperls.com/int

namespace MG_StrategyGame
{
public interface IEquatableUID<T>
{
    //uint 	0 to 4,294,967,295 	Unsigned 32-bit integer 	System.UInt32
    //void GenerateID();//Сгенерировать уникальный номер объекта
    bool EqualsUID(T other);//одинаковый ли ID  
}
}
