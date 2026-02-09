# CMPM171 2D Roguelike

A 2D Roguelike game project developed with Unity.
- **Engine**: Unity 6000.2.7f2
- **Genre**: 2D Roguelike
## Collaboration Workflow

### 1. Clone Repository
```bash
git clone git@github.com:Bigfish3012/cmpm171-2d-roguelike.git
```

### 2. Create Your Own Branch
```bash
# Create and switch to a new branch from main
# Change "your-branch-name" to a name that you like to call
# Make sure that you are working on your own branch
# don't merge any change to the main branch before testing
git checkout -b your-branch-name 
```

### 3. Work on Your Branch
- Develop and make changes on your branch
- Commit changes regularly:
```bash
git add .
// // or you can use "git add <file name>", "git add <file name> <file name>"
git commit -m "Describe your changes"
```

### 4. Push and Create Pull Request
After completing your work, push your branch to the remote repository and create a Pull Request to merge into the `main` branch:
```bash
# Push your branch to the remote repository
git push origin your-branch-name

# If you see an error about no upstream branch, set the upstream:
git push --set-upstream origin your-branch-name

# Create a Pull Request on GitHub/GitLab, targeting the main branch
```

**Note**: If you encounter the error "The current branch has no upstream branch", use `git push --set-upstream origin your-branch-name` to set the remote tracking branch.

### 5. Code Review and Merge
- Ensure your code is tested and reviewed before merging
- Only thoroughly tested and reviewed code should be merged into the `main` branch

### Notes
- Always create your working branch from the latest `main` branch
- Ensure your code compiles and runs correctly before committing
- Write clear and descriptive commit messages
- Regularly sync updates from the main branch to your branch:
```bash
git checkout your-branch-name
git merge main
```

## Project Overview

This project is a 2D Roguelike game featuring procedurally generated maps and random elements, providing players with a unique experience each time they play.

## Project Structure

项目文件组织结构如下，请按照以下规则放置文件：

```
CMPM171-game/
├── Assets/
│   ├── Scripts/              # 游戏逻辑脚本 / Game logic scripts
│   │   ├── Gameplay/         # 核心游戏玩法脚本 / Core gameplay scripts
│   │   │   ├── PlayerController.cs    # 玩家控制器 / Player controller
│   │   │   ├── Player_settings.cs     # 玩家设置 / Player settings
│   │   │   ├── CameraFollow.cs        # 相机跟随逻辑 / Camera follow logic
│   │   │   ├── GunAim.cs              # 武器瞄准系统 / Weapon aiming system
│   │   │   ├── RangedShooter.cs       # 远程射击系统 / Ranged shooting system
│   │   │   ├── Projectile.cs          # 投射物逻辑 / Projectile logic
│   │   │   ├── Enemy1.cs              # 敌人类型1 / Enemy type 1
│   │   │   ├── Enemy_shooter.cs       # 射击型敌人 / Shooter enemy
│   │   │   ├── Enemy_healthbar.cs     # 敌人血条 / Enemy health bar
│   │   │   ├── EnemySpawner.cs        # 敌人生成器 / Enemy spawner
│   │   │   ├── IDamageable.cs         # 可受伤接口 / Damageable interface
│   │   │   └── IHealth.cs             # 生命值接口 / Health interface
│   │   └── Scenes/           # 场景相关脚本 / Scene-related scripts
│   │       ├── MainMenu.cs            # 主菜单脚本 / Main menu script
│   │       └── GameOver.cs            # 游戏结束脚本 / Game over script
│   │
│   ├── Prefabs/              # Unity 预制体 / Unity prefabs
│   │   └── GamePlay/        # 游戏玩法相关预制体 / Gameplay-related prefabs
│   │       ├── Player_bullet.prefab   # 玩家子弹预制体 / Player bullet prefab
│   │       ├── Enemy_bullet.prefab    # 敌人子弹预制体 / Enemy bullet prefab
│   │       ├── Enemy1.prefab          # 敌人1预制体 / Enemy1 prefab
│   │       ├── Enemy_shooter.prefab  # 射击型敌人预制体 / Shooter enemy prefab
│   │       └── Health.prefab         # 生命值预制体 / Health prefab
│   │
│   ├── Scenes/               # Unity 场景文件 / Unity scene files
│   │   ├── MainMenu.unity    # 主菜单场景 / Main menu scene
│   │   ├── SC_Prototype.unity # 游戏原型场景 / Game prototype scene
│   │   └── Gameover.unity    # 游戏结束场景 / Game over scene
│   │
│   ├── Art/                  # 美术资源（图片、精灵等） / Art assets (images, sprites, etc.)
│   │   ├── ground.png        # 地面贴图 / Ground texture
│   │   ├── Health.png        # 生命值图标 / Health icon
│   │   ├── heart.png         # 完整心形图标 / Full heart icon
│   │   ├── half heart.png    # 半心形图标 / Half heart icon
│   │   ├── empty heart.png   # 空心图标 / Empty heart icon
│   │   └── ...               # 其他美术资源 / Other art assets
│   │
│   ├── Audio/                # 音频资源（音乐、音效） / Audio assets (music, sound effects)
│   │
│   ├── UI/                   # UI 相关资源（Canvas、UI 元素等） / UI-related assets (Canvas, UI elements, etc.)
│   │
│   ├── Tiles/                # 瓦片地图资源 / Tile map assets
│   │
│   ├── Settings/             # Unity 设置文件 / Unity settings files
│   │
│   ├── TextMesh Pro/         # TextMesh Pro 资源 / TextMesh Pro assets
│   │
│   └── Shaders/              # 着色器文件 / Shader files
```

## Branch Strategy

This is a team project with the following branches:

- **main** - Main production branch