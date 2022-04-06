using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPirateShip : TempShip
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
