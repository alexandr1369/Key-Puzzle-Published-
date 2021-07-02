using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Outline))]
public class Key : MonoBehaviour
{
    [Header("Settings")] 
    public ColorType colorType = ColorType.Blue;

    [Header("Others")]
    [SerializeField] private Rigidbody rb;

    [Header("Colored Parts")]
    [SerializeField] private List<MeshRenderer> parts; 
    public bool IsFree { get; set; }
    public Vector3 StartPosition { get; set; }
    public Vector3 StartRotation { get; set; }

    private Outline outline;

    private void Start()
    {
        outline = GetComponent<Outline>();

        StartPosition = transform.position;
        StartRotation = transform.rotation.eulerAngles;
    } 
    private void Update()
    {
#if UNITY_EDITOR
        //if (keyType != KeyType.None)
        //{
        //    Mesh sharedMesh;
        //    string _path = "Models/Keys/";
        //    switch (keyType)
        //    {
        //        case KeyType.One: _path += "Key1"; break;
        //        case KeyType.Two: _path += "Key2"; break;
        //        case KeyType.Three: _path += "Key3"; break;
        //    }
        //    sharedMesh = Resources.Load<Mesh>(_path);
        //    GetComponent<MeshFilter>().sharedMesh = sharedMesh;
        //}
        if (parts != null)
        {
            Material material = Resources.Load<Material>("Materials/Key Materials/" + colorType.ToString() + " Color Material");
            parts.ForEach(t => t.material = material);
            name = colorType.ToString() + " Key";
        }
#endif
    }

    public void ToggleOutline(bool state)
    {
        outline.OutlineWidth = !state ? 0 : 2f;
    }
    public void ToggleKinematic(bool state)
    {
        rb.isKinematic = state || IsFree;
        foreach(BoxCollider collider in GetComponentsInChildren(typeof(BoxCollider))){
            collider.isTrigger = state;
        }
    }
}
