using MyNamespace;
using UnityEngine;

namespace DefulatNameSpace
{
    public class Manager : MonoBehaviour
    {
        private void Awake()
        {
            GameScore.Instance.AddScore(10);
            // NewBehaviourScript.Instance.Test();
        }
    }
}