using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateShip : Ship
{

    
    public override void Update()
    {
        //Aim();
        if (Input.GetKeyDown(KeyCode.X))
        {
            shoot();
        }
    }



  
}
