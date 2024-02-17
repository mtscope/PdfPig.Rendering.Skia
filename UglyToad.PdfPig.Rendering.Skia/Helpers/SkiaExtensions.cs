﻿/*
 * Copyright 2024 BobLd
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using SkiaSharp;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.Graphics.Core;
using UglyToad.PdfPig.PdfFonts;

namespace UglyToad.PdfPig.Rendering.Skia.Helpers
{
    internal static class SkiaExtensions
    {
        public static SKFontStyle GetFontStyle(this FontDetails fontDetails)
        {
            if (fontDetails.IsBold && fontDetails.IsItalic)
            {
                return SKFontStyle.BoldItalic;
            }
            else if (fontDetails.IsBold)
            {
                return SKFontStyle.Bold;
            }
            else if (fontDetails.IsItalic)
            {
                return SKFontStyle.Italic;
            }
            return SKFontStyle.Normal;
        }

        public static string GetCleanFontName(this IFont font)
        {
            string fontName = font.Name;
            if (fontName.Length > 7 && fontName[6].Equals('+'))
            {
                string subset = fontName.Substring(0, 6);
                if (subset.Equals(subset.ToUpper()))
                {
                    return fontName.Split('+')[1];
                }
            }

            return fontName;
        }

        public static SKPoint ToSKPoint(this PdfPoint pdfPoint, int height)
        {
            return new SKPoint((float)pdfPoint.X, (float)(height - pdfPoint.Y));
        }

        public static SKStrokeJoin ToSKStrokeJoin(this LineJoinStyle lineJoinStyle)
        {
            switch (lineJoinStyle)
            {
                case LineJoinStyle.Bevel:
                    return SKStrokeJoin.Bevel;

                case LineJoinStyle.Miter:
                    return SKStrokeJoin.Miter;

                case LineJoinStyle.Round:
                    return SKStrokeJoin.Round;

                default:
                    throw new NotImplementedException($"Unknown LineJoinStyle '{lineJoinStyle}'.");
            }
        }

        public static SKStrokeCap ToSKStrokeCap(this LineCapStyle lineCapStyle)
        {
            switch (lineCapStyle)
            {
                case LineCapStyle.Butt:
                    return SKStrokeCap.Butt;

                case LineCapStyle.ProjectingSquare:
                    return SKStrokeCap.Square;

                case LineCapStyle.Round:
                    return SKStrokeCap.Round;

                default:
                    throw new NotImplementedException($"Unknown LineCapStyle '{lineCapStyle}'.");
            }
        }

        public static SKPathEffect ToSKPathEffect(this LineDashPattern lineDashPattern, float lineWidth)
        {
            const float oneOver72 = (float)(1.0 / 72.0);

            if (lineDashPattern.Phase == 0 && !(lineDashPattern.Array?.Count > 0))
            {
                return null;
            }

            float scale = lineWidth * oneOver72 / 2; // Scale is still not correct
            float phase = lineDashPattern.Phase * scale;

            if (lineDashPattern.Array.Count == 1)
            {
                var v = (float)lineDashPattern.Array[0] * scale;
                return SKPathEffect.CreateDash(new[] { v, v }, phase);
            }

            if (lineDashPattern.Array.Count > 0)
            {
                float[] pattern = new float[lineDashPattern.Array.Count];
                for (int i = 0; i < lineDashPattern.Array.Count; i++)
                {
                    var v = (float)lineDashPattern.Array[i] * scale;
                    if (v == 0)
                    {
                        pattern[i] = oneOver72;
                    }
                    else
                    {
                        pattern[i] = v;
                    }
                }

                return SKPathEffect.CreateDash(pattern, phase);
            }

            return SKPathEffect.CreateDash(new float[] { 0, 0 }, phase);
        }

        public static bool? IsStroke(this TextRenderingMode textRenderingMode)
        {
            switch (textRenderingMode)
            {
                case TextRenderingMode.Stroke:
                case TextRenderingMode.StrokeClip:
                    return true;

                case TextRenderingMode.Fill:
                case TextRenderingMode.FillClip:
                    return false;

                case TextRenderingMode.FillThenStroke:
                case TextRenderingMode.FillThenStrokeClip:
                    return true;

                case TextRenderingMode.NeitherClip:
                    return true; // TODO - Very not correct

                case TextRenderingMode.Neither:
                default:
                    return null;
            }
        }

        public static bool? IsFill(this TextRenderingMode textRenderingMode)
        {
            // TODO - to finish, not correct
            switch (textRenderingMode)
            {
                case TextRenderingMode.Stroke:
                case TextRenderingMode.StrokeClip:
                    return false;

                case TextRenderingMode.Fill:
                case TextRenderingMode.FillClip:
                    return true;

                case TextRenderingMode.FillThenStroke:
                case TextRenderingMode.FillThenStrokeClip:
                    return true;

                case TextRenderingMode.NeitherClip:
                    return false; // TODO - Very not correct

                case TextRenderingMode.Neither:
                default:
                    return null;
            }
        }

        public static SKPaintStyle? ToSKPaintStyle(this TextRenderingMode textRenderingMode)
        {
            // TODO - to finish, not correct
            switch (textRenderingMode)
            {
                case TextRenderingMode.Stroke:
                case TextRenderingMode.StrokeClip:
                    return SKPaintStyle.Stroke;

                case TextRenderingMode.Fill:
                case TextRenderingMode.FillClip:
                    return SKPaintStyle.Fill;

                case TextRenderingMode.FillThenStroke:
                case TextRenderingMode.FillThenStrokeClip:
                    return SKPaintStyle.StrokeAndFill;

                case TextRenderingMode.NeitherClip:
                    return SKPaintStyle.Stroke; // TODO - Very not correct

                case TextRenderingMode.Neither:
                default:
                    return null;
            }
        }

        public static SKPathFillType ToSKPathFillType(this FillingRule fillingRule)
        {
            return fillingRule == FillingRule.NonZeroWinding ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
        }

        public static SKColor ToSKColor(this IColor pdfColor, decimal alpha)
        {
            return ToSKColor(pdfColor, (double)alpha);
        }

        public static SKColor ToSKColor(this IColor pdfColor, double alpha)
        {
            var color = SKColors.Black;
            if (pdfColor != null)
            {
                var (r, g, b) = pdfColor.ToRGBValues();
                color = new SKColor(Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
            }

            return color.WithAlpha(Convert.ToByte(alpha * 255));
        }

        public static SKMatrix ToSkMatrix(this TransformationMatrix transformationMatrix)
        {
            return new SKMatrix((float)transformationMatrix.A, (float)transformationMatrix.C, (float)transformationMatrix.E,
                                (float)transformationMatrix.B, (float)transformationMatrix.D, (float)transformationMatrix.F,
                                0, 0, 1); // could use the actual values, but should always be 0, 0, 1 (?)
        }

        /*
        private static bool doBlending = false;

        public static SKBlendMode ToSKBlendMode(this BlendMode blendMode)
        {
            if (!doBlending)
            {
                return SKBlendMode.SrcOver;
            }

            switch (blendMode)
            {
                // Standard separable blend modes
                case BlendMode.Normal:
                case BlendMode.Compatible:
                    return SKBlendMode.SrcOver; // TODO - Check if correct

                case BlendMode.Multiply:
                    return SKBlendMode.Multiply;

                case BlendMode.Screen:
                    return SKBlendMode.Screen;

                case BlendMode.Overlay:
                    return SKBlendMode.Overlay;

                case BlendMode.Darken:
                    return SKBlendMode.Darken;

                case BlendMode.Lighten:
                    return SKBlendMode.Lighten;

                case BlendMode.ColorDodge:
                    return SKBlendMode.ColorDodge;

                case BlendMode.ColorBurn:
                    return SKBlendMode.ColorBurn;

                case BlendMode.HardLight:
                    return SKBlendMode.HardLight;

                case BlendMode.SoftLight:
                    return SKBlendMode.SoftLight;

                case BlendMode.Difference:
                    return SKBlendMode.Difference;

                case BlendMode.Exclusion:
                    return SKBlendMode.Exclusion;

                // Standard nonseparable blend modes
                case BlendMode.Hue:
                    return SKBlendMode.Hue;

                case BlendMode.Saturation:
                    return SKBlendMode.Saturation;

                case BlendMode.Color:
                    return SKBlendMode.Color;

                case BlendMode.Luminosity:
                    return SKBlendMode.Luminosity;

                default:
                    throw new NotImplementedException($"Cannot convert blend mode '{blendMode}' to SKBlendMode.");
            }
        }
        */

        /*
        public static void ApplySMask(this SKBitmap image, SKBitmap smask)
        {
            // What about 'Alpha source' flag?
            SKBitmap scaled;
            if (!image.Info.Rect.Equals(smask.Info.Rect))
            {
                scaled = new SKBitmap(image.Info);
                if (!smask.ScalePixels(scaled, SKFilterQuality.High))
                {
                    // log
                }
            }
            else
            {
                scaled = smask;
            }

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var pix = image.GetPixel(x, y);
                    byte alpha = scaled.GetPixel(x, y).Red; // Gray CS (r = g = b)
                    image.SetPixel(x, y, pix.WithAlpha(alpha));
                }
            }
            scaled.Dispose();
        }
        */

        public static byte[] GetImageBytes(this IPdfImage pdfImage)
        {
            // Try get png bytes
            if (pdfImage.TryGetPng(out byte[] bytes) && bytes?.Length > 0)
            {
                return bytes;
            }

            // Fallback to bytes
            if (pdfImage.TryGetBytes(out var bytesL) && bytesL?.Count > 0)
            {
                if (bytesL is byte[] bytesA)
                {
                    return bytesA;
                }
                return bytesL.ToArray();
            }

            // Fallback to raw bytes
            if (pdfImage.RawBytes is byte[] rawBytesA)
            {
                return rawBytesA;
            }

            return pdfImage.RawBytes.ToArray();
        }
    }
}
