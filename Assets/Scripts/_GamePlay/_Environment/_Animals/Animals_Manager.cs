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
        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.AwakeUpdate, Spawn_Animal);
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        InGame_Manager.instance.time.Register(TimeUpdateBus.AwakeUpdate, Spawn_Animal);
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

        TileScrObj[] spawnTiles = animalToSpawn.spawnTiles;
        TileScrObj randSpawnTile = spawnTiles[UnityEngine.Random.Range(0, spawnTiles.Length)];

        GameObject animalPrefab = Instantiate(animalToSpawn.prefab, transform);

        Animal spawnedAnimal = animalPrefab.GetComponent<Animal>();
        _spawnedAnimals.Add(spawnedAnimal);

        InGame_Manager manager = InGame_Manager.instance;

        List<Tile> sortedTiles = manager.tilesController.Current_Tiles(randSpawnTile);
        sortedTiles.Remove(manager.player.movement.tileTracker.data.CurrentTile());

        Tile spawnTile = sortedTiles[UnityEngine.Random.Range(0, sortedTiles.Count)];
        if (spawnTile == null) return;

        spawnedAnimal.transform.position = spawnTile.transform.position;
        spawnedAnimal.movement.tileTracker.Set_Data(spawnTile);

        spawnedAnimal.Set_Data();
        spawnedAnimal.Set_Data(animalToSpawn);
    }
}
