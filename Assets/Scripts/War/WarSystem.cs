using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the war system - proximity, sound, and impact on gameplay
/// </summary>
public class WarSystem : MonoBehaviour
{
    public static WarSystem Instance { get; private set; }

    [SerializeField] private float factionMoraleStart = 50f;
    [SerializeField] private float warProximityStart = 100f; // Distance in arbitrary units
    [SerializeField] private float warProximityDangerZone = 20f;

    private float factionMorale = 50f;
    private float warProximity = 100f;
    private float damageAccumulation = 0f;

    private List<AudioSource> warAudioSources = new List<AudioSource>();
    private bool warIsClose = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        factionMorale = factionMoraleStart;
        warProximity = warProximityStart;
        InitializeWarAudio();
    }

    private void Update()
    {
        UpdateWarProximity();
        UpdateWarAudio();
        CheckForCampDamage();
    }

    private void InitializeWarAudio()
    {
        // Setup audio sources for different war sounds
        // This would be implemented with proper audio setup
    }

    private void UpdateWarProximity()
    {
        // War proximity decreases based on faction morale
        // Higher faction morale pushes war away, lower brings it closer
        float moraleEffect = (factionMorale - 50f) / 50f; // -1 to 1 range
        warProximity += moraleEffect * Time.deltaTime * 5f;

        warProximity = Mathf.Clamp(warProximity, 0, warProximityStart);

        warIsClose = warProximity < warProximityDangerZone;
    }

    private void UpdateWarAudio()
    {
        // Adjust audio volume and frequency based on war proximity
        float audioIntensity = 1f - (warProximity / warProximityStart);
        
        // Play distant explosions, gunfire, planes at higher intensity
        if (warIsClose)
        {
            // Heavy explosions and combat sounds
        }
        else if (warProximity < warProximityStart / 2)
        {
            // Occasional distant explosions
        }
    }

    private void CheckForCampDamage()
    {
        if (warIsClose && Random.value < 0.001f) // Small chance of damage per frame
        {
            // Random building or area takes damage from stray ordnance
            Debug.Log("Stray ordnance hits camp!");
        }
    }

    public void ContributeToWarEffort(int weaponsCount, int ammoCount, float foodAmount = 0)
    {
        // Boost faction morale and push war away based on weapons, ammo, and food production
        float contribution = (weaponsCount * 5) + (ammoCount * 0.5f) + (foodAmount * 0.3f);
        factionMorale = Mathf.Min(factionMorale + contribution, 100f);
        
        Debug.Log($"War effort contribution: {weaponsCount} weapons, {ammoCount} ammo, {foodAmount} food. Faction morale: {factionMorale}");
    }

    public void ReceiveWarUpdate(float factionPerformance)
    {
        // Called periodically with faction's military performance
        // -1 = losing, 0 = stalemate, 1 = winning
        factionMorale += factionPerformance * 10 * Time.deltaTime;
        factionMorale = Mathf.Clamp(factionMorale, 0, 100);
    }

    public float GetWarProximity() => warProximity;
    public float GetFactionMorale() => factionMorale;
    public bool IsWarClose() => warIsClose;
}
