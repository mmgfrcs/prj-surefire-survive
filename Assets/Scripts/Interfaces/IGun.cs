using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun
{
    int CurrentAmmo { get; }
    int CurrentMagazine { get; }
    void AddMagazine(int amount);
    
}
