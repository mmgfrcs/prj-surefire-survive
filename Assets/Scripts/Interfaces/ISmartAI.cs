using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISmartAI
{
    void GetPointFromGM(Vector3 point);
    void MoveToPoint();
}
