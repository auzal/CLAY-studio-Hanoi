using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;

public class DiscBehaviour : MonoBehaviour
{

    // materials for automatic color assignment and highlighting
    public Material discMaterial;
    public Material activeMaterial;
    public Material topMaterial;
    public Material bottomMaterial;

    private Color calculatedColor;

    
    public int numberOfDiscs = 5; // how many discs in total, used for automatic color assignment 
    public int size; // size of the disc
    
    private bool selectable = false; 
    private bool selected = false;

    private Vector3 originPosition;
    private Vector3 destinationPosition;

    private float flyTime = 1f;
    private float flyStartTime;
    private bool flying = false;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MeshRenderer>().material = discMaterial; // set the right material just in case
        setColorBasedOnSize(); // asign color based on size
    }

    // Update is called once per frame
    void Update()
    {
        if (flying) // handle the interpolation for the movement animation
        {
            if (Time.time - flyStartTime < flyTime)
            {
                float lerpAmount = (Time.time - flyStartTime) / flyTime;

                // the following two lines are an implementation of Smootherstep -> https://en.wikipedia.org/wiki/Smoothstep
                float smoothLerpAmount = lerpAmount;
                smoothLerpAmount = smoothLerpAmount * smoothLerpAmount * smoothLerpAmount * (smoothLerpAmount * (6f * smoothLerpAmount - 15f) + 10f);

                Vector3 currentPosition = Vector3.Lerp(originPosition, destinationPosition, smoothLerpAmount); // calculate the current position
                gameObject.transform.position = currentPosition;
                gameObject.transform.rotation = Quaternion.Euler(smoothLerpAmount*360, 0, 0); // and calculate a point on a full rotation on the X axis
            }
            else
            {
                flying = false;
                // if the disc has reached it's destination, make sure it's at the right position and correct rotation
                gameObject.transform.position = destinationPosition;
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
  
    void OnMouseDown() // if the disc is clicked and the gamemanager has set it as selectable, select it
    {
        if (selectable)
        {
            select();
        }
    }

   public void setSelectable(bool s) { // called from GameManager.cs
        selectable = s;
   }

    public bool isSelected() { // called from GameManager.cs
        return selected;
    }

    public void select()
    {
        selected = true;
        gameObject.GetComponent<MeshRenderer>().material = activeMaterial; // change to "highlight" material
    }

    public void deselect() // when a disc is deselected, go back to calculated color 
    {
        selected = false;
        gameObject.GetComponent<MeshRenderer>().material.color = calculatedColor;
    }

    private void setColorBasedOnSize()  // calculate the disc color, interpolating between the topMaterial and bottomMaterial, based on disc size and number of discs
    {
        float lerpAmount =   (size-1) / (numberOfDiscs-1f);
        calculatedColor = Color.Lerp( topMaterial.color, bottomMaterial.color, lerpAmount);
        gameObject.GetComponent<MeshRenderer>().material.color = calculatedColor;
    }

    public void flyTo(Vector3 position, float time) // starts the movement animation, takes a destination position and flight time as arguments. Called from gameManager.cs
    {
        flyTime = time;
        destinationPosition = position;
        originPosition = gameObject.transform.position;
        flyStartTime = Time.time;
        flying = true;
    }

    public Boolean isFlying() // called from gameManager.cs
    {
       return flying;
    }
}
    

