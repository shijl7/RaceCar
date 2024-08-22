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
			//���³���ģ�͵�λ������ת
			wheelMeshes[i].transform.position = pos;
            wheelMeshes[i].Rotate(Vector3.right*Time.deltaTime*wheelRotateSpeed);
        }
        //�����¼�
        if (Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0)//������Ϊ�棬ˮƽ��ֵ������0
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

    //���³��ĽǶ�
    void UpdateTargetRotation()
    {
        if (Input.GetAxis("Horizontal") == 0)
        {
            if (Input.mousePosition.x > Screen.width*0.5f)
            {
                //��ת
                targetRotation = rotationAngle;
            }
            else
            {
                //��ת
                targetRotation = -rotationAngle;
            }
        }
        else
        {
            targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
        }
    }

    //������Ч
    void UpdateEffects()
    {
        //��̥�ڿ��оͼ�һ�����µ���
        bool inAir = true;
        //�Ƿ�����ת
        bool rotated = Mathf.Abs(lastUpdateRotationAngle - transform.localEulerAngles.y) > minSpawnSkidMarkAngle;
        for (int i = 0; i < 2; i++)
        {
            Transform wheelMesh = wheelMeshes[i + 2];
            //����̥����������߼��
            if (Physics.Raycast(wheelMesh.position, Vector3.down, grassEffectOffset * 1.5f))
            {
                if (!grassEffects[i].gameObject.activeSelf)
                {
                    grassEffects[i].gameObject.SetActive(true);
                }
                //����������ۼ���λ��
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

    //��
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

    //����ײ��
    public void FallApart()
    {
        Instantiate(ragdoll,transform.position,transform.rotation);
        gameObject.SetActive(false);
    }

}
