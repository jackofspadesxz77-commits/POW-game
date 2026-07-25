using UnityEngine;

/// <summary>
/// Represents a single POW in the camp with skills and stats
/// </summary>
public class POW : MonoBehaviour
{
    public enum POWState { Healthy, Sick, Injured, Escaped, Dead }
    public enum Rank { Enlisted, NCO, Officer }

    [SerializeField] private string powName;
    [SerializeField] private Rank rank = Rank.Enlisted;

    // Stats that affect labor efficiency and escape risk
    [SerializeField] private float strength = 70f;       // Physical labor capability
    [SerializeField] private float intelligence = 60f;   // Skilled labor, manufacturing
    [SerializeField] private float obedience = 55f;      // Likelihood to follow orders, resist escape

    private POWState currentState = POWState.Healthy;
    private float health = 100f;
    private float morale = 50f;  // Individual prisoner morale - separate from war progress
    private float hunger = 0f;
    private float fatigue = 0f;
    private float escapeRiskModifier = 1f;

    private Building assignedBuilding;
    private LaborTask currentTask = LaborTask.Idle;

    public enum LaborTask { Idle, Construction, Manufacturing, Farming, Mining }

    private void Update()
    {
        UpdateNeeds();
        UpdateState();
    }

    private void UpdateNeeds()
    {
        // Increase hunger and fatigue based on current task
        float taskIntensity = GetTaskIntensity();
        hunger += Time.deltaTime * (5f + taskIntensity * 10f);
        fatigue += Time.deltaTime * (3f + taskIntensity * 8f);

        // Clamp values
        hunger = Mathf.Clamp(hunger, 0, 100);
        fatigue = Mathf.Clamp(fatigue, 0, 100);

        // Affect individual morale based on conditions
        morale -= Time.deltaTime * (hunger * 0.05f + fatigue * 0.03f);
        if (currentState == POWState.Sick) morale -= Time.deltaTime * 0.2f;
        
        // War proximity can demoralize prisoners
        float warProgress = WarSystem.Instance.GetWarProgress();
        if (warProgress < 30f) morale -= Time.deltaTime * 0.3f; // War getting close is demoralizing
        
        morale = Mathf.Clamp(morale, 0, 100);

        // Affect health based on morale, hunger, and rank
        float healthDegradation = 0;
        if (hunger > 80f && morale < 30f) healthDegradation += 2f;
        if (currentState == POWState.Sick) healthDegradation += 1f;
        
        health -= Time.deltaTime * healthDegradation;
        health = Mathf.Clamp(health, 0, 100);
    }

    private void UpdateState()
    {
        // Check for escape attempts (low morale, low obedience, low rank = higher risk)
        float escapeChance = (1f - obedience / 100f) * (1f - morale / 100f);
        if (rank == Rank.Officer) escapeChance *= 2f; // Officers more likely to escape

        if (Random.value < escapeChance * 0.0001f) // Very small chance per frame
        {
            AttemptEscape();
        }

        // Health state transitions
        if (health <= 0)
        {
            currentState = POWState.Dead;
        }
        else if (health < 20f)
        {
            currentState = POWState.Injured;
        }
        else if (Random.value < 0.001f && hunger > 70f) // Small chance to get sick from poor conditions
        {
            currentState = POWState.Sick;
        }
    }

    private void AttemptEscape()
    {
        currentState = POWState.Escaped;
        Debug.Log($"{powName} ({rank}) has escaped!");
    }

    private float GetTaskIntensity()
    {
        return currentTask switch
        {
            LaborTask.Construction => 0.8f,
            LaborTask.Manufacturing => 0.6f,
            LaborTask.Farming => 0.7f,
            LaborTask.Mining => 0.9f,
            _ => 0f
        };
    }

    public void Feed(float amount)
    {
        hunger = Mathf.Max(0, hunger - amount);
        health += amount * 0.3f;
        morale += amount * 0.2f; // Fed prisoners are happier
        health = Mathf.Clamp(health, 0, 100);
        morale = Mathf.Clamp(morale, 0, 100);
    }

    public void Rest(float amount)
    {
        fatigue = Mathf.Max(0, fatigue - amount);
        health += amount * 0.2f;
        morale += amount * 0.1f;
    }

    public void TreatMedically()
    {
        if (currentState == POWState.Injured || currentState == POWState.Sick)
        {
            health = Mathf.Min(health + 20, 100);
            currentState = POWState.Healthy;
        }
    }

    public void AssignTask(LaborTask task, Building building)
    {
        currentTask = task;
        assignedBuilding = building;
    }

    public float GetLaborEfficiency()
    {
        // Efficiency based on assigned task, stats, and current state
        if (currentState != POWState.Healthy) return 0f;
        if (fatigue > 80f) return 0.2f;

        float baseEfficiency = currentTask switch
        {
            LaborTask.Construction => strength / 100f,
            LaborTask.Manufacturing => intelligence / 100f,
            LaborTask.Farming => (strength + intelligence) / 200f,
            LaborTask.Mining => strength / 100f,
            _ => 0f
        };

        float moraleFactor = (50f + morale) / 100f; // Morale affects all work
        return baseEfficiency * moraleFactor * (1f - fatigue / 200f);
    }

    // Getters
    public POWState GetState() => currentState;
    public float GetHealth() => health;
    public float GetMorale() => morale;
    public float GetHunger() => hunger;
    public float GetFatigue() => fatigue;
    public Rank GetRank() => rank;
    public float GetStrength() => strength;
    public float GetIntelligence() => intelligence;
    public float GetObedience() => obedience;
    public LaborTask GetCurrentTask() => currentTask;
}
