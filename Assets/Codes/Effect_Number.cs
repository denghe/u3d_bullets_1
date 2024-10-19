using UnityEngine;

public class Effect_Number {
    public Scene scene;
    public Stage stage;
    public GO[] gos = new GO[12];
    public int size;

    public const float charWidth = 9 - 2;
    public const float baseSpeed = 60f / Scene.fps;
    public float incX, incY;
    public const int life = (int)(0.5f * Scene.fps);
    public float x, y, scale;
    public int endLifeTime;

    public Effect_Number(Monster m, PlayerBullet b, double dmg, bool isCritical) {
        stage = m.stage;
        scene = stage.scene;
        stage.effectNumbers.Add(this);

        endLifeTime = scene.time + life;
        scale = 2;                                          // 先写死. 以后按照怪体积来适当缩放?
        var speed = baseSpeed * Random.Range(0.5f, 1);      // 速度稍微变化一下
        var offset = Helpers.GetRndPosDoughnut(Mathf.Min(16, m.radius), 0.1f);  // 中心坐标稍微偏移下
        x = m.x + offset.x;
        y = m.y + offset.y;

        // 根据 怪 和 子弹的相对位置 算飞行增量
        var r1 = m.radius;
        var r2 = b.cfg.radius;
        var dx = m.x - b.x;
        var dy = m.y - b.y;
        var dd = dx * dx + dy * dy;
        if (dd > 0) {
            var sdd = Mathf.Sqrt(dd);
            incX = dx / sdd * speed;
            incY = dy / sdd * speed;
        }

        // 将伤害值转为显示对象
        var sb = Helpers.ToStringEN(dmg);
        size = sb.Length;
        for (int i = 0; i < size; i++) {
            var o = new GO();
            GO.Pop(ref o, 0, "FG2");
            o.r.sprite = scene.sprites_font_outline[sb[i] - 32];
            o.g.transform.localScale = new Vector3(scale, scale, scale);
            if (isCritical) {
                o.r.color = Color.red;
            }
            gos[i] = o;
        }
    }

    public bool Update() {
        x += incX;
        y += incY;
        return endLifeTime < scene.time;
    }

    public virtual void Draw(float cx, float cy) {
        var w = (9 - 2) * scale;
        var baseX = x - w * size / 2 + charWidth / 2;
        var v = new Vector3(0, -y * Scene.cameraRatio, 0);
        for (int i = 0; i < size; ++i) {
            v.x = (baseX + w * i) * Scene.cameraRatio;
            gos[i].g.transform.position = v;
        }
    }

    public void Destroy() {
        for (int i = 0; i < size; ++i) {
#if UNITY_EDITOR
            if (gos[i].g != null)
#endif
            {
                GO.Push(ref gos[i]);
            }
        }
    }
}
