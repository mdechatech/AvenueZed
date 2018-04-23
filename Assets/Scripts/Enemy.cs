using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public float Speed;
    public float MaxHealth;

    public Collider2D Body;
    
    [Space]
    public float Health;
    public PathData Data;

    public int LastIndex;
    public int CurIndex;
    public Vector2 Start;
    public Vector2 End;
    public float Length;
    public float CurLength;

    private float _perlin;

    public void Initialize(PathData data, float speed)
    {
        Data = data;
        transform.position = Data.Points[0];
        Health = MaxHealth - 0.01f;
        Target(1);
        _perlin = Random.Range(-1000, 1000);
        //Speed = speed;
        Speed = Random.Range(speed * 0.5f, speed * 1.5f);

        Body.OnTriggerEnter2DAsObservable().Subscribe(Body_OnTriggerEnter);
        Body.OnTriggerExit2DAsObservable().Subscribe(Body_OnTriggerExit);
    }

    private List<GameObject> _slowers = new List<GameObject>();
    private void Body_OnTriggerEnter(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Slow"))
            _slowers.Add(collider2D.gameObject);
    }

    private void Body_OnTriggerExit(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Slow"))
            _slowers.Remove(collider2D.gameObject);
    }

    public void Target(int pathIndex)
    {
        LastIndex = pathIndex - 1;
        CurIndex = pathIndex;
        Start = Data.Points[LastIndex];
        End = Data.Points[pathIndex];
        Length = (End - Start).magnitude;
        CurLength = 0;
    }

    public void TakeDamage(int amount)
    {
        if ((Health -= amount) <= 0)
            Die();
    }

    public void Die()
    {
        GameManager.Instance.NotifyEnemyDied();
        Destroy(gameObject);
    }

    private void Update()
    {
        var slowFactor = _slowers.Count > 0 ? 0.5f : 1;
        CurLength += Speed * Time.deltaTime * slowFactor;
        if (CurLength > Length)
        {
            if (CurIndex == Data.Points.Length - 1)
            {
                GameManager.Instance.NotifyEnemyPassed();
                Destroy(gameObject);
            }
            else
            {
                Target(CurIndex + 1);
            }
        }

        transform.position = Vector2.Lerp(Start, End, CurLength / Length)
                             + new Vector2(Mathf.PerlinNoise(0, _perlin += Time.deltaTime) - 0.5f,
                                 Mathf.PerlinNoise(_perlin, 0) - 0.5f) * 0.1f;
    }
}
