using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Levels.asset", menuName = "Levels")]
public class Levels : ScriptableObject
{
    public LevelData[] Data;
}
