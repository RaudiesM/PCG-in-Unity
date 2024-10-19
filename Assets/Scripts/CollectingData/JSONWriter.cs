using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONWriter : MonoBehaviour
{
    [SerializeField]private string cellulaAutomata_path = "/ca_";
    [SerializeField]private string randomWalker_path = "/rw_";
    [SerializeField]private string binarySpacepartitioning_path = "/bsp_";
    [SerializeField] private string bspca_path = "/bspca_";
    [SerializeField] private string rwca_path = "/rwca_";

    private int numIterationsRW = 0;
    private int numIterationsCA = 0;
    private int numIterationsBSP = 0;
    private int numIterationsBSPCA = 0;
    private int numIterationsRWCA = 0;

    private string filePath;

    private void Awake()
    {
        filePath = Application.persistentDataPath;
        GetIterationNumbers();
    }
    public void WriteJsonFile(string data, AlgorithmType type)
    {
        string newfilePath = GetFilepath(type);
        Debug.Log(newfilePath);
        System.IO.File.WriteAllText(newfilePath, data);
        SaveIterationNumbers();
    }
    
    private string GetFilepath(AlgorithmType algorithmType)
    {
        string pathAddition = "/data";
        switch(algorithmType)
        {
            case AlgorithmType.RandomWalker:
                pathAddition = "/results_rw"+ randomWalker_path + numIterationsRW.ToString("0000");
                numIterationsRW++;
                break;

            case AlgorithmType.CellulaAutomata: 
                pathAddition = "/results_ca"+ cellulaAutomata_path + numIterationsCA.ToString("0000");
                numIterationsCA++;
                break;

            case AlgorithmType.BinarySpacepartitioning:
                pathAddition = "/results_bsp" + binarySpacepartitioning_path + numIterationsBSP.ToString("0000");
                numIterationsBSP++;
                break;

            case AlgorithmType.BSP_CA:
                pathAddition = "/results_bspca" + bspca_path + numIterationsBSPCA.ToString("0000");
                numIterationsBSPCA++;
                break;

            case AlgorithmType.RW_CA:
                pathAddition = "/results_rwca" + rwca_path + numIterationsRWCA.ToString("0000");
                numIterationsRWCA++;
                break;
        }
        return filePath + pathAddition + ".json";
    }
    private void SaveIterationNumbers()
    {
        string path = filePath + "/JSONWriter_Setup.json";
        string input = numIterationsRW +";"+numIterationsCA+";"+numIterationsBSP+";"+numIterationsBSPCA+";"+numIterationsRWCA;
        System.IO.File.WriteAllText(path, input);
    }
    private void GetIterationNumbers()
    {
        string path = filePath + "/JSONWriter_Setup.json";
        string input = System.IO.File.ReadAllText(path);
        string[] numIterations = input.Split(";");
        for (int i = 0; i < numIterations.Length; i++)
        {
            switch (i) 
            {
                case 0:
                    numIterationsRW = Int32.Parse(numIterations[i]);
                    break;
                case 1:
                    numIterationsCA = Int32.Parse(numIterations[i]);
                    break;
                case 2:
                    numIterationsBSP = Int32.Parse(numIterations[i]);
                    break;
                case 3:
                    numIterationsBSPCA= Int32.Parse(numIterations[i]);
                    break;
                case 4:
                    numIterationsRWCA= Int32.Parse(numIterations[i]); 
                    break;
            }
        }
    }

}
