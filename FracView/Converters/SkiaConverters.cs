﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FracView.Converters
{
    public static class SkiaConverters
    {
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
    }
}