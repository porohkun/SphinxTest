using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions
{
    private static Vector3 _zAxis = new Vector3(0, 0, 1);
    public static List<Vector2> Offset(this List<Vector2> path, float thick, bool right)
    {
        List<Vector2> offset = new List<Vector2>();

        bool first = true;
        Vector2 a = path[0];
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 b = path[i];
            var d = a - b;
            if (first)
            {
                offset.Add(right ? new Vector2(-d.y, d.x).normalized * thick + a
                    : new Vector2(d.y, -d.x).normalized * thick + a);
                first = false;
            }

            if (i == path.Count - 1)
            {
                offset.Add(right ? new Vector2(-d.y, d.x).normalized * thick + b
                    : new Vector2(d.y, -d.x).normalized * thick + b);
            }
            else
            {
                var v1 = (a - b).normalized;
                var b2 = path[i + 1];
                var v2 = (b2 - b).normalized;
                var angle = AngleBetween(v1, v2) / 2f;
                Vector2 r1 = Quaternion.AngleAxis(angle, _zAxis) * v1;
                Vector2 r2 = -r1;

                var aa = Mathf.Abs(angle - 90) * Mathf.Deg2Rad;
                var bis = thick / Mathf.Cos(aa);

                offset.Add(right ? r1.normalized * bis + b
                    : r2.normalized * bis + b);
            }
            a = b;
        }
        return offset;
    }

    private static float AngleBetween(Vector3 a, Vector3 b)
    {
        float anglea = Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
        float angleb = Mathf.Atan2(b.y, b.x) * Mathf.Rad2Deg;
        if (angleb < 0) angleb = 360 + angleb;
        float angle = (angleb - anglea) % 360;
        if (angle < 0) angle = 360 + angle;
        return angle;
    }

}
