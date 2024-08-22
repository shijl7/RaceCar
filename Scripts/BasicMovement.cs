using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    public float moveSpeed = -30f;
    public float rotateSpeed = 30f;
    public bool isLamp;

    WorldGenerator worldGenerator;
    Car car;
    Transform carTransform;
    void Start()
    {
        car = GameObject.FindObjectOfType<Car>();
        worldGenerator = GameObject.FindObjectOfType<WorldGenerator>();
        if(car != null )
        {
            carTransform = car.transform;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (car != null)
        {
            CheckRotation();
        }
    }

    void CheckRotation()
    {
        Vector3 direction = isLamp ? Vector3.right : Vector3.forward;
        float carRotation = carTransform.localEulerAngles.y;
        if(carRotation > car.rotationAngle*2f)
        {
            carRotation = (360 - carRotation) * -1f;
        }
        //旋转与汽车相反的方向
        transform.Rotate(direction * -rotateSpeed * (carRotation / (float)car.rotationAngle) * (36f / worldGenerator.dimension.x) * Time.deltaTime);

    }

}
