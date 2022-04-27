using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_DoDamage : MonoBehaviour
{
    public Ship ship;
    public int damage = 3;
    private void Update()
    {

        if (ship == null) return;

        if (Input.GetKeyDown(KeyCode.N))
        {

            ship.TakeDamage(damage);
        }
    }
}
