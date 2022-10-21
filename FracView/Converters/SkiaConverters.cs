using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FracView.Converters
{
    public static class SkiaConverters
    {
        /// <summary>
        /// Converts file extension (with period) to associated image format.
        /// </summary>
        /// <param name="extension">File extension.</param>
        /// <returns>Image format.</returns>
        /// <exception cref="NotSupportedException">If the file extension could not be resolved.</exception>
        public static SKEncodedImageFormat ExtensionToFormat(string extension)
        {
            return (extension?.ToLower()?.Trim()) switch
            {
                ".png" => SKEncodedImageFormat.Png,
                ".jpg" => SKEncodedImageFormat.Jpeg,
                ".jepg" => SKEncodedImageFormat.Jpeg,
                ".bmp" => SKEncodedImageFormat.Bmp,
                ".gif" => SKEncodedImageFormat.Gif,
                _ => throw new NotSupportedException($"Unknown file extension: {extension}"),
            };
        }

        /// <summary>
        /// Converts image format to associated file extension.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <returns>Three letter file extension with leading period.</returns>
        /// <exception cref="NotSupportedException">If the file extension could not be resolved.</exception>
        public static string FormatToExtension(SKEncodedImageFormat format)
        {
            return (format) switch
            {
                SKEncodedImageFormat.Png => ".png",
                SKEncodedImageFormat.Jpeg => ".jpg",
                SKEncodedImageFormat.Bmp => ".bmp",
                SKEncodedImageFormat.Gif => ".gif",
                _ => throw new NotSupportedException($"Unsupported format: {format}"),
            };
        }
    }
}
