using System;
using UnityEngine;

namespace DefaultNamespace
{
    public partial class TestComponent : MonoBehaviour
    {
        [GetComponent(GetComponentAttribute.TargetType.ChildExcludeSelf)]
        private BoxCollider2D boxCollider2D;

        private void Awake()
        {
            InitializeComponents();
        }
    }
}