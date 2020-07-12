using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAngle : MonoBehaviour
{
    #region Private Variables
    [SerializeField] private bool debug = false;
    [SerializeField] private float tolerance = 0.01f;
    [SerializeField] private float progress = 0f;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 lastPosition;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Quaternion lastRotation;
    #endregion

    #region Unity Callbacks
    private void Start() {
        targetPosition = this.transform.position;
        targetRotation = this.transform.rotation;
    }
    private void Update() {
        if (!debug)
        {
            if (Vector3.Distance(this.transform.position, targetPosition) > tolerance)
            {
                progress += Time.deltaTime;
                this.transform.position = Vector3.Lerp(lastPosition, targetPosition, progress);
            }
            if (this.transform.rotation != targetRotation)
            {
                this.transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, progress);
            }
        }
        
    }
    #endregion

    #region Public Methods
    public void Change()
    {
        if (Vector3.Distance(this.transform.position, targetPosition) <= tolerance)
        {
            progress = 0;
            targetPosition = lastPosition;
            lastPosition = this.transform.position;   
            targetRotation = lastRotation;
            lastRotation = this.transform.rotation;         
        }        

    }
    #endregion
    
}
