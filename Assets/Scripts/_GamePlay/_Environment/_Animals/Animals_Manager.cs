using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animals_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimalScrObj[] _spawnAnimals;

    private List<Animal> _spawnedAnimals = new();
    public List<Animal> spawnedAnimals => _spawnedAnimals;

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _maxSpawnCount;
    [SerializeField][Range(0, 10)] private int _spawnCoolTime;

    private int _currentSpawnCoolTime;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Data);
        InGame_Manager.instance.time.UnRegister(ActionUpdateBus.AwakeUpdate, Spawn_Animal);
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        InGame_Manager.instance.time.Register(ActionUpdateBus.AwakeUpdate, Spawn_Animal);
    }

    public List<Animal> SpwnedAnimals(Tile searchTile)
    {
        if (searchTile == null) return null;

        List<Animal> animalsOntile = new();

        for (int i = 0; i < _spawnedAnimals.Count; i++)
        {
            Animal spawnedAnimal = _spawnedAnimals[i];
            animalsOntile.Add(spawnedAnimal);
        }
        return animalsOntile;
    }


    // Spawn
    private Tile AnimalSpawn_Tile(AnimalScrObj spawnAnimal)
    {
        TileScrObj[] spawnTiles = spawnAnimal.spawnTiles;
        TileScrObj randSpawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Length)];

        InGame_Manager manager = InGame_Manager.instance;

        List<Tile> sortedTiles = new(manager.tilesController.Current_Tiles(randSpawnTile));
        Tile playerTile = manager.player.movement.tileTracker.data.CurrentTile();

        for (int i = sortedTiles.Count - 1; i >= 0; i--)
        {
            Tile sortedTile = sortedTiles[i];
            if (sortedTile == playerTile)
            {
                sortedTiles.RemoveAt(i);
                continue;
            }

            List<GameObject> prefabs = sortedTile.All_CurrentPrefabs();
            for (int j = 0; j < prefabs.Count; j++)
            {
                if (prefabs[j].TryGetComponent(out Animal animal) == false) continue;
                if (animal.data.isOnSight) continue;

                sortedTiles.RemoveAt(i);
                break;
            }
        }

        if (sortedTiles.Count <= 0) return null;
        return sortedTiles[UnityEngine.Random.Range(0, sortedTiles.Count)];
    }

    private void Spawn_Animal()
    {
        if (_spawnedAnimals.Count >= _maxSpawnCount) return;

        if (_currentSpawnCoolTime < _spawnCoolTime)
        {
            _currentSpawnCoolTime++;
            return;
        }
        _currentSpawnCoolTime = 0;

        AnimalScrObj animalToSpawn = _spawnAnimals[UnityEngine.Random.Range(0, _spawnAnimals.Length)];
        if (animalToSpawn == null) return;

        Tile spawnTile = AnimalSpawn_Tile(animalToSpawn);
        if (spawnTile == null) return;

        GameObject animalPrefab = Instantiate(animalToSpawn.prefab);

        Animal spawnedAnimal = animalPrefab.GetComponent<Animal>();
        _spawnedAnimals.Add(spawnedAnimal);

        spawnedAnimal.transform.position = spawnTile.transform.position;
        spawnedAnimal.movement.tileTracker.Set_Data(spawnTile);

        spawnedAnimal.Set_Data();
        spawnedAnimal.Set_Data(animalToSpawn);
    }
}
