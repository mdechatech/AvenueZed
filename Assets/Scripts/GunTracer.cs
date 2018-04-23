using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;

public class GunTracer : MonoBehaviour
{
    public LineRenderer Line;
    public ParticleSystem Particles;

    public Color Color;

    public void Initialize(Vector2 from, Vector2 to)
    {
        var yellow1 = Color;
        var yellow2 = new Color(Color.r, Color.g, Color.b, 0);
        
        Line.positionCount = 2;
        Line.SetPositions(new[] {(Vector3)from, (Vector3)to});
        Line.DOColor(new Color2(yellow1, yellow1),
            new Color2(yellow2, yellow2), 0.5f);

        transform.position = to;
        Destroy(gameObject, 1);
    }
}
