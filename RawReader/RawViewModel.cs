using JLib.Drawing;
using JLib.Wpf;

namespace RawReader
{
    internal class RawViewModel : ObservableObject
    {
        #region Fields

        private string _imagePath = "RAW Viewer";

        private int _rawHeaderSize;
        private int _rawImageWidth = 800;
        private int _rawImageHeight = 800;
        private Bpp _rawBpp = Bpp.Bpp8;
        private BayerPattern _rawPattern = BayerPattern.BGGR;
        private DemosaicMethod _demosaicMethod = DemosaicMethod.OpenCV;

        #endregion


        #region Properties

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public int RawHeaderSize
        {
            get => _rawHeaderSize;
            set => SetProperty(ref _rawHeaderSize, value);
        }

        public int RawImageWidth
        {
            get => _rawImageWidth;
            set => SetProperty(ref _rawImageWidth, value);
        }

        public int RawImageHeight
        {
            get => _rawImageHeight;
            set => SetProperty(ref _rawImageHeight, value);
        }

        public Bpp RawBpp
        {
            get => _rawBpp;
            set => SetProperty(ref _rawBpp, value);
        }

        public BayerPattern RawPattern
        {
            get => _rawPattern;
            set => SetProperty(ref _rawPattern, value);
        }

        public DemosaicMethod DemosaicMethod
        {
            get => _demosaicMethod;
            set => SetProperty(ref _demosaicMethod, value);
        }

        #endregion
    }
}
