﻿using System.Collections.Generic;
using System.Drawing;
using ResultMonad;
using TagsCloudDrawer.Drawer;
using TagsCloudDrawer.ImageSaveService;
using TagsCloudDrawer.ImageSettings;

namespace TagsCloudDrawer.ImageCreator
{
    public class ImageCreator : IImageCreator
    {
        private readonly IDrawer _drawer;
        private readonly IImageSaveService _saveService;
        private readonly IImageSettingsProvider _settingsProvider;

        public ImageCreator(IDrawer drawer, IImageSaveService saveService,
            IImageSettingsProvider settingsProvider)
        {
            _drawer = drawer;
            _saveService = saveService;
            _settingsProvider = settingsProvider;
        }

        public Result<None> Create(string filename, IEnumerable<IDrawable> drawables)
        {
            var size = _settingsProvider.ImageSize;
            using var bitmap = new Bitmap(size.Width, size.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(_settingsProvider.BackgroundColor);
            return _drawer.Draw(graphics, bitmap.Size, drawables)
                .OnSuccess(() => _saveService.Save(filename, bitmap));
        }
    }
}