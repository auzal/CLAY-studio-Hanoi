using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public bool debugConsole = false; // controls debug messages are logged to the console
    
    // references to the rods
    public GameObject leftRod;
    public GameObject centerRod;
    public GameObject rightRod;

    // all the audio clips
    public AudioSource moveSound;
    public AudioSource cancelSound;
    public AudioSource errorSound;
    public AudioSource finishedSound;
    public AudioSource clickSound;
    public AudioSource swooshSwound;

    // text elements that need to be controlled
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI finishedText;
    public TextMeshProUGUI instructionsText;

    public float discFlyTime = 0.3f; // in seconds, the duration of the disc movement animation

    private int moveCount = 0; // how many moves the player has made

    private string state = "waiting for disc click"; // state machine variable

    private GameObject[] allDiscs; // reference to all the discs
    private int numberOfDiscs = 5; 

    // these lists are dynamic and get used to figure out the current state of the game
    private List<GameObject> leftDiscs;
    private List<GameObject> centerDiscs;
    private List<GameObject> rightDiscs;

    // references to the disc and rod that have been selected for the next movement
    private GameObject selectedDisc;
    private GameObject selectedRod;

    private float diskYScale = 0.2f; // height of each disc

    // Start is called before the first frame update
    void Start()
    {
        if (debugConsole)
        {
            Debug.Log("Left rod X position: " + leftRod.transform.position.x);
            Debug.Log("Center rod X position: " + centerRod.transform.position.x);
            Debug.Log("Right rod X position: " + rightRod.transform.position.x);
        }

        // get all the discs, calculate how many there are
        allDiscs = GameObject.FindGameObjectsWithTag("Disc");
        numberOfDiscs = allDiscs.Length;
        if (debugConsole)
        {
            Debug.Log("Found " + numberOfDiscs + " discs");
        }
        determineDiscPositions();  // populate the disc lists based on their locations
        setDiscStates(); // sets only the top disc on each list as selectable
        makeRodsUnselectable();  // make sure the rods can't be selected
        changeState("waiting for disc click"); // go into first state just in case
        setMoveCounter(moveCount); // populate move counter
        finishedText.enabled = false; //  turn off the "Finished" UI element
        instructionsText.enabled = true; // turn on the "Instructions" UI element
    }

    // Update is called once per frame
    void Update() // update runs the state machine
    {
        if (state == "waiting for disc click") // the start of a move -> waiting for disc to be clicked
        {
            if (checkForClickedDisc()) // check all the discs, if one of them was clicked, move to next next state
            {
                clickSound.Play();
                makeDiscsUnselectable();
                makeRodsSelectable();
                changeState("waiting for rod click");
            }
        }
        else if (state == "waiting for rod click") // waiting for a rod to be clicked
        {
            if (checkForClickedRod()) // check all the rods, is one of them is clicked, attemp to perform the move to next state
            {
                changeState("attempting to move");
            }
        }
        else if (state == "attempting to move")  // checking if the desired move is valaid
        {
            makeRodsUnselectable();
            bool moveSuccessful = attemptMove();
            deselectAll();

            if (moveSuccessful) // if the move was successful
            {
                if (instructionsText.enabled) // turn off the instructions if they were on
                {
                    instructionsText.enabled = false;
                } 
                changeState("waiting for animation"); // move to next state
            }
            else
            {
                determineDiscPositions();
                setDiscStates();
                changeState("waiting for disc click"); //  go back to start, wait start of new move
            }
        }
        else if (state == "waiting for animation") // waiting for a disc to fly to its detination
        { 
            if (!anyDiscsAnimating()) // if none of the discs are animating
            {
                deselectAll();
                determineDiscPositions();
                setDiscStates(); 
                moveCount++; // register the move
                setMoveCounter(moveCount);
                changeState("checking victory"); // move to next state
            }
            
        }
        else if (state == "checking victory") // if a move was successful, check if the game is finished
        {
            if (rightDiscs.Count == allDiscs.Length) //  if the list that has the discs that are on the righmost rod has all the discs..
            {
                makeDiscsUnselectable();
                finishedSound.Play();
                finishedText.enabled = true;
                changeState("victory"); // the game is over, move to next state
            }
            else // otherwise, the game continues
            {
                moveSound.Play();
                changeState("waiting for disc click"); //  go back to start, wait start of new move
            }
        }
    }

    private bool attemptMove() // check if the desired move is valid
    {
        bool result = false; // assume the move is invalid
        if (selectedDisc.transform.position.x == selectedRod.transform.position.x) // if the selected rod is the same as the current rod for that disc
        {
            cancelSound.Play(); // play the "cancel move" sound
            if (debugConsole)
            {
                Debug.Log("Disc is already on that rod"); 
            }
        }
        else // otherwise, keep checking
        {
            if (checkIfValidMove()) // if the move is valid
            {
                // calculate the new y position for the disc based on which rod was selected and how many discs it currently has
                RodBehaviour rodScript = selectedRod.GetComponent<RodBehaviour>();
                string rodId = rodScript.id;
                float newY = 0.1f;

                if (rodId == "left")
                {
                    newY += leftDiscs.Count * diskYScale;
                }
                else if (rodId == "center")
                {
                    newY += centerDiscs.Count * diskYScale;
                }
                else if (rodId == "right")
                {
                    newY += rightDiscs.Count * diskYScale;
                }

                // start the movement animation, only play the "swoosh" sound if the animation time is long enough
                DiscBehaviour discScript = selectedDisc.GetComponent<DiscBehaviour>();
                discScript.flyTo(new Vector3(selectedRod.transform.position.x, newY, 0), discFlyTime);
                if(discFlyTime >= 0.1)
                {
                    swooshSwound.Play();
                }

                if (debugConsole)
                {
                    Debug.Log("Move successful");
                }
                result = true; // set the function to return a true value
            }
            else { // if the move isn't valid
                errorSound.Play(); // play error sound
                if (debugConsole)
                {
                    Debug.Log("Invalid move");
                }
            }
        }
        return result;
    }

    bool checkIfValidMove() // checks if the move is in fact valid
    {
        bool result = true; // assume it is a valid move

        // find the size for the selected disc and which rod it wants to go to
        DiscBehaviour discScript = selectedDisc.GetComponent<DiscBehaviour>();
        int selectedDiscSize = discScript.size;
       
        RodBehaviour rodScript = selectedRod.GetComponent<RodBehaviour>();
        string rodId = rodScript.id;

        // check if the selected disc is smaller than all the discs in the target rod
        if (rodId == "left")
        {
            result = checkIfDiscIsSmaller(leftDiscs, selectedDiscSize);
        }
        else if (rodId == "center")
        {
            result = checkIfDiscIsSmaller(centerDiscs, selectedDiscSize);
        }
        else if (rodId == "right")
        {
            result = checkIfDiscIsSmaller(rightDiscs, selectedDiscSize);
        }

        return result; // if the disc is larger than the discs below it, return false
    }

    private bool checkIfDiscIsSmaller(List<GameObject> stackOfDiscs, int selectedDiscSize) {  // check the size of the moving disc against an individual list of discs
        bool result = true;
        if (stackOfDiscs.Count > 0) // if there are any disks in the list, check them
        {
            for (int i = 0; i < stackOfDiscs.Count; i++)
            {
                int currentDiscSize = stackOfDiscs[i].GetComponent<DiscBehaviour>().size;
                if (selectedDiscSize > currentDiscSize)
                {
                    result = false;
                }
            }
        }
        else // otherwise, any disc can move to an empty rod
        {
            result = true;
        }
        return result;
    }

    private bool checkForClickedRod() // check if any of the rods have been selected, store a reference to the selected one
    {
        bool result = false;

        RodBehaviour rodScript = leftRod.GetComponent<RodBehaviour>();
        if (rodScript.isSelected())
        {
            selectedRod = leftRod;
            result = true;
        }

        rodScript = centerRod.GetComponent<RodBehaviour>();
        if (rodScript.isSelected())
        {
            selectedRod = centerRod;
            result = true;
        }

        rodScript = rightRod.GetComponent<RodBehaviour>();
        if (rodScript.isSelected())
        {
            selectedRod = rightRod;
            result = true;
        }
        return result;
    }

    bool checkForClickedDisc() // check if any discs have been selected, store a reference to the selected one
    {
        bool result = false;
        for (int i = 0; i < allDiscs.Length; i++) 
        {
            DiscBehaviour discScript = allDiscs[i].GetComponent<DiscBehaviour>();
            if (discScript.isSelected()) 
            {
                result = true;
                selectedDisc = allDiscs[i];
                break;
            }
        }
        return result;
    }

    private void setDiscStates() // set which disc can be selected on each list
    {
        setTopDisc(leftDiscs);
        setTopDisc(centerDiscs);
        setTopDisc(rightDiscs);
    }

    private void setTopDisc(List<GameObject> stackOfDiscs) // identify the top disc in a list and set it as selectable
    {
        int topDisc = 0;

        if (stackOfDiscs.Count > 0) // if there are any discs in the list
        {
            for (int i = 0; i < stackOfDiscs.Count; i++) // set them all as non-selecctable, turn colliders off and find the highest one in the same process
            {
                DiscBehaviour discScript = stackOfDiscs[i].GetComponent<DiscBehaviour>();
                discScript.setSelectable(false);
                stackOfDiscs[i].GetComponent<MeshCollider>().enabled = false;
                if (stackOfDiscs[i].transform.position.y > stackOfDiscs[topDisc].transform.position.y)
                {
                    topDisc = i;
                }
            }
            DiscBehaviour topDiscScript = stackOfDiscs[topDisc].GetComponent<DiscBehaviour>(); // set the highest disc as selectable, turn its collider on
            topDiscScript.setSelectable(true);
            stackOfDiscs[topDisc].GetComponent<MeshCollider>().enabled = true;
        }
    }

    private void makeDiscsUnselectable() // make all discs unselectable and turn all colliders off
    {
        for (int i = 0; i < allDiscs.Length; i++)
        {
            DiscBehaviour discScript = allDiscs[i].GetComponent<DiscBehaviour>();
            discScript.setSelectable(false);
            allDiscs[i].GetComponent<MeshCollider>().enabled = false;
        }
    }

    private void makeRodsUnselectable() // make all rods unselectable and turn all colliders off
    {
        RodBehaviour rodScript = leftRod.GetComponent<RodBehaviour>();
        rodScript.setSelectable(false);
        leftRod.GetComponent<BoxCollider>().enabled = false;

        rodScript = centerRod.GetComponent<RodBehaviour>();
        rodScript.setSelectable(false);
        centerRod.GetComponent<BoxCollider>().enabled = false;

        rodScript = rightRod.GetComponent<RodBehaviour>();
        rodScript.setSelectable(false);
        rightRod.GetComponent <BoxCollider>().enabled = false;
    }

    private void determineDiscPositions() // determine on which rod is each particular disc, and populate disc lists accordingly
    {
        leftDiscs = new List<GameObject>();
        centerDiscs = new List<GameObject>();
        rightDiscs = new List<GameObject>();

        for (int i = 0; i < allDiscs.Length; i++)
        {
            float discX = allDiscs[i].transform.position.x;
            if (discX == leftRod.transform.position.x)
            {
                leftDiscs.Add(allDiscs[i]);
            }
            else if (discX == centerRod.transform.position.x)
            {
                centerDiscs.Add(allDiscs[i]);
            }
            else if (discX == rightRod.transform.position.x)
            {
                rightDiscs.Add(allDiscs[i]);
            }
            else // oops, one of the discs has strayed from the rods
            {
                if (debugConsole)
                {
                    Debug.LogWarning("Problem. One of the discs isn't on a rod"); 
                }
            }
        }

        if (debugConsole)
        {
            Debug.Log(leftDiscs.Count + " Discs on left rod");
            Debug.Log(centerDiscs.Count + " Discs on center rod");
            Debug.Log(rightDiscs.Count + " Discs on right rod");
        }
    }

    void makeRodsSelectable() // enable colliders for all rods and make them selectable
    { 
        RodBehaviour rodBehaviour = leftRod.GetComponent<RodBehaviour>();
        rodBehaviour.setSelectable(true);
        leftRod.GetComponent<BoxCollider>().enabled = true;

        rodBehaviour = centerRod.GetComponent<RodBehaviour>();
        rodBehaviour.setSelectable(true);
        centerRod.GetComponent<BoxCollider>().enabled = true;

        rodBehaviour = rightRod.GetComponent<RodBehaviour>();
        rodBehaviour.setSelectable(true);
        rightRod.GetComponent<BoxCollider>().enabled = true;
    }

    void deselectAll()  // deselect all discs and rods
    {
        for (int i = 0; i < allDiscs.Length; i++)
        {
            DiscBehaviour discScript = allDiscs[i].GetComponent<DiscBehaviour>();
            discScript.deselect();
        }

        RodBehaviour rodBehaviour = leftRod.GetComponent<RodBehaviour>();
        rodBehaviour.deselect();

        rodBehaviour = centerRod.GetComponent<RodBehaviour>();
        rodBehaviour.deselect();

        rodBehaviour = rightRod.GetComponent<RodBehaviour>();
        rodBehaviour.deselect();
    }

    public void restartScene() // restart the game, only if a move has been made already
    {
        if (moveCount > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (debugConsole)
            {
                Debug.Log("Restarting scene");
            }
        }
    }

    private void changeState(string s) //  change the state and print to console if needed
    {
        state = s;
        if (debugConsole)
        {
            Debug.Log("New state: " + state);
        }
    }

    private void setMoveCounter(int count) // set the counter text element
    {
        moveCountText.text = "MOVES: " + count;
    }

    bool anyDiscsAnimating() // check if any of the discs are currently moving
    {
        bool isAnimating = false;
        for (int i = 0; i < allDiscs.Length; i++)
        {
            DiscBehaviour discBehaviour = allDiscs[i].GetComponent<DiscBehaviour>();
            if (discBehaviour.isFlying())
            {
                isAnimating = true;
                break;
            }
        }
        return isAnimating;
    }
}
