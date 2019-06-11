using System;

namespace CPS
{
    class Fourier
    {
        public static ComplexNumber[] DFT(ComplexNumber[] complexNumbers)
        {
            int N = complexNumbers.Length;
            ComplexNumber[] complexNumbers2 = new ComplexNumber[N];

            for (int k = 0; k < N; k++)
            {
                complexNumbers2[k] = new ComplexNumber(0, 0);
                for (int n = 0; n < N; n++)
                {
                    ComplexNumber tmp = ComplexNumber.ConvertFromPolarToRectangular(1, -2 * Math.PI * n * k / N);
                    tmp *= complexNumbers[n];
                    complexNumbers2[k] += tmp;
                }
            }
            return complexNumbers2;
        }

        public static ComplexNumber[] DIT_FFT(ComplexNumber[] complexNumbers)
        {
            int N = complexNumbers.Length;
            ComplexNumber[] complexNumbers2 = new ComplexNumber[N];
            ComplexNumber[] x, X, y, Y;

            if (N == 1)
            {
                complexNumbers2[0] = complexNumbers[0];
                return complexNumbers2;
            }

            int k;
            y = new ComplexNumber[N / 2];
            x = new ComplexNumber[N / 2];
            for (k = 0; k < N / 2; k++)
            {
                y[k] = complexNumbers[2 * k];
                x[k] = complexNumbers[2 * k + 1];
            }

            X = DIT_FFT(x);
            Y = DIT_FFT(y);

            for (k = 0; k < N / 2; k++)
            {
                ComplexNumber tmp = ComplexNumber.ConvertFromPolarToRectangular(1, -2 * Math.PI * k / N);
                X[k] *= tmp;
            }
            for (k = 0; k < N / 2; k++)
            {
                complexNumbers2[k] = Y[k] + X[k];
                complexNumbers2[k + N / 2] = Y[k] - X[k];
            }
            return complexNumbers2;
        }

        public static ComplexNumber[] DIF_FFT(ComplexNumber[] complexNumbers)
        {
            int N = complexNumbers.Length;
            ComplexNumber[] complexNumbers2 = new ComplexNumber[N];
            ComplexNumber[] x, X, y, Y;

            if (N == 1)
            {
                complexNumbers2[0] = complexNumbers[0];
                return complexNumbers2;
            }

            int k;
            y = new ComplexNumber[N / 2];
            x = new ComplexNumber[N / 2];
            for (k = 0; k < N / 2; k++)
            {
                y[k] = complexNumbers[2 * k];
                x[k] = complexNumbers[2 * k + 1];
            }

            X = DIF_FFT(x);
            Y = DIF_FFT(y);

            for (k = 0; k < N / 2; k++)
            {
                complexNumbers2[k] = Y[k] + X[k];
                complexNumbers2[k + N / 2] = Y[k] - X[k];
            }
            for (k = 0; k < N / 2; k++)
            {
                ComplexNumber tmp = ComplexNumber.ConvertFromPolarToRectangular(1, -2 * Math.PI * k / N);
                complexNumbers2[k] *= tmp;
            }
            return complexNumbers2;
        }
    }
}
