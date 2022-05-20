using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] Image ShipTypeImage;

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
        if (PausMenu.GameIsPaused) return;
        UI.SetActive(true);
    }

    public void UpdatePanel(Ship ship)
    {
        if (PausMenu.GameIsPaused) return;
        if (ship == null) return;

        SetShipTypeText(ship._shipType);
        SetHealthText(ship.Health, ship.MaxHealth);
        
        SetCannonStatsText(
            ship.GetNumberOfCannonsPerSide(), 
            Mathf.RoundToInt(ship.GetLeftSideCannonDamageList()[0]), 
            ship.FireRange);

        SetStatusText(ship.tag);

        SetAttackStatsText(ship);

        SetShipTypeImage(ship);

    }

    private void SetShipTypeImage(Ship ship)
    {

        if (ship.gameObject.name == "Blackbeard")
        {
            ShipTypeImage.sprite = Resources.Load<Sprite>("ShipIcons/ShipIconSpecial2");
            return;
        }

        var type = ship._shipType;

        switch (type)
        {
            case ShipType.Brig:
                ShipTypeImage.sprite = Resources.Load<Sprite>("ShipIcons/ShipIconWarship1");
                break;
            case ShipType.Frigate:
                ShipTypeImage.sprite = Resources.Load<Sprite>("ShipIcons/ShipIconWarship2");
                break;
            case ShipType.TreasureShip:
                ShipTypeImage.sprite = Resources.Load<Sprite>("ShipIcons/ShipIconSpecial1");
                break;
            case ShipType.ShipOfTheLine:
                ShipTypeImage.sprite = Resources.Load<Sprite>("ShipIcons/ShipIconWarship3");
                break;
        }

    }

    private void SetAttackStatsText(Ship ship)
    {
        if (ship.IsDead)
        {
            AttackStatsText.text = "";
            return;
        }

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


        var activeShip = gameManager.GetActualShip();

        var inArc = false;
        for (int i = 0; i < 2; i++)
        {
            var broadsideTiles = activeShip.GetAttackableTilesFor(i);

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

        if (!inArc)
        {
            AttackStatsText.text = "";
            return;
        }
        
        var hitChance = DamageModel.CalculateHitChance(activeShip, ship);
        var attackType = DamageModel.GetDirectionalAttackType(activeShip, ship);
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
