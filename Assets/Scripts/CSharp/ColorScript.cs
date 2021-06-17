using UnityEngine;

namespace CSharp
{
    public class ColorScript : MonoBehaviour
    {
        public Color[] all_colors;
        private Mesh mesh;

        void Start()
        {
            if (all_colors == null || all_colors.Length <= 0)
                all_colors = new Color[] { Color.red };
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) return;
            mesh = MakeCube(1.0f);
            mf.mesh = mesh;
            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc != null)
                mc.sharedMesh = mesh;
        }

        public void ChangeColor(int iTriangle)
        {
            Color colorT = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            Color[] colors = mesh.colors;
            int iStart = mesh.triangles[iTriangle * 3];
            for (int i = iStart; i < iStart + 4; i++)
                colors[i] = colorT;
            mesh.colors = colors;
        }

        Mesh MakeCube(float fSize)
        {
            float fHS = (fSize / 2.0f) * 0.97f;
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] {
                new Vector3(-fHS,  fHS,  fHS), new Vector3( fHS,  fHS,  fHS), new Vector3( fHS,  fHS, -fHS), new Vector3(-fHS,  fHS, -fHS),  // Top
                new Vector3(-fHS, -fHS, -fHS), new Vector3( fHS, -fHS, -fHS), new Vector3( fHS, -fHS,  fHS), new Vector3(-fHS, -fHS,  fHS),  // Bottom
                new Vector3(-fHS,  fHS, -fHS), new Vector3( fHS,  fHS, -fHS), new Vector3( fHS, -fHS, -fHS), new Vector3(-fHS, -fHS, -fHS),  // Front
                new Vector3( fHS,  fHS, -fHS), new Vector3( fHS,  fHS,  fHS), new Vector3( fHS, -fHS,  fHS), new Vector3( fHS, -fHS, -fHS),  // right
                new Vector3(-fHS, -fHS,  fHS), new Vector3( fHS, -fHS,  fHS), new Vector3( fHS,  fHS,  fHS), new Vector3(-fHS,  fHS,  fHS),  // Back
                new Vector3(-fHS,  fHS,  fHS), new Vector3(-fHS,  fHS, -fHS), new Vector3(-fHS, -fHS, -fHS), new Vector3(-fHS, -fHS,  fHS),  // Left
                }; 

            int[] triangles = new int[mesh.vertices.Length / 4 * 2 * 3];
            int iPos = 0;
            for (int i = 0; i < mesh.vertices.Length; i = i + 4)
            {
                triangles[iPos++] = i;
                triangles[iPos++] = i + 1;
                triangles[iPos++] = i + 2;
                triangles[iPos++] = i;
                triangles[iPos++] = i + 2;
                triangles[iPos++] = i + 3;
            }

            mesh.triangles = triangles;
            int color_index = -1;
            Color colorT = Color.red;
            Color[] colors = new Color[mesh.vertices.Length];
            Debug.Log("test:size=" + all_colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                if ((i % 4) == 0)
                {
                    color_index++;
                    colorT = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
                }
                //colors[i] = colorT;

                color_index = color_index % all_colors.Length;
                colors[i] = all_colors[color_index];
                colors[i].a = 1.0f;
            }

            mesh.colors = colors;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}

