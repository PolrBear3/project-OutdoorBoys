using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Animals_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _trailMarkRenderer;


    private List<Animal> _spawnedAnimals = new();
    public List<Animal> spawnedAnimals => _spawnedAnimals;

    
    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Data);

        InGame_Manager manager = InGame_Manager.instance;
        Movement_Controller playerMovement = manager.player.movement;
    }


    // Data
    private void Set_Data()
    {
        Spawn_Animal();
    }


    // Animals
    private AnimalScrObj Spawning_Animal()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        
        AnimalScrObj[] allAnimals = Data_Manager.instance.allAnimals;

        Dictionary<AnimalScrObj, int> spawnWeightDatas = new();
        int totalWeight = 0;

        for (int i = 0; i < allAnimals.Length; i++)
        {
            TileScrObj[] spawnTiles = allAnimals[i].spawnTiles;
            int spawnTileCount = 0;

            foreach (TileScrObj tile in spawnTiles)
            {
                spawnTileCount += tilesController.Tile_Count(tile);
            }

            spawnWeightDatas.Add(allAnimals[i], spawnTileCount);
            totalWeight += spawnTileCount;
        }

        int randValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var data in spawnWeightDatas)
        {
            cumulativeWeight += data.Value;

            if (randValue >= cumulativeWeight) continue;
            return data.Key;
        }
        return allAnimals[UnityEngine.Random.Range(0, allAnimals.Length)];   
    }

    private void Spawn_Animal()
    {
        // decrease spawn according to current spawned amount
        if (_spawnedAnimals.Count > 0) return;

        AnimalScrObj animalToSpawn = Spawning_Animal();
        if (animalToSpawn == null) return;

        GameObject animalPrefab = Instantiate(animalToSpawn.prefab, transform);
        Animal spawnedAnimal = animalPrefab.GetComponent<Animal>();

        _spawnedAnimals.Add(spawnedAnimal);
        spawnedAnimal.Set_Data(animalToSpawn);

        // set remaining trail track count

        // set tile
    }
}
