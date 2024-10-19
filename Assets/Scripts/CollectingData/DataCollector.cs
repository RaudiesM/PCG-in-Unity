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
    private CellulaAutomataAlgorithm_old ca_algorithm_old;
    private RandomWalker_Algorithm rw_algorithm;
    private DungeonAlgorithmBase currentAlgorithm;
    private JSONWriter jsonWriter;

    private EvaluationBase data;

    private float[] timeList = new float[1000];
    private int[] generatedTiles = new int[1000];

    public static Action OnFinished;

    private void Awake()
    {
        bsp_algorithm = FindObjectOfType<BinarySpacePartitioning_Algorithm>();
        ca_algorithm = FindObjectOfType<CellulaAutomataAlgorithm>();
        ca_algorithm_old = FindObjectOfType<CellulaAutomataAlgorithm_old>();
        rw_algorithm = FindObjectOfType<RandomWalker_Algorithm>();
        jsonWriter = FindObjectOfType<JSONWriter>();
        InputManager.OnIterate += CollectData;
    }
    private void CollectData()
    {
        if(algorithmType == AlgorithmType.RW_CA)
        {
            CollectHybridData_RWCA();
            return;
        }else if(algorithmType == AlgorithmType.BSP_CA)
        {
            CollectHybridData_BSPCA(); 
            return;
        }
        SetupCollection();
        DungeonTiles tiles = new DungeonTiles();
        for (int i = 0; i < numFiles; i++)
        {
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];
            for (int j = 0; j < numIterations; j++)
            {
                float timeBefore = Time.realtimeSinceStartup;
                tiles = currentAlgorithm.GenerateDungeonTiles();
                float timeAfter = Time.realtimeSinceStartup;
                int numTiles = tiles.Count();
                float generationDuration = timeAfter - timeBefore;
                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            data.SetUpTimetable(timeList);
            data.SetUpFilledTiles(generatedTiles);
            SaveData(data);
        }
        OnFinished?.Invoke();
    }

    private void CollectHybridData_BSPCA()
    {
        EvaluationBase bspData = GetAlgorithmInformation(bsp_algorithm);
        EvaluationBase caData = GetAlgorithmInformation(ca_algorithm);
        EvaluationBase completeData = new BSPCA_Evaluation((CA_Evaluation)caData, (BSP_Evaluation)bspData);

        DungeonTiles dungeonTiles = new DungeonTiles(algorithmType);
        HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
        HashSet<Vector3Int> centers = new HashSet<Vector3Int>();

        for (int i = 0; i < numFiles; i++)
        {
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];
            for (int j = 0; j < numIterations; j++)
            {
                float timeBefore = Time.realtimeSinceStartup;
                dungeonTiles = bsp_algorithm.GenerateDungeonTiles(out rooms, out centers);
                dungeonTiles = ca_algorithm_old.GenerateDungeonTiles(dungeonTiles, rooms, centers);
                int numTiles = dungeonTiles.Count();
                float timeAfter = Time.realtimeSinceStartup;
                float generationDuration = timeAfter - timeBefore;
                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            completeData.SetUpTimetable(timeList);
            completeData.SetUpFilledTiles(generatedTiles);
            SaveData(completeData);
        }

        OnFinished?.Invoke();
    }

    private void CollectHybridData_RWCA()
    {
        EvaluationBase rwData = GetAlgorithmInformation(rw_algorithm);
        EvaluationBase caData = GetAlgorithmInformation(ca_algorithm);
        EvaluationBase completeData = new RWCA_Evaluation((CA_Evaluation)caData, (RW_Evaluation)rwData);

        DungeonTiles dungeonTiles = new DungeonTiles(algorithmType);

        for (int i = 0; i < numFiles; i++)
        {
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];

            for (int j = 0; j < numIterations; j++)
            {
                float timeBefore = Time.realtimeSinceStartup;
                dungeonTiles = rw_algorithm.GenerateDungeonTiles();
                dungeonTiles = ca_algorithm.GenerateDungeonTiles(dungeonTiles);
                int numTiles = dungeonTiles.Count();
                float timeAfter = Time.realtimeSinceStartup;
                float generationDuration = timeAfter - timeBefore;
                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            completeData.SetUpTimetable(timeList);
            completeData.SetUpFilledTiles(generatedTiles);
            SaveData(completeData);
        }
        OnFinished?.Invoke();
    }

    public void SetupCollection()
    {
        data = null;
        currentAlgorithm = GetAlgorithm(algorithmType);
        if(currentAlgorithm == null)
        {
            throw new Exception("Algorithm is null");
        }
        data = GetAlgorithmInformation(currentAlgorithm);
    }

    private DungeonAlgorithmBase GetAlgorithm(AlgorithmType thisAlgorithmType)
    {
        if (thisAlgorithmType == AlgorithmType.RandomWalker)
        {
            return currentAlgorithm = rw_algorithm;
        }
        else if (thisAlgorithmType == AlgorithmType.CellulaAutomata)
        {

            return currentAlgorithm = ca_algorithm;
        }
        else if (thisAlgorithmType == AlgorithmType.BinarySpacepartitioning)
        {
            return currentAlgorithm = bsp_algorithm;
        }
        return null;
    }

    private EvaluationBase GetAlgorithmInformation(DungeonAlgorithmBase thisAlgorithm)
    {
        return thisAlgorithm.GetAlgorithmData();
    }

    private void SaveData(object data)
    {
        string dataAsString = JsonUtility.ToJson(data, true);
        jsonWriter.WriteJsonFile(dataAsString, algorithmType);
    }
}
