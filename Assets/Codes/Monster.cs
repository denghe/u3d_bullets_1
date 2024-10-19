using System.Collections.Generic;
using UnityEngine;

public class Monster : SpaceItem {
    // 各种指向
    public Scene scene;
    public Stage stage;
    public Player player;
    public List<Monster> monsters;

    /// <summary>
    /// 资源
    /// </summary>
    public GO go;

    /// <summary>
    /// 自己位于关卡怪数组的下标
    /// </summary>
    public int indexOfContainer;

    /// <summary>
    /// 原始半径( 和资源相关 )
    /// </summary>
    public const float defaultRadius = 32f;

    // radius 在基类里
    /// <summary>
    /// 最后移动朝向
    /// </summary>
    public float radians;

    /// <summary>
    /// 追赶玩家时的目标坐标偏移量( 防止放风筝时重叠到一起 )
    /// </summary>
    public float tarOffsetX, tarOffsetY, tarOffsetRadius;

    /// <summary>
    /// 怪血量
    /// </summary>
    public double hp;

    /// <summary>
    /// 每一帧的移动距离
    /// </summary>
    public float moveSpeed;

    /// <summary>
    /// 变色结束时间( 受伤会变色 )
    /// </summary>
    public int colorEndTime;

    /// <summary>
    /// 受伤变色的时长. 反复受伤就会重置变色结束时间
    /// </summary>
    public const int changeColorDelay = 10;

    /// <summary>
    /// 被击退结束时间点
    /// </summary>
    public int knockbackEndTime;

    /// <summary>
    /// 被击退移动增量衰减值
    /// </summary>
    public float knockbackDecay = 0.01f;

    /// <summary>
    /// 被击退移动增量倍率 每帧 -= decay
    /// </summary>
    public float knockbackIncRate = 1;

    /// <summary>
    /// 被击退移动增量 实际 x += inc * rate
    /// </summary>
    public float knockbackIncX, knockbackIncY;

    // todo: 血量显示?

    public Monster(Stage stage_) {
        spaceContainer = stage_.monstersSpaceContainer;
        stage = stage_;
        player = stage_.player;
        scene = stage_.scene;
        monsters = stage_.monsters;

        indexOfContainer = monsters.Count;
        monsters.Add(this);

        GO.Pop(ref go);
        go.r.sprite = scene.sprite_monster;
    }

    public void Init(float x_, float y_) {
        x = x_;
        y = y_;
        radius = scene.monsterRadius;
        tarOffsetRadius = Scene.cellSize * 3;   // = scene.tarOffsetRadius;
        hp = scene.monsterHP;
        moveSpeed = 150f / Scene.fps; // = scene.monsterMoveSpeed;
        spaceContainer.Add(this);   // 放入空间索引容器
        ResetTargetOffsetXY();
    }

    public virtual bool Update() {
        // 被击退中?
        if (knockbackEndTime >= scene.time) {
            x += knockbackIncX * knockbackIncRate;   // 位移
            y += knockbackIncY * knockbackIncRate;
            knockbackIncRate -= knockbackDecay;  // 衰减

            spaceContainer.Update(this);    // 更新在空间索引容器中的位置
            return false;
        }

        // 判断是否已接触到 玩家. 接触到就造成伤害, 没接触到就继续移动
        var dx = player.x - x;
        var dy = player.y - y;
        var dd = dx * dx + dy * dy;
        var r2 = player.radius + radius;
        if (dd < r2 * r2) {
            //player.Hurt(damage);
        } else {
            // 判断是否已到达 偏移点. 已到达: 重新选择偏移点. 未到达: 移动
            dx = player.x - x + tarOffsetX;
            dy = player.y - y + tarOffsetY;
            dd = dx * dx + dy * dy;
            if (dd < radius * radius) {
                ResetTargetOffsetXY();
            }
            // 计算移动方向并增量
            radians = Mathf.Atan2(dy, dx);
            var cos = Mathf.Cos(radians);
            x += cos * moveSpeed;
            y += Mathf.Sin(radians) * moveSpeed;
        }

        // 强行限制移动范围
        if (x < 0) x = 0;
        else if (x >= Scene.gridWidth) x = Scene.gridWidth - float.Epsilon;
        if (y < 0) y = 0;
        else if (y >= Scene.gridHeight) y = Scene.gridHeight - float.Epsilon;

        spaceContainer.Update(this);    // 更新在空间索引容器中的位置
        return false;
    }

    public virtual void Draw(float cx, float cy) {
        // 同步尺寸缩放( 根据半径推送算 ) 坐标系转换( y 坐标需要反转 )
        var t = go.g.transform;
        var s = radius / defaultRadius;
        t.localScale = new Vector3(s, s, s);
        t.position = new Vector3(x * Scene.cameraRatio, -y * Scene.cameraRatio, 0);
        t.eulerAngles = new Vector3(0, 0, -radians * (180f / Mathf.PI));

        // 看情况变色
        if (scene.time >= colorEndTime) {
            go.r.color = Color.blue;
        } else {
            go.r.color = Color.red;
        }
    }

    public virtual void Destroy(bool needRemoveFromContainer = true) {
#if UNITY_EDITOR
        if (go.g != null)
#endif
        {
            GO.Push(ref go);
        }

        // 从空间索引容器移除
        spaceContainer.Remove(this);

        // 从 stage 容器交换删除
        if (needRemoveFromContainer) {
            var ms = stage.monsters;
            var lastIndex = ms.Count - 1;
            var last = ms[lastIndex];
            last.indexOfContainer = indexOfContainer;
            ms[indexOfContainer] = last;
            ms.RemoveAt(lastIndex);
        }
    }

    // 重置偏移
    void ResetTargetOffsetXY() {
        var p = Helpers.GetRndPosDoughnut(tarOffsetRadius, 0.1f);
        tarOffsetX = p.x;
        tarOffsetY = p.y;
    }

    // 令怪受伤, 播特效. 返回怪是否 已死亡. 已死亡将从数组移除该怪( !!! 重要 : 需位于 倒循环 for 内 )
    public bool Hurt(PlayerBullet b) {

        // 结合暴击算最终伤害值
        var dmg = (double)b.cfg.damage * (double)player.damage;
        var isCritical = Random.value <= player.criticalRate;
        if (isCritical) {
            dmg *= player.criticalDamageRatio;
        }
        dmg = System.Math.Ceiling(dmg);

        if (hp <= dmg) {
            // 怪被打死: 删, 播特效
            new Effect_Explosion(stage, x, y, radius / defaultRadius);
            new Effect_Number(this, b, hp, isCritical);
            Destroy();
            return true;
        } else {
            // 怪没死: 播飙血特效( todo )
            hp -= dmg;
            new Effect_Number(this, b, dmg, isCritical);

            // todo
            //// 击退?
            //var knockbackForce = pb.cfg.knockbackForce;
            //if (knockbackForce > 0) {
            //    knockbackEndTime = scene.time + knockbackForce;
            //    knockbackIncRate = 1;
            //    knockbackDecay = 1 / knockbackForce;
            //    knockbackIncX = -Mathf.Cos(radians) * moveSpeed;
            //    knockbackIncY = -Mathf.Sin(radians) * moveSpeed;
            //}

            // 变色一会儿
            colorEndTime = scene.time + changeColorDelay;

            return false;

        }

    }
}
