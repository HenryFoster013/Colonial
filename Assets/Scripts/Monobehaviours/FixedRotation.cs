using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    [SerializeField] Transform Pivot;
    [SerializeField] Quaternion _FixedRotation;

    // Update is called once per frame
    void Update()
    {
        Transform parent = Pivot.transform.parent;
        Pivot.transform.parent = null;
        Pivot.rotation = _FixedRotation;
        Pivot.transform.parent = parent;
    }
}
