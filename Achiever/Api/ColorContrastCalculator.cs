using System;
using System.Drawing;

namespace Achiever.Api
{
    public static class ColorContrastCalculator
    {
        /// <summary>
        /// Calculates the relative luminance of a color according to WCAG 2.0.
        /// </summary>
        public static double GetRelativeLuminance(Color color)
        {
            // Convert R, G, B values to linear RGB space
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            r = (r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = (g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = (b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            // Calculate the luminance
            // Weighted for human perception (green is brightest, blue darkest)
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>
        /// Calculates the contrast ratio between two colors.
        /// </summary>
        public static double GetContrastRatio(Color color1, Color color2)
        {
            double luminance1 = GetRelativeLuminance(color1);
            double luminance2 = GetRelativeLuminance(color2);

            // Ensure L1 is the lighter color and L2 is the darker color
            double l1 = Math.Max(luminance1, luminance2);
            double l2 = Math.Min(luminance1, luminance2);

            // The WCAG contrast ratio formula
            return (l1 + 0.05) / (l2 + 0.05);
        }
    }
}