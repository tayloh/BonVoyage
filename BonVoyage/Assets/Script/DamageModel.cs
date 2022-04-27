using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour
{

    // WeatherParameters weatherParams;
    // And other stuff...

    public static float BowSternAngle = 30.0f;
    public static float BowSternAccuracyReduction = 0.5f;
    public static float SternDamageAmplifier = 1.5f;
    public static float BowDamageAmplifier = 0.5f;

    public static float CalculateDamageFor(Ship attackingShip, Ship targetedShip)
    {
        var attackDir = (attackingShip.gameObject.transform.position - targetedShip.gameObject.transform.position).normalized;
        var forwardDir = targetedShip.gameObject.transform.forward;

        var distance = (targetedShip.gameObject.transform.position - attackingShip.gameObject.transform.position).magnitude;
        var accuracyCoefficient = _calculateAccuracyCoefficient(distance);

        float attackingShipTotalDmg = 0;
        foreach (var dmg in attackingShip.GetCannonDamageList())
        {
            var attackType = _getDirectionalAttackType(attackDir, forwardDir, BowSternAngle);
            var resultingDmg = dmg;

            switch (attackType)
            {
                case AttackType.Bow:
                    accuracyCoefficient *= BowSternAccuracyReduction;
                    resultingDmg *= BowDamageAmplifier;
                    Debug.Log("Attack on Bow");
                    break;
                case AttackType.Stern:
                    accuracyCoefficient *= BowSternAccuracyReduction;
                    resultingDmg *= SternDamageAmplifier;
                    Debug.Log("Attack on Stern");
                    break;
                case AttackType.Side:
                    Debug.Log("Attack on Side");
                    break;
            }

            var accuracyThreshold = Random.value;

            Debug.Log("Coeff:" + accuracyCoefficient + " Thresh:" + accuracyThreshold);

            if (accuracyCoefficient < accuracyThreshold)
            {
                resultingDmg = 0;
            }

            attackingShipTotalDmg += resultingDmg;
        }

        return attackingShipTotalDmg;
    }


    private static float _calculateAccuracyCoefficient(float D)
    {

        float distBetweenHexCenters = HexCoordinates.xOffset * 2;

        float H = Mathf.RoundToInt(distBetweenHexCenters * 10);

        return Mathf.Max(0, 1 - (D / H));
    }

    private static AttackType _getDirectionalAttackType(Vector3 attackDir, Vector3 forwardDir, float decidingAngle)
    {   
        // Directional damage modifiers
        var dotForwAttack = Vector3.Dot(forwardDir, attackDir);
        var angleOfAttackThreshold = Mathf.Cos(decidingAngle * Mathf.PI/180);

        if (dotForwAttack > angleOfAttackThreshold)
        {
            // Attack on bow (front) of ship
            // Accuracy of attacker is reduced by 50%
            return AttackType.Bow;
        }
        else if (dotForwAttack < -angleOfAttackThreshold)
        {
            // Attack on stern (back) of ship
            // Accuracy of attacker is reduced by 50%
            // Damage of attacker is increased by 50%
            return AttackType.Stern;
        }

        return AttackType.Side;
    }

}

public enum AttackType
{
    Stern,
    Bow,
    Side
}
