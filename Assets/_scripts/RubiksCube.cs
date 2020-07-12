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
    [SerializeField] private bool automationOn = false;  
    [SerializeField] private bool solverOn = false;
    [SerializeField] private int numBlocks = 3;
    [SerializeField] private int minIterations = 20;
    [SerializeField] private int maxIterations = 50;
    [SerializeField] private int iterations;
    [SerializeField] private float size = 1;
    [SerializeField] private float posOffset;
    [SerializeField] private float center;
    [SerializeField] private float rotateSpeed = 0.075f;
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private GameObject subCube = null;
    [SerializeField] private GameObject rotator = null;
    [SerializeField] private GameObject rotateButton = null;
    [SerializeField] private GameObject placeHolder = null;
    [SerializeField] private GameObject target = null;
    [SerializeField] private GameObject temp = null;
    [SerializeField] private GameObject completeTarget = null;
    [SerializeField] private GameObject buttonParent = null;
    [SerializeField] private List<GameObject> rubiks;
    [SerializeField] private List<GameObject> toRotate;
    [SerializeField] private List<GameObject> buttons;    
    [SerializeField] private Stack<Commands> commands;

    #endregion 

    #region Public Variables
    public static RubiksCube Instance;
    #endregion

    #region Private Variables
    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;
    #endregion

    #region Unity Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (!Instance)
        {
            Instance = this;
        }
        if (!subCube)
        {
            Debug.LogError("Missing subCube prefab on SpawnCube.cs. Please Attach in editor");
        }
        if (numBlocks < 2)
        {
            numBlocks = 2;
        }
        commands = new Stack<Commands>();
        center = (numBlocks-1)/2*size;
        Vector3 sharedPos = new Vector3(0, center, center);
        placeHolder = Instantiate(rotator, sharedPos, Quaternion.identity);
        placeHolder.name = "PlaceHolder";
        target = Instantiate(rotator, sharedPos, Quaternion.identity);
        target.name = "Target";
        completeTarget = Instantiate(rotator, sharedPos, Quaternion.identity);
        completeTarget.name = "CompleteTarget";
        temp = Instantiate(rotator, sharedPos, Quaternion.identity);
        temp.name = "Temp";
        CreateCube();    
        CreateButtons();    
    }
    private void Update() 
    {      
        if (!Busy())
        {            
            ResetPlaceHolder();
        }

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                DetectHit(Input.GetTouch(i).position);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            DetectHit(Input.mousePosition);
        }

        MouseSwipe();
        TouchSwipe();
    }
    #endregion

    #region Public Methods
    public void Randomize()
    {
        if (Busy() || automationOn || solverOn)
        {
            return;
        }
        automationOn = true;
        iterations = Random.Range(minIterations, maxIterations);
        StartCoroutine(RandomCommands());
    }

    public void Solve()
    {
        if (Busy() || automationOn || solverOn)
        {
            return;
        }
        solverOn = true;
        StartCoroutine(SolveCommands());
    }
    public void RotatePieces(Section selector, float specifier, bool negative = false)
    {
        if (Busy())
        {            
            return;
        }
        toRotate = GetPieces(selector, specifier, negative);
        foreach (GameObject cube in toRotate)
        {
            cube.transform.parent = placeHolder.transform;
        }
        SetTarget(target, selector, negative);
        placeHolder.GetComponent<RotateInPlane>().RotateCubes(target.transform, rotateSpeed);
        if (!solverOn)
        {
            commands.Push(new Commands("RotatePieces", selector, specifier, !negative));
        }        
        return;
    }
    #endregion

    #region Private Methods
    void CreateCube()
    {
        rubiks = new List<GameObject>();

        float maxRange = numBlocks * size;                
        float startPos = 0;

        posOffset = - maxRange / numBlocks;
        this.transform.localScale = Vector3.one;
        this.transform.position = new Vector3(center, center, center);
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
        
        this.transform.localScale *= size;
        this.transform.position += new Vector3(posOffset, 0, 0);        
    }

    bool Busy()
    {
        return (placeHolder.GetComponent<RotateInPlane>().IsRotating || this.GetComponent<RotateInPlane>().IsRotating);
    }

    private void CreateButtons()
    {
        GameObject button;
        buttons = new List<GameObject>();
        buttonParent = new GameObject("Button Parent");
        buttonParent.transform.position = this.transform.position;
        for (int pos = 0; pos < numBlocks; pos++)
        {
            // Rotate Down
            button = Instantiate(rotateButton, new Vector3(pos, -size, 0), Quaternion.identity);
            button.transform.parent = buttonParent.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.x, pos+posOffset, true);
            buttons.Add(button);

            // Rotate Up
            button = Instantiate(rotateButton, new Vector3(pos, numBlocks*size, 0), Quaternion.identity);
            button.transform.parent = buttonParent.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.x, pos+posOffset, false);
            buttons.Add(button);

            // Rotate Left
            button = Instantiate(rotateButton, new Vector3(-size, pos, 0), Quaternion.identity);
            button.transform.parent = buttonParent.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.y, pos, false);
            buttons.Add(button);

            // Rotate Right
            button = Instantiate(rotateButton, new Vector3(numBlocks*size, pos, 0), Quaternion.identity);
            button.transform.parent = buttonParent.transform;
            button.GetComponent<RotateOnClick>().SetValues(Section.y, pos, true);
            buttons.Add(button);
        }
        buttonParent.transform.position += new Vector3(posOffset, 0, 0);        
    }

    private void SetTarget(GameObject t, Section selector, bool negative)
    {
        // I hate gimbal locks
        Vector3 axis = Vector3.right;
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
                axis = (negative) ? Vector3.left : Vector3.right;
                break;
            case Section.y:
                axis = (negative) ? Vector3.down : Vector3.up;
                break;
            case Section.z:
                axis = (negative) ? Vector3.back : Vector3.forward;
                break;
            default:
                Debug.LogError("Section state is not defined");
                break;
        }
        t.transform.rotation = Quaternion.AngleAxis(90, axis);
        return;
    }

    private void RotateAll(Section selector, bool negative)
    {
        if (placeHolder.GetComponent<RotateInPlane>().IsRotating || this.GetComponent<RotateInPlane>().IsRotating)
        {            
            return;
        }
        SetTarget(completeTarget, selector, negative);
        this.GetComponent<RotateInPlane>().RotateCubes(completeTarget.transform, rotateSpeed);
        if (!solverOn)
        {
            commands.Push(new Commands("RotateAll", selector, -1f, !negative));
        }        
    }

    private void ResetPlaceHolder()
    {
        foreach (GameObject cube in rubiks)
        {
            cube.transform.parent = temp.transform;
        }
        
        placeHolder.transform.rotation = Quaternion.identity;
        target.transform.rotation = Quaternion.identity;
        completeTarget.transform.rotation = Quaternion.identity;
        this.transform.rotation = Quaternion.identity;

        foreach (GameObject cube in rubiks)
        {
            cube.transform.parent = this.transform;
        }

        return;
    }

    private List<GameObject> GetPieces(Section selector, float specifier, bool negative = false)
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
            if (Vector3.Distance(Vector3.Scale(cube.transform.position, comparor), specifiedComparor) < 0.1F)
            {
                pieces.Add(cube);
            }            
        }
        return pieces;
    }    

    private void DetectHit(Vector3 rayPos)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(rayPos);
        if (Physics.Raycast(ray, out hit) && hit.collider && hit.collider.tag == "Button" && !automationOn)
        {
            Debug.Log(hit.collider.gameObject.name);
            hit.collider.gameObject.GetComponent<RotateOnClick>().Hit();
        }
    }

    private void Swipe()
    {
        currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
        
        //normalize the 2d vector
        currentSwipe.Normalize();

        //swipe upwards
        if(currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f && !automationOn)
        {            
            RotateAll(Section.x, false);
        }
        //swipe down
        if(currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f && !automationOn)
        {
            RotateAll(Section.x, true);
        }
        //swipe left
        if(currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && !automationOn)
        {
            RotateAll(Section.y, false);
        }
        //swipe right
        if(currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && !automationOn)
        {
            RotateAll(Section.y, true);
        }
    }
    
    private void TouchSwipe()
    {
        if(Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Began)
            {
                //save began touch 2d point
                firstPressPos = new Vector2(t.position.x,t.position.y);
            }
            if(t.phase == TouchPhase.Ended)
            {
                //save ended touch 2d point
                secondPressPos = new Vector2(t.position.x,t.position.y);
                            
                //create vector from the two points
                Swipe();
            }
        }
    }

    public void MouseSwipe()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //save began touch 2d point
            firstPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
        }
        if(Input.GetMouseButtonUp(0))
        {
                //save ended touch 2d point
            secondPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
        
            Swipe();
        }
    }
    #endregion

    #region Coroutines
    IEnumerator RandomCommands()
    {        
        for (int i = 0; i < iterations; i++)
        {
            while (Busy())
            {            
                yield return null;
            }
            Section newSection = (Section)Random.Range(0,3);
            int specifier = Random.Range(0,numBlocks+1);
            float spec = 0.0f + specifier;
            int neg = Random.Range(0,2);
            bool negative = (neg == 0);
            if (specifier == numBlocks)
            {
                RotateAll(newSection, negative);
            }
            else
            {
                RotatePieces(newSection, spec, negative);
            }
            // Needs buffer time or it'll break
            yield return new WaitForSeconds(waitTime);
        }
        automationOn = false;
        yield return null;
    } 

    IEnumerator SolveCommands()
    {
        while (commands.Count > 0)
        {
            Commands command = commands.Pop();
            if (command.Command == "RotatePieces")
            {
                RotatePieces(command.Section, command.Specifier, command.Negative);
            }
            else
            {
                RotateAll(command.Section, command.Negative);
            }
            // Needs buffer time or it'll break
            yield return new WaitForSeconds(waitTime);            
        }
        yield return null;
        solverOn = false;
    }
    #endregion
}
