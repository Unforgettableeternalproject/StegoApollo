using System;

namespace StegoLib.Services
{
    public static class DctUtils
    {
        public static double[,] ForwardDct(double[,] mat, int bx, int by)
        {
            // 實作 8x8 DCT
            int N = 8;
            double[,] F = new double[N, N];
            for (int u = 0; u < N; u++)
            {
                for (int v = 0; v < N; v++)
                {
                    double sum = 0;
                    for (int x = 0; x < N; x++)
                        for (int y = 0; y < N; y++)
                        {
                            sum += mat[by + y, bx + x]
                                * Math.Cos((2 * x + 1) * u * Math.PI / (2 * N))
                                * Math.Cos((2 * y + 1) * v * Math.PI / (2 * N));
                        }
                    double cu = u == 0 ? 1 / Math.Sqrt(2) : 1;
                    double cv = v == 0 ? 1 / Math.Sqrt(2) : 1;
                    F[u, v] = 0.25 * cu * cv * sum;
                }
            }
            return F;
        }

        public static void InverseDct(double[,] block, double[,] mat, int bx, int by)
        {
            int N = 8;
            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    double sum = 0;
                    for (int u = 0; u < N; u++)
                        for (int v = 0; v < N; v++)
                        {
                            double cu = u == 0 ? 1 / Math.Sqrt(2) : 1;
                            double cv = v == 0 ? 1 / Math.Sqrt(2) : 1;
                            sum += cu * cv * block[u, v]
                                * Math.Cos((2 * x + 1) * u * Math.PI / (2 * N))
                                * Math.Cos((2 * y + 1) * v * Math.PI / (2 * N));
                        }
                    mat[by + y, bx + x] = 0.25 * sum;
                }
            }
        }
    }
}