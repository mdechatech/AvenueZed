using UnityEngine;
using System.Collections;

public class SetResolution : MonoBehaviour
{

    private void Awake()
    {
        Screen.SetResolution(700, 700, false);
    }
}
