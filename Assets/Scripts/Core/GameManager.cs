using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main game manager for the POW camp
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int initialBudget = 50000;
    [SerializeField] private float maintenanceTickRate = 5f; // Maintenance cost every X seconds
    [SerializeField] private float foodConsumptionRate = 0.5f; // Food per POW per second

    private int currentBudget;
    private float nextMaintenanceTick;
    private float nextFoodTick;
    private List<POW> allPOWs = new List<POW>();
    private List<Building> allBuildings = new List<Building>();
    private float totalAmmunitionProduced = 0;
    private float totalWeaponsProduced = 0;
    private float totalFoodForWarEffort = 0;

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
        currentBudget = initialBudget;
        nextMaintenanceTick = maintenanceTickRate;
        nextFoodTick = 1f;
    }

    private void Update()
    {
        UpdateMaintenance();
        UpdateFoodConsumption();
        UpdatePOWs();
        UpdateBuildings();
        UpdateProduction();
    }

    private void UpdateMaintenance()
    {
        nextMaintenanceTick -= Time.deltaTime;
        if (nextMaintenanceTick <= 0)
        {
            ApplyMaintenance();
            nextMaintenanceTick = maintenanceTickRate;
        }
    }

    private void ApplyMaintenance()
    {
        float totalMaintenance = 0;
        foreach (Building building in allBuildings)
        {
            totalMaintenance += building.GetMaintenanceCost();
        }
        ModifyBudget(-(int)totalMaintenance);
    }

    private void UpdateFoodConsumption()
    {
        nextFoodTick -= Time.deltaTime;
        if (nextFoodTick <= 0)
        {
            float foodNeeded = allPOWs.Count * foodConsumptionRate;
            float foodForPrisoners = 0;

            // Collect food from farms allocated to prisoners
            foreach (Building building in allBuildings)
            {
                if (building is Farm farm && farm.GetAllocation() == Farm.FoodAllocation.Prisoners)
                {
                    float availableFood = farm.GetFoodStored();
                    float takeAmount = Mathf.Min(availableFood, foodNeeded - foodForPrisoners);
                    farm.UseFood(takeAmount);
                    foodForPrisoners += takeAmount;
                }
            }

            // Feed POWs with available food
            if (foodForPrisoners > 0)
            {
                foreach (POW pow in allPOWs)
                {
                    pow.Feed(foodForPrisoners / allPOWs.Count);
                }
            }
            else if (allPOWs.Count > 0)
            {
                // POWs go hungry
                Debug.LogWarning("Not enough food for prisoners! Morale will suffer.");
            }

            // Collect food from farms allocated to war effort and send to war system
            float foodForWar = 0;
            foreach (Building building in allBuildings)
            {
                if (building is Farm farm && farm.GetAllocation() == Farm.FoodAllocation.WarEffort)
                {
                    float availableFood = farm.GetFoodStored();
                    farm.UseFood(availableFood);
                    foodForWar += availableFood;
                }
            }

            if (foodForWar > 0)
            {
                totalFoodForWarEffort += foodForWar;
                Debug.Log($"Food allocated to war effort: {foodForWar}");
            }

            nextFoodTick = 1f;
        }
    }

    private void UpdatePOWs()
    {
        // Remove dead POWs
        allPOWs.RemoveAll(pow => pow.GetState() == POW.POWState.Dead || pow == null);
    }

    private void UpdateBuildings()
    {
        foreach (Building building in allBuildings)
        {
            if (building != null)
            {
                building.UpdateBuilding(Time.deltaTime);
            }
        }
    }

    private void UpdateProduction()
    {
        // Collect and report production to war system
        AmmunitionFactory ammoFactory = FindBuildingOfType<AmmunitionFactory>();
        WeaponFactory weaponFactory = FindBuildingOfType<WeaponFactory>();

        float ammoProduced = ammoFactory != null ? ammoFactory.GetAmmunition() : 0;
        float weaponsProduced = weaponFactory != null ? weaponFactory.GetWeapons() : 0;

        // Continuously contribute to war effort (weapons, ammo, and food push war progress forward)
        if (ammoProduced > 0 || weaponsProduced > 0 || totalFoodForWarEffort > 0)
        {
            float foodContribution = totalFoodForWarEffort / 100f; // Convert to smaller unit
            WarSystem.Instance.ContributeToWarEffort((int)weaponsProduced, (int)ammoProduced, foodContribution);
            totalFoodForWarEffort = 0; // Reset after contribution
        }
    }

    public bool BuildStructure(Building buildingPrefab, int x, int y)
    {
        if (currentBudget >= buildingPrefab.GetBuildCost())
        {
            if (GridManager.Instance.TryPlaceBuilding(x, y, buildingPrefab))
            {
                buildingPrefab.Initialize(x, y);
                allBuildings.Add(buildingPrefab);
                ModifyBudget(-buildingPrefab.GetBuildCost());
                return true;
            }
        }
        return false;
    }

    public void AddPOW(POW pow)
    {
        allPOWs.Add(pow);
    }

    public void RemovePOW(POW pow)
    {
        allPOWs.Remove(pow);
    }

    public void ModifyBudget(int amount)
    {
        currentBudget += amount;
    }

    private T FindBuildingOfType<T>() where T : Building
    {
        foreach (Building building in allBuildings)
        {
            if (building is T typedBuilding)
            {
                return typedBuilding;
            }
        }
        return null;
    }

    // Getters
    public int GetBudget() => currentBudget;
    public List<POW> GetAllPOWs() => allPOWs;
    public List<Building> GetAllBuildings() => allBuildings;
}
