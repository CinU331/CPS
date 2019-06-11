using System;

namespace CPS
{
    class ComplexNumber
    {
        private double realPart;
        private double imaginaryPart;

        public ComplexNumber(){}

        public ComplexNumber(double realPart, double imaginaryPart)
        {
            this.realPart = realPart;
            this.imaginaryPart = imaginaryPart;
        }

        public static ComplexNumber ConvertFromPolarToRectangular(double r, double radians)
        {
            ComplexNumber data = new ComplexNumber(r * Math.Cos(radians), r * Math.Sin(radians));
            return data;
        }

        public static ComplexNumber operator +(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber(x.realPart + y.realPart, x.imaginaryPart + y.imaginaryPart);
            return data;
        }
        public static ComplexNumber operator -(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber(x.realPart - y.realPart, x.imaginaryPart - y.imaginaryPart);
            return data;
        }
        public static ComplexNumber operator *(ComplexNumber x, ComplexNumber y)
        {
            ComplexNumber data = new ComplexNumber((x.realPart * y.realPart) - (x.imaginaryPart * y.imaginaryPart),
                (x.realPart * y.imaginaryPart + (x.imaginaryPart + y.realPart)));
            return data;
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(Math.Pow(realPart, 2) + Math.Pow(imaginaryPart, 2));
            }
        }

        public double Phase
        {
            get
            {
                return Math.Atan(imaginaryPart / realPart);
            }
        }
    }
}
