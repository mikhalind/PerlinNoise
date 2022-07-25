using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PerlinNoise
{
    struct Vector3D
    {
        public double x;
        public double y;
        public double z;
        public Vector3D(double a, double b, double c) { x = a; y = b; z = c; }
        public override string ToString() => $"{x:F2}; {y:F2}; {z:F2}";
    }

    public class Noise3D
    {
        private Smooth smooth = null;
        private Vector3D[,,] grid;
        private readonly int txdWidth;
        private readonly int txdHeight;
        private readonly int txdDepth;

        public Noise3D(int side, int frames)
        {
            txdWidth = side;
            txdHeight = side;
            txdDepth = frames;
        }

        private double MultiplyVectors(Vector3D v1, Vector3D v2) => v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        private double Interpolate(double a, double b, double weight) => a + smooth(weight) * (b - a);

        public double[,,] GeneratorManager(List<Tuple<int, int>> layers, Smooth method, IProgress<int> progress)
        {
            smooth = method;
            int totalFrames = (layers.Count * txdDepth);
            double[,,] map = new double[txdHeight, txdWidth, txdDepth];
            for (int a = 0; a < layers.Count; a++)
            {
                int dDim = txdDepth / 15 + 1;
                GenerateGrid(layers[a].Item1, dDim + 1);
                int dim = grid.GetLength(0) - 1;                
                for (int d = 0; d < txdDepth; d++)
                {
                    for (int i = 0; i < txdHeight; i++)
                    {
                        for (int j = 0; j < txdWidth; j++)
                        {
                            double noizedValue = GetNoise((double)j * dim / txdHeight,
                                                          (double)i * dim / txdWidth,
                                                          (double)d * dDim / txdDepth);
                            map[i, j, d] += noizedValue / (Math.Pow(2, layers[a].Item2));
                        }
                    }
                    var va = (int)Math.Ceiling((double)(a * txdDepth + d) * 100 / totalFrames);
                    progress?.Report(va);
                }
            }
            return map;
        }

        private double GetNoise(double x, double y, double z)
        {
            int x0 = (int)Math.Floor(x);
            double dx = x - x0;

            int y0 = (int)Math.Floor(y);
            double dy = y - y0;

            int z0 = (int)Math.Floor(z);
            double dz = z - z0;

            Vector3D vx0y0z0 = grid[x0, y0, z0];
            Vector3D vx1y0z0 = grid[x0 + 1, y0, z0];
            Vector3D vx0y1z0 = grid[x0, y0 + 1, z0];
            Vector3D vx1y1z0 = grid[x0 + 1, y0 + 1, z0];

            Vector3D vx0y0z1 = grid[x0, y0, z0 + 1];
            Vector3D vx1y0z1 = grid[x0 + 1, y0, z0 + 1];
            Vector3D vx0y1z1 = grid[x0, y0 + 1, z0 + 1];
            Vector3D vx1y1z1 = grid[x0 + 1, y0 + 1, z0 + 1];

            var mp000 = MultiplyVectors(vx0y0z0, new Vector3D(dx, dy, dz));
            var mp100 = MultiplyVectors(vx1y0z0, new Vector3D(dx - 1, dy, dz));
            var mp010 = MultiplyVectors(vx0y1z0, new Vector3D(dx, dy - 1, dz));
            var mp110 = MultiplyVectors(vx1y1z0, new Vector3D(dx - 1, dy - 1, dz));

            var mp001 = MultiplyVectors(vx0y0z1, new Vector3D(dx, dy, dz - 1));
            var mp101 = MultiplyVectors(vx1y0z1, new Vector3D(dx - 1, dy, dz - 1));
            var mp011 = MultiplyVectors(vx0y1z1, new Vector3D(dx, dy - 1, dz - 1));
            var mp111 = MultiplyVectors(vx1y1z1, new Vector3D(dx - 1, dy - 1, dz - 1));

            var a = Interpolate(mp000, mp010, dy);
            var b = Interpolate(mp100, mp110, dy);
            var c = Interpolate(mp001, mp011, dy);
            var d = Interpolate(mp101, mp111, dy);

            var ab = Interpolate(a, b, dx);
            var cd = Interpolate(c, d, dx);

            var abcd = Interpolate(ab, cd, dz);

            return abcd;
        }

        private void GenerateGrid(int dim, int depth)
        {
            Random rand = new Random();
            grid = new Vector3D[dim, dim, depth];
            for (int d = 0; d < depth; d++)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        double alpha = rand.NextDouble() * 2 * Math.PI;
                        double beta = rand.NextDouble() * 2 * Math.PI;
                        Vector3D v = new Vector3D(Math.Cos(alpha) * Math.Cos(beta),
                                                  -Math.Sin(beta),
                                                  Math.Sin(alpha) * Math.Cos(beta));
                        grid[i, j, d] = v;
                    }
                }
            }
        }

        public WriteableBitmap GetImageByID(double[,,] map, int depthFrame)
        {
            int txdWidth = map.GetLength(0);
            int txdHeight = map.GetLength(1);
            WriteableBitmap bmp = new WriteableBitmap(txdWidth, txdHeight, 72, 72, PixelFormats.Bgra32, null);
            for (int i = 0; i < txdHeight; i++)
            {
                for (int j = 0; j < txdWidth; j++)
                {
                    double alpha = (map[i, j, depthFrame] + 1) / 2 * 255;
                    byte[] color = { (byte)alpha, (byte)alpha, (byte)alpha, 255 };
                    Int32Rect rect = new Int32Rect(i, j, 1, 1);
                    bmp.WritePixels(rect, color, 4, 0);
                }
            }
            return bmp;
        }

        public List<WriteableBitmap> GetListOfImages(double[,,] map, IProgress<int> progress)
        {
            List<WriteableBitmap> imagesList = new List<WriteableBitmap>();
            for (int i = 0; i < txdDepth; i++)
            {
                var pic = GetImageByID(map, i);
                pic.Freeze();
                imagesList.Add(pic);
                progress?.Report((i + 1) * 100 / txdDepth);
            }
            return imagesList;
        }
    }
}
