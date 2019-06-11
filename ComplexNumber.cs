using System;

namespace CPS
{
    class ComplexNumber
    {
        public double RealPart { get; set; }
        public double ImaginaryPart { get; set; }
        public ComplexNumber() { }

        public ComplexNumber(double realPart, double imaginaryPart)
        {
            this.RealPart = realPart;
            this.ImaginaryPart = imaginaryPart;
        }

        public static ComplexNumber ConvertFromPolarToRectangular(double r, double radians)
        {
            ComplexNumber data = new ComplexNumber(r * Math.Cos(radians), r * Math.Sin(radians));
            return data;
        }

        public static ComplexNumber operator +(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber(x.RealPart + y.RealPart, x.ImaginaryPart + y.ImaginaryPart);
            return data;
        }
        public static ComplexNumber operator -(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber(x.RealPart - y.RealPart, x.ImaginaryPart - y.ImaginaryPart);
            return data;
        }
        public static ComplexNumber operator *(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber((x.RealPart * y.RealPart) - (x.ImaginaryPart * y.ImaginaryPart),
                (x.RealPart * y.ImaginaryPart + (x.ImaginaryPart + y.RealPart)));
            return data;
        }
    }
}
