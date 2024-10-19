using UnityEngine;

partial class Helpers {


    /// <summary>
    /// 获取甜甜圈形状里的随机点
    /// </summary>
    public static Vector2 GetRndPosDoughnut(float maxRadius, float safeRadius) {
        var len = maxRadius - safeRadius;
        var len_radius = len / maxRadius;
        var safeRadius_radius = safeRadius / maxRadius;
        var radius = Mathf.Sqrt(Random.Range(0, len_radius) + safeRadius_radius) * maxRadius;
        var radians = Random.Range(-Mathf.PI, Mathf.PI);
        return new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);
    }

}
