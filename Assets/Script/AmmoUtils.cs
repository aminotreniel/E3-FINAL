using UnityEngine;

// Utility to return ammunition counts as text for UI or debug.
public static class AmmoUtils
{
    public static string GetAmmoTextForPlayer(Transform playerTransform)
    {
        if (playerTransform == null)
            return "No Player";

        var character = playerTransform.GetComponent<InfimaGames.LowPolyShooterPack.Character>();
        if (character == null)
            return "No Character";

        var inventory = character.GetInventory();
        if (inventory == null)
            return "No Inventory";

        var weapon = inventory.GetEquipped();
        if (weapon == null)
            return "No Weapon";

        int current = weapon.GetAmmunitionCurrent();
        int total = weapon.GetAmmunitionTotal();
        return current + "/" + total;
    }
}
