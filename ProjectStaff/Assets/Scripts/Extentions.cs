using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions{

    public static Vector3 ClosestPoint(this Vector3 point, Vector3 pointA, Vector3 pointB) {
        Vector3 AtoB = pointB - pointA;
        Vector3 AtoP = point - pointA;

        float dist = Vector3.Dot(AtoP, AtoB) / AtoB.sqrMagnitude;

        return pointA + dist * AtoB;
    }
}
