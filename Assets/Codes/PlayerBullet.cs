using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet {
    // 各种引用
    public Scene scene;
    public Stage stage;
    public Player player;
    public PlayerSkill skill;
    public SpaceContainer monstersSpaceContainer;
    public GO go;                                   // 保存底层 u3d 资源

    public const float defaultRadius = 16f;         // 原始半径

    public float x, y, radians;                     // 坐标, 弧度
    public float incX, incY;                        // 每帧的移动增量
    public int lifeEndTime;                         // 自杀时间点
    public bool noPierce;                           // 不能穿刺( 特殊优化 )
    public PlayerBulletConfig cfg;                  // 从技能复制
    public List<KeyValuePair<SpaceItem, int>> hitBlackList = new();   // 带超时的穿透黑名单

    public PlayerBullet(PlayerSkill ps) {
        skill = ps;
        player = ps.player;
        stage = ps.stage;
        scene = ps.scene;
        monstersSpaceContainer = stage.monstersSpaceContainer;
        stage.playerBullets.Add(this);
        cfg = ps.cfg;
    }

    public PlayerBullet Init(float x_, float y_, float radians_, float cos_, float sin_) {
        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);
        go.r.sprite = scene.sprite_bullet;
        go.r.color = Color.yellow;
        go.g.transform.rotation = Quaternion.Euler(0, 0, -radians_ * (180f / Mathf.PI));

        lifeEndTime = scene.time + cfg.life;
        radians = radians_;
        x = x_;
        y = y_;
        incX = cos_ * cfg.moveSpeed;
        incY = sin_ * cfg.moveSpeed;
        noPierce = cfg.pierceCount < 1;

        return this;
    }

    public virtual bool Update() {
        // 维护 超时黑名单. 这步先把超时的删光
        var now = scene.time;
        var newTime = now + cfg.pierceDelay;
        for (var i = hitBlackList.Count - 1; i >= 0; --i) {
            if (hitBlackList[i].Value < now) {
                var lastIndex = hitBlackList.Count - 1;
                hitBlackList[i] = hitBlackList[lastIndex];
                hitBlackList.RemoveAt(lastIndex);
            }
        }

        if (noPierce) {                                                                          // 针对不能穿刺的情况优化
            SpaceItem m = null;
            if (cfg.radius <= Scene.cellSize) {
                m = monstersSpaceContainer.FindFirstCrossBy9(x, y, cfg.radius);                 // 在 9 宫范围内查询 首个相交
            } else {
                m = monstersSpaceContainer.FindNearestByRange(Scene.sd, x, y, cfg.radius);      // 扩散查询 首个相交
            }
            if (m != null) {
                ((Monster)m).Hurt(this);
                return true;    // 和怪一起死
            }
        } else {
            if (cfg.radius <= Scene.cellSize) {
                monstersSpaceContainer.Foreach9All(x, y, HitCheck);                             // 遍历九宫 挨个处理相交, 消耗 穿刺
            } else {
                monstersSpaceContainer.ForeachByRange(Scene.sd, x, y, cfg.radius, HitCheck);    // 扩散遍历 挨个处理相交, 消耗 穿刺
            }
            if (cfg.pierceCount < 0) return true;                                               // 穿刺已消耗完毕
        }

        // 让子弹直线移动
        x += incX;
        y += incY;

        // 坐标超出 grid地图 范围: 自杀
        if (x < 0 || x >= Scene.gridWidth || y < 0 || y >= Scene.gridHeight) return true;

        // 生命周期完结: 自杀
        return lifeEndTime < scene.time;
    }

    public bool HitCheck(SpaceItem m) {
        var vx = m.x - x;
        var vy = m.y - y;
        var r = m.radius + cfg.radius;
        if (vx * vx + vy * vy < r * r) {

            // 判断当前怪有没有存在于 超时黑名单
            var listLen = hitBlackList.Count;
            for (var i = 0; i < listLen; ++i) {
                if (hitBlackList[i].Key == m) return false;     // 存在: 不产生伤害, 继续遍历下一只怪
            }

            // 不存在：加入列表
            hitBlackList.Add(new KeyValuePair<SpaceItem, int>(m, scene.time + cfg.pierceDelay));

            // 伤害怪
            ((Monster)m).Hurt(this);

            // 如果穿刺计数 已用光，停止遍历
            if (--cfg.pierceCount < 0) {
                // 放点特效?
                return true;
            }
        }
        // 未命中：继续遍历下一只怪
        return false;
    }

    public virtual void Draw(float cx, float cy) {
        // 同步 & 坐标系转换( y 坐标需要反转 )
        var t = go.g.transform;
        t.position = new Vector3(x * Scene.cameraRatio, -y * Scene.cameraRatio, 0);

        // 根据半径同步缩放
        var s = cfg.radius / defaultRadius;
        t.localScale = new Vector3(s, s, s);
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
