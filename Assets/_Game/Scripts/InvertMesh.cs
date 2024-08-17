using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InvertMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        this.GetComponent<MeshFilter>().mesh.triangles = mesh.triangles.Reverse().ToArray();
        Debug.Log("invert mesh");
    }
}
