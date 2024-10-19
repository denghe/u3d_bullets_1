using UnityEngine;

public class MonsterGenerator {
    public Scene scene;
    public Stage stage;

    public int activeTime, destroyTime;
    public float countPool, countIncPerFrame;

    public MonsterGenerator(Stage stage_, int activeTime_, int destroyTime_, float generateNumsPerSeconds_) {
        stage = stage_;
        scene = stage_.scene;
        activeTime = activeTime_;
        destroyTime = destroyTime_;
        countIncPerFrame = generateNumsPerSeconds_ / Scene.fps;
    }

    public virtual void Update() {
        for (countPool += countIncPerFrame; countPool >= 1; --countPool) {
            if (stage.monsters.Count < scene.numMaxMonsters) {
                var pos = stage.GetRndPosOutSideTheArea();
                new Monster(stage).Init(pos.x, pos.y);
            }
        }
    }

    public virtual void Destroy() {
    }
}
