using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class GameMap : MonoBehaviour
{
    public int Width = 100;
    public int Height = 100;
    public int Fill = 100;

    public InputField Text1;

    private Camera _camera;
    private Transform _unitsContainer;

    private bool[,] _units;
    private bool[,] _virtualUnits;
    private SpriteRenderer[,] _renderers;

    public bool this[int x, int y]
    {
        get
        {
            return CorrectPosition(x, y) ? _units[x, y] : false;
        }
    }

    void Start()
    {
        _camera = Camera.main;
        var unitPrefab = Resources.Load<SpriteRenderer>("Prefabs/Unit");
        _unitsContainer = (new GameObject("Units")).GetComponent<Transform>();
        FocusCamera();
        _units = new bool[Width, Height];
        _virtualUnits = new bool[Width, Height];
        _renderers = new SpriteRenderer[Width, Height];
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                _renderers[x, y] = Instantiate<SpriteRenderer>(unitPrefab);
                _renderers[x, y].transform.SetParent(transform);
                _renderers[x, y].transform.localPosition = MapToWorld(new Vector2(x, y));
                _renderers[x, y].enabled = false;
            }
        SetBorders();
        ShowUnits();
    }

    public void ClearField()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                _units[x, y] = false;
                _renderers[x, y].enabled = false;
            }
    }

    public void SetFill(string text)
    {
        if (!int.TryParse(text, out Fill))
            Text1.text = Fill.ToString();
    }

    public void FocusCamera()
    {
        float width = (Width + Height) * Global.CellWidth / 2f;
        float height = (Width + Height) * Global.CellHeight / 2f;
        float sWidth = Screen.width;
        float sHeight = Screen.height;

        float mod = width/height;
        float sMod = sWidth/sHeight;
        float size = (mod <= sMod ? height / 2f : width / sMod / 2f) + Global.CellHeight;

        _camera.transform.position = new Vector3(width / 2 - Height * Global.CellWidth / 2f, height / 2, -10f);
        _camera.orthographicSize = size;
    }

    public bool CorrectPosition(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    List<Vector2> _path = new List<Vector2>();
    Vector2 _lastPoint;
    void Update()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _virtualUnits[x, y] = false;
            
        Vector3 mousePos = Input.mousePosition;
        Vector2 worldPos = _camera.ScreenToWorldPoint(mousePos);
        Vector2 mapPos = WorldToMap(worldPos);

        if (Input.GetMouseButtonDown(0))
        {
            _path.Clear();
            _path.Add(mapPos);
            _lastPoint = mapPos;
        }
        if ((Input.GetMouseButton(0) && Vector2.Distance(_lastPoint, mapPos) >= Global.SegmentMinLength)
            || (Input.GetMouseButtonUp(0) && Vector2.Distance(_lastPoint, mapPos) >= Global.SegmentMinLength / 5f))
        {
            _path.Add(mapPos);
            _lastPoint = mapPos;
        }
        if (Input.GetMouseButtonUp(0) && _path.Count < 2)
            _path.Clear();
        bool started = _path.Count >= 1;

        if ((Input.GetMouseButton(0)||Input.GetMouseButtonUp(0)) && _path.Count > 2)
        {
            _path.Add(mapPos);
            int points = 0;
            int prevPoints = 0;
            int tryes = 0;
            var path1 = _path;
            var path2 = _path;
            bool one = true;
            while (points < Fill)
            {
                points = FillLine(one ? path1 : path2, points);
                one = !one;
                if (one)
                    path1 = path1.Offset(0.1f, true);
                else
                    path2 = path2.Offset(0.1f, false);
                if (points == prevPoints) tryes++;
                if (tryes > 100) break;
                prevPoints = points;
            }
            if (Input.GetMouseButtonUp(0))
            {
                ShowUnits(true);
                _path.Clear();
            }
            else
            {
                ShowUnits(false);
                _path.RemoveAt(_path.Count - 1);
            }
        }
        if (Input.GetMouseButtonUp(0) && _path.Count > 1)
        {
            ShowUnits(true);
            _path.Clear();
        }
        
    }

    private int FillLine(List<Vector2> path, int points)
    {
        Vector2 a = path[0];
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 b = path[i];

            points = RasterLine(points, a.x, a.y, b.x, b.y);
            if (points == Fill) break;
            a = b;
        }
        return points;
    }

    private int RasterLine(int points, float x0, float y0, float x1, float y1)
    {
        var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        
        if (steep)
        {
            Swap(ref x0, ref y0);
            Swap(ref x1, ref y1);
        }
        
        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        float dx = x1 - x0;
        float dy = Math.Abs(y1 - y0);
        float error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;
        int y = Mathf.FloorToInt(y0);
        for (int x = Mathf.FloorToInt(x0); x <= x1; x++)
        {
            if (DrawPoint(steep ? y : x, steep ? x : y))
                points++;
            if (points == Fill) return points;
            error -= dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
        return points;
    }

    bool DrawPoint(int x, int y)
    {
        if (!CorrectPosition(x, y)) return false;
        bool oldValue = _units[x, y] || _virtualUnits[x, y];
        _virtualUnits[x, y] = true;
        return !oldValue;
    }

    private void Swap(ref float a, ref float b)
    {
        float c = a;
        a = b;
        b = c;
    }

    private void ShowUnits(bool real=false)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                if (_virtualUnits[x, y] && real)
                    _virtualUnits[x, y] = _virtualUnits[x, y];
                if(real)
                    _units[x, y] = _units[x, y] || _virtualUnits[x, y];
                _renderers[x, y].enabled = real ? _units[x, y] : (_units[x, y] || _virtualUnits[x, y]);
            }
    }

    private void SetBorders()
    {
        var yOffset = new Vector2(0f, Global.CellHeight / 2f);
        var border = (new GameObject("Border")).GetComponent<Transform>();
        border.SetParent(transform);
        var linePrefab = Resources.Load<LineRenderer>("Prefabs/line");
        var line = GameObject.Instantiate<LineRenderer>(linePrefab);
        line.SetPosition(0, MapToWorld(new Vector2(-1, -1)) + yOffset);
        line.SetPosition(1, MapToWorld(new Vector2(-1, Height)) + yOffset);
        line.transform.SetParent(border);
        line = GameObject.Instantiate<LineRenderer>(linePrefab);
        line.SetPosition(0, MapToWorld(new Vector2(-1, Height)) + yOffset);
        line.SetPosition(1, MapToWorld(new Vector2(Width, Height)) + yOffset);
        line.transform.SetParent(border);
        line = GameObject.Instantiate<LineRenderer>(linePrefab);
        line.SetPosition(0, MapToWorld(new Vector2(Width, Height)) + yOffset);
        line.SetPosition(1, MapToWorld(new Vector2(Width, -1)) + yOffset);
        line.transform.SetParent(border);
        line = GameObject.Instantiate<LineRenderer>(linePrefab);
        line.SetPosition(0, MapToWorld(new Vector2(Width, -1)) + yOffset);
        line.SetPosition(1, MapToWorld(new Vector2(-1, -1)) + yOffset);
        line.transform.SetParent(border);
    }

    public static Vector2 WorldToMap(Vector2 world)
    {
        float xp = world.x / Global.CellWidth;
        float yp = world.y / Global.CellHeight;

        return new Vector2(xp + yp, yp - xp);
    }

    public static Vector2 MapToWorld(Vector2 map)
    {
        return new Vector2(
            (map.x - map.y) * Global.CellWidth / 2f,
            (map.x + map.y) * Global.CellHeight / 2f
            );
    }
}
