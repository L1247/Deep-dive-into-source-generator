#region

using UnityEngine;

#endregion

namespace MyNamespace
{
    [Singleton]
    public partial class NewBehaviourScript : MonoBehaviour
    {
    #region Public Variables

        public Rigidbody RB
        {
            get
            {
                if (rb == null) rb = GetComponent<Rigidbody>();
                return rb;
            }
        }

    #endregion

    #region Private Variables

        private int _intValue;

        [AutoProp(AXS.PublicGetPrivateSet) , SerializeField]
        private float _hogehoge;

        // [Required]
        [AutoProp]
        [SerializeField]
        [GetComponent(GetComponentAttribute.TargetType.ChildExcludeSelf)]
        private BoxCollider2D boxCollider2D;

        [AutoProp , SerializeField]
        [GetComponent(GetComponentAttribute.TargetType.Child)]
        private Rigidbody2D rigidbody2D;

        [SerializeField]
        // [GetComponent(GetComponentAttribute.TargetType.Child)]
        private Rigidbody rb;

    #endregion

    #region Unity events

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            // var children = this.GetComponentInChildren<BoxCollider2D>(Self.Exclude );
            // Debug.Log($"{children}");
            // Hogehoge
            InitializeComponents();
            // Assert.IsNotNull(RB , $"RB is null in {gameObject}");
        }

    #endregion

    #region Public Methods

        public void Test()
        {
            Debug.Log($"Test");
        }

    #endregion
    }
}