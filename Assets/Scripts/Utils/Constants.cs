using UnityEngine;

public static class Constants
{
    // === GAMEPLAY ===
    public const float GRAVITY = 9.81f;
    public const float JUMP_COOLDOWN = 0.3f;

    // === CHARACTER STATS ===
    public const int PYRO_MAX_HP = 100;
    public const int TERRA_MAX_HP = 150;
    public const int AQUA_MAX_HP = 100;
    public const int WINDY_MAX_HP = 80;

    public const float PYRO_SPEED = 5f;
    public const float TERRA_SPEED = 2.5f;
    public const float AQUA_SPEED = 5f;
    public const float WINDY_SPEED = 7f;

    public const float PYRO_JUMP = 5f;
    public const float TERRA_JUMP = 4.5f;
    public const float AQUA_JUMP = 5f;
    public const float WINDY_JUMP = 5.5f;

    // === ABILITIES ===
    public const float TORCH_LIGHT_DURATION = 5f;
    public const float TORCH_LIGHT_RADIUS = 5f;

    public const float WINDY_FLY_DURATION = 3f;
    public const float WINDY_FLY_COOLDOWN = 4f;

    public const float AQUA_WATER_SPEED_MULTIPLIER = 2f;

    // === TILE DAMAGE ===
    public const int WATER_DAMAGE_PER_SECOND = 10;
    public const int LAVA_DAMAGE_PER_SECOND = 20;
    public const int LAVA_INSTANT_DAMAGE = 50;

    // === WIND SYSTEM ===
    public const float WIND_FORCE = 10f;
    public const float WIND_SLOW_MULTIPLIER = 0.5f; // Pyro jadi lambat di angin

    // === PROTECTION ===
    public const float TERRA_PROTECTION_RADIUS = 1f;

    // === LAYERS ===
    public const string LAYER_CHARACTER = "Character";
    public const string LAYER_TILE = "Tile";
    public const string LAYER_INTERACTABLE = "Interactable";
    public const string LAYER_PLATFORM = "Platform";
    public const string LAYER_WIND = "Wind";

    // === TAGS ===
    public const string TAG_PYRO = "Pyro";
    public const string TAG_TERRA = "Terra";
    public const string TAG_AQUA = "Aqua";
    public const string TAG_WINDY = "Windy";
    public const string TAG_TORCH = "Torch";
    public const string TAG_BLOCK_HEAVY = "BlockHeavy";
    public const string TAG_BLOCK_LIGHT = "BlockLight";
}