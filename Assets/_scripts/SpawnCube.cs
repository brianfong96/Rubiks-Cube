#region Using Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

/// <summary>
/// Spawn a Rubik's Cube with at least 2 cubes
/// </summary>
public class SpawnCube : MonoBehaviour
{
    #region Private Variables
    [SerializeField] private int numBlocks = 3;
    [SerializeField] private float size = 1;
    [SerializeField] private GameObject subCube = null;
    [SerializeField] private GameObject rotator = null;
    [SerializeField] private List<GameObject> rubiks;
    
    #endregion 

    #region Unity Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (!subCube)
        {
            Debug.LogError("Missing subCube prefab on SpawnCube.cs. Please Attach in editor");
        }
        if (numBlocks < 2)
        {
            numBlocks = 2;
        }

        rubiks = new List<GameObject>();

        float maxRange = numBlocks * size;        
        float posOffset = maxRange / numBlocks;
        float startPos = -posOffset;
        for (int z = 0; z < numBlocks; z++)
        {
            for (int y = 0; y < numBlocks; y++)
            {
                for (int x = 0; x < numBlocks; x++)
                {
                    float xPos = startPos + x;
                    float yPos = startPos + y;
                    float zPos = startPos + z;
                    GameObject newSub = Instantiate(subCube, new Vector3(xPos, yPos, zPos), Quaternion.identity);
                    newSub.transform.parent = this.transform;
                    rubiks.Add(newSub);
                }                
            }
        }
    }
    #endregion
}
