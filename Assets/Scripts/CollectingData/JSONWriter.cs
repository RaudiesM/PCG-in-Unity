using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONWriter : MonoBehaviour
{
    private string cellulaAutomata_path = "/ca_";
    private string randomWalker_path = "/rw_";
    private string binarySpacepartitioning_path = "/bsp_";
    
    private int numIterationsRW = 0;
    private int numIterationsCA = 0;
    private int numIterationsBSP = 0;

    private string filePath;
    private AlgorithmType lastAlgorithm;
    private void Awake()
    {
        filePath = Application.persistentDataPath;
        SetupIterationNumbers();
        numIterationsRW = 0;
        numIterationsCA = 0;
        numIterationsBSP = 0;
    }
    private void WriteIterationNumbers()
    {
        string path = filePath + "/JSONWriter_Setup.json";
        string input = numIterationsRW +";"+numIterationsCA+";"+numIterationsBSP;
        System.IO.File.WriteAllText(path, input);
    }

    private void SetupIterationNumbers()
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
            }
        }
        Debug.Log(input);
    }

    public void WriteJsonFile(string data, AlgorithmType type)
    {
        string newfilePath = GetFilepath(type);
        Debug.Log(newfilePath);
        System.IO.File.WriteAllText(newfilePath, data);
        WriteIterationNumbers();
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
        }
        return filePath + pathAddition + ".json";
    }
}
