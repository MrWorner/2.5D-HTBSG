
namespace MG_StrategyGame
{
public enum BorderDirection //направление границы
{ 
    None, A, B, C, D, E, F, AB, AC, AD, AE, AF, BC, BD, BE, BF, CD, CE, CF, DE, DF, EF, ABC, ABD, ABE, ABF, ACD, ACE, ACF, 
    ADE, ADF, AEF, BCD, BCE, BCF, BDE, BDF, BEF, CDE, CDF, CEF, DEF, CDEF, BDEF, BCEF, BCDF, BCDE, ADEF, ACEF, ACDF, ACDE, 
    ABEF, ABDF, ABDE, ABCF, ABCE, ABCD, BCDEF, ACDEF, ABDEF, ABCEF, ABCDF, ABCDE, ABCDEF 
}
}

//public enum BorderOneSide { A, B, C, D, E, F }//граница
//public enum BorderTwoSides { AB, AC, AD, AE, AF, BC, BD, BE, BF, CD, CE, CF, DE, DF, EF }//граница
//public enum BorderThreeSides { ABC, ABD, ABE, ABF, ACD, ACE, ACF, ADE, ADF, AEF, BCD, BCE, BCF, BDE, BDF, BEF, CDE, CDF, CEF, DEF }//граница
//public enum BorderFourSides { CDEF, BDEF, BCEF, BCDF, BCDE, ADEF, ACEF, ACDF, ACDE, ABEF, ABDF, ABDE, ABCF, ABCE, ABCD }//граница
//public enum BorderFiveSides { BCDEF, ACDEF, ABDEF, ABCEF, ABCDF, ABCDE }//граница
//public enum BorderSixSides { ABCDEF }//граница
//public enum BorderZeroSides { None }//граница