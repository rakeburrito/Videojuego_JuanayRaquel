
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Requisitos")]
    public int requiredCoins = 55;
    public int requiredDiamonds = 15;
    public int requiredKeys = 1;

    [Header("Recompensa")]
    public int bonusScore = 100;

    public bool isOpen = false;

    public int OpenChest(int coins, int diamonds, int keys)
    {
        if (isOpen) return 0;

        // Comprobar requisitos
        if (coins < requiredCoins ||
            diamonds < requiredDiamonds ||
            keys < requiredKeys)
        {
            Debug.Log("Todavía faltan coleccionables.");
            return 0;
        }

        // Abrir cofre
        isOpen = true;

        Debug.Log("¡Cofre desbloqueado! +" + bonusScore + " puntos.");

        return bonusScore;
    }
}
