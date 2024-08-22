using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Material meshMaterial;
    public float scale;
    public Vector2 dimension;//地形尺寸 横截面宽度 与 地形长度
    public int startTransitionLength;

    //柏林噪声
    public float perlinScale;
    public float offset;
    public float randomness;
    public float waveHeight;//起伏高度

    public float globalSpeed;

    //障碍物相关
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

    //协程
    IEnumerator UpdateWorldPieces()
    {
        Destroy(pieces[0]);
        pieces[0] = pieces[1];
        pieces[1] = CreateCylinder();
        //设置新生成的位置
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
        //添加移动组件
		BasicMovement basicMovement = piece.AddComponent<BasicMovement>();
		basicMovement.moveSpeed = -globalSpeed;

        //创建结束点
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
        //创建 并赋值name
        GameObject newCylinder = new GameObject();
        newCylinder.name = "World Piece";
        currentCyclinder = newCylinder;
        //挂组件
        MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();//模型组件
        MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();//渲染组件，有模型后必须有渲染才行
        //对组件进行操作
        //为渲染组件添加材质
        meshRenderer.material = meshMaterial;
        //为模型组件添加模型
        meshFilter.mesh = Generate();

        //添加碰撞，适配Mesh
        newCylinder.AddComponent<MeshCollider>();

        return newCylinder;
    }

    //**核心函数**
    //创建网格
    Mesh Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MESH";

        //需要指定Mehs的顶点、UV、三角形等数据
        Vector3[] vertices = null;
        Vector2[] uvs = null;
        int[] triangles=null;

        //执行函数，来创建这些数据
        CreateShape(ref vertices, ref uvs, ref triangles);

        //数据赋值
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

		return mesh;
    }

    //创建Mesh相关数据的函数
    void CreateShape(ref Vector3[] vertices,ref Vector2[] uvs,ref int[] triangles)
    {
        int xCount = (int)dimension.x;//地形的横截面宽
        int zCount = (int)dimension.y;//地形的长度

        //通过地形尺寸，初始化顶点与uv
        vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        uvs = new Vector2[(xCount + 1) * (zCount + 1)];

        //计算半径
        float radius = xCount * scale * 0.5f;
        int index = 0;
        //通过双循环，设置顶点与uv
        for (int x = 0; x <= xCount; x++)
        {
            for (int z = 0; z <= zCount; z++)
            {
                //根据x的位置，获取圆柱体的角度
                float angle = x * Mathf.PI * 2f / xCount;

                //通过角度计算顶点的值
                vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);

                //计算uv的值
                uvs[index] =new Vector2(x*scale, z*scale);

                //使用柏林噪声
                float pX = (vertices[index].x * perlinScale) + offset;
				float pZ = (vertices[index].z * perlinScale) + offset;
                //此z轴截面对应的圆心
                Vector3 center = new Vector3(0, 0, vertices[index].z);
                //为平坦的圆柱表面的顶点添加指向圆心的偏移，这个顶点偏移的方向由顶点指向圆心，偏移量由柏林噪声计算而来
                vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;

                //无缝地图拼接处理，把生成的两个圆柱无缝的拼到一起
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

        //所有顶点创建完毕
        //开始设置三角形
        //
        triangles = new int[xCount * zCount * 6];

        int current = 0;
        //创建一个数组，存6个三角形的顶点，方便调用
        int[] boxBase = new int[6];
        for (int x = 0; x < xCount; x++)
        {
            //每次根据x的变化，重新赋值
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
                //增长一下这个索引，方便计算下一个正方形
                for(int i = 0; i < 6; i++)
                {
                    boxBase[i] = boxBase[i] + 1;
                }
                //把这6个顶点填充到具体的三角形中
                for(int j = 0; j < 6; j++)
                {
                    triangles[current + j] = boxBase[j] - 1;
                }

                current += 6;
            }
        }

	}

    //创建障碍物
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
