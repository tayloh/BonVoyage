using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageModel : MonoBehaviour
{

    // WeatherParameters weatherParams;
    // And other stuff...

    public static float BowSternAngle = 30.0f;
    public static float BowSternAccuracyReduction = 0.65f;
    public static float SternDamageAmplifier = 2.5f;
    public static float BowDamageAmplifier = 0.5f;
    public static float TileDistanceForWorstAccuracy = 10;

    public static float[] GetCannonWiseDamageFor(Ship attackingShip, Ship targetedShip)
    {
        var attackDir = (attackingShip.gameObject.transform.position - targetedShip.gameObject.transform.position).normalized;
        var forwardDir = targetedShip.gameObject.transform.forward;

        var distance = (targetedShip.gameObject.transform.position - attackingShip.gameObject.transform.position).magnitude;
        var accuracyCoefficient = _calculateAccuracyCoefficient(distance);

        var attackType = _getDirectionalAttackType(attackDir, forwardDir, BowSternAngle);

        var currentBroadSideDmgList = new List<float>();

        //if (allCannonsDmgList.Length % 2 != 0)
        //{
        //    Debug.Log("CalculateDamageFor() inside DamageModel.cs is not updated for different number of cannons per side.");
        //}

        // This if and else if needs to change, need to be able to use smthing like ship.GetLeftSideCannons()
        if (attackingShip.HasFiredLeft)
        {
            var leftSideDmgList = attackingShip.GetLeftSideCannonDamageList();
            for (int i = 0; i < leftSideDmgList.Length; i++)
            {
                currentBroadSideDmgList.Add(leftSideDmgList[i]);
            }
        }
        else if (attackingShip.HasFiredRight)
        {
            var rightSideDmgList = attackingShip.GetRightSideCannonDamageList();
            for (int i = 0; i < rightSideDmgList.Length; i++)
            {
                currentBroadSideDmgList.Add(rightSideDmgList[i]);
            }
        }

        // Apply effects of attack type on accuracy once
        switch (attackType)
        {
            case AttackType.Bow:
                accuracyCoefficient *= BowSternAccuracyReduction;
                Debug.Log("DMG - Attack on Bow");
                break;
            case AttackType.Stern:
                accuracyCoefficient *= BowSternAccuracyReduction;
                Debug.Log("DMG - Attack on Stern");
                break;
            case AttackType.Side:
                Debug.Log("DMG - Attack on Side");
                break;
        }

        Debug.Log("DMG - " + attackingShip + "->" + targetedShip);
        Debug.Log("DMG - " + "Coeff:" + accuracyCoefficient);

        var hits = currentBroadSideDmgList.Count;
        var totalDmg = 0.0f;
        var cannonWiseDmgList = new List<float>();
        var cannonIndex = 0;
        foreach (var dmg in currentBroadSideDmgList)
        {
            var resultingDmg = dmg;

            // Apply effects of attack type on dmg amp. per cannon (cannons can have different dmg)
            switch (attackType)
            {
                case AttackType.Bow:
                    resultingDmg *= BowDamageAmplifier;
                    break;
                case AttackType.Stern:
                    resultingDmg *= SternDamageAmplifier;
                    break;
                case AttackType.Side:
                    break;
            }

            var accuracyThreshold = Random.value;

            Debug.Log("DMG - " + "Threshold:" + accuracyThreshold);

            if (accuracyCoefficient < accuracyThreshold)
            {
                resultingDmg = 0;
                hits--;
            }

            cannonWiseDmgList.Add(Mathf.CeilToInt(resultingDmg));
            totalDmg += cannonWiseDmgList[cannonIndex];

            if (totalDmg > targetedShip.Health)
            {
                break;
            } 

            cannonIndex++;
        }

        //cannonWiseDmgList = Mathf.CeilToInt(cannonWiseDmgList);
        Debug.Log("DMG - " + hits + " hits on " + DamageModel.GetAttackTypeString(attackType));

        // Convert to float[]
        float[] result = new float[cannonWiseDmgList.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = cannonWiseDmgList[i];
        }

        return result;
    }

    public static float CalculateTotalDamageFor(Ship attackingShip, Ship targetedShip)
    {
        var attackDir = (attackingShip.gameObject.transform.position - targetedShip.gameObject.transform.position).normalized;
        var forwardDir = targetedShip.gameObject.transform.forward;

        var distance = (targetedShip.gameObject.transform.position - attackingShip.gameObject.transform.position).magnitude;
        var accuracyCoefficient = _calculateAccuracyCoefficient(distance);

        var attackType = _getDirectionalAttackType(attackDir, forwardDir, BowSternAngle);

        var allCannonsDmgList = attackingShip.GetCannonDamageList();
        var currentBroadSideDmgList = new List<float>();

        if (allCannonsDmgList.Length % 2 != 0) 
        {
            Debug.Log("CalculateDamageFor() inside DamageModel.cs is not updated for different number of cannons per side.");
        } 
        
        // This if and else if needs to change, need to be able to use smthing like ship.GetLeftSideCannons()
        if (attackingShip.HasFiredLeft)
        {
            for (int i = 0; i < attackingShip.NumCannons / 2; i++)
            {
                currentBroadSideDmgList.Add(allCannonsDmgList[i]);
            }
        }
        else if (attackingShip.HasFiredRight)
        {
            for (int i = attackingShip.NumCannons / 2; i < attackingShip.NumCannons; i++)
            {
                currentBroadSideDmgList.Add(allCannonsDmgList[i]);
            }
        }

        // Apply effects of attack type on accuracy once
        switch (attackType)
        {
            case AttackType.Bow:
                accuracyCoefficient *= BowSternAccuracyReduction;
                Debug.Log("DMG - Attack on Bow");
                break;
            case AttackType.Stern:
                accuracyCoefficient *= BowSternAccuracyReduction;
                Debug.Log("DMG - Attack on Stern");
                break;
            case AttackType.Side:
                Debug.Log("DMG - Attack on Side");
                break;
        }

        Debug.Log("DMG - " + attackingShip + "->" + targetedShip);
        Debug.Log("DMG - " + "Coeff:" + accuracyCoefficient);

        var hits = currentBroadSideDmgList.Count;
        float attackingShipTotalDmg = 0;
        foreach (var dmg in currentBroadSideDmgList)
        {
            var resultingDmg = dmg;

            // Apply effects of attack type on dmg amp. per cannon (cannons can have different dmg)
            switch (attackType)
            {
                case AttackType.Bow:
                    resultingDmg *= BowDamageAmplifier;
                    break;
                case AttackType.Stern:
                    resultingDmg *= SternDamageAmplifier;
                    break;
                case AttackType.Side:
                    break;
            }

            var accuracyThreshold = Random.value;

            Debug.Log("DMG - " + "Threshold:" + accuracyThreshold);

            if (accuracyCoefficient < accuracyThreshold)
            {
                resultingDmg = 0;
                hits--;
            }

            attackingShipTotalDmg += resultingDmg;
        }

        attackingShipTotalDmg = Mathf.CeilToInt(attackingShipTotalDmg);
        Debug.Log("DMG - " + hits + " hits for " + attackingShipTotalDmg + " damage on " + DamageModel.GetAttackTypeString(attackType));

        return attackingShipTotalDmg;
    }

    public static float CalculateHitChance(Ship attackingShip, Ship targetedShip)
    {
        var distance = (attackingShip.transform.position - targetedShip.transform.position).magnitude;
        var accuracyCoeff = _calculateAccuracyCoefficient(distance);

        var attackType = DamageModel.GetDirectionalAttackType(attackingShip, targetedShip);

        var hitChance = accuracyCoeff;

        if (attackType == AttackType.Bow || attackType == AttackType.Stern)
        {
            hitChance *= DamageModel.BowSternAccuracyReduction;
        }


        return hitChance;
    }

    private static float _calculateAccuracyCoefficient(float D)
    {
        float distBetweenHexCenters = HexCoordinates.xOffset;

        float H = Mathf.RoundToInt(distBetweenHexCenters * TileDistanceForWorstAccuracy);

        return Mathf.Max(0, 1 - (D / H));
    }

    private static AttackType _getDirectionalAttackType(Vector3 attackDir, Vector3 forwardDir, float decidingAngle)
    {   
        // Directional damage modifiers
        var dotForwAttack = Vector3.Dot(forwardDir, attackDir);
        //Debug.Log("DMG - " + "cos theta:" + dotForwAttack);
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

    public static AttackType GetDirectionalAttackType(Ship attackingShip, Ship targetedShip)
    {
        var attackDir = (attackingShip.gameObject.transform.position - targetedShip.gameObject.transform.position).normalized;
        var forwardDir = targetedShip.gameObject.transform.forward;
        var attackType = _getDirectionalAttackType(attackDir, forwardDir, DamageModel.BowSternAngle);
        
        return attackType;
    }

    /// <summary>
    /// attackDir: direction from position that is attacked to position that is attacking
    /// forwardDir: forward direction of ship that is attacked (or specify any desired forward direction)
    /// </summary>
    public static AttackType GetDirectionalAttackType(Vector3 attackDir, Vector3 forwardDir)
    {
        return _getDirectionalAttackType(attackDir, forwardDir, DamageModel.BowSternAngle);
    }

    public static string GetAttackTypeString(AttackType type)
    {
        switch (type)
        {
            case AttackType.Bow:
                return "Bow";

            case AttackType.Stern:
                return "Stern";

            case AttackType.Side:
                return "Side";

            default:
                return "";
        }
    }

}

public enum AttackType
{
    Bow,
    Side,
    Stern
}
