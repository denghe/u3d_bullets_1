using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// todo:
// fps 做到 UI 配置
// 击退相关?
// 开始，停止，修改 radius 的预览? 伤害统计面板? 连击?? 添加弹道???
// 考虑运行时反射生成 一些有规律的控件

partial class Scene {

    /*******************************************************************************************/
    /*******************************************************************************************/
    // 需要拖拽相应的控件到此

    public Button button_generateMonsters;

    public TextMeshProUGUI text_playerRadius;
    public Slider slider_playerRadius;

    public TextMeshProUGUI text_playerBulletRadius;
    public Slider slider_playerBulletRadius;

    public TextMeshProUGUI text_monsterRadius;
    public Slider slider_monsterRadius;

    public TextMeshProUGUI text_fps;

    public Toggle toggle_edgeLeft;
    public Toggle toggle_edgeRight;
    public Toggle toggle_edgeTop;
    public Toggle toggle_edgeBottom;


    public TextMeshProUGUI text_numMaxMonsters;
    public Slider slider_numMaxMonsters;

    public TextMeshProUGUI text_genSpeed;
    public Slider slider_genSpeed;

    public TextMeshProUGUI text_genDuration;
    public Slider slider_genDuration;

    public TextMeshProUGUI text_monsterHP;
    public Slider slider_monsterHP;

    public TextMeshProUGUI text_playerBulletShootSpeed;
    public Slider slider_playerBulletShootSpeed;

    public TextMeshProUGUI text_playerBulletMoveSpeed;
    public Slider slider_playerBulletMoveSpeed;

    public TextMeshProUGUI text_playerBulletAimRange;
    public Slider slider_playerBulletAimRange;

    public TextMeshProUGUI text_playerBulletLifeSeconds;
    public Slider slider_playerBulletLifeSeconds;

    public TextMeshProUGUI text_playerBulletPierceCount;
    public Slider slider_playerBulletPierceCount;

    public TextMeshProUGUI text_playerBulletPierceIntervalSecods;
    public Slider slider_playerBulletPierceIntervalSecods;

    public TextMeshProUGUI text_playerBulletDamage;
    public Slider slider_playerBulletDamage;

    public TextMeshProUGUI text_playerCriticalRate;
    public Slider slider_playerCriticalRate;

    public TextMeshProUGUI text_playerCriticalDamageRatio;
    public Slider slider_playerCriticalDamageRatio;


    public Button button_clearMonsters;
    public TextMeshProUGUI text_numMonsters;

    public Button button_clearPlayerBullets;
    public TextMeshProUGUI text_numPlayerBullets;

    public Button button_clearMonsterGeneraters;
    public TextMeshProUGUI text_numMonsterGeneraters;


    /*******************************************************************************************/
    /*******************************************************************************************/
    // 下面的变量和 UI 面板绑定( 会被 stage, skill 直读 )

    /// <summary>
    /// 玩家半径( 当这个值 大于 cellRadius 时，便不能再用于 space 9宫查询 )
    /// </summary>
    internal float playerRadius = 32, playerRadiusMin = 4, playerRadiusMax = 128;

    /// <summary>
    /// 玩家子弹半径( 当这个值 大于 cellRadius 时，便不能再用于 space 9宫查询 )
    /// </summary>
    internal float playerBulletRadius = 8, playerBulletRadiusMin = 4, playerBulletRadiusMax = 512;

    /// <summary>
    /// 怪物半径( 最大值不得超过 cellRadius )
    /// </summary>
    internal float monsterRadius = 32, monsterRadiusMin = 4, monsterRadiusMax = Scene.cellRadius;

    /// <summary>
    /// 怪物随机生成时从屏幕的哪些边缘出现，这个是边缘枚举集合
    /// </summary>
    internal System.Collections.Generic.List<int> genMonsterSideNumbers = new(new int[] { 1 });

    /// <summary>
    /// 怪物最大数量控制
    /// </summary>
    internal int numMaxMonsters = 200, numMaxMonstersMin = 1, numMaxMonstersMax = 10000;

    /// <summary>
    /// 每秒怪物生成数量
    /// </summary>
    internal float genSpeed = 1, genSpeedMin = 1f, genSpeedMax = 100;

    /// <summary>
    /// 生成行为持续时间( 秒 )
    /// </summary>
    internal float genDuration = 30, genDurationMin = 1, genDurationMax = 120;

    /// <summary>
    /// 怪物血量
    /// </summary>
    internal int monsterHP = 5, monsterHPMin = 1, monsterHPMax = 100;


    /// <summary>
    /// 玩家射速( 每秒多少颗 )
    /// </summary>
    internal float playerBulletShootSpeed = 1, playerBulletShootSpeedMin = 0.1f, playerBulletShootSpeedMax = 120;

    /// <summary>
    /// 玩家子弹每秒飞行距离( 逻辑像素 )
    /// </summary>
    internal float playerBulletMoveSpeed = 600, playerBulletMoveSpeedMin = 1, playerBulletMoveSpeedMax = 3000;

    /// <summary>
    /// 怪物进入多少半径范围，玩家就开火
    /// </summary>
    internal float playerBulletAimRange = 470, playerBulletAimRangeMin = 1, playerBulletAimRangeMax = 1000;

    /// <summary>
    /// 玩家子弹存活多少秒
    /// </summary>
    internal float playerBulletLifeSeconds = 3, playerBulletLifeSecondsMin = 0.1f, playerBulletLifeSecondsMax = 100;

    /// <summary>
    /// 玩家子弹穿透次数
    /// </summary>
    internal int playerBulletPierceCount = 1, playerBulletPierceCountMin = 0, playerBulletPierceCountMax = 50;

    /// <summary>
    /// 玩家子弹穿透相同怪冷却时长
    /// </summary>
    internal float playerBulletPierceIntervalSecods = 0.2f, playerBulletPierceIntervalSecodsMin = 0.01f, playerBulletPierceIntervalSecodsMax = 1;

    /// <summary>
    /// 玩家子弹威力( 总能量? 范围伤害时均摊??? )
    /// </summary>
    internal int playerBulletDamage = 3, playerBulletDamageMin = 1, playerBulletDamageMax = 100;

    /// <summary>
    /// 玩家子弹暴击机率
    /// </summary>
    internal float playerCriticalRate = 0.5f, playerCriticalRateMin = 0f, playerCriticalRateMax = 1;

    /// <summary>
    /// 玩家子弹暴伤倍率
    /// </summary>
    internal float playerCriticalDamageRatio = 2f, playerCriticalDamageRatioMin = 1f, playerCriticalDamageRatioMax = 10;

    /*******************************************************************************************/
    /*******************************************************************************************/
    // 处理函数

    /// <summary>
    /// 简化一下成员反射函数写法
    /// </summary>
    internal FieldInfo GetField(string name) {
        return GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// 根据成员名字来反射定位并绑定 text + slider
    /// </summary>
    internal void Bind_T_S(string name) {
        var t = (TextMeshProUGUI)GetField("text_" + name).GetValue(this);
        var s = (Slider)GetField("slider_" + name).GetValue(this);
        var f = GetField(name);                                         // 指向成员变量
        var value = f.GetValue(this);                                   // 读当前值
        t.text = value.ToString();
        if (f.FieldType == typeof(int)) {                               // int
            s.wholeNumbers = true;
            s.minValue = (int)GetField(name + "Min").GetValue(this);
            s.maxValue = (int)GetField(name + "Max").GetValue(this);
            s.value = (int)value;
            s.onValueChanged.AddListener(v => {
                t.text = v.ToString();
                f.SetValue(this, (int)v);
            });
        } else {                                                        // float
            s.wholeNumbers = false;
            s.minValue = (float)GetField(name + "Min").GetValue(this);
            s.maxValue = (float)GetField(name + "Max").GetValue(this);
            s.value = (float)value;
            s.onValueChanged.AddListener(v => {
                t.text = v.ToString();
                f.SetValue(this, v);
            });
        }
    }

    /// <summary>
    /// 通过 toggle 同步 genMonsterSideNumbers
    /// </summary>
    internal void SyncGenMonsterSideNumbers(int idx, bool add) {
        if (add) {
            genMonsterSideNumbers.Add(idx);
        } else {
            genMonsterSideNumbers.Remove(idx);
        }
    }

    /// <summary>
    /// 绑定部分 UI 控件的监听事件
    /// </summary>
    internal void BindUI() {
        button_generateMonsters.onClick.AddListener(() => {
            stage.GenerateMonsters();
        });

        toggle_edgeLeft.onValueChanged.AddListener(b => SyncGenMonsterSideNumbers(0, b));
        toggle_edgeRight.onValueChanged.AddListener(b => SyncGenMonsterSideNumbers(1, b));
        toggle_edgeTop.onValueChanged.AddListener(b => SyncGenMonsterSideNumbers(2, b));
        toggle_edgeBottom.onValueChanged.AddListener(b => SyncGenMonsterSideNumbers(3, b));

        Bind_T_S(nameof(playerRadius));
        Bind_T_S(nameof(playerBulletRadius));
        Bind_T_S(nameof(monsterRadius));
        Bind_T_S(nameof(numMaxMonsters));
        Bind_T_S(nameof(genSpeed));
        Bind_T_S(nameof(genDuration));
        Bind_T_S(nameof(monsterHP));
        Bind_T_S(nameof(playerBulletShootSpeed));
        Bind_T_S(nameof(playerBulletMoveSpeed));
        Bind_T_S(nameof(playerBulletAimRange));
        Bind_T_S(nameof(playerBulletLifeSeconds));
        Bind_T_S(nameof(playerBulletPierceCount));
        Bind_T_S(nameof(playerBulletPierceIntervalSecods));
        Bind_T_S(nameof(playerBulletDamage));
        Bind_T_S(nameof(playerCriticalRate));
        Bind_T_S(nameof(playerCriticalDamageRatio));

        button_clearMonsterGeneraters.onClick.AddListener(() => {
            stage.ClearMonsterGeneraters();
        });
        button_clearMonsters.onClick.AddListener(() => {
            stage.ClearMonsters();
        });
        button_clearPlayerBullets.onClick.AddListener(() => {
            stage.ClearPlayerBullets();
        });
    }

    /// <summary>
    /// 更新部分 UI 显示
    /// </summary>
    internal void UpdateUI() {
        // FPS
        ++drawCounter;
        var nowSecs = Time.fixedTime;
        var elapsedSecs = nowSecs - lastSecs;
        if (elapsedSecs >= 1) {
            lastSecs = nowSecs;
            text_fps.text = ((int)(drawCounter / elapsedSecs)).ToString();
            drawCounter = 0;
        }

        // 统计
        text_numMonsters.text = stage.monsters.Count.ToString();
        text_numMonsterGeneraters.text = stage.monsterGenerators.Count.ToString();
        text_numPlayerBullets.text = stage.playerBullets.Count.ToString();
    }

}
