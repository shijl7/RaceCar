using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public Transform backTransform;
    public float constantBackForce;
    public Transform[] wheelMeshes;
    public WheelCollider[] wheelColliders;
    public float grassEffectOffset;

    public int rotateSpeed;
    public int rotationAngle;
    public int wheelRotateSpeed;

    private int targetRotation;

    public Transform[] grassEffects;
    public float skidMarkDelay = 1;
    public Transform[] skidMarkPivots;
    public GameObject skidMark;
    public float skidMarkSize = 1f;
    public float minSpawnSkidMarkAngle;

    public GameObject ragdoll;

    bool skidMarkEnable;
    float lastUpdateRotationAngle;
    WorldGenerator worldGenerator;
    void Start()
    {
        worldGenerator = GameObject.FindObjectOfType<WorldGenerator>();
        StartCoroutine(SkidMark());
    }

	private void FixedUpdate()
	{
		UpdateEffects();
	}

	void LateUpdate()
    {
        for (int i = 0; i < wheelMeshes.Length; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos, out quat);
			//更新车轮模型的位置与旋转
			wheelMeshes[i].transform.position = pos;
            wheelMeshes[i].Rotate(Vector3.right*Time.deltaTime*wheelRotateSpeed);
        }
        //输入事件
        if (Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0)//鼠标左键为真，水平轴值不等于0
        {
            UpdateTargetRotation();
        }
        else if(targetRotation != 0)
        {
            targetRotation = 0;
        }
        Vector3 rotation = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotateSpeed * Time.deltaTime);
    }

    //更新车的角度
    void UpdateTargetRotation()
    {
        if (Input.GetAxis("Horizontal") == 0)
        {
            if (Input.mousePosition.x > Screen.width*0.5f)
            {
                //右转
                targetRotation = rotationAngle;
            }
            else
            {
                //左转
                targetRotation = -rotationAngle;
            }
        }
        else
        {
            targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
        }
    }

    //更新特效
    void UpdateEffects()
    {
        //轮胎在空中就加一个向下的力
        bool inAir = true;
        //是否在旋转
        bool rotated = Mathf.Abs(lastUpdateRotationAngle - transform.localEulerAngles.y) > minSpawnSkidMarkAngle;
        for (int i = 0; i < 2; i++)
        {
            Transform wheelMesh = wheelMeshes[i + 2];
            //在轮胎下面进行射线检测
            if (Physics.Raycast(wheelMesh.position, Vector3.down, grassEffectOffset * 1.5f))
            {
                if (!grassEffects[i].gameObject.activeSelf)
                {
                    grassEffects[i].gameObject.SetActive(true);
                }
                //更新粒子与痕迹的位置
                float effectHeight = wheelMesh.position.y - grassEffectOffset;
                Vector3 targetPosition = new Vector3(wheelMesh.position.x, effectHeight, wheelMesh.position.z);
                grassEffects[i].position = targetPosition;
                skidMarkPivots[i].position = targetPosition;

                inAir = false;
            }
            else if (grassEffects[i].gameObject.activeSelf)
            {
                grassEffects[i].gameObject.SetActive(false);
            }
        }
        if (inAir)
        {
            carRigidbody.AddForceAtPosition(backTransform.position, Vector3.down * constantBackForce);
            skidMarkEnable = false;
        }
        else
        {
            if(targetRotation!=0)
            {
                if(rotated && !skidMarkEnable)
                {
                    skidMarkEnable = true;
                }
                else if(!rotated && skidMarkEnable)
                {
                    skidMarkEnable = false;
                }
            }
            else 
            { 
                skidMarkEnable = false; 
            }
        }
        lastUpdateRotationAngle = transform.localEulerAngles.y;
    }

    //打滑
    IEnumerator SkidMark()
    {
        while (true)
        {
            yield return new WaitForSeconds(skidMarkDelay);
            if (skidMarkEnable)
            {
				for (int i = 0; i < skidMarkPivots.Length; i++)
				{
					GameObject newSkidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
					newSkidMark.transform.parent = worldGenerator.GetWorldPiece();
					newSkidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
				}
			}    
        }
    }

    //汽车撞碎
    public void FallApart()
    {
        Instantiate(ragdoll,transform.position,transform.rotation);
        gameObject.SetActive(false);
    }

}
