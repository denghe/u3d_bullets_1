using UnityEngine;

public struct PlayerBulletConfig {
    public float radius;                                    // 碰撞检测半径
    public int damage;                                      // 技能自己的伤害倍率
    public float moveSpeed;                                 // 按照 fps 来算的每一帧的移动距离
    public int life;                                        // 子弹存在时长( 帧 ): fps * 秒
    public int pierceCount;                                 // 最大可穿透次数
    public int pierceDelay;                                 // 穿透时间间隔 帧数( 针对相同目标 )
    public int knockbackForce;                              // 击退强度( 退多少帧, 多远 )
}

public class PlayerSkill {
    // 各种引用
    public Scene scene;
    public Stage stage;
    public Player player;

    public int icon = 123;                                  // 用于 UI 展示
    public float progress = 1;                              // 进度
    public float shootInitDistance;                         // 子弹射击时和玩家的初始距离

    public float countPool;                                 // 累计应该发射的颗数
    public float countIncPerFrame;                          // 每帧的累计值增量

    // 创建子弹时，复制到子弹上
    public PlayerBulletConfig cfg = new();

    public PlayerSkill(Stage stage_) {
        stage = stage_;
        scene = stage_.scene;
        player = scene.player;
        player.skills.Add(this);
        ReadDataFromUI();
    }

    /// <summary>
    /// 从 UI 同步参数
    /// </summary>
    public void ReadDataFromUI() {
        cfg.radius = scene.playerBulletRadius;
        cfg.damage = scene.playerBulletDamage;
        cfg.moveSpeed = scene.playerBulletMoveSpeed / Scene.fps;
        cfg.life = (int)(scene.playerBulletLifeSeconds * Scene.fps);
        cfg.pierceCount = scene.playerBulletPierceCount;
        cfg.pierceDelay = (int)(scene.playerBulletPierceIntervalSecods * Scene.fps);
        //cfg.knockbackForce = scene.playerBulletKnockbackForce;

        countIncPerFrame = scene.playerBulletShootSpeed / Scene.fps;
        shootInitDistance = player.radius + Mathf.Min(Scene.cellRadius, cfg.radius);
    }

    public virtual void Update() {
        ReadDataFromUI();
        countPool += countIncPerFrame;
        var count = (int)countPool;
        if (count > 0) {
            countPool -= count;
            var speedStep = cfg.moveSpeed / count;
            for (int i = 0; i < count; ++i) {
                // 子弹发射逻辑：
                // 找射程内 距离最近的 1 只 朝向其发射 1 子弹
                var x = player.x;
                var y = player.y;
                var o = stage.monstersSpaceContainer.FindNearestByRange(Scene.sd, x, y, scene.playerBulletAimRange);
                if (o != null) {
                    var dy = o.y - y;
                    var dx = o.x - x;
                    var r = Mathf.Atan2(dy, dx);
                    var cos = Mathf.Cos(r);
                    var sin = Mathf.Sin(r);
                    var d = shootInitDistance - speedStep * i;
                    var tarX = x + cos * d;
                    var tarY = y + sin * d;
                    new PlayerBullet(this).Init(tarX, tarY, r, cos, sin);
                }
            }
        }
        progress = countPool;
    }

    public virtual void Destroy() {
    }
}
