using UnityEngine;

/// <summary>
/// 一个圈圈，从小变大, 最后消失
/// </summary>
public class Effect_Explosion {
    // 各种引用
    public Scene scene;
    public Stage stage;
    public GO go;

    public const float scaleStep = 6f / Scene.fps;
    public float x, y, scale, baseScale;

    public Effect_Explosion(Stage stage_, float x_, float y_, float scale_) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        stage.effectExplosions.Add(this);

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go, 0, "FG");
        go.g.transform.SetPositionAndRotation(new Vector3(x_ * Scene.cameraRatio, -y_ * Scene.cameraRatio, 0)
            , Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        go.r.sprite = scene.sprite_explode;

        baseScale = scale_;
        scale = 0.1f;
        x = x_;
        y = y_;
    }

    public bool Update() {
        scale += scaleStep;
        return scale >= 2;
    }

    public virtual void Draw(float cx, float cy) {
        var s = scale * baseScale;
        go.g.transform.localScale = new Vector3(s, s, s);
    }

    public void Destroy() {
#if UNITY_EDITOR
        if (go.g != null)
#endif
        {
            GO.Push(ref go);
        }
    }
}
