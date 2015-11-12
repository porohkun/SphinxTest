using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Global
{
    public static int CellWidth { get; private set; }
    public static int CellHeight { get; private set; }
    public static float SegmentMinLength { get; private set; }

    public static void LoadSettings(Settings settings)
    {
        CellWidth = settings.CellWidth;
        CellHeight = settings.CellHeight;
        SegmentMinLength = settings.SegmentMinLength;
    }
}