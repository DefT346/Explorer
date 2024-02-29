using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster
{
    private Camera _camera;
    private string _filterTag;

    private bool hitTarget;

    public Raycaster(Camera camera, string filterTag)
    {
        _camera = camera;
        _filterTag = filterTag;
    }

    public Action onRayEnter;
    public Action onRayExit;

    public void Update()
    {
        int layerMask = 1 << 8;

        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            var isCar = hit.transform.tag == "Car";

        }

    }
}
