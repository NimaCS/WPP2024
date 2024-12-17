using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidpointScript : MonoBehaviour
{
    [SerializeField] private GameObject point1, point2;

    void Update()
    {
        transform.position = (point1.transform.position + point2.transform.position) / 2;
    }
}
