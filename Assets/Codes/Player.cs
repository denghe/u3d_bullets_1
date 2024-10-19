using System;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    // 各种引用
    public Scene scene;
    public Stage stage;
    public GO go, go2;

    /// <summary>
    /// 原始半径( 和资源相关 )
    /// </summary>
    public const float defaultRadius = 32f;

    /// <summary>
    /// 每帧移动距离( 逻辑像素 )
    /// </summary>
    public float moveSpeed;

    /// <summary>
    /// 半径( 逻辑像素 )
    /// </summary>
    public float radius;

    /// <summary>
    /// 坐标( 逻辑像素 )
    /// </summary>
    public float x, y;

    /// <summary>
    /// 基础伤害倍率
    /// </summary>
    public int damage;

    /// <summary>
    /// 暴击率
    /// </summary>
    public float criticalRate;

    /// <summary>
    /// 暴击倍率
    /// </summary>
    public float criticalDamageRatio;

    /// <summary>
    /// 技能数组
    /// </summary>
    public List<PlayerSkill> skills = new();

    public Player(Scene scene_) {
        scene = scene_;

        GO.Pop(ref go);
        go.r.sprite = scene.sprite_player;

        GO.Pop(ref go2);
        go2.r.sprite = scene.sprite_player;
        go2.r.color = new Color(1, 1, 1, 0.01f);
    }

    public void Init(Stage stage_, float x_, float y_) {
        stage = stage_;
        x = x_;
        y = y_;
        ReadDataFromUI();
    }

    /// <summary>
    /// 从 UI 同步参数
    /// </summary>
    public void ReadDataFromUI() {
        moveSpeed = 300f / Scene.fps;   //  = scene.playerMoveSpeed;
        radius = scene.playerRadius;
        damage = 1; // = scene.playerDamage;
        criticalRate = scene.playerCriticalRate;
        criticalDamageRatio = scene.playerCriticalDamageRatio;
    }

    public bool Update() {
        ReadDataFromUI();

        // 玩家控制移动(条件: 还活着)
        var mv = scene.playerMoveValue;
        x += mv.x * moveSpeed;
        y += mv.y * moveSpeed;

        // 强行限制移动范围
        if (x < 0) x = 0;
        else if (x >= Scene.gridWidth) x = Scene.gridWidth - float.Epsilon;
        if (y < 0) y = 0;
        else if (y >= Scene.gridHeight) y = Scene.gridHeight - float.Epsilon;

        // 驱动技能
        var len = skills.Count;
        for (int i = 0; i < len; ++i) {
            skills[i].Update();
        }

        return false;
    }

    public void Draw() {
        // 同步 & 坐标系转换( y 坐标需要反转 )
        var t = go.g.transform;
        var p = new Vector3(x * Scene.cameraRatio, -y * Scene.cameraRatio, 0);
        t.position = p;
        t.eulerAngles = new Vector3(0, 0, -scene.playerMoveRadians * (float)(180 / Math.PI) );
        var s = radius / defaultRadius;
        t.localScale = new Vector3(s, s, s);

        // 用来表达攻击范围
        t = go2.g.transform;
        t.position = p;
        s = scene.playerBulletAimRange / defaultRadius;
        t.localScale = new Vector3(s, s, s);
    }

    public void Destroy() {
        foreach (var skill in skills) {
            skill.Destroy();
        }
#if UNITY_EDITOR
        if (go.g != null)
#endif
        {
            GO.Push(ref go);
        }
#if UNITY_EDITOR
        if (go2.g != null)
#endif
        {
            GO.Push(ref go2);
        }
    }
}

