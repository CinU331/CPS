using System.Collections.Generic;

namespace CPS
{
    class Utilities
    {
        public static ComplexNumber[] ConvertRealToComplex(List<KeyValuePair<double, double>> values)
        {
            ComplexNumber[] complexNumbers = new ComplexNumber[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                complexNumbers[i] = new ComplexNumber(values[i].Value, 0.0);
            }
            return complexNumbers;
        }

        public static List<KeyValuePair<double, double>> ConvertComplexTableToKeyValuePair(ComplexNumber[] complexNumbers, List<KeyValuePair<double,double>> values, bool real)
        {
            List<KeyValuePair<double, double>> result = new List<KeyValuePair<double, double>>();
            if(real)
            {
                for (int i = 0; i < complexNumbers.Length; i++)
                {
                    result.Add(new KeyValuePair<double, double>(values[i].Key, complexNumbers[i].RealPart));
                }
                return result;
            }
            else
            {
                for (int i = 0; i < complexNumbers.Length; i++)
                {
                    result.Add(new KeyValuePair<double, double>(values[i].Key, complexNumbers[i].ImaginaryPart));
                }
                return result;
            }
        }
    }
}
