using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BombTurret : Turret
{
    public float SplashRange;

    protected override void Fire(Enemy target)
    {
        var amount = Physics2D.OverlapCircleNonAlloc(target.transform.position, SplashRange, Cache, ZombieLayer);

        var enemies = new List<Enemy>();
        for (var i = 0; i < amount; ++i)
        {
            var enemy = Cache[i].GetComponentInParent<Enemy>();
            if (enemy)
                enemies.Add(enemy);
        }

        foreach (var e in enemies)
            e.TakeDamage(1);

        Instantiate(GameManager.Instance.BombTracerPrefab)
            .Initialize(transform.position, target.transform.position);
    }
}
