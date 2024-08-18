using System.Linq;
using UnityEngine;

namespace _Game.Scripts {
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
}
