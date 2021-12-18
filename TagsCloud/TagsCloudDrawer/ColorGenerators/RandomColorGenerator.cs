﻿using System;
using System.Drawing;

namespace TagsCloudDrawer.ColorGenerators
{
    public class RandomColorGenerator : IColorGenerator
    {
        private readonly Random _random;

        public RandomColorGenerator(Random random)
        {
            _random = random;
        }

        public Color Generate()
        {
            return Color.FromArgb(
                _random.Next(175, 256),
                _random.Next(256),
                _random.Next(256),
                _random.Next(256));
        }
    }
}