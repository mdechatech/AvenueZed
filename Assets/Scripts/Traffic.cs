using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

public class TrafficData
{
    public RoadData[] Rows;
    public RoadData[] Columns;

    public CrossingData[] Crossings;

    public PathData Path;
    public WaveData WaveData;
}

public class PathData
{
    public Vector2[,] PointGrid;
    public Vector2[] Points;
}

public class CrossingData
{
    public RoadData Column;
    public RoadData Row;
    public Vector2 Position;
}

public class RoadData
{
    public int Index;
    public Vector2 Start;
    public Vector2 End;
    public string Name;

    public bool IsVertical;
    public float Angle { get { return Mathf.Atan2((End - Start).y, (End - Start).x) * Mathf.Rad2Deg; } }

    public RoadData(int index, Vector2 start, Vector2 end, bool isVertical)
    {
        Index = index;
        Start = start;
        End = end;
        Name = Streets.Random;
        IsVertical = isVertical;
    }
}

public class WaveData
{
    public List<Entry> Waves;

    public class Entry
    {
        public float Amount;
        public float Delay;
        public float Speed;

        [FormerlySerializedAs("Density")]
        public float Frequency;
    }
}

public class Traffic : MonoBehaviour {
    
    public static TrafficData Generate(LevelData data)
    {
        // min 1x1, max 2x2
        var rect = FindObjectOfType<Bounds>().Rect;

        var cols = data.Cols;
        var rows = data.Rows;

        var colParts = (cols + 1) * 4;
        var rowParts = (rows + 1) * 4;

        var colSegments = new RoadData[cols];
        var rowSegments = new RoadData[rows];

        #region Roads + Crossings
        for (var i = 0; i < cols; ++i)
        {
            var startSkew = Random.Range(-1, 2) / (float) colParts;
            var endSkew = Random.Range(-1, 2) / (float) colParts;

            var startLerp = (i + 1) / (float)(cols + 1) + startSkew;
            //var endLerp = startLerp - startSkew + endSkew;
            var endLerp = startLerp - (2 * startSkew);

            colSegments[i] = new RoadData(i,
                new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, startLerp), rect.yMax),
                new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, endLerp), rect.yMin),
                true);
        }

        for (var i = 0; i < rows; ++i)
        {
            var startSkew = Random.Range(-1, 2) / (float) rowParts;
            var endSkew = Random.Range(-1, 2) / (float)rowParts;

            var startLerp = (i + 1) / (float) (rows + 1) + startSkew;
            //var endLerp = startLerp - startSkew + endSkew;
            var endLerp = startLerp - (2 * startSkew);

            rowSegments[i] = new RoadData(i, 
                new Vector2(rect.xMin, Mathf.Lerp(rect.yMax, rect.yMin, startLerp)),
                new Vector2(rect.xMax, Mathf.Lerp(rect.yMax, rect.yMin, endLerp)),
                false);
        }

        var crossings = new CrossingData[cols * rows];
        var crossingIndex = 0;
        for (var i = 0; i < rows; ++i)
        {
            for (var j = 0; j < cols; ++j)
            {
                var col = colSegments[j];
                var row = rowSegments[i];

                crossings[crossingIndex++] = new CrossingData
                {
                    Column = col,
                    Row = row,
                    Position = Intersect(col, row)
                };
            }
        }

        #endregion

        var pathGrid = new Vector2[2*cols + 3, 2*rows + 3];
        #region Path Grid

        // corners
        pathGrid[0, 0] = new Vector2(Bounds.Rect.xMin, Bounds.Rect.yMax);
        pathGrid[pathGrid.GetLength(0) - 1, 0] = new Vector2(Bounds.Rect.xMax, Bounds.Rect.yMax);
        pathGrid[0, pathGrid.GetLength(1) - 1] = new Vector2(Bounds.Rect.xMin, Bounds.Rect.yMin);
        pathGrid[pathGrid.GetLength(0) - 1, pathGrid.GetLength(1) - 1] = new Vector2(Bounds.Rect.xMax, Bounds.Rect.yMin);

        // road start and end
        for (var i = 0; i < pathGrid.GetLength(1); i += 2)
        {
            for (var j = 0; j < pathGrid.GetLength(0); j += 2)
            {
                // filter corners
                if ((i == 0 || i == pathGrid.GetLength(1) - 1) &&
                    (j == 0 || j == pathGrid.GetLength(0) - 1))
                    continue;

                // filter crossings
                if (i > 0 && i < pathGrid.GetLength(1) - 1 &&
                    j > 0 && j < pathGrid.GetLength(0) - 1)
                    continue;

                if (i == 0)
                    pathGrid[j, i] = colSegments[(j - 2) / 2].Start;
                else if (i == pathGrid.GetLength(1) - 1)
                    pathGrid[j, i] = colSegments[(j - 2) / 2].End;
                else if (j == 0)
                    pathGrid[j, i] = rowSegments[(i - 2) / 2].Start;
                else if (j == pathGrid.GetLength(0) - 1)
                    pathGrid[j, i] = rowSegments[(i - 2) / 2].End;
            }
        }

        // crossings
        crossingIndex = 0;
        for (var i = 2; i < pathGrid.GetLength(1) - 2; i += 2)
        {
            for (var j = 2; j < pathGrid.GetLength(0) - 2; j += 2)
            {
                pathGrid[j, i] = crossings[crossingIndex++].Position;
            }
        }
        
        // vertical inbetweens
        for (var i = 1; i < pathGrid.GetLength(1); i += 2)
        {
            for (var j = 0; j < pathGrid.GetLength(0); j += 2)
            {
                pathGrid[j, i] = (pathGrid[j, i - 1] +
                                  pathGrid[j, i + 1]) / 2;
            }
        }
        
        // horizontal inbetweens
        for (var i = 0; i < pathGrid.GetLength(1); i += 2)
        {
            for (var j = 1; j < pathGrid.GetLength(0); j += 2)
            {
                pathGrid[j, i] = (pathGrid[j - 1, i] +
                                  pathGrid[j + 1, i]) / 2;
            }
        }
        
        // diagonal inbetweens
        for (var i = 1; i < pathGrid.GetLength(1); i += 2)
        {
            for (var j = 1; j < pathGrid.GetLength(0); j += 2)
            {
                pathGrid[j, i] = (pathGrid[j - 1, i] +
                                  pathGrid[j + 1, i] +
                                  pathGrid[j, i - 1] +
                                  pathGrid[j, i + 1]) / 4;
            }
        }
        #endregion

        #region Path

        //var turns = 1;
        var turns = Mathf.Min(rows + 1, data.Turns);
        var downs = 2 + rows;
        var path = new Vector2[3 + rows + turns];

        int pathCol;
        var pathRow = 0;
        do
        {
            pathCol = Random.Range(0, pathGrid.GetLength(0));
        } while (pathCol % 2 == 0 && pathCol > 0 && pathCol < pathGrid.GetLength(0));
        path[0] = pathGrid[pathCol, pathRow++];
        pathCol = Mathf.Clamp(pathCol, 1, pathGrid.GetLength(0) - 1);
        path[1] = pathGrid[pathCol, pathRow];
        --downs;

        var turned = false;

        for (var i = 2; i < path.Length - 1; ++i)
        {
            if (!turned && Random.value < turns / Mathf.Max(1f, (float) downs))
                goto turn;
            else
                goto down;
                
            turn:
            var dir = (pathGrid.GetLength(0)/2 - pathCol) > 0 ? 2 : -2;
            pathCol += dir;
            pathCol = Mathf.Clamp(pathCol, 1, pathGrid.GetLength(0) - 1);
            --turns;
            turned = true;
            goto next;

            down:
            pathRow += 2;
            --downs;
            turned = false;
            goto next;

            next:
            path[i] = pathGrid[pathCol, pathRow];
        }

        ++pathRow;
        path[path.Length - 1] = pathGrid[pathCol, pathRow];

        #endregion
        #region Waves

        var waves = new List<WaveData.Entry>();
        for (var i = 0; i < 100; ++i)
        {
            var wave = new WaveData.Entry();
            wave.Amount = data.WaveAmtStart + (data.WaveAmtGrowth * i);
            wave.Delay = data.WaveDelay;
            wave.Frequency = data.WaveFreqStart + (data.WaveFreqGrowth * i);
            wave.Speed = data.WaveSpeedStart + (data.WaveSpeedGrowth * i);
            waves.Add(wave);
        }

        #endregion

        return new TrafficData
        {
            Columns = colSegments,
            Rows = rowSegments,
            Crossings = crossings,
            Path = new PathData
            {
                PointGrid = pathGrid,
                Points = path,
            },

            WaveData = new WaveData
            {
                Waves = waves
            }
        };
    }

    public static Bounds Bounds { get { return GameManager.Instance.Bounds; } }

    public TrafficData Data;


    public void Initialize(TrafficData data)
    {
        foreach (var r in FindObjectsOfType<Road>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<Car>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<CarSpawner>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<Crossing>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<Path>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<Enemy>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<Turret>())
            Destroy(r.gameObject);

        foreach (var r in FindObjectsOfType<BoxCollider2D>()
            .Where(c => c.tag == "Blocker"))
            Destroy(r.gameObject);

        Data = data;
        Draw();
    }

    private void Start()
    {

            
    }

    private void Update()
    {

    }

    private static Vector2 Intersect(RoadData col, RoadData row)
    {
        return Intersect(col.Start, col.End, row.Start, row.End);
    }

    private static Vector2 Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var a1 = b.y - a.y;
        var b1 = a.x - b.x;
        var c1 = a1 * a.x + b1 * a.y;

        var a2 = d.y - c.y;
        var b2 = c.x - d.x;
        var c2 = a2 * c.x + b2 * c.y;

        var det = a1 * b2 - a2 * b1;
        return new Vector2((b2 * c1 - b1 * c2) / det, (a1 * c2 - a2 * c1) / det);
    }
    /*
    private static float Area(RoadData columnLeft, RoadData rowLeft, RoadData columRight, RoadData rowRight)
    {

    }
    */
    private static float Area(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return 0.5f * Mathf.Abs(a.x * b.y + b.x * c.y + c.x * d.y + d.x * a.y - b.x * a.y - c.x * b.y - d.x * c.y - a.x * d.y);
    }

    public Road RoadPrefab { get { return GameManager.Instance.RoadPrefab; } }
    public Crossing CrossingPrefab { get { return GameManager.Instance.CrossingPrefab; } }
    public CarSpawner CarSpawnerPrefab { get { return GameManager.Instance.CarSpawnerPrefab; } }
    public Path PathPrefab { get { return GameManager.Instance.PathPrefab; } }

    private void Draw()
    {
        foreach (var column in Data.Columns)
        {
            var road = Instantiate(RoadPrefab).Initialize(column);
            Instantiate(CarSpawnerPrefab).Initialize(road);
        }

        foreach (var row in Data.Rows)
        {
            var road = Instantiate(RoadPrefab).Initialize(row);
            Instantiate(CarSpawnerPrefab).Initialize(road);
        }

        foreach (var crossing in Data.Crossings)
        {
            Instantiate(CrossingPrefab).Initialize(crossing);
        }

        Instantiate(PathPrefab).Initialize(Data.Path, Data.WaveData);
    }
    

    private void Draw(Vector2 pos)
    {
        //pos += (Vector2) (Random.onUnitSphere * 0.05f);
        Debug.DrawLine(pos + Vector2.up * 0.1f, pos + Vector2.down * 0.1f);
        Debug.DrawLine(pos + Vector2.right * 0.1f, pos + Vector2.left * 0.1f);
    }

    private void Draw(RoadData road)
    {
        Debug.DrawLine(road.Start, road.End);
    }
}
