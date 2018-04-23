using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;

public class Path : MonoBehaviour
{
    public TMP_Text Text;
    public PathData Data;
    public GameObject End;
    public LineRenderer Line;

    public EnemySpawner Spawner;

    public void Initialize(PathData pathData, WaveData waveData)
    {
        Data = pathData;

        Text.text = "AVENUE ZED";

        var roadIndex = Random.Range(1, pathData.Points.Length - 2);
        var roadStart = pathData.Points[roadIndex];
        var roadEnd = pathData.Points[roadIndex + 1];

        DOTween.To(() => Line.widthMultiplier, value => Line.widthMultiplier = value,
                0, 1).From()
            .SetEase(Ease.InOutCubic);

        var vec = roadEnd - roadStart;
        var rot = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
        if (rot < -90 || rot > 90)
            rot += 180;

        Text.transform.position = roadStart + vec / 2;
        Text.transform.eulerAngles = new Vector3(
            Text.transform.eulerAngles.x,
            Text.transform.eulerAngles.y,
            rot);

        for (var i = 1; i < pathData.Points.Length; ++i)
        {
            CreateBlockers(pathData.Points[i - 1], pathData.Points[i]);
        }

        Spawner.Initialize(pathData, waveData);
        Line.positionCount = pathData.Points.Length;
        Line.SetPositions(Data.Points.Select(p => (Vector3)p).ToArray());
        End.transform.position = Data.Points[Data.Points.Length - 1];
    }

    private void CreateBlockers(Vector2 start, Vector2 end)
    {
        var length = (end - start).magnitude;
        var blockerLength = GameManager.Instance.TurretBlockerSize;
        var blockIters = Mathf.CeilToInt(length / blockerLength);
        var blockLerp = blockerLength / length;
        for (var i = 0; i < blockIters; ++i)
        {
            var blocker = Instantiate(GameManager.Instance.TurretBlockerPrefab,
                Vector2.Lerp(start, end, i * blockLerp), Quaternion.identity);
        }
    }
}
