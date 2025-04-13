using UnityEngine;

[CreateAssetMenu(fileName ="SimpleRandomWalkRarameters_", menuName ="PCG/SimpleRandomWalkData")]
public class SimpleRandomWalkSO : ScriptableObject
{
    public int iterations = 10 , walkLength = 10;
    public bool startRandomlyEachIteration = true;
}
