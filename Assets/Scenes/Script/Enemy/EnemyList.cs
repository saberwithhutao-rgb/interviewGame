// 新建一个 EnemyList.cs，挂在场景里（比如叫 EnemyManager 的空物体上）
using System.Collections.Generic;
using UnityEngine;

public class EnemyList : MonoBehaviour
{
    // 在 Inspector 里把你场景里所有的敌人都拖进来
    public List<Enemy> sceneEnemies = new List<Enemy>();
}