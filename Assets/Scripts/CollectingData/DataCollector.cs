using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    [SerializeField] private AlgorithmType algorithmType;
    [SerializeField] private int numFiles = 10;
    [SerializeField] private int numIterations = 1000;

    private BinarySpacePartitioning_Algorithm bsp_algorithm;
    private CellulaAutomataAlgorithm ca_algorithm;
    private RandomWalker_Algorithm rw_algorithm;
    private DungeonAlgorithm currentAlgorithm;
    private JSONWriter jsonWriter;

    private string data;
    List<float> timeList = new List<float>();

    private void Awake()
    {
        bsp_algorithm = FindObjectOfType<BinarySpacePartitioning_Algorithm>();
        ca_algorithm = FindObjectOfType<CellulaAutomataAlgorithm>();
        rw_algorithm = FindObjectOfType<RandomWalker_Algorithm>();
        jsonWriter = FindObjectOfType<JSONWriter>();
        InputManager.OnIterate += CollectData;
    }
    private void CollectData()
    {
        SetupCollection();
        for (int i = 0; i < numFiles; i++)
        {
            timeList.Clear();

            for (int j = 0; j < numIterations; j++)
            {
                float timeBefore = Time.realtimeSinceStartup;
                currentAlgorithm.GenerateDungeonTiles();
                float timeAfter = Time.realtimeSinceStartup;
                float generationDuration = timeAfter - timeBefore;
                timeList.Add(generationDuration);
            }

            data += "\n" + JsonUtility.ToJson(timeList);
            SaveData(data);
        }
    }

    public void SetupCollection()
    {
        data = string.Empty;
        currentAlgorithm = GetAlgorithm();
        if(currentAlgorithm == null)
        {
            throw new Exception("Algorithm is null");
        }
        data = GetAlgorithmInformation();
    }

    private DungeonAlgorithm GetAlgorithm()
    {
        if (algorithmType == AlgorithmType.RandomWalker)
        {
            return currentAlgorithm = rw_algorithm;
        }
        else if (algorithmType == AlgorithmType.CellulaAutomata)
        {

            return currentAlgorithm = ca_algorithm;
        }
        else if (algorithmType == AlgorithmType.BinarySpacepartitioning)
        {
            return currentAlgorithm = bsp_algorithm;
        }
        return null;
    }

    private string GetAlgorithmInformation()
    {
        return currentAlgorithm.GetAlgorithmData();
    }

    private void SaveData(string data)
    {
        jsonWriter.WriteJsonFile(data, algorithmType);
    }
}
