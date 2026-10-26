using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public int damage = 10;
    public float lifetime = 3f;

    [HideInInspector] public float spawnTime;
    [HideInInspector] public Vector3 startPosition;

    private void OnEnable()
    {
        spawnTime = Time.time;
        startPosition = transform.position;
    
    }
}
