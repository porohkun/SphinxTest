using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Global
{
    public static Point CellSize { get; private set; }
    public static float SegmentMinLength { get; private set; }

    public static void LoadSettings(Settings settings)
    {
        CellSize = settings.CellSize;
        SegmentMinLength = settings.SegmentMinLength;
    }
}