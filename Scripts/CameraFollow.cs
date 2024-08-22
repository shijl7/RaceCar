using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameTarget;
    public float distance;
    public float height;
    //��ת������߶ȵ�������
    public float rotationDamping;
    public float heightDamping;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

	void LateUpdate()
	{
        if (cameTarget == null)
        {
            return;
        }
        //��ǰ��ת�Ƕ���������ת�Ƕ�
        float currentRotationAngle = transform.eulerAngles.y;
        float wantedRotationAngle = cameTarget.eulerAngles.y;
        //��ǰ�߶��������߶�
        float currentHeight = transform.position.y;
        float wantedHeight = cameTarget.position.y+height;
        //����ǰֵLerp������ֵ
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0,currentRotationAngle,0);

        //����λ��
        transform.position = cameTarget.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        //���ø߶�
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        //����Ŀ��
        transform.LookAt(cameTarget);

    }

}
