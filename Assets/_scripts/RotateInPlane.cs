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
    [SerializeField] private float rotationSpeed = 0.05f;
    [SerializeField] private Transform from;
    [SerializeField] private Transform to;
    #endregion
    
    #region Public Variable
    public bool IsRotating 
    {
        get {return isRotating;}
    }
    #endregion

    #region Public Methods
    public void RotateCubes(Transform dest)
    {
        to = dest;
        isRotating = true;
        StartCoroutine(Rotate());
        return;
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
