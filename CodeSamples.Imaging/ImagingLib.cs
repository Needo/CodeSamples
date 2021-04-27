using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace CodeSamples.Imaging
{
    public static class IntExtensions
    {
        public static bool Between(this int val, int a, int b)
        {
            return val >= a && val <= b;
        }

        public static bool Between(this float val, float a, float b)
        {
            return val >= a && val <= b;
        }
    }

    public enum ColorChannelRGBA
    {
        R,
        G,
        B,
        A
    }

    public enum CompressionLevel : long
    {
        /// <summary>
        /// Specifies the LZW compression scheme.
        /// </summary>
        CompressionLZW = 2,

        /// <summary>
        /// Specifies the CCITT3 compression scheme.
        /// </summary>
        CompressionCCITT3 = 3,

        /// <summary>
        /// Specifies the CCITT4 compression scheme.
        /// </summary>
        CompressionCCITT4 = 4,

        /// <summary>
        /// Specifies the RLE compression scheme.
        /// </summary>
        CompressionRle = 5,

        /// <summary>
        /// Specifies no compression.
        /// </summary>
        CompressionNone = 6,
    }

    public enum ImagePosition
    {
    }

    public static class DrawingExtensions
    {
        public static ColorMatrix DefaultColorMatrix
        {
            //return new ColorMatrix(new float[][]
            //{
            //    new float[] { 1f, 0, 0, 0, 0 },
            //    new float[] { 0, 1f, 0, 0, 0 },
            //    new float[] { 0, 0, 1f, 0, 0 },
            //    new float[] { 0, 0, 0, 1f, 0 },
            //    new float[] { 0, 0, 0, 0, 1f }
            //});
            get { return new ColorMatrix(new float[][] { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } }); }
        }

        #region ColorMatrix

        public static ColorMatrix Reset(this ColorMatrix obj)
        {
            return new ColorMatrix(new float[][] { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 } });
        }

        public static float[][] ToArray(this ColorMatrix obj)
        {
            float[][] colorMatrixElements =
            {
                new float[] {obj[0,0], obj[0,1], obj[0,2], obj[0,3], obj[0,4]},
                new float[] {obj[1,0], obj[1,1], obj[1,2], obj[1,3], obj[1,4]},
                new float[] {obj[2,0], obj[2,1], obj[2,2], obj[2,3], obj[2,4]},
                new float[] {obj[3,0], obj[3,1], obj[3,2], obj[3,3], obj[3,4]},
                new float[] {obj[4,0], obj[4,1], obj[4,2], obj[4,3], obj[4,4]}
            };
            return colorMatrixElements;
        }

        public static float[] GetMatrixRow(this ColorMatrix obj, int rowindex)
        {
            if (!rowindex.Between(0, 4))
                throw new ArgumentOutOfRangeException("rowindex", "rowindex must be between 0 and 4");
            float[] rtnflarr = new float[5];
            for (int i = 0; i < 5; i++)
                rtnflarr[i] = obj[rowindex, i];
            return rtnflarr;
        }

        public static ColorMatrix SetMatrixRow(this ColorMatrix obj, int rowindex, params float[] columns)
        {
            if (!rowindex.Between(0, 4))
                throw new ArgumentOutOfRangeException("rowindex", "rowindex must be between 0 and 4");
            if (!columns.Length.Between(1, 5))
                throw new ArgumentOutOfRangeException("row", "row array must an length between 1 and 5");
            float[][] tmparr = obj.ToArray();
            tmparr[rowindex] = columns;
            return new ColorMatrix(tmparr);
        }

        public static ColorMatrix SetMatrixValue(this ColorMatrix obj, int rowindex, int columnindex, float value)
        {
            if (!rowindex.Between(0, 4))
                throw new ArgumentOutOfRangeException("rowindex", "rowindex must be between 0 and 4");
            if (!columnindex.Between(0, 4))
                throw new ArgumentOutOfRangeException("columnindex", "columnindex must an between 0 and 4");
            obj[rowindex, columnindex] = value;
            return obj;
        }

        public static ColorMatrix MakeBrightnessMatrix(this ColorMatrix obj, float value)
        {
            // return obj.ScaleColor(value, value, value);
            return new ColorMatrix(new float[][]
            {
                new float[] { 1f, 0, 0, 0, 0 },
                new float[] { 0, 1f, 0, 0, 0 },
                new float[] { 0, 0, 1f, 0, 0 },
                new float[] { 0, 0, 0, 1f, 0 },
                new float[] { value, value, value, 0, 1f }
            });
        }

        public static ColorMatrix MakeContrastMatrix(this ColorMatrix obj, float value)
        {
            //return obj.Reset().SetMatrixValue(0, 0, value).SetMatrixValue(1, 1, value).SetMatrixValue(2, 2, value);
            return new ColorMatrix(new float[][]
            {
                new float[] { value, 0, 0, 0, 0 },
                new float[] { 0, value, 0, 0, 0 },
                new float[] { 0, 0, value, 0, 0 },
                new float[] { 0, 0, 0, 1f, 0 },
                new float[] { 0, 0, 0, 0, 1f }
            });
        }

        public static ColorMatrix MakeTintMatrix(this ColorMatrix obj, float value)
        {
            //return obj.SetMatrixRow(3, ((float)value / 100f), ((float)value / 100f), ((float)value / 100f), 1f, 0f);
            return new ColorMatrix(new float[][]
            {
                new float[] { 1f, 0, 0, 0, 0 },
                new float[] { 0, 1f, 0, 0, 0 },
                new float[] { 0, 0, 1f, 0, 0 },
                new float[] { ((float)value / 100f), ((float)value / 100f), ((float)value / 100f), 1f, 0 },
                new float[] { 0, 0, 0, 0, 1f }
            });
        }

        public static ColorMatrix MakeInvertColorMatrix(this ColorMatrix obj)
        {
            //return obj.Reset().SetMatrixValue(0, 0, -1).SetMatrixValue(1, 1, -1).SetMatrixValue(2, 2, -1);
            return new ColorMatrix(new float[][]
            {
                new float[] { -1f, 0, 0, 0, 0 },
                new float[] { 0, -1f, 0, 0, 0 },
                new float[] { 0, 0, -1f, 0, 0 },
                new float[] { 0, 0, 0, 1f, 0 },
                new float[] { 0, 0, 0, 0, 1f }
            });
        }

        public static ColorMatrix MakeColorMatrix(this ColorMatrix obj)
        {
            //{ 1f, 0f, 0f }, { 0.5f, 1.0f, 0.5f }, { 0.2f, 0.4f, 0.6f }
            return new ColorMatrix(new float[][]
            {
                new float[] { 1f, 0, 0, 0, 0 },
                new float[] { 0.5f, 1f, 0.5f, 0, 0 },
                new float[] { 0.2f, 0.4f, 0.6f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });
        }

        public static ColorMatrix ScaleColor(this ColorMatrix obj, int Red, int Green, int Blue)
        {
            if (!Red.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Red", "Red must be between -100 and 100 percent");
            if (!Green.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Green", "Green must be between -100 and 100 percent");
            if (!Blue.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Blue", "Blue must be between -100 and 100 percent");
            obj.Matrix40 = ((float)Red / 20f);
            obj.Matrix41 = ((float)Green / 20f);
            obj.Matrix42 = ((float)Blue / 20f);
            return obj;
        }

        public static ColorMatrix ScaleColor(this ColorMatrix obj, float Red, float Green, float Blue)
        {
            if (!Red.Between(-5, 5))
                throw new ArgumentOutOfRangeException("Red", "Red must be between -5 and 5 percent");
            if (!Green.Between(-5, 5))
                throw new ArgumentOutOfRangeException("Green", "Green must be between -5 and 5 percent");
            if (!Blue.Between(-5, 5))
                throw new ArgumentOutOfRangeException("Blue", "Blue must be between -5 and 5 percent");
            obj.Matrix40 = Red;
            obj.Matrix41 = Green;
            obj.Matrix42 = Blue;
            return obj;
        }

        public static ColorMatrix ScaleColor(this ColorMatrix obj, int Percent)
        {
            if (!Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Percent", "Percent must be between -100 and 100");
            return obj.ScaleColor(Percent, Percent, Percent);
        }

        public static ColorMatrix Multiply(this ColorMatrix obj, ColorMatrix multiplier)
        {
            return obj.Multiply(multiplier.ToArray());
        }

        public static ColorMatrix Multiply(this ColorMatrix obj, float[][] multiplier)
        {
            ColorMatrix rtnmatrix = new ColorMatrix();
            float[] matrixcolumn = new float[5];
            for (int j = 0; j < 5; j++)
            {
                for (int x = 0; x < 5; x++)
                    matrixcolumn[x] = obj[x, j];
                for (int y = 0; y < 5; y++)
                {
                    float s = 0;
                    for (int x = 0; x < 5; x++)
                        s += multiplier[y][x] * matrixcolumn[x];
                    rtnmatrix[y, j] = s;
                }
            }
            return rtnmatrix;
        }

        public static ColorMatrix Rotate(this ColorMatrix obj, float degrees)
        {
            double r = degrees * System.Math.PI / 180; // degrees to radians
            //ColorMatrix rtnmatrix = obj.SetMatrixRow(0, new float[] { (float)System.Math.Cos(r), (float)System.Math.Sin(r), 0, 0, 0 });
            //rtnmatrix = rtnmatrix.SetMatrixRow(1, new float[] { (float)-System.Math.Sin(r), (float)-System.Math.Cos(r), 0, 0, 0 });

            //ColorMatrix rtnmatrix = obj.SetMatrixRow(0, new float[] { (float)Math.Cos(r), (float)Math.Sin(r), 0, 0, 0 });
            //rtnmatrix = rtnmatrix.SetMatrixRow(1, new float[] { (float)-Math.Sin(r), (float)-Math.Cos(r), 0, 0, 0 });
            //rtnmatrix = rtnmatrix.SetMatrixRow(2, new float[] { 0, 0, 2, 0, 0 });
            float[][] colorMatrixElements =
            {
                new float[] {(float)System.Math.Cos(r), (float)System.Math.Sin(r), 0, 0, 0},
                new float[] {(float)-System.Math.Sin(r), (float)System.Math.Cos(r), 0, 0, 0},
                new float[] {0, 0, 2, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
                //obj.GetMatrixRow(2),
                //obj.GetMatrixRow(3),
                //obj.GetMatrixRow(4)
            };
            return new ColorMatrix(colorMatrixElements);
            //return rtnmatrix;
        }

        public static ColorMatrix Rotate(this ColorMatrix obj, float degrees, ColorChannelRGBA channel)
        {
            double rad = degrees * (Math.PI / 180d);
            int x = 0;
            int y = 0;
            switch (channel)
            {
                case ColorChannelRGBA.R:
                    y = 2;
                    x = 1;
                    break;

                case ColorChannelRGBA.G:
                    y = 0;
                    x = 2;
                    break;

                case ColorChannelRGBA.B:
                    y = 1;
                    x = 0;
                    break;

                case ColorChannelRGBA.A:
                    y = 0;
                    x = 0;
                    break;

                default:
                    y = 0;
                    x = 0;
                    break;
            }
            float[][] farray = DrawingExtensions.DefaultColorMatrix.ToArray();
            farray[x][x] = farray[y][y] = (float)Math.Cos(rad);

            float s = (float)Math.Sin(rad);
            //if (channel == ColorChannelRGBA.G)
            //{
            farray[y][x] = s;
            farray[x][y] = -s;
            //}
            //else
            //{
            //    farray[y][x] = -s;
            //    farray[x][y] = s;
            //}

            //Multiply(m, order);
            return obj.Multiply(farray);
        }

        #endregion ColorMatrix

        #region Image

        #region Help Methods NOT EXTENSIONS and not public

        private static Image CombineFrames(Image firstpage, Bitmap[] pages, EncoderParameters ep)
        {
            Bitmap Tiff = null;
            MemoryStream ms = new MemoryStream();
            foreach (Bitmap page in pages)
            {
                if (Tiff == null)
                {
                    if (firstpage != null)
                    {
                        Tiff = new Bitmap(firstpage);
                        Tiff.Save(ms, ImageFormat.Tiff.GetImageCodecInfo(), ep);
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                        Tiff.SaveAdd(page, ep);
                    }
                    else
                    {
                        Tiff = new Bitmap(page);
                        Tiff.Save(ms, ImageFormat.Tiff.GetImageCodecInfo(), ep);
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                    }
                }
                else
                    Tiff.SaveAdd(page, ep);
            }
            ms.Flush();
            Image rtnimg = Image.FromStream(ms);
            ms.Close();
            return rtnimg;
        }

        private static void CombineFrames(Image firstpage, Bitmap[] pages, EncoderParameters ep, string filename)
        {
            Bitmap Tiff = null;
            FileStream ms = new FileStream(filename, FileMode.Create);
            foreach (Bitmap page in pages)
            {
                if (Tiff == null)
                {
                    if (firstpage != null)
                    {
                        Tiff = new Bitmap(firstpage);
                        Tiff.Save(ms, ImageFormat.Tiff.GetImageCodecInfo(), ep);
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                        Tiff.SaveAdd(page, ep);
                    }
                    else
                    {
                        Tiff = new Bitmap(page);
                        Tiff.Save(ms, ImageFormat.Tiff.GetImageCodecInfo(), ep);
                        ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                    }
                }
                else
                    Tiff.SaveAdd(page, ep);
            }
            ms.Flush();
            ms.Close();
        }

        #endregion Help Methods NOT EXTENSIONS and not public

        #region Help Methods NOT EXTENSIONS and public

        public static void FixGenericGDIplusError(ref Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            bmp.MakeTransparent();
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img, bmp.GetRectangle(), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            img.Dispose();
            img = bmp;
        }

        #endregion Help Methods NOT EXTENSIONS and public

        public static Image AddFrames(this Image obj, Bitmap[] pages)
        {
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            return CombineFrames(obj, pages, ep);
        }

        public static Image AddFrames(this Image obj, Bitmap[] pages, CompressionLevel compress)
        {
            EncoderParameters ep = new EncoderParameters(2);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
            return CombineFrames(obj, pages, ep);
        }

        public static Image AddFrames(this Image obj, Bitmap[] pages, CompressionLevel compress, ColorDepth ColorDepth)
        {
            EncoderParameters ep = new EncoderParameters(3);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            ep.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)compress);
            ep.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, (long)ColorDepth);
            return CombineFrames(obj, pages, ep);
        }

        public static Image Brightness(this Image obj, int Percent)
        {
            if (!Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Percent", "Percent must be between -100 and 100");
            return obj.SetColorMatrix(DefaultColorMatrix.MakeBrightnessMatrix((float)Percent / 100f));
        }

        public static Image Compress(this Image obj, ImageFormat format, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");
            MemoryStream ms = new MemoryStream();
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
            obj.Save(ms, format.GetImageCodecInfo(), ep);
            ms.Flush();
            return Image.FromStream(ms);
        }

        public static Image Contrast(this Image obj, int Percent)
        {
            if (!Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Percent", "Percent must be between -100 and 100");
            return obj.SetColorMatrix(DefaultColorMatrix.MakeContrastMatrix((float)Percent / 100f));
        }

        public static Image Crop(this Image obj, Rectangle cropArea)
        {
            try
            {
                Bitmap bmpImage = new Bitmap(obj);
                Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                return (Image)(bmpCrop);
            }
            catch
            {
                throw;
            }
        }

        public static Image DrawString(this Image obj, string text, Font font, Brush brush, ContentAlignment textAlign, TextRenderingHint textRender)
        {
            Bitmap myBitmap = new Bitmap(obj);
            using (Graphics grObj = Graphics.FromImage(myBitmap))
            {
                grObj.TextRenderingHint = textRender;
                StringFormat strFormat = new StringFormat();

                #region Alignment Switch

                switch (textAlign)
                {
                    case ContentAlignment.BottomCenter:
                        {
                            strFormat.Alignment = StringAlignment.Center;
                            strFormat.LineAlignment = StringAlignment.Far;
                            break;
                        }
                    case ContentAlignment.BottomLeft:
                        {
                            strFormat.Alignment = StringAlignment.Near;
                            strFormat.LineAlignment = StringAlignment.Far;
                            break;
                        }
                    case ContentAlignment.BottomRight:
                        {
                            strFormat.Alignment = StringAlignment.Far;
                            strFormat.LineAlignment = StringAlignment.Far;
                            break;
                        }
                    case ContentAlignment.MiddleCenter:
                        {
                            strFormat.Alignment = StringAlignment.Center;
                            strFormat.LineAlignment = StringAlignment.Center;
                            break;
                        }
                    case ContentAlignment.MiddleLeft:
                        {
                            strFormat.Alignment = StringAlignment.Near;
                            strFormat.LineAlignment = StringAlignment.Center;
                            break;
                        }
                    case ContentAlignment.MiddleRight:
                        {
                            strFormat.Alignment = StringAlignment.Far;
                            strFormat.LineAlignment = StringAlignment.Center;
                            break;
                        }
                    case ContentAlignment.TopCenter:
                        {
                            strFormat.Alignment = StringAlignment.Center;
                            strFormat.LineAlignment = StringAlignment.Near;
                            break;
                        }
                    case ContentAlignment.TopLeft:
                        {
                            strFormat.Alignment = StringAlignment.Near;
                            strFormat.LineAlignment = StringAlignment.Near;
                            break;
                        }
                    case ContentAlignment.TopRight:
                        {
                            strFormat.Alignment = StringAlignment.Far;
                            strFormat.LineAlignment = StringAlignment.Near;
                            break;
                        }
                    default:
                        {
                            strFormat.Alignment = StringAlignment.Near;
                            strFormat.LineAlignment = StringAlignment.Near;
                            break;
                        }
                }

                #endregion Alignment Switch

                grObj.DrawString(text, font, brush, new RectangleF(0, 0, obj.Width, obj.Height), strFormat);
            }
            return (Image)myBitmap;
        }

        public static Image DrawString(this Image obj, string text, Font font, Brush brush, ContentAlignment textAlign)
        {
            return obj.DrawString(text, font, brush, textAlign, TextRenderingHint.SystemDefault);
        }

        public static Image DrawString(this Image obj, string text, ContentAlignment textAlign)
        {
            return obj.DrawString(text, SystemFonts.DefaultFont, Brushes.Black, textAlign);
        }

        public static Image[] GetFrames(this Image obj, ImageFormat format)
        {
            try
            {
                if (obj.RawFormat.Equals(ImageFormat.Tiff) || obj.RawFormat.Equals(ImageFormat.Gif))
                {
                    List<Image> rtnarr = new List<Image>();
                    FrameDimension fd = new FrameDimension(obj.FrameDimensionsList[0]);
                    for (int i = 0; i < obj.GetFrameCount(fd); i++)
                    {
                        obj.SelectActiveFrame(fd, i);
                        using (MemoryStream byteStream = new MemoryStream())
                        {
                            obj.Save(byteStream, format);
                            rtnarr.Add(Image.FromStream(byteStream));
                        }
                    }
                    return rtnarr.ToArray();
                }
                else
                    return new Image[] { obj };
            }
            catch
            {
                return new Image[] { obj };
            }
        }

        public static Rectangle GetRectangle(this Image obj)
        {
            return new Rectangle(0, 0, obj.Width, obj.Height);
        }

        public static Rectangle GetRectangleTilt(this Image obj)
        {
            return new Rectangle(0, 0, obj.Height, obj.Width);
        }

        public static Image GrayScale(this Image obj)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            return obj.SetColorMatrix(colorMatrix);
        }

        public static Image Invert(this Image obj)
        {
            return obj.SetColorMatrix(DefaultColorMatrix.MakeInvertColorMatrix());
        }

        public static Image Merge(this Image obj, Image mergeWith, ContentAlignment alignment)
        {
            int xpos, ypos;
            switch (alignment)
            {
                case ContentAlignment.BottomCenter:
                    xpos = (obj.Width - mergeWith.Width) / 2;
                    ypos = (obj.Height - mergeWith.Height);
                    break;

                case ContentAlignment.BottomLeft:
                    xpos = 0;
                    ypos = (obj.Height - mergeWith.Height);
                    break;

                case ContentAlignment.BottomRight:
                    xpos = (obj.Width - mergeWith.Width);
                    ypos = (obj.Height - mergeWith.Height);
                    break;

                case ContentAlignment.MiddleCenter:
                    xpos = (obj.Width - mergeWith.Width) / 2;
                    ypos = (obj.Height - mergeWith.Height) / 2;
                    break;

                case ContentAlignment.MiddleLeft:
                    xpos = 0;
                    ypos = (obj.Height - mergeWith.Height) / 2;
                    break;

                case ContentAlignment.MiddleRight:
                    xpos = (obj.Width - mergeWith.Width);
                    ypos = (obj.Height - mergeWith.Height) / 2;
                    break;

                case ContentAlignment.TopCenter:
                    xpos = (obj.Width - mergeWith.Width) / 2;
                    ypos = 0;
                    break;

                case ContentAlignment.TopLeft:
                    xpos = 0;
                    ypos = 0;
                    break;

                case ContentAlignment.TopRight:
                    xpos = (obj.Width - mergeWith.Width);
                    ypos = 0;
                    break;

                default:
                    xpos = 0;
                    ypos = 0;
                    break;
            }
            Bitmap img = new Bitmap(obj);
            Graphics g = Graphics.FromImage(img);
            g.DrawImageUnscaled(mergeWith, xpos, ypos);
            g.Save();
            return img;
        }

        public static Image MergeToTiff(this Bitmap[] obj)
        {
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            return CombineFrames(null, obj, ep);
        }

        public static void MergeToTiff(this Bitmap[] obj, string filename)
        {
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            CombineFrames(null, obj, ep, filename);
        }

        public static Image Resize(this Image obj, float Percent)
        {
            return obj.Resize(Percent, obj.PixelFormat);
        }

        public static Image Resize(this Image obj, float Percent, InterpolationMode interpolationMode)
        {
            return obj.Resize(Percent, obj.PixelFormat, interpolationMode);
        }

        public static Image Resize(this Image obj, float Percent, PixelFormat pixelFormat)
        {
            return obj.Resize(Percent, pixelFormat, obj.HorizontalResolution, obj.VerticalResolution);
        }

        public static Image Resize(this Image obj, float Percent, PixelFormat pixelFormat, InterpolationMode interpolationMode)
        {
            return obj.Resize(Percent, pixelFormat, obj.HorizontalResolution, obj.VerticalResolution, interpolationMode);
        }

        public static Image Resize(this Image obj, float Percent, PixelFormat pixelFormat, float horizontalResolution, float verticalResolution)
        {
            return obj.Resize(Percent, pixelFormat, horizontalResolution, verticalResolution, InterpolationMode.HighQualityBicubic);
        }

        public static Image Resize(this Image obj, float Percent, PixelFormat pixelFormat, float horizontalResolution, float verticalResolution, InterpolationMode interpolationMode)
        {
            try
            {
                float nPercent = Percent; // ((float)Percent / 100);
                int destWidth = (int)Math.Round(((float)obj.Width * nPercent));
                int destHeight = (int)Math.Round(((float)obj.Height * nPercent));
                Bitmap bmObj = new Bitmap(destWidth, destHeight, pixelFormat);
                bmObj.SetResolution(horizontalResolution, verticalResolution);
                using (Graphics grObj = Graphics.FromImage(bmObj))
                {
                    grObj.Clear(Color.Transparent);
                    grObj.InterpolationMode = interpolationMode;
                    grObj.DrawImage(obj, new Rectangle(0, 0, destWidth, destHeight), new Rectangle(0, 0, obj.Width, obj.Height), GraphicsUnit.Pixel);
                }
                return (Image)bmObj;
            }
            catch { throw; }
        }

        public static Image Resize(this Image obj, int Width, int Height)
        {
            return obj.Resize(Width, Height, obj.PixelFormat);
        }

        public static Image Resize(this Image obj, int Width, int Height, InterpolationMode interpolationMode)
        {
            return obj.Resize(Width, Height, obj.PixelFormat, interpolationMode);
        }

        public static Image Resize(this Image obj, int Width, int Height, PixelFormat pixelFormat)
        {
            return obj.Resize(Width, Height, pixelFormat, obj.HorizontalResolution, obj.VerticalResolution);
        }

        public static Image Resize(this Image obj, int Width, int Height, PixelFormat pixelFormat, InterpolationMode interpolationMode)
        {
            return obj.Resize(Width, Height, pixelFormat, obj.HorizontalResolution, obj.VerticalResolution, interpolationMode);
        }

        public static Image Resize(this Image obj, int Width, int Height, PixelFormat pixelFormat, float horizontalResolution, float verticalResolution)
        {
            return obj.Resize(Width, Height, pixelFormat, horizontalResolution, verticalResolution, InterpolationMode.HighQualityBicubic);
        }

        public static Image Resize(this Image obj, int Width, int Height, PixelFormat pixelFormat, float horizontalResolution, float verticalResolution, InterpolationMode interpolationMode)
        {
            try
            {
                int sourceWidth = obj.Width;
                int sourceHeight = obj.Height;
                int destX = 0;
                int destY = 0;

                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                nPercentW = ((float)Width / (float)sourceWidth);
                nPercentH = ((float)Height / (float)sourceHeight);
                if (nPercentH < nPercentW)
                {
                    nPercent = nPercentH;
                    destX = System.Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
                }
                else
                {
                    nPercent = nPercentW;
                    destY = System.Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
                }

                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);

                Bitmap bmObj = new Bitmap(Width, Height, pixelFormat);
                bmObj.SetResolution(horizontalResolution, verticalResolution);

                using (Graphics grObj = Graphics.FromImage(bmObj))
                {
                    grObj.Clear(Color.Transparent);
                    grObj.InterpolationMode = interpolationMode;
                    grObj.DrawImage(obj, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
                }
                return (Image)bmObj;
            }
            catch { throw; }
        }

        public static Image Resize(this Image obj, int Size, bool KeepAspectRatio)
        {
            if (KeepAspectRatio)
            {
                if (obj.Width >= obj.Height)
                    return obj.Resize(((float)Size / (float)obj.Width));
                else
                    return obj.Resize(((float)Size / (float)obj.Height));
            }
            else
                return obj.Resize(Size, Size);
        }

        public static Image Resize(this Image obj, int Size, bool KeepAspectRatio, InterpolationMode interpolationMode)
        {
            if (KeepAspectRatio)
            {
                if (obj.Width >= obj.Height)
                    return obj.Resize(((float)Size / (float)obj.Width), interpolationMode);
                else
                    return obj.Resize(((float)Size / (float)obj.Height), interpolationMode);
            }
            else
                return obj.Resize(Size, Size, interpolationMode);
        }

        public static Image Resize(this Image obj, int Size, bool KeepAspectRatio, PixelFormat pixelFormat, InterpolationMode interpolationMode)
        {
            if (KeepAspectRatio)
            {
                if (obj.Width >= obj.Height)
                    return obj.Resize(((float)Size / (float)obj.Width), pixelFormat, interpolationMode);
                else
                    return obj.Resize(((float)Size / (float)obj.Height), pixelFormat, interpolationMode);
            }
            else
                return obj.Resize(Size, Size, pixelFormat, interpolationMode);
        }

        public static Image Rotate(this Image obj, int angle)
        {
            Bitmap bmObj = new Bitmap(obj.Width, obj.Height);
            Graphics grObj = Graphics.FromImage(bmObj);
            grObj.TranslateTransform((float)obj.Width / 2, (float)obj.Height / 2);
            grObj.RotateTransform(angle);
            grObj.TranslateTransform(-(float)obj.Width / 2, -(float)obj.Height / 2);
            grObj.DrawImage(obj, new Point(0, 0));
            return bmObj;
        }

        public static Image ScaleColor(this Image obj, int R_Percent, int G_Percent, int B_Percent)
        {
            if (!R_Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("R_Percent", "R_Percent must be between -100 and 100");
            if (!G_Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("G_Percent", "G_Percent must be between -100 and 100");
            if (!B_Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("B_Percent", "B_Percent must be between -100 and 100");
            return obj.SetColorMatrix(DefaultColorMatrix.ScaleColor(R_Percent, G_Percent, B_Percent));
        }

        public static Image SetColorMatrix(this Image obj, ColorMatrix colormatrix)
        {
            return obj.SetColorMatrix(colormatrix, GraphicsUnit.Pixel);
        }

        public static Image SetColorMatrix(this Image obj, ColorMatrix colormatrix, GraphicsUnit graphicunit)
        {
            try
            {
                Bitmap bmObj = new Bitmap(obj.Width, obj.Height);
                using (Graphics g = Graphics.FromImage(bmObj))
                {
                    ImageAttributes attributes = new ImageAttributes();
                    attributes.SetColorMatrix(colormatrix);
                    g.DrawImage(obj, new Rectangle(0, 0, obj.Width, obj.Height), 0, 0, obj.Width, obj.Height, graphicunit, attributes);
                }
                return (Image)bmObj;
            }
            catch { throw; }
        }

        public static Image ToImage(this byte[] obj)
        {
            MemoryStream ms = new MemoryStream(obj);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static byte[] ToByteArray(this Image obj)
        {
            byte[] rtnval = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Image img = obj.FixGenericGDIplusError();
                img.Save(ms, img.RawFormat);
                rtnval = ms.GetBuffer();
            }
            return rtnval;
        }

        public static Stream ToStream(this Image obj)
        {
            MemoryStream ms = new MemoryStream();
            Image img = obj.FixGenericGDIplusError();
            img.Save(ms, img.RawFormat);
            return ms;
        }

        public static Icon ToIcon(this Image obj)
        {
            try
            {
                Bitmap bmObj = new Bitmap(obj);
                return Icon.FromHandle(bmObj.GetHicon());
            }
            catch
            {
                try
                {
                    MemoryStream buffer = new MemoryStream();
                    obj.Save(buffer, ImageFormat.Icon);
                    buffer.Position = 0;
                    return new Icon(buffer);
                }
                catch
                {
                    throw;
                }
            }
        }

        public static Icon ToIcon(this Image obj, int Size, bool KeepAspectRatio)
        {
            return obj.Resize(Size, KeepAspectRatio).ToIcon();
        }

        public static Image Tint(this Image obj, int Percent)
        {
            if (!Percent.Between(-100, 100))
                throw new ArgumentOutOfRangeException("Percent", "Percent must be between -100 and 100");
            return obj.SetColorMatrix(DefaultColorMatrix.MakeTintMatrix((float)Percent));
        }

        public static Image FixGenericGDIplusError(this Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            bmp.MakeTransparent();
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            return bmp;
        }

        public static string ToBase64(this Image obj)
        {
            return Convert.ToBase64String(obj.ToByteArray());
        }

        public static Image FromBase64(this string obj)
        {
            return Convert.FromBase64String(obj).ToImage();
        }

        #endregion Image

        #region Font

        /// <summary>
        /// Sets the style of the font.
        /// </summary>
        /// <param name="obj">The font type to change</param>
        /// <param name="style">style to apply</param>
        /// <returns>new font that has the new style applied.</returns>
        /// <remarks>Developed by Paw Jershauge. Find more Extensions and C# code on: http://pawjershauge.blogspot.com</remarks>
        public static Font SetStyle(this Font obj, FontStyle style)
        {
            return new Font(obj, style);
        }

        #endregion Font

        public static ImageCodecInfo GetImageCodecInfo(this ImageFormat obj)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
                if (codec.FormatID == obj.Guid)
                    return codec;
            return null;
        }
    }
}