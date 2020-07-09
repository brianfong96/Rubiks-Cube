#region Using Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

/// <summary>
/// Gets all cubes intersecting the plane and rotates it
/// </summary>
public class RotateInPlane : MonoBehaviour
{
    #region Private Variables
    [SerializeField] private bool isRotating = false;
    [SerializeField] private Vector3 point1;
    [SerializeField] private Vector3 point2;
    [SerializeField] private float rotationSpeed = 0.075f;
    [SerializeField] private Transform from;
    [SerializeField] private Transform to;
    #endregion

    #region Public Methods
    public void RotateCubes(Vector3 p1, Vector3 p2, Transform destination)
    {
        isRotating = true;
        point1 = p1;
        point2 = p2;
        to = destination;

        StartCoroutine(Rotate());
    }
    #endregion

    #region Coroutines
    IEnumerator Rotate()
    {
        from = this.transform;
        float progress = 0;
        while(from.rotation != to.rotation)
        {
            progress += rotationSpeed;
            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, progress);
            yield return null;
        }
        isRotating = false;
        yield return null;
    }
    #endregion

}
