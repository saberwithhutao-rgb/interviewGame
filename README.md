# EXP Nexus

Unity 做的第一人称动作 demo,个人面试作品:远程与近战武器、数据驱动的技能和状态效果、敌人、对话、存档,以及基于 Steam P2P 的联机。

## 环境
- Unity 2022.3.62f3c1(中国版)
- Netcode for GameObjects 1.12.2,联机走 Facepunch.Steamworks 的 Steam P2P 传输
- TextMesh Pro / ProBuilder / Polybrush

## 打开工程
1. 用上面的 Unity 版本打开本目录,首次导入会生成 Library,要等几分钟。
2. 入口场景是 Assets/MainMenu.unity(Build Settings 第 1 个),按 Play。
3. 联机需要本机开着 Steam,并保留根目录的 steam_appid.txt;只玩单机可以不联机。

## 操作
- WASD 移动,鼠标转视角
- 左键攻击/开火
- 左 Alt 锁定或解锁鼠标(解锁后可以点 UI)
- T 打开聊天,ESC 菜单
- 按键定义见 Assets/Scenes/Script/Player/PlayerControl.cs 与 ThirdPersonCamera.cs

## 代码结构
- Assets/Scenes/Script:Player、Weapons(枪/剑,Client 与 Server 分开)、Enemy、Skill、Buffs、Status、Dialogues、Level、Save、UI、Network、Game、Experience
- Assets/ScriptableObjects/Skills/SkillEffect:技能效果全部做成 ScriptableObject(伤害、治疗、持续回血、中毒、护甲、耐力……),调数值不用改代码
- Assets/Scripts/steam:Facepunch.Steamworks 和它的 Netcode 传输

## 没有上传的第三方资源
仓库只放代码、场景和设置,下面这些美术与音频包被 .gitignore 排除了(总共 1.5G),想看完整效果请从 Unity Asset Store 重新导入:

| 目录 | 内容 |
| --- | --- |
| Assets/RPG_FPS_game_assets_industrial | 场景建筑与道具 |
| Assets/unity-chan!、Assets/Scenes/FBX/unity-chan! | 角色模型与动画 |
| Assets/Zombie | 敌人 |
| Assets/Footsteps - Essentials | 脚步声 |
| Assets/Casual Game Sounds U6 | 音效 |
| Assets/Music Loops Mini Set | 背景音乐 |
| Assets/Free Pack、Assets/SazenGames、Assets/Meshint Free Toon Assets | 其他模型与材质 |
| Assets/Scenes/Font | 字体 |

缺这些资源时场景仍能打开,但模型、贴图、音效会是空的。
