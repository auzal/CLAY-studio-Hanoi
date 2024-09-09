using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RodBehaviour : MonoBehaviour
{

    public Material rodMaterial;
    public Material selectedMaterial;

    public float fadeTime = 0.3f; // in seconds, duration of color fade for mouse over
    public string id;   // used in GameManager.cs

    private bool selectable = false;
    private bool selected = false;

    private float fadeStartTime;
    private bool fading = false;

    private Color currentColor;
    private Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
      gameObject.GetComponent<MeshRenderer>().material = rodMaterial;   // just in case, make sure the rods start with the right material
    }

    // Update is called once per frame
    void Update()
    {
        if (fading) { // control the color fade, interpolate between current and target color

            if (Time.time - fadeStartTime < fadeTime) 
            {
                float lerpAmount = (Time.time - fadeStartTime) / fadeTime;
                Color lerpedColor = Color.Lerp(currentColor, targetColor, lerpAmount);
                gameObject.GetComponent<Renderer>().material.color = lerpedColor;
            }
            else 
            {
                fading = false;
                gameObject.GetComponent<Renderer>().material.color = targetColor;
            }
        } 
    }

    private void OnMouseEnter()
    {
        // if the rod is hovered and the gamemanager has set it as selectable, fade to selected color
        if (selectable)
        {
            startColorFade(selectedMaterial.color);
        }
    }

    private void OnMouseDown()
    {
        // if the rod is clicked and the gamemanager has set it as selectable, select it
        if (selectable)
        {
            select();
        }
    }

    private void OnMouseExit()
    {
        // if the mouse leaves the rod collider, fade back to idle color
        startColorFade(rodMaterial.color);
    }

    public void setSelectable(bool s) // called from GameManager.cs
    {
        selectable = s;
    }

    public void select()
    {
        selected = true;
    }

    public void deselect() // called from GameManager.cs
    {
        selected = false;
        startColorFade(rodMaterial.color); // fade back to idle color just in case
    }

    public bool isSelected() // called from GameManager.cs
    {
        return selected;
    }

    private void startColorFade(Color target) // handles the start of color fades
    {
        fading = true;
        fadeStartTime = Time.time;
        currentColor = gameObject.GetComponent<Renderer>().material.color;
        targetColor = target;
    }
}
