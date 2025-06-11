using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImagesProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage originalBitmap;
        private BitmapImage processedBitmap;
        private ImageProcessor imageProcessor = new ImageProcessor();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                originalBitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                processedBitmap = new BitmapImage(originalBitmap.UriSource);
                originalImage.Source = originalBitmap;
                processedImage.Source = processedBitmap;
            }
        }

        private void channelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ShowChannel_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null) return;

            var selectedItem = channelSelector.SelectedItem as ComboBoxItem;
            var channel = selectedItem?.Content.ToString();
            if (string.IsNullOrEmpty(channel)) return;

            processedBitmap = imageProcessor.ApplyChannelFilter(originalBitmap, channel);
            processedImage.Source = processedBitmap;
        }

        private void Grayscale_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null) return;
            processedBitmap = imageProcessor.Grayscale(originalBitmap);
            processedImage.Source = processedBitmap;
        }

        private void Sepia_Click(object sender, RoutedEventArgs e)
        {
            if (originalBitmap == null) return;
            processedBitmap = imageProcessor.Sepia(originalBitmap);
            processedImage.Source = processedBitmap;
        }

        private void brightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBrightnessContrast();
        }

        private void contrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBrightnessContrast();
        }

        private void UpdateBrightnessContrast()
        {
            if (originalBitmap == null) return;

            double brightness = brightnessSlider.Value;
            double contrast = contrastSlider.Value;

            processedBitmap = imageProcessor.AdjustBrightnessContrast(originalBitmap, brightness, contrast);
            processedImage.Source = processedBitmap;
        }
    }

    public class ImageProcessor
    {
        public BitmapImage ApplyChannelFilter(BitmapImage source, string channel)
        {
            if (source == null) return null;

            var result = new WriteableBitmap(source);
            var pixels = new byte[result.PixelWidth * result.PixelHeight * 4];
            result.CopyPixels(pixels, result.PixelWidth * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                switch (channel?.ToLower())
                {
                    case "red":
                        pixels[i] = 0;     // Blue (i+0)
                        pixels[i + 1] = 0; // Green (i+1)
                        break;
                    case "green":
                        pixels[i] = 0;     // Blue (i+0)
                        pixels[i + 2] = 0; // Red (i+2)
                        break;
                    case "blue":
                        pixels[i + 1] = 0; // Green (i+1)
                        pixels[i + 2] = 0; // Red (i+2)
                        break;
                }
            }

            result.WritePixels(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight), pixels, result.PixelWidth * 4, 0);
            return result.ToBitmapImage();
        }

        public BitmapImage Grayscale(BitmapImage source)
        {
            var result = new WriteableBitmap(source);
            var pixels = new byte[result.PixelWidth * result.PixelHeight * 4];
            result.CopyPixels(pixels, result.PixelWidth * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                var avg = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                pixels[i] = pixels[i + 1] = pixels[i + 2] = (byte)avg; // здесь ругался на явное приведение - следить
            }

            result.WritePixels(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight), pixels, result.PixelWidth * 4, 0);
            return result.ToBitmapImage();
        }

        public BitmapImage Sepia(BitmapImage source)
        {
            var result = new WriteableBitmap(source);
            var pixels = new byte[result.PixelWidth * result.PixelHeight * 4];
            result.CopyPixels(pixels, result.PixelWidth * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte r = pixels[i];
                byte g = pixels[i + 1];
                byte b = pixels[i + 2];

                byte newR = (byte)Math.Min(255, 0.393 * r + 0.769 * g + 0.189 * b);
                byte newG = (byte)Math.Min(255, 0.349 * r + 0.686 * g + 0.168 * b);
                byte newB = (byte)Math.Min(255, 0.272 * r + 0.534 * g + 0.131 * b);

                pixels[i] = newR;
                pixels[i + 1] = newG;
                pixels[i + 2] = newB;
            }

            result.WritePixels(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight), pixels, result.PixelWidth * 4, 0);
            return result.ToBitmapImage();
        }

        public BitmapImage AdjustBrightnessContrast(BitmapImage source, double brightness, double contrast)
        {
            var result = new WriteableBitmap(source);
            var pixels = new byte[result.PixelWidth * result.PixelHeight * 4];
            result.CopyPixels(pixels, result.PixelWidth * 4, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Бrightness adjustment
                pixels[i] = (byte)Math.Max(0, Math.Min(255, pixels[i] + brightness));
                pixels[i + 1] = (byte)Math.Max(0, Math.Min(255, pixels[i + 1] + brightness));
                pixels[i + 2] = (byte)Math.Max(0, Math.Min(255, pixels[i + 2] + brightness));

                // Contrast adjustment
                double factor = (259.0 * (contrast + 255.0)) / (255.0 * (259.0 - contrast));
                pixels[i] = (byte)Math.Max(0, Math.Min(255, factor * (pixels[i] - 128) + 128));
                pixels[i + 1] = (byte)Math.Max(0, Math.Min(255, factor * (pixels[i + 1] - 128) + 128));
                pixels[i + 2] = (byte)Math.Max(0, Math.Min(255, factor * (pixels[i + 2] - 128) + 128));
            }

            result.WritePixels(new Int32Rect(0, 0, result.PixelWidth, result.PixelHeight), pixels, result.PixelWidth * 4, 0);
            return result.ToBitmapImage();
        }
    }

    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this WriteableBitmap bitmap)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                stream.Position = 0;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }
    }
}