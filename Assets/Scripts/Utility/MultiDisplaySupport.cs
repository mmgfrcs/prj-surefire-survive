using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDisplaySupport : MonoBehaviour
{
    static MultiDisplaySupport instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this);
            return;
        }

        Debug.Log($"Multi-Display: {Display.displays.Length} displays connected.");
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }
}
