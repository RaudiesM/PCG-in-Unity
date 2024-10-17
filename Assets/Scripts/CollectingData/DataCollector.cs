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
    private DungeonAlgorithmBase currentAlgorithm;
    private JSONWriter jsonWriter;

    private EvaluationBase data;

    [SerializeField]private float[] timeList = new float[1000];
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
            timeList = new float[numIterations];

            for (int j = 0; j < numIterations; j++)
            {
                float timeBefore = Time.realtimeSinceStartup;
                currentAlgorithm.GenerateDungeonTiles();
                float timeAfter = Time.realtimeSinceStartup;
                float generationDuration = timeAfter - timeBefore;
                timeList[j] = generationDuration;
            }
            data.SetUpTimetable(timeList);
            SaveData(data);
        }
    }

    public void SetupCollection()
    {
        data = null;
        currentAlgorithm = GetAlgorithm();
        if(currentAlgorithm == null)
        {
            throw new Exception("Algorithm is null");
        }
        data = GetAlgorithmInformation();
    }

    private DungeonAlgorithmBase GetAlgorithm()
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

    private EvaluationBase GetAlgorithmInformation()
    {
        return currentAlgorithm.GetAlgorithmData();
    }

    private void SaveData(object data)
    {
        string dataAsString = JsonUtility.ToJson(data, true);
        jsonWriter.WriteJsonFile(dataAsString, algorithmType);
    }
}
