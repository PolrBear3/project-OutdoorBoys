using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animals_Manager : MonoBehaviour
{
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
        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.AwakeUpdate, Spawn_Animal);
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        InGame_Manager.instance.time.Register(TimeUpdateBus.AwakeUpdate, Spawn_Animal);
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
        if (_spawnedAnimals.Count > 0) return; // decrease spawn rate according to current spawned amount

        AnimalScrObj animalToSpawn = Spawning_Animal();
        if (animalToSpawn == null) return;

        TileScrObj[] spawnTiles = animalToSpawn.spawnTiles;
        TileScrObj randSpawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Length)];

        GameObject animalPrefab = Instantiate(animalToSpawn.prefab, transform);

        Animal spawnedAnimal = animalPrefab.GetComponent<Animal>();
        _spawnedAnimals.Add(spawnedAnimal);

        InGame_Manager manager = InGame_Manager.instance;

        List<Tile> sortedTiles = manager.tilesController.Current_Tiles(randSpawnTile);
        sortedTiles.Remove(manager.player.movement.currentTile);

        Tile spawnTile = sortedTiles[UnityEngine.Random.Range(0, sortedTiles.Count)];

        Movement_Controller animalMovement = spawnedAnimal.movement;

        animalMovement.Update_MoveDurationValue(0);
        animalMovement.Update_Offset(Vector2.zero);
        animalMovement.MoveTo_Tile(spawnTile);

        spawnedAnimal.Set_Data(animalToSpawn);
        spawnedAnimal.Set_Data();
        spawnedAnimal.Update_Animation();
    }
}
