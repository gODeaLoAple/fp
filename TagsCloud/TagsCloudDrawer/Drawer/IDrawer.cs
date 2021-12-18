﻿using System.Collections.Generic;
using System.Drawing;

namespace TagsCloudDrawer.Drawer
{
    public interface IDrawer
    {
        void Draw(Graphics graphics, Size size, IEnumerable<IDrawable> drawables);
    }
}