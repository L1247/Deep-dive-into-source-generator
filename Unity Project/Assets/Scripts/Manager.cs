using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Manager: MonoBehaviour
    {
        private void Awake()
        {
            NewBehaviourScript.Instance.Test();
        }
    }
}