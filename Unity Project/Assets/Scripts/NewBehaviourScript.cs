using System;
using System.Linq;
using AutoProperty;
using rStarUtility;
using UnityEngine;

public partial class NewBehaviourScript : MonoBehaviour
{
    [AutoProp(AXS.PublicGetPrivateSet) , SerializeField]
    private float _hogehoge;

    // [Required]
    [AutoProp]
    [SerializeField]
    [GetComponent(GetComponentAttribute.TargetType.ChildExcludeSelf )]
    private BoxCollider2D boxCollider2D;

    [AutoProp , SerializeField]
    [GetComponent(GetComponentAttribute.TargetType.Child)]
    private Rigidbody2D rigidbody2D;

    private int _intValue;

    [SerializeField]
    // [GetComponent(GetComponentAttribute.TargetType.Child)]
    private Rigidbody rb;

    public Rigidbody RB
    {
        get
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // var children = this.GetComponentInChildren<BoxCollider2D>(Self.Exclude );
        // Debug.Log($"{children}");
        InitializeComponents();
        // Assert.IsNotNull(RB , $"RB is null in {gameObject}");
    }
}