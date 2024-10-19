using System;
using UnityEngine;

public partial class Scene : MonoBehaviour {

    /*******************************************************************************************/
    /*******************************************************************************************/
    // 需要拖拽相应的资源到此

    public Material material;
    public Sprite sprite_player;
    public Sprite sprite_monster;
    public Sprite sprite_bullet;
    public Sprite sprite_explode;
    public Sprite[] sprites_font_outline;

    /*******************************************************************************************/
    /*******************************************************************************************/
    // 配置常量区

    /// <summary>
    /// 逻辑帧率
    /// </summary>
    internal const int fps = 120;   //60;

    /// <summary>
    /// 逻辑帧率间隔时长
    /// </summary>
    internal const float frameDelay = 1.0f / fps;

    /// <summary>
    /// 每个格子的边长( 正方形 ), 根据设计尺寸和最大体积的 玩家或怪 来推导
    /// </summary>
    internal const float cellSize = 64;

    /// <summary>
    /// 格子边长的 "半径"
    /// </summary>
    internal const float cellRadius = cellSize / 2;

    /// <summary>
    /// 大地图格子行列数
    /// </summary>
    internal const int numRows = 2048, numCols = 2048;

    /// <summary>
    /// 大地图总宽度( 逻辑像素 )
    /// </summary>
    internal const float gridWidth = numCols * cellSize;

    /// <summary>
    /// 大地图总高度( 逻辑像素 )
    /// </summary>
    internal const float gridHeight = numRows * cellSize;

    /// <summary>
    /// 大地图中心点坐标( 逻辑像素 )
    /// </summary>
    internal const float gridWidth_2 = gridWidth / 2, gridHeight_2 = gridHeight / 2;
    internal const float gridCenterX = gridWidth_2, gridCenterY = gridHeight_2;

    /// <summary>
    /// 设计分辨率( 通常用来做裁剪 )
    /// </summary>
    internal const float designWidth = 1920, designHeight = 1080;

    /// <summary>
    /// 设计分辨率的一半 方便计算和使用
    /// </summary>
    internal const float designWidth_2 = designWidth / 2, designHeight_2 = designHeight / 2;

    /// <summary>
    /// 设计分辨率到 摄像头坐标 的转换系数( camera size 为 5.4 )
    /// </summary>
    internal const float cameraRatio = 0.01f;

    /*******************************************************************************************/
    /*******************************************************************************************/
    // 运行时变量, 关卡啥的

    /// <summary>
    /// 空间索引容器要用到的公用查表数据
    /// </summary>
    internal static SpaceRingDiffuseData sd = new(100, (int)cellSize);

    /// <summary>
    /// 当前总的运行帧编号
    /// </summary>
    internal int time = 0;

    /// <summary>
    /// 计算 fps 用的 Update() 次数 ( Draw )
    /// </summary>
    internal int drawCounter = 0;

    /// <summary>
    /// 计算 fps 用的最后一次时间点
    /// </summary>
    internal float lastSecs;

    /// <summary>
    /// 用于稳定调用 逻辑 Update 的时间累计变量
    /// </summary>
    internal float timePool = 0;

    /// <summary>
    /// 当前玩家, 一开始就创建出来，不会为空
    /// </summary>
    internal Player player;

    /// <summary>
    /// 当前关卡, 一开始就创建出来，不会为空( 一开始是 菜单关卡 )
    /// </summary>
    internal Stage stage;

    /// <summary>
    /// 切换关卡
    /// </summary>
    internal void SetStage(Stage newStage) {
        stage.Destroy();
        stage = newStage;
    }

    /*******************************************************************************************/
    /*******************************************************************************************/
    // u3d 系统生命周期函数

    void Start() {
        Debug.Assert(sprite_player != null);
        Debug.Assert(sprite_monster != null);
        Debug.Assert(sprite_bullet != null);

        // 初始化 UI 绑定
        BindUI();

        // 初始化玩家输入系统
        InitInputAction();

        // 初始化 HDR 显示模式
        try {
            HDROutputSettings.main.RequestHDRModeChange(true);
        } catch (Exception e) {
            Debug.Log(e);
        }

        // 初始化 底层绘制对象池
        GO.Init(material, 20000);

        // 初始化 玩家
        player = new Player(this);

        // 初始化 关卡
        stage = new Stage(this);
    }

    void Update() {
        // 处理输入( 只是填充 playerMoving 等状态值 )
        HandlePlayerInput();

        // 同步 UI 参数
        player.radius = playerRadius;

        // 按设计帧率驱动游戏逻辑
        timePool += Time.deltaTime;     // 实测发现这个时间，有最大值限制. 当 fps 不足 30 时，整个游戏会拖慢
        if (timePool > frameDelay) {
            timePool -= frameDelay;
            ++time;
            stage.Update();
        }

        // 同步显示
        stage.Draw();

        // 更新 UI
        UpdateUI();
    }

    void OnDestroy() {
        stage.Destroy();
        GO.Destroy();
    }

}
