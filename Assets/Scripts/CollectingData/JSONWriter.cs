using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONWriter : MonoBehaviour
{
    [SerializeField] private string cellulaAutomata_path = "/ca";
    [SerializeField] private string randomWalker_path = "/rw";
    [SerializeField] private string binarySpacepartitioning_path = "/bsp";
    private int numIterationsCA = 0;
    private int numIterationsRW = 0;
    private int numIterationsBSP = 0;
    private string filePath;
    private void Awake()
    {
        filePath = Application.persistentDataPath;
    }
    public void WriteJsonFile(string data, AlgorithmType type)
    {
        filePath = GetFilepath(type);
        Debug.Log(filePath);
        string input = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(filePath, input);
    }

    private string GetFilepath(AlgorithmType algorithmType)
    {
        string pathAddition = "/data";
        switch(algorithmType)
        {
            case AlgorithmType.RandomWalker:
                pathAddition = randomWalker_path + numIterationsRW.ToString();
                numIterationsRW++;
                break;
            case AlgorithmType.CellulaAutomata: 
                pathAddition = cellulaAutomata_path + numIterationsCA.ToString();
                numIterationsCA++;
                break;
            case AlgorithmType.BinarySpacepartitioning:
                pathAddition = binarySpacepartitioning_path + numIterationsBSP.ToString();
                numIterationsBSP++;
                break;
        }
        return filePath + pathAddition + ".json";
    }
}
