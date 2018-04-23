using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class Road : MonoBehaviour
{
    public LineRenderer Line;
    public TMP_Text Text;

    public Collider2D EndSignal;

    public RoadData Data;

    public Road Initialize(RoadData data)
    {
        var vec = data.End - data.Start;
        var rot = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;

        Line.SetPositions(new[]
        {
            new Vector3(data.Start.x, data.Start.y, 0),
            new Vector3(data.Start.x, data.Start.y, 0),
        });

        DOTween.To(() => data.Start, end =>
                    Line.SetPositions(new[]
                    {
                        new Vector3(data.Start.x, data.Start.y, 0),
                        new Vector3(end.x, end.y, 0),
                    }), data.End, 0.5f)
            .SetEase(Ease.InOutCubic);

        Text.text = data.Name;
        Text.transform.position = data.Start + vec / 2;
        Text.transform.eulerAngles = new Vector3(
            Text.transform.eulerAngles.x,
            Text.transform.eulerAngles.y,
            rot);

        Text.DOFade(0, 2)
            .From();

        EndSignal.transform.position = data.End;
        Data = data;

        var length = (data.End - data.Start).magnitude;
        var blockerLength = GameManager.Instance.TurretBlockerSize;
        var blockIters = Mathf.CeilToInt(length / blockerLength);
        var blockLerp = blockerLength/length;
        for (var i = 0; i < blockIters; ++i)
        {
            var blocker = Instantiate(GameManager.Instance.TurretBlockerPrefab,
                Vector2.Lerp(data.Start, data.End, i * blockLerp), Quaternion.identity);
        }

        return this;
    }
}
