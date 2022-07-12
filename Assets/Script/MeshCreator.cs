using UnityEngine;

public class MeshCreator : BaseMono
{

    public int width = 10;
    public int height = 10;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateMesh()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        GameObject obj = new GameObject();
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        //����mesh
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        //��׼����
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        renderer.material = mat;

        int y = 0;
        int x = 0;

        //���������UV
        Vector3[] vertices = new Vector3[height * width];
        Vector2[] uv = new Vector2[height * width];

        //��uv���ŵ�0 - 1
        Vector2 uvScale = new Vector2(1.0f / (width - 1), 1.0f / (height - 1));

        for (y = 0; y < height; y++)
        {
            for (x = 0; x < width; x++)
            {
                //���ɶ���
                vertices[y * width + x] = new Vector3(x, 0, y);
                //����uv
                uv[y * width + x] = Vector2.Scale(new Vector2(x, y), uvScale);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        //������index
        int[] triangles = new int[(height - 1) * (width - 1) * 6];
        int index = 0;
        for (y = 0; y < height - 1; y++)
        {
            for (x = 0; x < width - 1; x++)
            {
                //ÿ������2��������,�ܹ�6��index
                triangles[index++] = (y * width) + x;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = (y * width) + x + 1;

                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = ((y + 1) * width) + x + 1;
                triangles[index++] = (y * width) + x + 1;
            }
        }
        mesh.triangles = triangles; //������
        mesh.RecalculateNormals();  //���㷨��
    }


}
