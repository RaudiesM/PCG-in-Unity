using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    [SerializeField] private BinarySpacePartitioning_Algorithm bsp_algorithm;
    [SerializeField] private CellulaAutomataAlgorithm ca_algorithm;
    [SerializeField] private RandomWalker_Algorithm rw_algorithm;
    [SerializeField] private JSONWriter jsonWriter;

    [SerializeField] private int numIterations = 10000;
}
