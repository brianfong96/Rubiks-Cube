#region Using Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

public class RotateOnClick : MonoBehaviour
{
    #region Serialized Private Fields
    [SerializeField] private GameObject rubik;
    [SerializeField] private RubiksCube.Section sector;
    [SerializeField] private float specifier;
    [SerializeField] private bool negative;
    #endregion

    #region Public Method
    public void SetValues(RubiksCube.Section sec, float spec, bool neg)
    {
        sector = sec;
        specifier = spec;
        negative = neg;
    }

    public void Hit()
    {
        RubiksCube.Instance.RotatePieces(sector, specifier, negative);
    }
    #endregion
}
