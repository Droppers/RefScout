// ReSharper disable All

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using SharpVectors.Converters;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers.Utils;
using SharpVectors.Renderers.Wpf;

namespace RefScout.Wpf.Helpers
{
    public sealed class ImageSvgConverterEx : SvgConverter
    {
        /// <summary>
        ///     This is the last drawing generated.
        /// </summary>
        private DrawingGroup _drawing;

        /// <overloads>
        ///     Initializes a new instance of the <see cref="ImageSvgConverterEx" /> class.
        /// </overloads>
        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageSvgConverterEx" /> class
        ///     with the specified drawing or rendering settings.
        /// </summary>
        /// <param name="settings">
        ///     This specifies the settings used by the rendering or drawing engine.
        ///     If this is <see langword="null" />, the default settings is used.
        /// </param>
        public ImageSvgConverterEx(WpfDrawingSettings settings)
            : this(false, false, settings) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageSvgConverterEx" /> class
        ///     with the specified drawing or rendering settings and the saving options.
        /// </summary>
        /// <param name="saveXaml">
        ///     This specifies whether to save result object tree in image file.
        /// </param>
        /// <param name="saveZaml">
        ///     This specifies whether to save result object tree in ZAML file. The
        ///     ZAML is simply a G-Zip compressed image format, similar to the SVGZ.
        /// </param>
        /// <param name="settings">
        ///     This specifies the settings used by the rendering or drawing engine.
        ///     If this is <see langword="null" />, the default settings is used.
        /// </param>
        public ImageSvgConverterEx(
            bool saveXaml,
            bool saveZaml,
            WpfDrawingSettings settings) : base(saveXaml, saveZaml, settings)
        {
            long pixelWidth = 0;
            long pixelHeight = 0;

            if (settings != null)
            {
                //settings.EnsureViewboxSize = true;

                settings.EnsureViewboxSize = false;
                settings.IgnoreRootViewbox = true;

                if (settings.HasPixelSize)
                {
                    pixelWidth = settings.PixelWidth;
                    pixelHeight = settings.PixelHeight;
                }
            }

            EncoderType = ImageEncoderType.PngBitmap;
            _wpfRenderer = new WpfDrawingRenderer(DrawingSettings);
            _wpfWindow = new WpfSvgWindow(pixelWidth, pixelHeight, _wpfRenderer);
        }

        /// <summary>
        ///     Gets a value indicating whether a writer error occurred when
        ///     using the custom image writer.
        /// </summary>
        /// <value>
        ///     This is <see langword="true" /> if an error occurred when using
        ///     the custom image writer; otherwise, it is <see langword="false" />.
        /// </value>
        public bool WriterErrorOccurred { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to fall back and use
        ///     the .NET Framework image writer when an error occurred in using the
        ///     custom writer.
        /// </summary>
        /// <value>
        ///     This is <see langword="true" /> if the converter falls back to using
        ///     the system image writer when an error occurred in using the custom
        ///     writer; otherwise, it is <see langword="false" />. If <see langword="false" />,
        ///     an exception, which occurred in using the custom writer will be
        ///     thrown. The default is <see langword="false" />.
        /// </value>
        public bool FallbackOnWriterError { get; set; }

        /// <summary>
        ///     Gets or set the bitmap encoder type to use in encoding the drawing
        ///     to an image file.
        /// </summary>
        /// <value>
        ///     An enumeration of the type <see cref="ImageEncoderType" /> specifying
        ///     the bitmap encoder. The default is the <see cref="ImageEncoderType.PngBitmap" />.
        /// </value>
        public ImageEncoderType EncoderType { get; set; }

        /// <summary>
        ///     Gets or sets a custom bitmap encoder to use in encoding the drawing
        ///     to an image file.
        /// </summary>
        /// <value>
        ///     A derived <see cref="BitmapEncoder" /> object specifying the bitmap
        ///     encoder for encoding the images. The default is <see langword="null" />,
        ///     and the <see cref="EncoderType" /> property determines the encoder used.
        /// </value>
        /// <remarks>
        ///     If the value of this is set, it must match the MIME type or file
        ///     extension defined by the <see cref="EncoderType" /> property for it
        ///     to be used.
        /// </remarks>
        public BitmapEncoder Encoder { get; set; }

        /// <summary>
        ///     Gets the last created drawing.
        /// </summary>
        /// <value>
        ///     A <see cref="DrawingGroup" /> specifying the last converted drawing.
        /// </value>
        public DrawingGroup Drawing => _drawing;

        /// <summary>
        ///     Gets the output XAML file path if generated.
        /// </summary>
        /// <value>
        ///     A string containing the full path to the XAML if generated; otherwise,
        ///     it is <see langword="null" />.
        /// </value>
        public string XamlFile { get; private set; }

        /// <summary>
        ///     Gets the output ZAML file path if generated.
        /// </summary>
        /// <value>
        ///     A string containing the full path to the ZAML if generated; otherwise,
        ///     it is <see langword="null" />.
        /// </value>
        public string ZamlFile { get; private set; }

        /// <overloads>
        ///     This performs the conversion of the specified SVG file, and saves
        ///     the output to an image file.
        /// </overloads>
        /// <summary>
        ///     This performs the conversion of the specified SVG file, and saves
        ///     the output to an image file with the same file name.
        /// </summary>
        /// <param name="svgFileName">
        ///     The full path of the SVG source file.
        /// </param>
        /// <returns>
        ///     This returns <see langword="true" /> if the conversion is successful;
        ///     otherwise, it return <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="svgFileName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="svgFileName" /> is empty.
        ///     <para>-or-</para>
        ///     If the <paramref name="svgFileName" /> does not exists.
        /// </exception>
        public bool Convert(string svgFileName) => Convert(svgFileName, string.Empty);

        /// <summary>
        ///     This performs the conversion of the specified SVG file, and saves
        ///     the output to the specified image file.
        /// </summary>
        /// <param name="svgFileName">
        ///     The full path of the SVG source file.
        /// </param>
        /// <param name="imageFileName">
        ///     The output image file. This is optional. If not specified, an image
        ///     file is created in the same directory as the SVG file.
        /// </param>
        /// <returns>
        ///     This returns <see langword="true" /> if the conversion is successful;
        ///     otherwise, it return <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="svgFileName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="svgFileName" /> is empty.
        ///     <para>-or-</para>
        ///     If the <paramref name="svgFileName" /> does not exists.
        /// </exception>
        public bool Convert(string svgFileName, string imageFileName)
        {
            if (svgFileName == null)
            {
                throw new ArgumentNullException(nameof(svgFileName),
                    "The SVG source file cannot be null (or Nothing).");
            }

            if (svgFileName.Length == 0)
            {
                throw new ArgumentException("The SVG source file cannot be empty.", nameof(svgFileName));
            }

            if (!File.Exists(svgFileName))
            {
                throw new ArgumentException("The SVG source file must exists.", nameof(svgFileName));
            }

            XamlFile = null;
            ZamlFile = null;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                var workingDir = Path.GetDirectoryName(imageFileName);
                if (!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                }
            }

            return ProcessFile(svgFileName, imageFileName);
        }

        /// <summary>
        ///     This performs the conversion of the specified SVG source, and saves
        ///     the output to the specified image file.
        /// </summary>
        /// <param name="svgStream">
        ///     A stream providing access to the SVG source data.
        /// </param>
        /// <param name="imageFileName">
        ///     The output image file. This is optional. If not specified, an image
        ///     file is created in the same directory as the SVG file.
        /// </param>
        /// <returns>
        ///     This returns <see langword="true" /> if the conversion is successful;
        ///     otherwise, it return <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="imageFileName" /> is <see langword="null" />.
        ///     <para>-or-</para>
        ///     If the <paramref name="svgStream" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="imageFileName" /> is empty.
        /// </exception>
        public bool Convert(Stream svgStream, string imageFileName)
        {
            if (svgStream == null)
            {
                throw new ArgumentNullException(nameof(svgStream),
                    "The SVG source file cannot be null (or Nothing).");
            }

            if (imageFileName == null)
            {
                throw new ArgumentNullException(nameof(imageFileName),
                    "The image destination file path cannot be null (or Nothing).");
            }

            if (imageFileName.Length == 0)
            {
                throw new ArgumentException("The image destination file path cannot be empty.", nameof(imageFileName));
            }

            XamlFile = null;
            ZamlFile = null;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                var workingDir = Path.GetDirectoryName(imageFileName);
                if (!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                }
            }

            return ProcessFile(svgStream, imageFileName);
        }

        /// <summary>
        ///     This performs the conversion of the specified SVG source, and saves
        ///     the output to the specified image file.
        /// </summary>
        /// <param name="svgTextReader">
        ///     A text reader providing access to the SVG source data.
        /// </param>
        /// <param name="imageFileName">
        ///     The output image file. This is optional. If not specified, an image
        ///     file is created in the same directory as the SVG file.
        /// </param>
        /// <returns>
        ///     This returns <see langword="true" /> if the conversion is successful;
        ///     otherwise, it return <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="imageFileName" /> is <see langword="null" />.
        ///     <para>-or-</para>
        ///     If the <paramref name="svgTextReader" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="imageFileName" /> is empty.
        /// </exception>
        public bool Convert(TextReader svgTextReader, string imageFileName)
        {
            if (svgTextReader == null)
            {
                throw new ArgumentNullException(nameof(svgTextReader),
                    "The SVG source file cannot be null (or Nothing).");
            }

            if (imageFileName == null)
            {
                throw new ArgumentNullException(nameof(imageFileName),
                    "The image destination file path cannot be null (or Nothing).");
            }

            if (imageFileName.Length == 0)
            {
                throw new ArgumentException("The image destination file path cannot be empty.", nameof(imageFileName));
            }

            XamlFile = null;
            ZamlFile = null;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                var workingDir = Path.GetDirectoryName(imageFileName);
                if (!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                }
            }

            return ProcessFile(svgTextReader, imageFileName);
        }

        /// <summary>
        ///     This performs the conversion of the specified SVG source, and saves
        ///     the output to the specified image file.
        /// </summary>
        /// <param name="svgXmlReader">
        ///     An XML reader providing access to the SVG source data.
        /// </param>
        /// <param name="imageFileName">
        ///     The output image file. This is optional. If not specified, an image
        ///     file is created in the same directory as the SVG file.
        /// </param>
        /// <returns>
        ///     This returns <see langword="true" /> if the conversion is successful;
        ///     otherwise, it return <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="imageFileName" /> is <see langword="null" />.
        ///     <para>-or-</para>
        ///     If the <paramref name="svgXmlReader" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="imageFileName" /> is empty.
        /// </exception>
        public bool Convert(XmlReader svgXmlReader, string imageFileName)
        {
            if (svgXmlReader == null)
            {
                throw new ArgumentNullException(nameof(svgXmlReader),
                    "The SVG source file cannot be null (or Nothing).");
            }

            if (imageFileName == null)
            {
                throw new ArgumentNullException(nameof(imageFileName),
                    "The image destination file path cannot be null (or Nothing).");
            }

            if (imageFileName.Length == 0)
            {
                throw new ArgumentException("The image destination file path cannot be empty.", nameof(imageFileName));
            }

            XamlFile = null;
            ZamlFile = null;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                var workingDir = Path.GetDirectoryName(imageFileName);
                if (!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                }
            }

            return ProcessFile(svgXmlReader, imageFileName);
        }

        private bool ProcessFile(string fileName, string imageFileName)
        {
            BeginProcessing();

            _wpfWindow.LoadDocument(fileName, _wpfSettings);

            _wpfRenderer.InvalidRect = SvgRectF.Empty;

            _wpfRenderer.Render(_wpfWindow.Document as SvgDocument);

            _drawing = _wpfRenderer.Drawing;
            if (_drawing == null)
            {
                EndProcessing();

                return false;
            }

            // Save to the image file...
            SaveImageFile(_drawing, fileName, imageFileName);

            // Save to image and/or ZAML file if required...
            if (SaveXaml || SaveZaml)
            {
                SaveXamlFile(_drawing, fileName, imageFileName);
            }

            EndProcessing();

            return true;
        }

        private bool ProcessFile(Stream svgStream, string imageFileName)
        {
            BeginProcessing();

            _wpfWindow.LoadDocument(svgStream, _wpfSettings);

            _wpfRenderer.InvalidRect = SvgRectF.Empty;

            _wpfRenderer.Render(_wpfWindow.Document as SvgDocument);

            _drawing = _wpfRenderer.Drawing;
            if (_drawing == null)
            {
                EndProcessing();

                return false;
            }

            // Save to the image file...
            SaveImageFile(_drawing, imageFileName, imageFileName);

            // Save to image and/or ZAML file if required...
            if (SaveXaml || SaveZaml)
            {
                SaveXamlFile(_drawing, imageFileName, imageFileName);
            }

            EndProcessing();

            return true;
        }

        private bool ProcessFile(TextReader svgTextReader, string imageFileName)
        {
            BeginProcessing();

            _wpfWindow.LoadDocument(svgTextReader, _wpfSettings);

            _wpfRenderer.InvalidRect = SvgRectF.Empty;

            _wpfRenderer.Render(_wpfWindow.Document as SvgDocument);

            _drawing = _wpfRenderer.Drawing;
            if (_drawing == null)
            {
                EndProcessing();

                return false;
            }

            // Save to the image file...
            SaveImageFile(_drawing, imageFileName, imageFileName);

            // Save to image and/or ZAML file if required...
            if (SaveXaml || SaveZaml)
            {
                SaveXamlFile(_drawing, imageFileName, imageFileName);
            }

            EndProcessing();

            return true;
        }

        private bool ProcessFile(XmlReader svgXmlReader, string imageFileName)
        {
            BeginProcessing();

            _wpfWindow.LoadDocument(svgXmlReader, _wpfSettings);

            _wpfRenderer.InvalidRect = SvgRectF.Empty;

            _wpfRenderer.Render(_wpfWindow.Document as SvgDocument);

            _drawing = _wpfRenderer.Drawing;
            if (_drawing == null)
            {
                EndProcessing();

                return false;
            }

            // Save to the image file...
            SaveImageFile(_drawing, imageFileName, imageFileName);

            // Save to image and/or ZAML file if required...
            if (SaveXaml || SaveZaml)
            {
                SaveXamlFile(_drawing, imageFileName, imageFileName);
            }

            EndProcessing();

            return true;
        }

        private bool SaveImageFile(Drawing drawing, string fileName, string imageFileName)
        {
            var outputExt = GetImageFileExtention();
            string outputFileName;
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                var workingDir = Path.GetDirectoryName(fileName);
                outputFileName = Path.Combine(workingDir, fileNameWithoutExt + outputExt);
            }
            else
            {
                var fileExt = Path.GetExtension(imageFileName);
                if (string.IsNullOrWhiteSpace(fileExt))
                {
                    outputFileName = imageFileName + outputExt;
                }
                else if (!string.Equals(fileExt, outputExt, StringComparison.OrdinalIgnoreCase))
                {
                    outputFileName = Path.ChangeExtension(imageFileName, outputExt);
                }
                else
                {
                    outputFileName = imageFileName;
                }
            }

            var outputFileDir = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputFileDir))
            {
                Directory.CreateDirectory(outputFileDir);
            }

            var bitmapEncoder = GetBitmapEncoder(outputExt);

            // The image parameters...
            var drawingBounds = drawing.Bounds;
            var imageWidth = drawingBounds.Width;
            var imageHeight = drawingBounds.Height;
            var pixelWidth = _wpfSettings.PixelWidth;
            var pixelHeight = _wpfSettings.PixelHeight;
            double dpiX = 96;
            double dpiY = 96;

            var ratioX = pixelWidth / imageWidth;
            var ratioY = pixelHeight / imageHeight;

            var ratio = ratioX < ratioY ? ratioX : ratioY;

            var imageTransform = new ScaleTransform(ratio, ratio);

            // The Visual to use as the source of the RenderTargetBitmap.
            DrawingVisual drawingVisual = new();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.PushTransform(imageTransform);
            if (Background != null)
            {
                drawingContext.DrawRectangle(Background, null, drawing.Bounds);
            }

            drawingContext.DrawDrawing(drawing);
            drawingContext.Pop();
            drawingContext.Close();

            // The BitmapSource that is rendered with a Visual.
            var targetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            targetBitmap.Render(drawingVisual);

            // Encoding the RenderBitmapTarget as an image file.
            bitmapEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            using (var stream = File.Create(outputFileName))
            {
                bitmapEncoder.Save(stream);
            }

            return true;
        }

        private BitmapEncoder GetBitmapEncoder(string fileExtension)
        {
            BitmapEncoder bitmapEncoder = null;

            if (Encoder != null && Encoder.CodecInfo != null)
            {
                var mimeType = string.Empty;
                var codecInfo = Encoder.CodecInfo;
                var mimeTypes = codecInfo.MimeTypes;
                var fileExtensions = codecInfo.FileExtensions;
                switch (EncoderType)
                {
                    case ImageEncoderType.BmpBitmap:
                        mimeType = "image/bmp";
                        break;
                    case ImageEncoderType.GifBitmap:
                        mimeType = "image/gif";
                        break;
                    case ImageEncoderType.JpegBitmap:
                        mimeType = "image/jpeg,image/jpe,image/jpg";
                        break;
                    case ImageEncoderType.PngBitmap:
                        mimeType = "image/png";
                        break;
                    case ImageEncoderType.TiffBitmap:
                        mimeType = "image/tiff,image/tif";
                        break;
                    case ImageEncoderType.WmpBitmap:
                        mimeType = "image/vnd.ms-photo";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(fileExtensions) &&
                    fileExtensions.IndexOf(fileExtension, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bitmapEncoder = Encoder;
                }
                else if (!string.IsNullOrWhiteSpace(mimeTypes) &&
                         !string.IsNullOrWhiteSpace(mimeType))
                {
                    var arrayMimeTypes = mimeType.Split(',');
                    if (arrayMimeTypes.Any(mime => mimeTypes.IndexOf(mime, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        bitmapEncoder = Encoder;
                    }
                }
            }

            return bitmapEncoder ?? (bitmapEncoder = GetBitmapEncoder(EncoderType));
        }

        private string GetImageFileExtention() => GetImageFileExtention(EncoderType);

        private bool SaveXamlFile(Drawing drawing, string fileName, string imageFileName)
        {
            WriterErrorOccurred = false;
            string xamlFileName = null;
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                var workingDir = Path.GetDirectoryName(fileName);
                xamlFileName = Path.Combine(workingDir, fileNameWithoutExt + XamlExt);
            }
            else
            {
                var fileExt = Path.GetExtension(imageFileName);
                if (string.IsNullOrWhiteSpace(fileExt))
                {
                    xamlFileName = imageFileName + XamlExt;
                }
                else if (!string.Equals(fileExt, XamlExt, StringComparison.OrdinalIgnoreCase))
                {
                    xamlFileName = Path.ChangeExtension(imageFileName, XamlExt);
                }
            }

            if (File.Exists(xamlFileName))
            {
                File.SetAttributes(xamlFileName, FileAttributes.Normal);
                File.Delete(xamlFileName);
            }

            if (UseFrameXamlWriter)
            {
                XmlWriterSettings writerSettings = new()
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    OmitXmlDeclaration = true
                };

                using var xamlFile = File.Create(xamlFileName);
                using var writer = XmlWriter.Create(xamlFile, writerSettings);
                XamlWriter.Save(drawing, writer);
            }
            else
            {
                try
                {
                    XmlXamlWriter xamlWriter = new(DrawingSettings);

                    using (var xamlFile = File.Create(xamlFileName))
                    {
                        xamlWriter.Save(drawing, xamlFile);
                    }
                }
                catch
                {
                    WriterErrorOccurred = true;

                    if (FallbackOnWriterError)
                    {
                        if (File.Exists(xamlFileName))
                        {
                            File.Move(xamlFileName, xamlFileName + BackupExt);
                        }

                        XmlWriterSettings writerSettings = new()
                        {
                            Indent = true,
                            Encoding = Encoding.UTF8,
                            OmitXmlDeclaration = true
                        };

                        using var xamlFile = File.Create(xamlFileName);
                        using var writer = XmlWriter.Create(xamlFile, writerSettings);
                        XamlWriter.Save(drawing, writer);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (SaveZaml)
            {
                var zamlFileName = Path.ChangeExtension(xamlFileName, CompressedXamlExt);

                if (File.Exists(zamlFileName))
                {
                    File.SetAttributes(zamlFileName, FileAttributes.Normal);
                    File.Delete(zamlFileName);
                }

                FileStream zamlSourceFile = new(xamlFileName, FileMode.Open,
                    FileAccess.Read, FileShare.Read);
                var buffer = new byte[zamlSourceFile.Length];
                // Read the file to ensure it is readable.
                var count = zamlSourceFile.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    zamlSourceFile.Close();
                    return false;
                }

                zamlSourceFile.Close();

                var zamlDestFile = File.Create(zamlFileName);

                GZipStream zipStream = new(zamlDestFile, CompressionMode.Compress, true);
                zipStream.Write(buffer, 0, buffer.Length);

                zipStream.Close();

                zamlDestFile.Close();

                ZamlFile = zamlFileName;
            }

            XamlFile = xamlFileName;

            if (!SaveXaml && File.Exists(xamlFileName))
            {
                File.Delete(xamlFileName);
                XamlFile = null;
            }

            return true;
        }
    }
}

// ReSharper enable All