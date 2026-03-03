using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public interface ISaveLoadable
{
    void Save_Data();
    void Load_Data();
}
 
public class SaveLoad_Controller : MonoBehaviour
{
    public static SaveLoad_Controller instance;
    
    
    // UnityEngine
    private void Awake()
    {
        instance = this;
        
        LoadAll_ISaveLoadable();
    }

    private void OnApplicationQuit()
    {
        SaveAll_ISaveLoadable();
    }


    // Main
    private static List<ISaveLoadable> All_ISaveLoadables()
    {
        IEnumerable<ISaveLoadable> saveLoadableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadable>();
        return new(saveLoadableObjects);
    }


    public void SaveAll_ISaveLoadable()
    {
        ES3.Save(SaveKeys.SaveVersionKey, SaveKeys.SaveVersionNumber);
        
        List<ISaveLoadable> allSaveLoadables = All_ISaveLoadables();
        for (int i = 0; i < allSaveLoadables.Count; i++)
        {
            allSaveLoadables[i].Save_Data();
        }
    }
    
    private void LoadAll_ISaveLoadable()
    {
        MigrateIfNeeded(ES3.Load(SaveKeys.SaveVersionKey, 0));
        
        List<ISaveLoadable> allSaveLoadables = All_ISaveLoadables();
        for (int i = 0; i < allSaveLoadables.Count; i++)
        {
            allSaveLoadables[i].Load_Data();
        }
    }


    private void MigrateIfNeeded(int v)
    {
        if (v < 1)
        {
            ES3.Save(SaveKeys.SaveVersionKey, 1);
            v = 1;
        }

        // 1. bump up SaveVersionNumber from SaveKeys

        // 2. add this block
        /*
        if (v < 2)
        {
            3. load and save old keys here
            
            ES3.Save(SaveKeys.SaveVersionKey, 2);
            v = 2;
        }
        */
    }
}