using System.Collections.Generic;
using UnityEngine;

public class Stage {

    // 各种引用
    public Scene scene;
    public Player player;

    // todo: backgounds?
    public List<PlayerBullet> playerBullets = new();
    public List<Monster> monsters = new();
    public SpaceContainer monstersSpaceContainer;
    public List<Effect_Explosion> effectExplosions = new();
    public List<Effect_Number> effectNumbers = new();
    public List<MonsterGenerator> monsterGenerators = new();


    /*************************************************************************************************************************/
    /*************************************************************************************************************************/

    /// <summary>
    /// 添加一些 怪物发生器
    /// </summary>
    public virtual void GenerateMonsters() {
        var time = scene.time;
        monsterGenerators.Add(new MonsterGenerator(this
            , time + (int)(Scene.fps * 0)
            , time + (int)(Scene.fps * scene.genDuration)
            , scene.genSpeed));
    }

    /*************************************************************************************************************************/
    /*************************************************************************************************************************/

    public Stage(Scene scene_) {
        scene = scene_;
        player = scene_.player;
        monstersSpaceContainer = new(Scene.numRows, Scene.numCols, Scene.cellSize);

        // 设置 Player 坐标
        player.Init(this, Scene.gridCenterX, Scene.gridCenterY);

        // 设置 Player 技能
        new PlayerSkill(this);
    }


    public virtual void Update() {
        Update_Effect_Explosions();
        Update_Effect_Numbers();
        Update_Monsters();
        Update_MonstersGenerators();
        Update_PlayerBullets();
        player.Update();
    }

    public virtual void Draw() {
        // 同步 camera 的位置
        var t = scene.transform;
        t.position = new Vector3(player.x * Scene.cameraRatio, -player.y * Scene.cameraRatio, t.position.z);

        // 剔除 & 同步 GO
        var cx = player.x;
        var cy = player.y;

        var len = monsters.Count;
        for (int i = 0; i < len; ++i) {
            monsters[i].Draw(cx, cy);
        }
        len = playerBullets.Count;
        for (int i = 0; i < len; ++i) {
            playerBullets[i].Draw(cx, cy);
        }
        len = effectExplosions.Count;
        for (int i = 0; i < len; ++i) {
            effectExplosions[i].Draw(cx, cy);
        }
        len = effectNumbers.Count;
        for (int i = 0; i < len; ++i) {
            effectNumbers[i].Draw(cx, cy);
        }

        player.Draw();
    }

    public virtual void Destroy() {
        ClearMonsterGeneraters();
        ClearMonsters();
        ClearPlayerBullets();
        ClearEffectExplosions();
        ClearEffectNumbers();
        // ...
    }


    /*************************************************************************************************************************/
    /*************************************************************************************************************************/

    /// <summary>
    /// 清理所有怪物生成器
    /// </summary>
    public void ClearMonsterGeneraters() {
        foreach (var o in monsterGenerators) {
            o.Destroy();
        }
        monsterGenerators.Clear();
    }

    /// <summary>
    /// 清理所有怪
    /// </summary>
    public void ClearMonsters() {
        foreach (var o in monsters) {
            o.Destroy(false);             // 纯 destroy，不从 monsters 移除自己
        }
        monsters.Clear();
    }

    /// <summary>
    /// 清理所有玩家子弹
    /// </summary>
    public void ClearPlayerBullets() {
        foreach (var o in playerBullets) {
            o.Destroy();
        }
        playerBullets.Clear();
    }

    /// <summary>
    /// 清理所有爆炸特效
    /// </summary>
    public void ClearEffectExplosions() {
        foreach (var o in effectExplosions) {
            o.Destroy();
        }
        effectExplosions.Clear();
    }

    /// <summary>
    /// 清理所有数字特效
    /// </summary>
    public void ClearEffectNumbers() {
        foreach (var o in effectNumbers) {
            o.Destroy();
        }
        effectNumbers.Clear();
    }


    /// <summary>
    /// 执行怪生成配置并返回是否已经全部执行完毕
    /// </summary>
    public int Update_MonstersGenerators() {
        var time = scene.time;
        var os = monsterGenerators;
        for (int i = os.Count - 1; i >= 0; i--) {
            var mg = os[i];
            if (mg.activeTime <= time) {
                if (mg.destroyTime >= time) {
                    mg.Update();
                } else {
                    int lastIndex = os.Count - 1;
                    os[i] = os[lastIndex];
                    os.RemoveAt(lastIndex);
                }
            }
        }
        return os.Count;
    }

    /// <summary>
    /// 驱动所有怪
    /// </summary>
    public int Update_Monsters() {
        var os = monsters;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                o.Destroy();    // 会从 容器 自动移除自己
            }
        }
        return os.Count;
    }

    /// <summary>
    /// 驱动所有爆炸特效
    /// </summary>
    public int Update_Effect_Explosions() {
        var os = effectExplosions;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                int lastIndex = os.Count - 1;
                os[i] = os[lastIndex];
                os.RemoveAt(lastIndex);
                o.Destroy();
            }
        }
        return os.Count;
    }

    /// <summary>
    /// 驱动所有数字特效
    /// </summary>
    public int Update_Effect_Numbers() {
        var os = effectNumbers;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                int lastIndex = os.Count - 1;
                os[i] = os[lastIndex];
                os.RemoveAt(lastIndex);
                o.Destroy();
            }
        }
        return os.Count;
    }

    /// <summary>
    /// 驱动所有玩家子弹
    /// </summary>
    public int Update_PlayerBullets() {
        var os = playerBullets;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                int lastIndex = os.Count - 1;
                os[i] = os[lastIndex];
                os.RemoveAt(lastIndex);
                o.Destroy();
            }
        }
        return os.Count;
    }

    /// <summary>
    /// 驱动所有玩家
    /// </summary>
    public int Update_Player() {
        player.Update();
        return 1;
    }

    /// <summary>
    /// 当前玩家所在屏幕区域边缘随机一个点返回
    /// </summary>
    public Vector2 GetRndPosOutSideTheArea() {
        var idxs = scene.genMonsterSideNumbers;
        if (idxs.Count > 0) {
            var e = idxs[Random.Range(0, idxs.Count)];
            switch (e) {
                case 0:
                    return new(player.x - Scene.designWidth_2 - Scene.cellSize, player.y + Random.Range(-Scene.designHeight_2 - Scene.cellSize, Scene.designHeight_2 + Scene.cellSize));
                case 1:
                    return new(player.x + Scene.designWidth_2 + Scene.cellSize, player.y + Random.Range(-Scene.designHeight_2 - Scene.cellSize, Scene.designHeight_2 + Scene.cellSize));
                case 2:
                    return new(player.x + Random.Range(-Scene.designWidth_2 - Scene.cellSize, Scene.designWidth_2 + Scene.cellSize), player.y - Scene.designHeight_2 - Scene.cellSize);
                case 3:
                    return new(player.x + Random.Range(-Scene.designWidth_2 - Scene.cellSize, Scene.designWidth_2 + Scene.cellSize), player.y + Scene.designHeight_2 + Scene.cellSize);
            }
        }
        return new(player.x, player.y);
    }

}
