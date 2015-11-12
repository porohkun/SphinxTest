using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Settings : MonoBehaviour
{
    public int CellWidth;
    public int CellHeight;
    public float SegmentMinLength = 5f;

    void Start()
    {
        Global.LoadSettings(this);
    }

}
