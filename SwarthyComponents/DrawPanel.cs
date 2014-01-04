//Author: Alexander Mochalin
//Copyright (c) 2013-2014 All Rights Reserved

using System;
using System.Windows.Forms;
using System.Drawing;

namespace SwarthyComponents.UI
{
    public class DrawPanel : Panel
    {
        public DrawPanel()
            : base()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

    }
}
