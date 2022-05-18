using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipStatsPanel : MonoBehaviour
{

    public static ShipStatsPanel Instance;

    [SerializeField] GameManager gameManager;
    [SerializeField] HexGrid hexGrid;

    [SerializeField] GameObject UI;

    [SerializeField] TextMeshProUGUI ShipTypeText;
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] TextMeshProUGUI CannonsStatText;
    [SerializeField] TextMeshProUGUI StatusText;
    [SerializeField] TextMeshProUGUI AttackStatsText;

    private void Start()
    {
        Instance = this;
    }

    public void Hide()
    {
        UI.SetActive(false);
    }

    public void Show()
    {
        UI.SetActive(true);
    }

    public void UpdatePanel(Ship ship)
    {

        SetShipTypeText(ship._shipType);
        SetHealthText(ship.Health, ship.MaxHealth);
        
        SetCannonStatsText(
            ship.GetNumberOfCannonsPerSide(), 
            Mathf.RoundToInt(ship.GetLeftSideCannonDamageList()[0]), 
            ship.FireRange);

        SetStatusText(ship.tag);

        SetAttackStatsText(ship);

    }

    private void SetAttackStatsText(Ship ship)
    {
        if (gameManager.state != GameState.PlayerFire)
        {
            AttackStatsText.text = "";
            return;
        }

        if (ship.CompareTag("PlayerShip"))
        {
            AttackStatsText.text = "";
            return;
        }

        var actualShip = gameManager.GetActualShip();

        var inArc = false;
        for (int i = 0; i < 2; i++)
        {
            var broadsideTiles = actualShip.GetAttackableTilesFor(i);

            foreach (var tilePos in broadsideTiles)
            {
                var tile = hexGrid.GetTileAt(tilePos);
                if (tile == null) continue;
                if (tile.Ship == null) continue;

                if (tile.Ship.GetInstanceID() == ship.GetInstanceID())
                {
                    inArc = true;
                    break;
                }
            }
        }
        Debug.Log("IN ARC:" + inArc);

        if (!inArc)
        {
            AttackStatsText.text = "";
            return;
        }
        
        var hitChance = DamageModel.CalculateHitChance(actualShip, ship);
        var attackType = DamageModel.GetDirectionalAttackType(actualShip, ship);
        var attackTypeString = DamageModel.GetAttackTypeString(attackType);

        AttackStatsText.text = "Chance to\nhit: " + Mathf.RoundToInt(hitChance * 100) + "%" + "\n" + attackTypeString;

    }

    private void SetStatusText(string tag)
    {
        switch (tag)
        {
            case "Pirate":
                StatusText.text = "Enemy";
                StatusText.color = Color.red;
                break;
            case "PlayerShip":
                StatusText.text = "Friendly";
                StatusText.color = Color.green;
                break;
        }
    }

    private void SetCannonStatsText(int numCannons, int damage, int range)
    {
        CannonsStatText.text = numCannons + " Per Side" + "\n" + range + " Range" + "\n" + damage + " Damage";
    }

    private void SetHealthText(float health, float maxHealth)
    {
        HealthText.text = health + "/" + maxHealth;
    }

    private void SetShipTypeText(ShipType type)
    {
        var shipTypeText = type.ToString();

        if (type == ShipType.TreasureShip) 
        {
            shipTypeText = "Galleon";
        }
        else if (type == ShipType.ShipOfTheLine)
        {
            shipTypeText = "SotL";
        }

        ShipTypeText.text = shipTypeText;
    }

}
