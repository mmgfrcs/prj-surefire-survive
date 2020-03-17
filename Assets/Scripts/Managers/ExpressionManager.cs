using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Affdex;

public class ExpressionManager : ImageResultsListener
{
    internal static bool FaceFound { get; private set; }
    internal static Face FaceResults { get; private set; }

    private void Start()
    {
        print("ExpressionManager: FER active");
    }

    public override void onFaceFound(float timestamp, int faceId)
    {
        FaceFound = true;
    }

    public override void onFaceLost(float timestamp, int faceId)
    {
        FaceFound = false;
    }

    public override void onImageResults(Dictionary<int, Face> faces)
    {
        if (faces.Count >= 1) FaceResults = faces[0];
    }
}
