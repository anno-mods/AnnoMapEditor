using AnnoMapEditor.Utilities;
using Pfim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives
{
    public abstract class DataArchive : ObservableBase, IDataArchive
    {
        public abstract Stream? OpenRead(string path);

        public abstract IEnumerable<string> Find(string pattern);

        private static string? AdjustDataPath(string? path)
        {
            if (path is null)
                return null;
            if (File.Exists(Path.Combine(path, "maindata/data0.rda")))
                return path;
            if (File.Exists(Path.Combine(path, "data0.rda")))
                return Path.GetDirectoryName(path);
            if (Directory.Exists(Path.Combine(path, "data/dlc01")))
                return path;
            if (Directory.Exists(Path.Combine(path, "dlc01")))
                return Path.GetDirectoryName(path);
            return null;
        }

        public ImageSource? TryLoadIcon(string iconPath, Point? desiredSize = null)
        {
            // Icons are referenced as .png but stored as .dds.
            if (iconPath.EndsWith(".png"))
                iconPath = iconPath[0..^4] + "_0.dds";
            
            if (iconPath.Contains("/fhd/"))
                iconPath = iconPath.Replace("/fhd/", "/4k/");
            
            // open the file
            using Stream? stream = OpenRead(iconPath);
            if (stream == null)
                return null;

            IImage iconImage = Pfimage.FromStream(stream);
            return desiredSize != null ? ConvertToWpfImageMipmapped(iconImage, (Point)desiredSize) : ConvertToWpfImage(iconImage);
        }

        public BitmapImage? TryLoadPng(string pngPath)
        {
            using Stream? stream = OpenRead(pngPath);
            if (stream == null)
                return null;

            BitmapImage thumbnail = new();
            thumbnail.BeginInit();
            thumbnail.StreamSource = stream;
            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            thumbnail.EndInit();
            thumbnail.Freeze();

            return thumbnail;
        }

        private static ImageSource ConvertToWpfImage(IImage image, Point? desiredSize = null)
        {
            var pinnedArray = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            var addr = pinnedArray.AddrOfPinnedObject();

            var bsource = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
                GetWpfPixelFormat(image), null, addr, image.DataLen, image.Stride);

            bsource.Freeze();
            return bsource;
        }

        private static ImageSource ConvertToWpfImageMipmapped(IImage image, Point size)
        {
            var pinnedArray = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            var addr = pinnedArray.AddrOfPinnedObject();

            var mip = image.MipMaps.Where(x => x.Height >= size.Y && x.Width >= size.X).LastOrDefault();
            if (mip is null)
                return ConvertToWpfImage(image);

            var mipAddr = addr + mip.DataOffset;
            var mipSource = BitmapSource.Create(mip.Width, mip.Height, 96.0, 96.0,
                GetWpfPixelFormat(image), null, mipAddr, mip.DataLen, mip.Stride);

            mipSource.Freeze();
            return mipSource;
        }

        private static PixelFormat GetWpfPixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }
    }
}
