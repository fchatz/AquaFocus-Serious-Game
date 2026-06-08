using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateObjects : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 1;

    private void Update()
    {
        transform.Rotate(0, rotateSpeed, 0, Space.World);
    }

}
