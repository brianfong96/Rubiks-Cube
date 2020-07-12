#region Using Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

/// <summary>
/// Spawn a Rubik's Cube with at least 2 cubes
/// </summary>
public class RubiksCube : MonoBehaviour
{
    #region Enums
    public enum Section
    {
        x,
        y,
        z
    }
    #endregion
    #region Private Serialized Variables    
    [SerializeField] private int numBlocks = 3;
    [SerializeField] private float size = 1;
    [SerializeField] private GameObject subCube = null;
    [SerializeField] private GameObject rotator = null;
    [SerializeField] private GameObject rotateButton = null;
    [SerializeField] private GameObject placeHolder = null;
    [SerializeField] private GameObject target = null;
    [SerializeField] private List<GameObject> rubiks;
    [SerializeField] private List<GameObject> toRotate;
    [SerializeField] private List<GameObject> buttons;
    
    #endregion 

    #region Public Variables
    public static RubiksCube Instance;
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
        float center = (numBlocks-1)/2*size;
        Vector3 sharedPos = new Vector3(center, center, center);
        placeHolder = Instantiate(rotator, sharedPos, Quaternion.identity);
        placeHolder.name = "PlaceHolder";
        target = Instantiate(rotator, sharedPos, Quaternion.identity);
        target.name = "Target";
        CreateCube();        
    }
    private void Update() 
    {                
        if (!placeHolder.GetComponent<RotateInPlane>().IsRotating)
        {
            foreach (GameObject cube in toRotate)
            {
                cube.transform.parent = this.transform;
            }
            placeHolder.transform.rotation = Quaternion.identity;
            target.transform.rotation = Quaternion.identity;
        }

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                DetectHit(i);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            DetectHit(0);
        }
    }
    #endregion

    #region Public Methods
    public void RotatePieces(Section selector, int specifier, bool negative = false)
    {
        if (placeHolder.GetComponent<RotateInPlane>().IsRotating)
        {            
            return;
        }

        
        toRotate = GetPieces(selector, specifier, negative);
        foreach (GameObject cube in toRotate)
        {
            cube.transform.parent = placeHolder.transform;
        }
        float newX = target.transform.eulerAngles.x;
        float newY = target.transform.eulerAngles.y;
        float newZ = target.transform.eulerAngles.z;
        float angle = (negative) ? -90 : 90;
        switch (selector)
        {            
            /*
             *      -x1 -x2 -x3 
             *  -y3             +y3  
             *  -y2             +y2   
             *  -y1             +y1    
             *      +x1 +x2 +x3      
             */
            case Section.x:
                newX += angle;
                break;
            case Section.y:
                newY += angle;
                break;
            case Section.z:
                newZ += angle;
                break;
            default:
                Debug.LogError("Section state is not defined");
                break;
        }

        target.transform.eulerAngles = new Vector3(newX,newY,newZ);        
        placeHolder.GetComponent<RotateInPlane>().RotateCubes(target.transform);
        return;
    }
    #endregion

    #region Private Methods
    void CreateCube()
    {
        rubiks = new List<GameObject>();

        float maxRange = numBlocks * size;        
        float posOffset = - maxRange / numBlocks;
        float startPos = 0;
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
                    newSub.name = xPos.ToString()+yPos.ToString()+zPos.ToString();
                    newSub.transform.parent = this.transform;
                    rubiks.Add(newSub);
                }                
            }
        }
        GameObject button;

        for (int pos = 0; pos < numBlocks; pos++)
        {
            button = Instantiate(rotateButton, new Vector3(pos, -size, 0), Quaternion.identity);
            button.transform.parent = this.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.x, pos, false);
            buttons.Add(button);

            button = Instantiate(rotateButton, new Vector3(pos, numBlocks*size, 0), Quaternion.identity);
            button.transform.parent = this.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.x, pos, true);
            buttons.Add(button);

            button = Instantiate(rotateButton, new Vector3(-size, pos, 0), Quaternion.identity);
            button.transform.parent = this.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.y, pos, false);
            buttons.Add(button);

            button = Instantiate(rotateButton, new Vector3(numBlocks*size, pos, 0), Quaternion.identity);
            button.transform.parent = this.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.y, pos, true);
            buttons.Add(button);
        }
        this.transform.localScale *= size;
        this.transform.position += new Vector3(posOffset, 0, 0);
    }

    private List<GameObject> GetPieces(Section selector, int specifier, bool negative = false)
    {
        /*
         *      -x1 -x2 -x3 
         *  -y3             +y3  
         *  -y2             +y2   
         *  -y1             +y1    
         *      +x1 +x2 +x3      
         */
        List<GameObject> pieces = new List<GameObject>();
        Vector3 comparor = Vector3.one; 
        Vector3 specifiedComparor = Vector3.zero;       
        switch (selector)
        {            
            case Section.x:                
                comparor = Vector3.right;
                specifiedComparor = new Vector3(specifier, 0, 0);
                break;
            case Section.y:
                comparor = Vector3.up;
                specifiedComparor = new Vector3(0, specifier, 0);
                break;
            case Section.z:
                comparor = Vector3.forward;
                specifiedComparor = new Vector3(0, 0, specifier);
                break;
            default:
                Debug.LogError("Section state is not defined");
                break;
        }        
        foreach (GameObject cube in rubiks)
        {
            if (Vector3.Scale(cube.transform.localPosition, comparor) == specifiedComparor)
            {
                pieces.Add(cube);
            }            
        }
        return pieces;
    }    

    private void DetectHit(int i)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
        if (Physics.Raycast(ray, out hit) && hit.collider)
        {
            hit.collider.gameObject.GetComponent<RotateOnClick>().Hit();
        }
    }
    #endregion
}
