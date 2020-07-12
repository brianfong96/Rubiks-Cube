#region Using Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

public class Commands
{
    #region Private Variables
    string command;
    RubiksCube.Section section;
    float specifier;
    bool negative;
    #endregion

    #region Public Variables
    public string Command  
    {
        get { return command;}
    }
    public RubiksCube.Section Section  
    {
        get { return section;}
    }
    public float Specifier
    {
        get { return specifier;}
    }
    public bool Negative
    {
        get { return negative;}
    }
    #endregion

    #region Public Methods
    public Commands(string c, RubiksCube.Section sec, float spec, bool neg)
    {
        command = c;
        section = sec;
        specifier = spec;
        negative = neg;
    }
    #endregion
}