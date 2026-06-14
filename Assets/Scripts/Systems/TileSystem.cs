using UnityEngine;

public enum TileType
{
    Normal,
    Water,
    Lava,
    Rocky,
    Dark,
    Wind,
    Platform
}

[RequireComponent(typeof(BoxCollider2D))]
public class TileCollisionHandler : MonoBehaviour
{
    [SerializeField] private TileType tileType = TileType.Normal;
    [SerializeField] private float damageTickRate = 0.5f;

    private float lastDamageTime = -999f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        CharacterBase character = collision.GetComponent<CharacterBase>();
        if (character == null || !character.IsActive()) return;

        // Prevent damage spam
        if (Time.time - lastDamageTime < damageTickRate)
            return;

        HandleTileEffect(character);
    }

    private void HandleTileEffect(CharacterBase character)
    {
        switch (tileType)
        {
            case TileType.Water:
                HandleWater(character);
                break;

            case TileType.Lava:
                HandleLava(character);
                break;

            case TileType.Wind:
                HandleWind(character);
                break;

            case TileType.Dark:
                HandleDark(character);
                break;
        }
    }

    private void HandleWater(CharacterBase character)
    {
        if (character is Pyro)
        {
            character.TakeDamage(Constants.WATER_DAMAGE_PER_SECOND);
            lastDamageTime = Time.time;
        }
        else if (character is Aqua aqua)
        {
            // Aqua benefit dari air (sudah di Aqua.cs)
        }
        else if (character is Terra terra)
        {
            character.TakeDamage(Constants.WATER_DAMAGE_PER_SECOND);
            lastDamageTime = Time.time;
        }
    }

    private void HandleLava(CharacterBase character)
    {
        if (character is Pyro)
        {
            // Pyro safe in lava (immunity)
            return;
        }

        character.TakeDamage(Constants.LAVA_DAMAGE_PER_SECOND);
        lastDamageTime = Time.time;
    }

    private void HandleWind(CharacterBase character)
    {
        if (character is Pyro pyro)
        {
            if (!pyro.IsProtectedFromWind())
            {
                pyro.ApplyWindForce(Vector2.right * Constants.WIND_FORCE);
            }
        }
        else if (character is Terra)
        {
            // Terra immune to wind (too heavy)
            return;
        }
        else if (character is Windy)
        {
            // Windy terbawa angin tapi bisa terbang
            character.GetComponent<Rigidbody2D>().AddForce(
                Vector2.right * Constants.WIND_FORCE * 0.5f,
                ForceMode2D.Force
            );
        }
    }

    private void HandleDark(CharacterBase character)
    {
        // Dark tile: jika tidak ada cahaya, visibility 0
        // Implementasi di LightSystem.cs
    }
}