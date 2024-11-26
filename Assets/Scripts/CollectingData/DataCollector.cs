using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public static Action OnFinished;

    [SerializeField] private AlgorithmType algorithmType;
    [SerializeField] private int numFiles = 10;
    [SerializeField] private int numIterations = 1000;

    private BinarySpacePartitioning_Algorithm bsp_algorithm;
    private CellulaAutomataAlgorithm ca_algorithm;
    private CellulaAutomataAlgorithm_alt ca_algorithm_alt;
    private RandomWalker_Algorithm rw_algorithm;
    private DungeonAlgorithmBase currentAlgorithm;

    private JSONWriter jsonWriter;

    private EvaluationBase data;

    private float[] timeList = new float[1000];
    private int[] generatedTiles = new int[1000];


    private void Awake()
    {
        SetupCollecter();
        PrintInformation();
    }

    #region Setups
    private void SetupCollecter()
    {
        //set classes
        bsp_algorithm = FindObjectOfType<BinarySpacePartitioning_Algorithm>();
        ca_algorithm = FindObjectOfType<CellulaAutomataAlgorithm>();
        ca_algorithm_alt = FindObjectOfType<CellulaAutomataAlgorithm_alt>();
        rw_algorithm = FindObjectOfType<RandomWalker_Algorithm>();

        jsonWriter = FindObjectOfType<JSONWriter>();

        InputManager.OnSpawnDungeon += CollectData;
    }
    private void SetupCollection()
    {
        //prepare Algorithm and data
        data = null;
        currentAlgorithm = GetAlgorithm(algorithmType);
        if(currentAlgorithm == null)
        {
            throw new Exception("Algorithm is null");
        }
        data = GetAlgorithmInformation(currentAlgorithm);
    }
    #endregion
    #region collectData
    private void CollectData()
    {
        //switch to special functions if using hybrid algorithm
        if(algorithmType == AlgorithmType.RW_CA)
        {
            CollectHybridData_RWCA();
            return;
        }else if(algorithmType == AlgorithmType.BSP_CA)
        {
            Debug.Log("BSPCA");
            CollectHybridData_BSPCA();
            return;
        }

        SetupCollection();
        DungeonTiles tiles = new DungeonTiles();
        for (int i = 0; i < numFiles; i++)
        {
            //write files
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];
            for (int j = 0; j < numIterations; j++)
            {
                //collect iterations
                float timeBefore = Time.realtimeSinceStartup;
                tiles = currentAlgorithm.GenerateDungeonTiles();
                float timeAfter = Time.realtimeSinceStartup;
                //get amount of generated tiles and duration of this process
                int numTiles = tiles.Count();
                Debug.Log(numTiles);
                float generationDuration = timeAfter - timeBefore;

                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            //save data
            data.SetUpTimetable(timeList);
            data.SetUpFilledTiles(generatedTiles);
            SaveData(data);
        }
        //inform visualizer to play sound
        OnFinished?.Invoke();
    }

    private void CollectHybridData_BSPCA()
    {
        EvaluationBase bspData = GetAlgorithmInformation(bsp_algorithm);
        EvaluationBase caData = GetAlgorithmInformation(ca_algorithm_alt);
        //combine bsp & ca data
        EvaluationBase completeData = new BSPCA_Evaluation((CA_Evaluation)caData, (BSP_Evaluation)bspData);

        DungeonTiles dungeonTiles = new DungeonTiles(algorithmType);
        HashSet<BoundsInt> rooms = new HashSet<BoundsInt>();
        HashSet<Vector3Int> centers = new HashSet<Vector3Int>();

        for (int i = 0; i < numFiles; i++)
        {
            //write file
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];
            for (int j = 0; j < numIterations; j++)
            {
                //collect iterations
                float timeBefore = Time.realtimeSinceStartup;
                dungeonTiles = bsp_algorithm.Hybrid_GenerateDungeonTiles(out rooms, out centers);
                dungeonTiles = ca_algorithm_alt.Hybrid_GenerateDungeonTiles(dungeonTiles, rooms, centers);
                float timeAfter = Time.realtimeSinceStartup;
                //get amount of generated tiles and duration of this process
                float generationDuration = timeAfter - timeBefore;
                int numTiles = dungeonTiles.Count();

                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            //save data
            completeData.SetUpTimetable(timeList);
            completeData.SetUpFilledTiles(generatedTiles);
            SaveData(completeData);
        }
        //inform visualizer to play sound
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
            //write file
            timeList = new float[numIterations];
            generatedTiles = new int[numIterations];

            for (int j = 0; j < numIterations; j++)
            {
                //collect iterations
                float timeBefore = Time.realtimeSinceStartup;
                dungeonTiles = rw_algorithm.GenerateDungeonTiles();
                dungeonTiles = ca_algorithm.Hybrid_GenerateDungeonTiles(dungeonTiles);
                float timeAfter = Time.realtimeSinceStartup;
                //get amount of generated tiles and duration of this process
                float generationDuration = timeAfter - timeBefore;
                int numTiles = dungeonTiles.Count();

                timeList[j] = generationDuration;
                generatedTiles[j] = numTiles;
            }
            //save data
            completeData.SetUpTimetable(timeList);
            completeData.SetUpFilledTiles(generatedTiles);
            SaveData(completeData);
        }
        //inform visualizer to play sound
        OnFinished?.Invoke();
    }
    #endregion
    #region Getter
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
        else if (thisAlgorithmType == AlgorithmType.BinarySpacePartitioning)
        {
            return currentAlgorithm = bsp_algorithm;
        }
        return null;
    }
    private EvaluationBase GetAlgorithmInformation(DungeonAlgorithmBase thisAlgorithm)
    {
        return thisAlgorithm.GetAlgorithmData();
    }
    #endregion
    private void SaveData(object data)
    {
        //Converts class Evaluation into string and saves it
        string dataAsString = JsonUtility.ToJson(data, true);
        jsonWriter.WriteJsonFile(dataAsString, algorithmType);
    }
    private void PrintInformation()
    {
        Debug.Log("Press [D] to start collecting data");
    }
}
