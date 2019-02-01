using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDraw : MonoBehaviour
{

    private Vector2 orgBoxPos = Vector2.zero;
    private Vector2 endBoxPos = Vector2.zero;
    private RaycastHit hit;

    private Texture2D tex;
    private Color texColor;

    // Use this for initialization
    void Start()
    {
        texColor = new Color(255, 0, 0);
        tex = new Texture2D(100, 100);
        var texPixels = tex.GetPixels();
        for (int i = 0; i < texPixels.Length; i++)
        {
            texPixels[i] = texColor;
        }
        tex.SetPixels(texPixels);
        tex.Apply();

        hit = new RaycastHit();
    }

    // Update is called once per frame
    void Update()
    {
        // Called while the user is holding the mouse down.
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Called on the first update where the user has pressed the mouse button.
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.Log("start drawing");
                orgBoxPos = Input.mousePosition;
            }
            else  // Else we must be in "drag" mode.
            {

                endBoxPos = Input.mousePosition;
            }

        }
       
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Debug.Log("stop drawing");
            // Handle the case where the player had been drawing a box but has now released.
           
            //if (endBoxPos != Vector2.zero && orgBoxPos != Vector2.zero)
            //    HandleUnitSelection();
           
            // Reset box positions.
            endBoxPos = orgBoxPos = Vector2.zero;
        }
    }


    /// <summary>
    /// Draws the selection rectangle if the user is holding the mouse down.
    /// </summary>
    void OnGUI()
    {       
        // If we are in the middle of a selection draw the texture.
        if (orgBoxPos != Vector2.zero && endBoxPos != Vector2.zero)
        {
            Debug.Log("draw");
            // Create a rectangle object out of the start and end position while transforming it
            // to the screen's cordinates.
            var rect = new Rect(orgBoxPos.x, Screen.height - orgBoxPos.y,
                                endBoxPos.x - orgBoxPos.x,
                                -1 * (endBoxPos.y - orgBoxPos.y));
            // Draw the texture.
            GUI.DrawTexture(rect, tex);
            
        }
    }
}
