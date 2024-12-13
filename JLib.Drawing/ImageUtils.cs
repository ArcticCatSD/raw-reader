using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace JLib.Drawing
{
    public static class ImageUtils
    {
        #region Add Border

        public static T[] AddBorder<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            BorderType type,
            T arg = default) where T : struct
        {
            int stride = thickness + width + thickness;
            var result = new T[stride * (thickness + height + thickness)];

            int idxSrc = 0;
            int idxDst = (stride * thickness) + thickness;

            for (int y = 0; y < height; y++)
            {
                Array.Copy(image, idxSrc, result, idxDst, width);
                idxSrc += width;
                idxDst += stride;
            }

            Border(result, width, height, thickness, stride, type, arg);

            return result;
        }

        private static void Border<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride,
            BorderType type,
            T arg) where T : struct
        {
            switch (type)
            {
                case BorderType.Constant:
                    BorderConstant(image, width, height, thickness, stride, arg);
                    break;

                case BorderType.Replicate:
                    BorderReplicate(image, width, height, thickness, stride);
                    break;

                case BorderType.Reflect:
                    BorderReflect(image, width, height, thickness, stride);
                    break;

                case BorderType.Wrap:
                    BorderWrap(image, width, height, thickness, stride);
                    break;

                case BorderType.Reflect101:
                    BorderReflect101(image, width, height, thickness, stride);
                    break;

                case BorderType.Isolated:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static void BorderConstant<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride,
            T filling) where T : struct
        {
            int scanline1;
            int scanline2;

            // Draw top and bottom borders
            scanline1 = 0;
            scanline2 = stride * (thickness + height);
            for (int y = 0; y < thickness; y++)
            {
                int idx1 = scanline1;
                int idx2 = scanline2;

                for (int x = 0; x < thickness + width + thickness; x++)
                {
                    image[idx1++] = filling;
                    image[idx2++] = filling;
                }

                scanline1 += stride;
                scanline2 += stride;
            }

            // Draw left and right borders
            scanline1 = stride * thickness;
            scanline2 = scanline1 + thickness + width;
            for (int y = 0; y < height; y++)
            {
                int idx1 = scanline1;
                int idx2 = scanline2;

                for (int x = 0; x < thickness; x++)
                {
                    image[idx1++] = filling;
                    image[idx2++] = filling;
                }

                scanline1 += stride;
                scanline2 += stride;
            }
        }

        private static void BorderReplicate<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride) where T : struct
        {
            int scanlinePixel1;
            int scanlinePixel2;
            int scanlineFilling1;
            int scanlineFilling2;

            // Draw top and bottom borders
            scanlinePixel1 = thickness;
            scanlineFilling1 = stride * thickness + thickness;
            scanlinePixel2 = stride * (thickness + height) + thickness;
            scanlineFilling2 = scanlinePixel2 - stride;
            for (int x = 0; x < width; x++)
            {
                T filling1 = image[scanlineFilling1++];
                T filling2 = image[scanlineFilling2++];

                int idxPixel1 = scanlinePixel1++;
                int idxPixel2 = scanlinePixel2++;

                for (int y = 0; y < thickness; y++)
                {
                    image[idxPixel1] = filling1;
                    idxPixel1 += stride;

                    image[idxPixel2] = filling2;
                    idxPixel2 += stride;
                }
            }

            // Draw left and right borders
            scanlinePixel1 = 0;
            scanlineFilling1 = thickness;
            scanlinePixel2 = thickness + width;
            scanlineFilling2 = scanlinePixel2 - 1;

            for (int y = 0; y < thickness + height + thickness; y++)
            {
                T filling1 = image[scanlineFilling1];
                scanlineFilling1 += stride;

                T filling2 = image[scanlineFilling2];
                scanlineFilling2 += stride;

                int idxPixel1 = scanlinePixel1;
                scanlinePixel1 += stride;

                int idxPixel2 = scanlinePixel2;
                scanlinePixel2 += stride;

                for (int x = 0; x < thickness; x++)
                {
                    image[idxPixel1++] = filling1;
                    image[idxPixel2++] = filling2;
                }
            }
        }

        private static void BorderReflect<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride) where T : struct
        {
            int scanlinePixel1;
            int scanlineFilling1;
            int scanlinePixel2;
            int scanlineFilling2;

            // Draw top and bottom borders
            scanlineFilling1 = stride * thickness + thickness;
            scanlinePixel1 = scanlineFilling1 - stride;
            scanlinePixel2 = stride * (thickness + height) + thickness;
            scanlineFilling2 = scanlinePixel2 - stride;
            for (int y = 0; y < thickness; y++)
            {
                int idxPixel1 = scanlinePixel1;
                int idxFilling1 = scanlineFilling1;

                int idxPixel2 = scanlinePixel2;
                int idxFiller2 = scanlineFilling2;

                for (int x = 0; x < width; x++)
                {
                    image[idxPixel1++] = image[idxFilling1++];
                    image[idxPixel2++] = image[idxFiller2++];
                }

                scanlinePixel1 -= stride;
                scanlineFilling1 += stride;

                scanlinePixel2 += stride;
                scanlineFilling2 -= stride;
            }

            // Draw left and right borders
            scanlinePixel1 = thickness - 1;
            scanlineFilling1 = thickness;
            scanlinePixel2 = thickness + width;
            scanlineFilling2 = scanlinePixel2 - 1;
            for (int y = 0; y < thickness + height + thickness; y++)
            {
                int idxPixel1 = scanlinePixel1;
                int idxFilling1 = scanlineFilling1;

                int idxPixel2 = scanlinePixel2;
                int idxFilling2 = scanlineFilling2;

                for (int x = 0; x < thickness; x++)
                {
                    image[idxPixel1--] = image[idxFilling1++];
                    image[idxPixel2++] = image[idxFilling2--];
                }

                scanlinePixel1 += stride;
                scanlineFilling1 += stride;

                scanlinePixel2 += stride;
                scanlineFilling2 += stride;
            }
        }

        private static void BorderWrap<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride) where T : struct
        {
            int scanlinePixel;
            int scanlineFilling;
            int startIdxFilling;
            int endIdxFilling;

            // Draw top border
            endIdxFilling = stride * thickness + thickness;
            startIdxFilling = endIdxFilling + stride * (height - 1);
            scanlineFilling = startIdxFilling;
            scanlinePixel = endIdxFilling - stride;
            for (int y = 0; y < thickness; y++)
            {
                int idxPixel = scanlinePixel;
                int idxFilling = scanlineFilling;

                for (int x = 0; x < width; x++)
                {
                    image[idxPixel++] = image[idxFilling++];
                }

                scanlinePixel -= stride;

                if (scanlineFilling == endIdxFilling)
                {
                    scanlineFilling = startIdxFilling;
                }
                else
                {
                    scanlineFilling -= stride;
                }
            }

            // Draw bottom border
            (startIdxFilling, endIdxFilling) = (endIdxFilling, startIdxFilling);
            scanlineFilling = startIdxFilling;
            scanlinePixel = endIdxFilling + stride;
            for (int y = 0; y < thickness; y++)
            {
                int idxPixel = scanlinePixel;
                int idxFiller = scanlineFilling;

                for (int x = 0; x < width; x++)
                {
                    image[idxPixel++] = image[idxFiller++];
                }

                scanlinePixel += stride;

                if (scanlineFilling == endIdxFilling)
                {
                    scanlineFilling = startIdxFilling;
                }
                else
                {
                    scanlineFilling += stride;
                }
            }

            // Draw left border
            endIdxFilling = thickness;
            scanlinePixel = thickness - 1;
            startIdxFilling = scanlinePixel + width;
            scanlineFilling = startIdxFilling;
            for (int y = 0; y < thickness + height + thickness; y++)
            {
                int idxPixel = scanlinePixel;
                int idxFilling = scanlineFilling;

                for (int x = 0; x < thickness; x++)
                {
                    image[idxPixel--] = image[idxFilling];

                    if (idxFilling == endIdxFilling)
                    {
                        idxFilling = startIdxFilling;
                    }
                    else
                    {
                        idxFilling--;
                    }
                }

                scanlinePixel += stride;
                scanlineFilling += stride;

                startIdxFilling += stride;
                endIdxFilling += stride;
            }

            // Draw right border
            startIdxFilling = thickness;
            scanlineFilling = thickness;
            scanlinePixel = thickness + width;
            endIdxFilling = scanlinePixel - 1;
            for (int y = 0; y < thickness + height + thickness; y++)
            {
                int idxPixel = scanlinePixel;
                int idxFilling = scanlineFilling;

                for (int x = 0; x < thickness; x++)
                {
                    image[idxPixel++] = image[idxFilling];

                    if (idxFilling == endIdxFilling)
                    {
                        idxFilling = startIdxFilling;
                    }
                    else
                    {
                        idxFilling++;
                    }
                }

                scanlinePixel += stride;
                scanlineFilling += stride;

                startIdxFilling += stride;
                endIdxFilling += stride;
            }
        }

        private static void BorderReflect101<T>(
            T[] image,
            int width,
            int height,
            int thickness,
            int stride) where T : struct
        {
            int scanlinePixel1;
            int scanlineFilling1;
            int scanlinePixel2;
            int scanlineFilling2;

            // Draw top and bottom borders
            scanlineFilling1 = stride * (thickness + 1) + thickness;
            scanlinePixel1 = scanlineFilling1 - stride - stride;
            scanlinePixel2 = stride * (thickness + height) + thickness;
            scanlineFilling2 = scanlinePixel2 - stride - stride;
            for (int y = 0; y < thickness; y++)
            {
                int idxPixel1 = scanlinePixel1;
                int idxFilling1 = scanlineFilling1;

                int idxPixel2 = scanlinePixel2;
                int idxFiller2 = scanlineFilling2;

                for (int x = 0; x < width; x++)
                {
                    image[idxPixel1++] = image[idxFilling1++];
                    image[idxPixel2++] = image[idxFiller2++];
                }

                scanlinePixel1 -= stride;
                scanlineFilling1 += stride;

                scanlinePixel2 += stride;
                scanlineFilling2 -= stride;
            }

            // Draw left and right borders
            scanlinePixel1 = thickness - 1;
            scanlineFilling1 = thickness + 1;
            scanlinePixel2 = thickness + width;
            scanlineFilling2 = scanlinePixel2 - 2;
            for (int y = 0; y < thickness + height + thickness; y++)
            {
                int idxPixel1 = scanlinePixel1;
                int idxFilling1 = scanlineFilling1;

                int idxPixel2 = scanlinePixel2;
                int idxFilling2 = scanlineFilling2;

                for (int x = 0; x < thickness; x++)
                {
                    image[idxPixel1--] = image[idxFilling1++];
                    image[idxPixel2++] = image[idxFilling2--];
                }

                scanlinePixel1 += stride;
                scanlineFilling1 += stride;

                scanlinePixel2 += stride;
                scanlineFilling2 += stride;
            }
        }

        #endregion


        #region Bayer Image

        public static byte[]? ReadBayerRawData_8Bpp(string imagePath, int headerSize, int width, int height)
        {
            var fi = new FileInfo(imagePath);

            int rawDataSize = width * height;

            if (fi.Length < headerSize + rawDataSize)
            {
                return null;
            }

            var raw = new byte[rawDataSize];

            using (FileStream fs = fi.OpenRead())
            {
                _ = fs.Read(raw, headerSize, rawDataSize);
            }

            return raw;
        }

        public static ushort[]? ReadBayerRawData_10Bpp(string imagePath, int headerSize, int width, int height)
        {
            var fi = new FileInfo(imagePath);

            int imageSize = width * height;
            int rawDataSize = imageSize * 2;

            if (fi.Length < headerSize + rawDataSize)
            {
                return null;
            }

            var raw = new ushort[imageSize];

            using (FileStream fs = fi.OpenRead())
            {
                var buf = new byte[rawDataSize];
                _ = fs.Read(buf, headerSize, rawDataSize);

                Buffer.BlockCopy(buf, 0, raw, 0, rawDataSize);
            }

            return raw;
        }

        public static byte[] BayerToBgr32(
            byte[] raw,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            Debug.Assert(
                pattern == BayerPattern.BGGR ||
                pattern == BayerPattern.GBRG ||
                pattern == BayerPattern.GRBG ||
                pattern == BayerPattern.RGGB
            );
            Debug.Assert(
                method == DemosaicMethod.OpenCV ||
                method == DemosaicMethod.Imatest ||
                method == DemosaicMethod.MATLAB ||
                method == DemosaicMethod.LabVIEW
            );
            Debug.Assert(raw.Length == width * height);

            return BayerToBgr32_Dispatch(raw, width, height, pattern, method);
        }

        public static ushort[] BayerToRgb48(
            ushort[] raw,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            Debug.Assert(
                pattern == BayerPattern.BGGR ||
                pattern == BayerPattern.GBRG ||
                pattern == BayerPattern.GRBG ||
                pattern == BayerPattern.RGGB
            );
            Debug.Assert(
                method == DemosaicMethod.OpenCV ||
                method == DemosaicMethod.Imatest ||
                method == DemosaicMethod.MATLAB ||
                method == DemosaicMethod.LabVIEW
            );
            Debug.Assert(raw.Length == width * height);

            var buf = (ushort[])raw.Clone();

            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] <<= 6;
            }

            return BayerToRgb48_Dispatch(buf, width, height, pattern, method);
        }

        private static byte[] BayerToBgr32_Dispatch(
            byte[] raw,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            (byte[] channelR, byte[] channelG, byte[] channelB) = method switch
            {
                DemosaicMethod.OpenCV => GetChannels_OpenCV(raw, width, height, pattern),
                DemosaicMethod.Imatest => GetChannels_Imatest(raw, width, height, pattern),
                DemosaicMethod.MATLAB => GetChannels_MATLAB(raw, width, height, pattern),
                DemosaicMethod.LabVIEW => GetChannels_LabVIEW(raw, width, height, pattern),
                _ => throw new NotSupportedException(),
            };
            byte[] result = Merge(byte.MaxValue, channelB, channelG, channelR);
            return result;
        }

        private static ushort[] BayerToRgb48_Dispatch(
            ushort[] raw,
            int width,
            int height,
            BayerPattern pattern,
            DemosaicMethod method)
        {
            (ushort[] channelR, ushort[] channelG, ushort[] channelB) = method switch
            {
                DemosaicMethod.OpenCV => GetChannels_OpenCV(raw, width, height, pattern),
                DemosaicMethod.Imatest => GetChannels_Imatest(raw, width, height, pattern),
                DemosaicMethod.MATLAB => GetChannels_MATLAB(raw, width, height, pattern),
                DemosaicMethod.LabVIEW => GetChannels_LabVIEW(raw, width, height, pattern),
                _ => throw new NotSupportedException(),
            };
            ushort[] result = Merge(channelR, channelG, channelB);
            return result;
        }

        private static (T[] r, T[] g, T[] b) GetChannels_OpenCV<T>(
            T[] raw,
            int width,
            int height,
            BayerPattern pattern) where T : struct, INumber<T>
        {
            var channelR = new T[width * height];
            var channelG = new T[width * height];
            var channelB = new T[width * height];

            switch (pattern)
            {
                case BayerPattern.BGGR:
                    Copy2D<T>(raw, channelR, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 0, 2, 2);
                    break;

                case BayerPattern.GBRG:
                    Copy2D<T>(raw, channelR, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 0, 2, 2);
                    break;

                case BayerPattern.GRBG:
                    Copy2D<T>(raw, channelR, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 1, 2, 2);
                    break;

                default:
                    // BayerPattern.RGGB
                    Copy2D<T>(raw, channelR, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 1, 2, 2);
                    break;
            }

            double[] kernelRB =
            [
                0.25, 0.50, 0.25,
                0.50, 1.00, 0.50,
                0.25, 0.50, 0.25
            ];
            double[] kernelG =
            [
                0.00, 0.25, 0.00,
                0.25, 1.00, 0.25,
                0.00, 0.25, 0.00
            ];

            double[] resultR = Convolve(channelR, width, height, kernelRB, 3, BorderType.Reflect101);
            double[] resultG = Convolve(channelG, width, height, kernelG, 3, BorderType.Reflect101);
            double[] resultB = Convolve(channelB, width, height, kernelRB, 3, BorderType.Reflect101);

            // bytes per element
            const int bpe = sizeof(double);

            Buffer.BlockCopy(resultR, bpe * (width + 1), resultR, bpe, bpe * (width - 2));
            Buffer.BlockCopy(resultG, bpe * (width + 1), resultG, bpe, bpe * (width - 2));
            Buffer.BlockCopy(resultB, bpe * (width + 1), resultB, bpe, bpe * (width - 2));

            Buffer.BlockCopy(
                resultR,
                bpe * (width * (height - 2) + 1),
                resultR,
                bpe * (width * (height - 1) + 1),
                bpe * (width - 2)
            );
            Buffer.BlockCopy(
                resultG,
                bpe * (width * (height - 2) + 1),
                resultG,
                bpe * (width * (height - 1) + 1),
                bpe * (width - 2)
            );
            Buffer.BlockCopy(
                resultB,
                bpe * (width * (height - 2) + 1),
                resultB,
                bpe * (width * (height - 1) + 1),
                bpe * (width - 2)
            );

            for (int i = 0, j = 1, k = width - 1, l = k - 1, stride = width;
                i < resultR.Length;
                i += stride, j += stride, k += stride, l += stride)
            {
                resultR[i] = resultR[j];
                resultR[k] = resultR[l];

                resultG[i] = resultG[j];
                resultG[k] = resultG[l];

                resultB[i] = resultB[j];
                resultB[k] = resultB[l];
            }

            return (RoundAndConvert<T>(resultR), RoundAndConvert<T>(resultG), RoundAndConvert<T>(resultB));
        }

        private static (T[] r, T[] g, T[] b) GetChannels_Imatest<T>(
            T[] raw,
            int width,
            int height,
            BayerPattern pattern) where T : struct, INumber<T>
        {
            var channelR = new T[width * height];
            var channelG = new T[width * height];
            var channelB = new T[width * height];

            switch (pattern)
            {
                case BayerPattern.BGGR:
                    Copy2D<T>(raw, channelR, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 0, 2, 2);
                    break;

                case BayerPattern.GBRG:
                    Copy2D<T>(raw, channelR, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 0, 2, 2);
                    break;

                case BayerPattern.GRBG:
                    Copy2D<T>(raw, channelR, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 1, 2, 2);
                    break;

                default:
                    // BayerPattern.RGGB
                    Copy2D<T>(raw, channelR, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 1, 2, 2);
                    break;
            }

            double[] kernelRB =
            [
                0.25, 0.00, 0.25,
                0.00, 1.00, 0.00,
                0.25, 0.00, 0.25
            ];
            double[] kernelG =
            [
                0.00, 0.25, 0.00,
                0.25, 1.00, 0.25,
                0.00, 0.25, 0.00
            ];

            double[] resultR = Convolve(channelR, width, height, kernelRB, 3, BorderType.Isolated);
            double[] resultG = Convolve(channelG, width, height, kernelG, 3, BorderType.Isolated);
            double[] resultB = Convolve(channelB, width, height, kernelRB, 3, BorderType.Isolated);

            double[] kernel =
            [
                0.00, 0.25, 0.00,
                0.25, 0.00, 0.25,
                0.00, 0.25, 0.00
            ];

            double[] resultR2 = Convolve(resultR, width, height, kernel, 3, BorderType.Isolated);
            AddInPlace(resultR2, resultR);

            double[] resultB2 = Convolve(resultB, width, height, kernel, 3, BorderType.Isolated);
            AddInPlace(resultB2, resultB);

            return (RoundAndConvert<T>(resultR2), RoundAndConvert<T>(resultG), RoundAndConvert<T>(resultB2));
        }

        private static (T[] r, T[] g, T[] b) GetChannels_LabVIEW<T>(
            T[] raw,
            int width,
            int height,
            BayerPattern pattern) where T : struct, INumber<T>
        {
            var channelR = new T[width * height];
            var channelG = new T[width * height];
            var channelB = new T[width * height];

            bool isG;

            switch (pattern)
            {
                case BayerPattern.BGGR:
                    isG = false;
                    Copy2D<T>(raw, channelR, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 0, 2, 2);
                    break;

                case BayerPattern.GBRG:
                    isG = true;
                    Copy2D<T>(raw, channelR, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 0, 2, 2);
                    break;

                case BayerPattern.GRBG:
                    isG = true;
                    Copy2D<T>(raw, channelR, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 0, 1, 2, 2);
                    break;

                default:
                    // BayerPattern.RGGB
                    isG = false;
                    Copy2D<T>(raw, channelR, width, height, 0, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D<T>(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D<T>(raw, channelB, width, height, 1, 1, 2, 2);
                    break;
            }

            double[] kernelRB =
            [
                0.25, 0.50, 0.25,
                0.50, 1.00, 0.50,
                0.25, 0.50, 0.25
            ];
            double[] kernelG =
            [
                0.00, 0.25, 0.00,
                0.25, 1.00, 0.25,
                0.00, 0.25, 0.00
            ];

            double[] resultR = Convolve(channelR, width, height, kernelRB, 3, BorderType.Reflect101);
            double[] resultG = Convolve(channelG, width, height, kernelG, 3, BorderType.Reflect101);
            double[] resultB = Convolve(channelB, width, height, kernelRB, 3, BorderType.Reflect101);

            var r = new T[resultR.Length];
            var g = new T[resultR.Length];
            var b = new T[resultR.Length];

            for (int i = 0; i < resultR.Length; i += width)
            {
                r[i] = T.CreateChecked(resultR[i]);
                g[i] = T.CreateChecked(resultG[i]);
                b[i] = T.CreateChecked(resultB[i]);

                int j = i + width - 1;
                r[j] = T.CreateChecked(resultR[j]);
                g[j] = T.CreateChecked(resultG[j]);
                b[j] = T.CreateChecked(resultB[j]);
            }

            for (int i = 1; i < resultR.Length; i += width)
            {
                isG = !isG;

                if (isG)
                {
                    r[i] = T.CreateChecked(resultR[i] + 0.5);
                    b[i] = T.CreateChecked(resultB[i] + 0.5);
                }
                else
                {
                    r[i] = T.CreateChecked(resultR[i]);
                    b[i] = T.CreateChecked(resultB[i]);
                }

                g[i] = T.CreateChecked(resultG[i]);
            }

            for (int i = 0; i < resultR.Length; i += width)
            {
                for (int j = i + 2, end = i + width - 1; j < end; j++)
                {
                    r[j] = T.CreateChecked(resultR[j] + 0.5);
                    g[j] = T.CreateChecked(resultG[j] + 0.5);
                    b[j] = T.CreateChecked(resultB[j] + 0.5);
                }
            }

            return (r, g, b);
        }

        private static (T[] r, T[] g, T[] b) GetChannels_MATLAB<T>(
            T[] raw,
            int width,
            int height,
            BayerPattern pattern) where T : struct, INumber<T>
        {
            T[] src = AddBorder(raw, width, height, 2, BorderType.Reflect101);

            var channelR = new double[width * height];
            var channelG = new double[width * height];
            var channelB = new double[width * height];

            double[] kernelG =
            [
                 0,      0,     -0.125,  0,      0,
                 0,      0,      0.250,  0,      0,
                -0.125,  0.250,  0.500,  0.250, -0.125,
                 0,      0,      0.250,  0,      0,
                 0,      0,     -0.125,  0,      0,
            ];
            double[] kernelRB1 =
            [
                 0,      0,      0.0625,  0,      0,
                 0,     -0.125,  0,      -0.125,  0,
                -0.125,  0.500,  0.6250,  0.500, -0.125,
                 0,     -0.125,  0,      -0.125,  0,
                 0,      0,      0.0625,  0,      0,
            ];
            double[] kernelRB2 =
            [
                0,       0,     -0.125,  0,     0,
                0,      -0.125,  0.500, -0.125, 0,
                0.0625,  0,      0.625,  0,     0.0625,
                0,      -0.125,  0.500, -0.125, 0,
                0,       0,     -0.125,  0,     0,
            ];
            double[] kernelRB3 =
            [
                 0,      0,    -0.1875,  0,     0,
                 0,      0.25,  0,       0.25,  0,
                -0.1875, 0,     0.75,    0,    -0.1875,
                 0,      0.25,  0,       0.25,  0,
                 0,      0,    -0.1875,  0,     0,
            ];

            switch (pattern)
            {
                case BayerPattern.BGGR:
                    Copy2D(raw, channelR, width, height, 1, 1, 2, 2);
                    Copy2D(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D(raw, channelB, width, height, 0, 0, 2, 2);

                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    break;

                case BayerPattern.GBRG:
                    Copy2D(raw, channelR, width, height, 0, 1, 2, 2);
                    Copy2D(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D(raw, channelB, width, height, 1, 0, 2, 2);

                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    break;

                case BayerPattern.GRBG:
                    Copy2D(raw, channelR, width, height, 1, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 0, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 1, 1, 2, 2);
                    Copy2D(raw, channelB, width, height, 0, 1, 2, 2);

                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    break;

                default:
                    // BayerPattern.RGGB
                    Copy2D(raw, channelR, width, height, 0, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 1, 0, 2, 2);
                    Copy2D(raw, channelG, width, height, 0, 1, 2, 2);
                    Copy2D(raw, channelB, width, height, 1, 1, 2, 2);

                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelR,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB3,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB2,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelB,
                        width,
                        height,
                        kernelRB1,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );

                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 0,
                        startY: 0,
                        strideX: 2,
                        strideY: 2
                    );
                    Convolve(
                        src,
                        dst: channelG,
                        width,
                        height,
                        kernelG,
                        kernelWidth: 5,
                        kernelHeight: 5,
                        startX: 1,
                        startY: 1,
                        strideX: 2,
                        strideY: 2
                    );
                    break;
            }

            return (FloorAndConvert<T>(channelR), FloorAndConvert<T>(channelG), FloorAndConvert<T>(channelB));
        }

        #endregion


        #region Convolution

        /// <summary>
        /// Convolution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="kernel"></param>
        /// <param name="kernelSize">Must be an odd number.</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static double[] Convolve<T>(
            T[] image,
            int width,
            int height,
            double[] kernel,
            int kernelSize,
            BorderType type) where T : struct, INumber<T>
        {
            var dst = new double[image.Length];
            T[] src = AddBorder(image, width, height, kernelSize / 2, type);

            Convolve(src, dst, width, height, kernel, kernelSize, kernelSize);

            return dst;
        }

        /// <summary>
        /// Convolution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">
        /// The src width = widthDst + widthKernel - 1.
        /// The src height = heightDst + heightKernel - 1.
        /// </param>
        /// <param name="dst"></param>
        /// <param name="dstWidth"></param>
        /// <param name="dstHeight"></param>
        /// <param name="kernel"></param>
        /// <param name="kernelWidth">Must be an odd number.</param>
        /// <param name="kernelHeight">Must be an odd number.</param>
        public static void Convolve<T>(
            T[] src,
            double[] dst,
            int dstWidth,
            int dstHeight,
            double[] kernel,
            int kernelWidth,
            int kernelHeight,
            int startX = 0,
            int startY = 0,
            int strideX = 1,
            int strideY = 1) where T : struct, INumber<T>
        {
            int srcWidth = dstWidth + kernelWidth - 1;

            int strideDst = dstWidth * strideY;
            int strideSrc = srcWidth * strideY;

            int indexDst = dstWidth * startY + startX;
            int indexSrc = srcWidth * startY + startX;

            for (int y = startY; y < dstHeight; y += strideY)
            {
                int i = indexDst;
                int j = indexSrc;

                for (int x = startX; x < dstWidth; x += strideX, i += strideX, j += strideX)
                {
                    double val = ConvolveOneUnit(
                        src,
                        sourceStartIndex: j,
                        sourceStride: srcWidth,
                        kernel,
                        kernelWidth,
                        kernelHeight
                    );
                    dst[i] = val;
                }

                indexDst += strideDst;
                indexSrc += strideSrc;
            }
        }

        private static double ConvolveOneUnit<T>(
            T[] source,
            int sourceStartIndex,
            int sourceStride,
            double[] kernel,
            int kernelWidth,
            int kernelHeight) where T : struct, INumber<T>
        {
            double sum = 0;

            int indexSrc = sourceStartIndex;
            int indexKernel = 0;

            for (int y = 0; y < kernelHeight; y++)
            {
                int i = indexSrc;

                for (int x = 0; x < kernelWidth; x++)
                {
                    double val = double.CreateChecked(source[i++]);
                    sum += kernel[indexKernel++] * val;
                }

                indexSrc += sourceStride;
            }

            return sum;
        }

        #endregion


        #region Helpers

        /// <summary>
        /// Add 2 arrays into a new array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The result array.</returns>
        public static T[] Add<T>(T[] a, T[] b) where T : struct, INumber<T>
        {
            Debug.Assert(a != null);
            Debug.Assert(b != null);
            Debug.Assert(a.Length == b.Length);

            var sum = new T[a.Length];

            for (int i = 0; i < sum.Length; i++)
            {
                sum[i] = a[i] + b[i];
            }

            return sum;
        }

        /// <summary>
        /// Add array target and array source and store them in target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns>The array target.</returns>
        public static T[] AddInPlace<T>(T[] target, T[] source) where T : struct, INumber<T>
        {
            Debug.Assert(target != null);
            Debug.Assert(source != null);
            Debug.Assert(target.Length == source.Length);

            for (int i = 0; i < target.Length; i++)
            {
                target[i] += source[i];
            }

            return target;
        }

        public static void Copy2D<T>(
            T[] src,
            T[] dst,
            int width,
            int height,
            int startX,
            int startY,
            int strideX,
            int strideY) where T : struct
        {
            int index = width * startY + startX;
            int stride = width * strideY;

            for (int y = startY; y < height; y += strideY)
            {
                int i = index;

                for (int x = startX; x < width; x += strideX)
                {
                    dst[i] = src[i];
                    i += strideX;
                }

                index += stride;
            }
        }

        public static void Copy2D<TSrc, TDst>(
            TSrc[] src,
            TDst[] dst,
            int width,
            int height,
            int startX,
            int startY,
            int strideX,
            int strideY) where TSrc : struct, INumber<TSrc> where TDst : struct, INumber<TDst>
        {
            int index = width * startY + startX;
            int stride = width * strideY;

            for (int y = startY; y < height; y += strideY)
            {
                int i = index;

                for (int x = startX; x < width; x += strideX)
                {
                    dst[i] = TDst.CreateChecked(src[i]);
                    i += strideX;
                }

                index += stride;
            }
        }

        public static T[] Merge<T>(params T[][] channels) where T : struct
        {
            Debug.Assert(channels != null);
            Debug.Assert(channels.Length > 0);

            int channelCount = channels.Length;
            int elementCount = channels[0].Length;
            var result = new T[elementCount * channelCount];

            for (int idxResult = 0, idxElement = 0;
                idxResult < result.Length;
                idxResult += channelCount, idxElement++)
            {
                for (int idxChannel = 0; idxChannel < channelCount; idxChannel++)
                {
                    result[idxResult + idxChannel] = channels[idxChannel][idxElement];
                }
            }

            return result;
        }

        /// <summary>
        /// Merge channels.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filling">The value to fill in the last channel.</param>
        /// <param name="channels"></param>
        /// <returns>The result array.</returns>
        public static T[] Merge<T>(T filling, params T[][] channels) where T : struct
        {
            Debug.Assert(channels != null);
            Debug.Assert(channels.Length > 0);

            int channelCount = channels.Length;
            int elementCount = channels[0].Length;
            var result = new T[elementCount * (channelCount + 1)];

            for (int idxResult = 0, idxElement = 0;
                idxResult < result.Length;
                idxResult += channelCount + 1, idxElement++)
            {
                for (int idxChannel = 0; idxChannel < channelCount; idxChannel++)
                {
                    result[idxResult + idxChannel] = channels[idxChannel][idxElement];
                }

                result[idxResult + channelCount] = filling;
            }

            return result;
        }

#if false
        public static void Round(double[] array, int decimals = 0, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            Debug.Assert(array != null);

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Math.Round(array[i], decimals, mode);
            }
        }

        public static byte[] RoundAndConvertToByteArray(double[] input)
        {
            var result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                Debug.Assert(input[i] + 0.5 < 256);

                result[i] = (byte)(input[i] + 0.5);
            }

            return result;
        }

        public static ushort[] RoundAndConvertToUshortArray(double[] input)
        {
            var result = new ushort[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                Debug.Assert(input[i] + 0.5 < 65536);

                result[i] = (ushort)(input[i] + 0.5);
            }

            return result;
        }

#endif

        public static T[] RoundAndConvert<T>(double[] input) where T : struct, INumber<T>
        {
            var result = new T[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                result[i] = T.CreateChecked(input[i] + 0.5);
                //result[i] = T.CreateSaturating(input[i] + 0.5);
            }

            return result;
        }

        public static T[] FloorAndConvert<T>(double[] input) where T : struct, INumber<T>
        {
            var result = new T[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                result[i] = T.CreateChecked(input[i]);
            }

            return result;
        }

        #endregion
    }
}
