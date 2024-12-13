using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

using JLib.Drawing;
using JLib.Wpf.Rule;

namespace RawReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly RawViewModel _viewModel = new();

        private readonly OpenFileDialog _dlgOpenFile = new()
        {
            Filter = "RAW Image|*.raw"
        };

        private readonly SaveFileDialog _dlgSaveFile = new()
        {
            Filter = "Image|*.bmp;*.png;*.jpg;*.jpeg",
            DefaultExt = "bmp",
        };

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            DataContext = _viewModel;

            BindingRawImageWidth.ValidationRules.Add(new IntegerRule
            {
                Min = 1,
                Max = 10000,
                CanEmpty = false,
                ValidatesOnTargetUpdated = true
            });

            BindingRawImageHeight.ValidationRules.Add(new IntegerRule
            {
                Min = 1,
                Max = 10000,
                CanEmpty = false,
                ValidatesOnTargetUpdated = true
            });

            BindingRawHeaderSize.ValidationRules.Add(new IntegerRule
            {
                Min = 0,
                Max = 100,
                CanEmpty = false,
                ValidatesOnTargetUpdated = true
            });

            //string p = @"C:\Users\JenLiu.ALTEK\Downloads\IMG_20201231_101657.jpg";
            //var t = ImageUtils.GetBgrImage(p);

            //int[] src = {
            //    1, 2, 3, 4, 5,
            //    6, 7, 8, 9, 10,
            //    11, 12, 13, 14,15,
            //    16, 17, 18, 19, 20,
            //};
            //var dst = new int[3 * 2];
            //double[] kernel =
            //{
            //    9.6, 8.1, 7.6,
            //    6, 5.41, 4.4,
            //    3, 2.7, 1.8,
            //};

            //ImageUtils.Convolve(
            //    src,
            //    dst,
            //    widthDst: 3,
            //    heightDst: 2,
            //    kernel,
            //    widthKernel: 3,
            //    heightKernel: 3
            //);
            //int w = 5;
            //int h = 4;
            //int t = 2;
            //var a = new byte[w * h];

            //for (int i = 0; i < w * h; i++)
            //{
            //    a[i] = (byte)(i % 256);
            //    a[i]++;
            //}

            //var s = DateTime.Now;
            //var b = JLib.Drawing.ImageUtils.AddBorder(a, w, h, t, JLib.Drawing.BorderType.Replicate);
            //var d = DateTime.Now - s;

            //var buf = new StringBuilder();
            //for (int i = 0; i < b.Length; i++)
            //{
            //    if (i != 0 && i % (w + t * 2) == 0)
            //    {
            //        _ = buf.Append('\n');
            //    }

            //    buf.Append($"{b[i],4}");
            //}
            //_ = buf.Append($"\n\n{d.TotalSeconds}");
            //System.IO.File.WriteAllText("d:/out.txt", buf.ToString());
        }

        private async void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            bool? r = _dlgOpenFile.ShowDialog();

            if (r == true)
            {
                IsEnabled = false;

                if (string.IsNullOrEmpty(_dlgOpenFile.InitialDirectory))
                {
                    _dlgOpenFile.InitialDirectory = System.IO.Path.GetDirectoryName(_dlgOpenFile.FileName);
                }

                //_dlgOpenFile.InitialDirectory
                _viewModel.ImagePath = _dlgOpenFile.FileName;

                await Task.Factory.StartNew(() =>
                {
                    switch (_viewModel.RawBpp)
                    {
                        case Bpp.Bpp8:
                            OnRawImage_8pp(
                                _viewModel.ImagePath,
                                _viewModel.RawHeaderSize,
                                _viewModel.RawImageWidth,
                                _viewModel.RawImageHeight,
                                _viewModel.RawPattern,
                                _viewModel.DemosaicMethod
                            );
                            break;

                        case Bpp.Bpp10:
                            OnRawImage_10pp(
                                _viewModel.ImagePath,
                                _viewModel.RawHeaderSize,
                                _viewModel.RawImageWidth,
                                _viewModel.RawImageHeight,
                                _viewModel.RawPattern,
                                _viewModel.DemosaicMethod
                            );
                            break;

                        default:
                            break;
                    }
                });

                _ = MessageBox.Show("Done", "Read Image", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEnabled = true;
            }
        }

        private void OnRawImage_8pp(
            string imagePath,
            int headerSize,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            byte[]? raw = ImageUtils.ReadBayerRawData_8Bpp(imagePath, headerSize, width, height);

            if (raw is not null)
            {
                byte[] bgra = ImageUtils.BayerToBgr32(raw, width, height, pattern, method);
                var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
                bmp.WritePixels(
                    new Int32Rect(0, 0, width, height),
                    bgra,
                    4 * width,
                    0
                );
                bmp.Freeze();

                Dispatcher.Invoke(() =>
                {
                    ImageViewer.Source = bmp;
                    ImageViewer.Width = bmp.Width;
                    ImageViewer.Height = bmp.Height;

                    ButtonSave.IsEnabled = true;
                });
            }
        }

        private void OnRawImage_10pp(
            string imagePath,
            int headerSize,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            ushort[]? raw = ImageUtils.ReadBayerRawData_10Bpp(imagePath, headerSize, width, height);

            if (raw is not null)
            {
                ushort[] rgb48 = ImageUtils.BayerToRgb48(raw, width, height, pattern, method);
                var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb48, null);
                bmp.WritePixels(
                    new Int32Rect(0, 0, width, height),
                    rgb48,
                    6 * width,
                    0
                );
                bmp.Freeze();

                Dispatcher.Invoke(() =>
                {
                    ImageViewer.Source = bmp;
                    ImageViewer.Width = bmp.Width;
                    ImageViewer.Height = bmp.Height;

                    ButtonSave.IsEnabled = true;
                });
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            bool? r = _dlgSaveFile.ShowDialog();

            if (r == true)
            {
                IsEnabled = false;

                if (string.IsNullOrEmpty(_dlgSaveFile.InitialDirectory))
                {
                    _dlgSaveFile.InitialDirectory = System.IO.Path.GetDirectoryName(_dlgSaveFile.FileName);
                }

                await Task.Factory.StartNew((state) =>
                {
                    string ext = System.IO.Path.GetExtension(_dlgSaveFile.FileName).ToUpper();
                    BitmapEncoder encoder = ext switch
                    {
                        ".PNG" => new PngBitmapEncoder(),
                        ".JPG" or ".JPEG" => new JpegBitmapEncoder(),
                        _ => new BmpBitmapEncoder(),
                    };

                    var bmp = (BitmapSource)state!;

                    if (bmp.Format == PixelFormats.Bgr32)
                    {
                        FormatConvertedBitmap bgr24 = Convert(bmp, PixelFormats.Bgr24);
                        encoder.Frames.Add(BitmapFrame.Create(bgr24));
                    }
                    else
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                    }

                    using var stream = new FileStream(_dlgSaveFile.FileName, FileMode.Create);
                    encoder.Save(stream);
                }, ImageViewer.Source);

                _ = MessageBox.Show("Done", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEnabled = true;
            }
        }

        private static FormatConvertedBitmap Convert(BitmapSource source, PixelFormat destinationFormat)
        {
            var converter = new FormatConvertedBitmap(source, destinationFormat, null, 0);
            converter.Freeze();
            return converter;
        }
    }
}