using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildEditor
{
    public static void NoBuild()
    {
        Debug.Log(Application.productName + " v" + Application.version);
        Debug.Log("Caching Library");
    }
}
