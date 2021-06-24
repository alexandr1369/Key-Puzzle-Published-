using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class Key : MonoBehaviour
{
    [Header("Settings")] 
    public ColorType colorType = ColorType.Blue;
    //public bool isInHole;

    [Header("Others")]
    [SerializeField] private Rigidbody rb;

    [Header("Colored Parts")]
    [SerializeField] private List<MeshRenderer> parts; 
    public bool IsFree { get; set; }
    public Vector3 StartPosition { get; set; }
    public Vector3 StartRotation { get; set; }

    private void Start()
    {
        StartPosition = transform.position;
        StartRotation = transform.rotation.eulerAngles;
    } 
    private void Update()
    {
#if UNITY_EDITOR
        //ToggleKinematic(isInHole);
        if (parts != null)
        {
            Material material = Resources.Load<Material>("Materials/" + colorType.ToString() + " Color Material");
            parts.ForEach(t => t.material = material);
        }
#endif
    }

    public void ToggleKinematic(bool state)
    {
        rb.isKinematic = state || IsFree;
        foreach(BoxCollider collider in GetComponentsInChildren(typeof(BoxCollider))){
            collider.isTrigger = state;
        }
    }
}
