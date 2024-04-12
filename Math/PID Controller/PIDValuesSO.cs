using UnityEngine;

namespace Sleep0
{
    [CreateAssetMenu(fileName = "PIDValues", menuName = "ScriptableObjects/PIDValues", order = 1)]
    public class PIDValuesSO : ScriptableObject
    {
        public float PGain; // Proportional gain
        public float IGain; // Integral gain
        public float DGain; // Derivative gain
        public float OscillationTime;
    }
}