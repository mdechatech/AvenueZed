using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public enum TurretType
{
    Gun,
    Bomb
}

public class Turret : MonoBehaviour
{
    public SpriteRenderer RangeSprite;
    public Image FireTimerImage;

    [Space]
    public float BodyRadius;
    public float Range;
    public float FireInterval;
    public int Damage;
    public SpriteRenderer PlaceSprite;
    public TurretType TurretType;

    [Space]
    public float FireTimer;
    public bool IsActivated;

    public LayerMask ZombieLayer { get { return GameManager.Instance.ZombieLayer; } }

    protected virtual void Awake()
    {
        FireTimer = FireInterval;
    }

    public bool IsBlocked()
    {
        var hit = Physics2D.OverlapCircle(transform.position, BodyRadius, GameManager.Instance.BlockerLayer);
        return hit;
    }

    private void Update()
    {
        RangeSprite.transform.localScale = Vector3.one * Range * 2;

        if (!IsActivated)
            return;

        if ((FireTimer -= Time.deltaTime) < 0)
        {
            TryFire();
        }

        if (FireTimerImage)
            FireTimerImage.fillAmount = 1 - (FireTimer / FireInterval);
    }

    protected static readonly Collider2D[] Cache = new Collider2D[128];
    protected virtual void TryFire()
    {
        Enemy target = null;
        var dist = float.MaxValue;

        var amount = Physics2D.OverlapCircleNonAlloc(transform.position, Range, Cache, ZombieLayer);
        for (var i = 0; i < amount; ++i)
        {
            var enemy = Cache[i].GetComponentInParent<Enemy>();
            if (!enemy)
                continue;

            var thisDist = Vector2.Distance(transform.position, enemy.transform.position);
            if (thisDist < dist)
            {
                target = enemy;
                dist = thisDist;
            }
        }

        if (!target)
            return;

        FireTimer = FireInterval;
        Fire(target);
    }

    protected virtual void Fire(Enemy target)
    {
        if (TurretType == TurretType.Gun)
        {
            Instantiate(GameManager.Instance.TracerPrefab)
                .Initialize(transform.position, target.transform.position);
        }
        target.TakeDamage(Damage);
    }
}
