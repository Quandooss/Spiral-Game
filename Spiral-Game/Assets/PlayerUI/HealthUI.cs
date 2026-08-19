using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI healthText;

    void Update()
    {
        healthText.text =
            "HP: " +
            player.CurrentHealth +
            " / " +
            player.MaxHealth;
    }
}