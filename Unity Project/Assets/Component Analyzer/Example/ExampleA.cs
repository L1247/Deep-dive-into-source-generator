using UnityEngine;

public class ExampleA : MonoBehaviour
{
    private Rigidbody rigidbody;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    private void Awake()
    {
        rigidbody.velocity   = Vector3.zero;
        meshRenderer.enabled = false;
        boxCollider.size     = Vector3.zero;
    }
}