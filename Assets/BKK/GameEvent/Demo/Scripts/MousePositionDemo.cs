using System.Collections;
using System.Collections.Generic;
using BKK.GameEventArchitecture;
using UnityEngine;

public class MousePositionDemo : MonoBehaviour
{
    [SerializeField] private Vector3GameEvent mouseEvent;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseEvent.Raise(Input.mousePosition);
        }
    }
}
