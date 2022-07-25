using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PerlinNoise
{
    public delegate double Smooth(double t);

    public partial class MainWindow : Window
    {
        Noise3D Noise { get; set; }
        public static Smooth smooth = null;
        public double NoSmoothing(double t) => t;
        public double SmoothStep(double t) => 3 * Math.Pow(t, 2) - 2 * Math.Pow(t, 3);
        public double SmootherStep(double t) => 6 * Math.Pow(t, 5) - 15 * Math.Pow(t, 4) + 10 * Math.Pow(t, 3);
        public double Smooth7Step(double t) => -20 * Math.Pow(t, 7) + 70 * Math.Pow(t, 6) - 84 * Math.Pow(t, 5) + 35 * Math.Pow(t, 4);
        public double Smooth9Step(double t) => 70 * Math.Pow(t, 9) - 315 * Math.Pow(t, 8) + 540 * Math.Pow(t, 7) - 420 * Math.Pow(t, 6) + 126 * Math.Pow(t, 5);
        public double Sinusoid(double t) => 0.5 * Math.Sin(Math.PI * (t - 0.5)) + 0.5;

        public MainWindow()
        {
            InitializeComponent();
            smoothingBox.ItemsSource = new List<string>() { "No smoothing",
                                                            "Sinusoid",
                                                            "Smooth-step",
                                                            "Smoother-step ",
                                                            "7th-order equation", 
                                                            "9th-order equation" };
            smoothingBox.SelectedIndex = 3;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                List<Tuple<int, int>> list = new List<Tuple<int, int>>();
                if (Pers1Slider.Value != 6) list.Add(new Tuple<int, int>(Convert.ToInt32(Freq1Slider.Value), Convert.ToInt32(Pers1Slider.Value)));
                if (Pers2Slider.Value != 6) list.Add(new Tuple<int, int>(Convert.ToInt32(Freq2Slider.Value), Convert.ToInt32(Pers2Slider.Value)));
                if (Pers3Slider.Value != 6) list.Add(new Tuple<int, int>(Convert.ToInt32(Freq3Slider.Value), Convert.ToInt32(Pers3Slider.Value)));
                if (Pers4Slider.Value != 6) list.Add(new Tuple<int, int>(Convert.ToInt32(Freq4Slider.Value), Convert.ToInt32(Pers4Slider.Value)));
                if (Pers5Slider.Value != 6) list.Add(new Tuple<int, int>(Convert.ToInt32(Freq5Slider.Value), Convert.ToInt32(Pers5Slider.Value)));

                switch (smoothingBox.SelectedIndex)
                {
                    case 0: smooth = NoSmoothing; break;
                    case 1: smooth = Sinusoid; break;
                    case 2: smooth = SmoothStep; break;
                    case 3: smooth = SmootherStep; break;
                    case 4: smooth = Smooth7Step; break;
                    default: smooth = Smooth9Step; break;
                }

                Noise = new Noise3D(128, 15);

                var progress = new Progress<int>(report => generateBar.Value = report);
                double[,,] result = await Task.Run(() => Noise.GeneratorManager(list, smooth, progress));

                var progress2 = new Progress<int>(report => generateBar.Value = 100 + report);
                var images = await Task.Run(() => Noise.GetListOfImages(result, progress2));
                perlinImg.Source = images[2];

                /*
                for (int i = 0; i < images.Count; i++)
                {
                    string ph = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Noise", "Gifs", $"{i + 1}.png");
                    using (FileStream stream = new FileStream(ph, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(images[i]));
                        encoder.Save(stream);
                    }
                }
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).Y <= 30) DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Freq1Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => f1Block.Text = Convert.ToInt32(e.NewValue).ToString();
        private void Freq2Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => f2Block.Text = Convert.ToInt32(e.NewValue).ToString();        
        private void Freq3Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => f3Block.Text = Convert.ToInt32(e.NewValue).ToString();        
        private void Freq4Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => f4Block.Text = Convert.ToInt32(e.NewValue).ToString();
        private void Freq5Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => f5Block.Text = Convert.ToInt32(e.NewValue).ToString();

        private void Pers1Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => 
            p1Block.Text = (e.NewValue == 6)? "0" : (e.NewValue == 0) ? "1" : $"1/{Convert.ToInt32(Math.Pow(2, e.NewValue))}";

        private void Pers2Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            p2Block.Text = (e.NewValue == 6) ? "0" : (e.NewValue == 0) ? "1" : $"1/{Convert.ToInt32(Math.Pow(2, e.NewValue))}";

        private void Pers3Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            p3Block.Text = (e.NewValue == 6) ? "0" : (e.NewValue == 0) ? "1" : $"1/{Convert.ToInt32(Math.Pow(2, e.NewValue))}";

        private void Pers4Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            p4Block.Text = (e.NewValue == 6) ? "0" : (e.NewValue == 0) ? "1" : $"1/{Convert.ToInt32(Math.Pow(2, e.NewValue))}";

        private void Pers5Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            p5Block.Text = (e.NewValue == 6) ? "0" : (e.NewValue == 0) ? "1" : $"1/{Convert.ToInt32(Math.Pow(2, e.NewValue))}";
    }
}
