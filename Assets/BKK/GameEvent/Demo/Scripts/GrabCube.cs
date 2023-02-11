using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCube : MonoBehaviour
{
    public float heightOffset = 0.5f;

    private Vector3 receivedPos;
    
    public void DragCube(Vector3 screenPos)
    {
        receivedPos = screenPos;
    }

    public void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(receivedPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.name.Contains(gameObject.name))
            {
                transform.position = new Vector3(hit.point.x, heightOffset, hit.point.z);
            }
        }
    }
}
