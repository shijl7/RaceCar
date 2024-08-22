using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Material meshMaterial;
    public float scale;
    public Vector2 dimension;//���γߴ� ������� �� ���γ���
    public int startTransitionLength;

    //��������
    public float perlinScale;
    public float offset;
    public float randomness;
    public float waveHeight;//����߶�

    public float globalSpeed;

    //�ϰ������
    public int startObstacleChance;
    public int ObstacleChanceScoreIncrease;
    public int ObstacleChanceDistanceIncrease;
    public GameObject gate;
    public GameObject[] obstacles;
    public int gateChance;

    GameObject currentCyclinder;
	Vector3[] beginPoints;
    GameObject[] pieces = new GameObject[2];
	void Start()
    {
        beginPoints = new Vector3[(int)dimension.x + 1];
        beginPoints[0] = Vector3.zero;

        for (int i = 0; i < 2; i++)
        {
            GenerateWorldPiece(i);
        }
    }

    void LateUpdate()
    {
        if (pieces[1] && pieces[1].transform.position.z<=-15f)
        {
            StartCoroutine(UpdateWorldPieces());
        }
    }

    //Э��
    IEnumerator UpdateWorldPieces()
    {
        Destroy(pieces[0]);
        pieces[0] = pieces[1];
        pieces[1] = CreateCylinder();
        //���������ɵ�λ��
        pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * dimension.y * scale * Mathf.PI;
        pieces[1].transform.rotation = pieces[0].transform.rotation;

        UpdateSinglePiece(pieces[1]);

        yield return 0;
    }

    void GenerateWorldPiece(int i)
    {
        pieces[i] = CreateCylinder();
        pieces[i].transform.Translate(Vector3.forward * dimension.y * scale * Mathf.PI * i);
        UpdateSinglePiece(pieces[i]);
    }

    void UpdateSinglePiece(GameObject piece)
    {
        //����ƶ����
		BasicMovement basicMovement = piece.AddComponent<BasicMovement>();
		basicMovement.moveSpeed = -globalSpeed;

        //����������
        GameObject endPoint = new GameObject();
        endPoint.name = "End Point";
        endPoint.transform.position = piece.transform.position + Vector3.forward * dimension.y * scale * Mathf.PI;
        endPoint.transform.parent = piece.transform;

        offset += randomness;
        if(startObstacleChance >= 2*ObstacleChanceDistanceIncrease)
        {
            startObstacleChance -= ObstacleChanceDistanceIncrease;
        }
	}

    public GameObject CreateCylinder()
    {
        //���� ����ֵname
        GameObject newCylinder = new GameObject();
        newCylinder.name = "World Piece";
        currentCyclinder = newCylinder;
        //�����
        MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();//ģ�����
        MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();//��Ⱦ�������ģ�ͺ��������Ⱦ����
        //��������в���
        //Ϊ��Ⱦ�����Ӳ���
        meshRenderer.material = meshMaterial;
        //Ϊģ��������ģ��
        meshFilter.mesh = Generate();

        //�����ײ������Mesh
        newCylinder.AddComponent<MeshCollider>();

        return newCylinder;
    }

    //**���ĺ���**
    //��������
    Mesh Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MESH";

        //��Ҫָ��Mehs�Ķ��㡢UV�������ε�����
        Vector3[] vertices = null;
        Vector2[] uvs = null;
        int[] triangles=null;

        //ִ�к�������������Щ����
        CreateShape(ref vertices, ref uvs, ref triangles);

        //���ݸ�ֵ
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

		return mesh;
    }

    //����Mesh������ݵĺ���
    void CreateShape(ref Vector3[] vertices,ref Vector2[] uvs,ref int[] triangles)
    {
        int xCount = (int)dimension.x;//���εĺ�����
        int zCount = (int)dimension.y;//���εĳ���

        //ͨ�����γߴ磬��ʼ��������uv
        vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        uvs = new Vector2[(xCount + 1) * (zCount + 1)];

        //����뾶
        float radius = xCount * scale * 0.5f;
        int index = 0;
        //ͨ��˫ѭ�������ö�����uv
        for (int x = 0; x <= xCount; x++)
        {
            for (int z = 0; z <= zCount; z++)
            {
                //����x��λ�ã���ȡԲ����ĽǶ�
                float angle = x * Mathf.PI * 2f / xCount;

                //ͨ���Ƕȼ��㶥���ֵ
                vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);

                //����uv��ֵ
                uvs[index] =new Vector2(x*scale, z*scale);

                //ʹ�ð�������
                float pX = (vertices[index].x * perlinScale) + offset;
				float pZ = (vertices[index].z * perlinScale) + offset;
                //��z������Ӧ��Բ��
                Vector3 center = new Vector3(0, 0, vertices[index].z);
                //Ϊƽ̹��Բ������Ķ������ָ��Բ�ĵ�ƫ�ƣ��������ƫ�Ƶķ����ɶ���ָ��Բ�ģ�ƫ�����ɰ��������������
                vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;

                //�޷��ͼƴ�Ӵ��������ɵ�����Բ���޷��ƴ��һ��
                if(z < startTransitionLength && beginPoints[beginPoints.Length-1] != Vector3.zero )
                {
                    float perlinPercentage = z * (1f / startTransitionLength);
                    Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
                    vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
                }
                else if(z == zCount)
                {
                    beginPoints[x] = vertices[index];
                }

                if(UnityEngine.Random.Range(0,startObstacleChance) == 0 && !(gate == null && obstacles.Length ==0))
                {
                    CreateItem(vertices[index],x);
                }

                index++;
			}
        }

        //���ж��㴴�����
        //��ʼ����������
        //
        triangles = new int[xCount * zCount * 6];

        int current = 0;
        //����һ�����飬��6�������εĶ��㣬�������
        int[] boxBase = new int[6];
        for (int x = 0; x < xCount; x++)
        {
            //ÿ�θ���x�ı仯�����¸�ֵ
            boxBase = new int[]
            {
                x*(zCount+1),
                x*(zCount+1)+1,
                (x+1)*(zCount+1),
                x*(zCount+1)+1,
                (x+1)*(zCount+1)+1,
                (x+1)*(zCount+1)
            };
            for (int z = 0; z < zCount; z++)
            {
                //����һ��������������������һ��������
                for(int i = 0; i < 6; i++)
                {
                    boxBase[i] = boxBase[i] + 1;
                }
                //����6��������䵽�������������
                for(int j = 0; j < 6; j++)
                {
                    triangles[current + j] = boxBase[j] - 1;
                }

                current += 6;
            }
        }

	}

    //�����ϰ���
    void CreateItem(Vector3 vert, int x)
    {
        Vector3 zCenter= new Vector3(0,0,vert.z);
        GameObject newItem = Instantiate(UnityEngine.Random.Range(0, gateChance) == 0
            ? gate
            : obstacles[UnityEngine.Random.Range(0, obstacles.Length)]);
        newItem.transform.rotation = Quaternion.LookRotation((zCenter-vert),Vector3.up);
        newItem.transform.position = vert;

        newItem.transform.SetParent(currentCyclinder.transform, false);
    }

    public Transform GetWorldPiece()
    {
        return pieces[0].transform;
    }

    public void IncreaseItem()
    {
        if(startObstacleChance >= 2*ObstacleChanceScoreIncrease)
        {
			startObstacleChance -= ObstacleChanceScoreIncrease;
		}
	}
}
