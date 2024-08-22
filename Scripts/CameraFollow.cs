using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameTarget;
    public float distance;
    public float height;
    //旋转阻尼与高度调整阻尼
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
        //当前旋转角度与期望旋转角度
        float currentRotationAngle = transform.eulerAngles.y;
        float wantedRotationAngle = cameTarget.eulerAngles.y;
        //当前高度与期望高度
        float currentHeight = transform.position.y;
        float wantedHeight = cameTarget.position.y+height;
        //将当前值Lerp到期望值
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0,currentRotationAngle,0);

        //设置位置
        transform.position = cameTarget.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        //设置高度
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        //看向目标
        transform.LookAt(cameTarget);

    }

}
