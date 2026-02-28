using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class throwableManager : MonoBehaviour
{
    public GameObject ossiclePrefab;
    public GameObject dicePrefab;
    public float height = 1.0f;
    public int numberOfOssiclesToThrow = 8;
    public float circleRadius = 0.5f;
    public float randomVelocityMagnitude = 0.5f;
    public float randomAngularMomentumMagnitude = 6.0f;

    public float epsilon = 1e-2f;
    public float timeout_period = 10.0f;
    public Vector3 lowBound = new Vector3(-1.0f,0.0f,-1.0f);
    public Vector3 highBound = new Vector3(1.0f,2.0f,1.0f);

    public List<Rigidbody> throwableInstances = new List<Rigidbody>();

    public static event Action<int> ossicleThrowFinished;
    public static event Action<int> diceThrowFinished;

    IEnumerator WaitForOssiclesSolvedThrow()
    {
        float startTime = Time.time;
        for(;;)
        {
            yield return new WaitForSeconds(0.25f);
            int isStatic = 1;
            
            for(int i = 0; i < throwableInstances.Count; i++)
            {
                // if OOB throw again
                if(    throwableInstances[i].position.x < lowBound.x
                    || throwableInstances[i].position.y < lowBound.y
                    || throwableInstances[i].position.z < lowBound.z
                    || throwableInstances[i].position.x > highBound.x
                    || throwableInstances[i].position.y > highBound.y
                    || throwableInstances[i].position.z > highBound.z
                )
                {
                    float theta = -Mathf.PI*2.0f*Mathf.InverseLerp(0,numberOfOssiclesToThrow,i);
                    throwableInstances[i].rotation = UnityEngine.Random.rotationUniform;
                    throwableInstances[i].position = new Vector3(Mathf.Cos(theta)*circleRadius, 2.0f, Mathf.Sin(theta)*circleRadius);
                    throwableInstances[i].linearVelocity = UnityEngine.Random.onUnitSphere*randomVelocityMagnitude;
                    throwableInstances[i].angularVelocity = UnityEngine.Random.onUnitSphere*randomAngularMomentumMagnitude;
                }
            }
            
            for(int i = 0; i < throwableInstances.Count; i++)
            {
                if(throwableInstances[i].linearVelocity.magnitude>epsilon || throwableInstances[i].angularVelocity.magnitude>epsilon)
                {
                    isStatic=0;
                    break;
                }
            }
            if(isStatic==1 || Time.time-startTime>timeout_period)
            {
                break;
            }
        }
        int upDog = 0;
        for(int i = 0; i < throwableInstances.Count; i++)
        {
            if(Quaternion.Dot(throwableInstances[i].rotation,Quaternion.identity)>0.0)
            {
                upDog++;
            }
        }
        ossicleThrowFinished?.Invoke(upDog);
        Debug.Log(upDog);
    }

    [ContextMenu("Throw Ossicles")] 
    public void ThrowOssicles()
    {
        for (int i = 0; i < numberOfOssiclesToThrow; i++)
        {
            float theta = -Mathf.PI*2.0f*Mathf.InverseLerp(0,numberOfOssiclesToThrow,i);
            Rigidbody instance = Instantiate(ossiclePrefab, new Vector3(Mathf.Cos(theta)*circleRadius, 2.0f, Mathf.Sin(theta)*circleRadius), Quaternion.identity).GetComponent<Rigidbody>(); 
            throwableInstances.Add(instance);
            instance.rotation = UnityEngine.Random.rotationUniform;
            instance.linearVelocity = UnityEngine.Random.onUnitSphere*randomVelocityMagnitude;
            instance.angularVelocity = UnityEngine.Random.onUnitSphere*randomAngularMomentumMagnitude;
        }
        StartCoroutine(WaitForOssiclesSolvedThrow());
    }
    
    [ContextMenu("Clear")] 
    public void ClearBoard()
    {
        for(int i = 0; i < throwableInstances.Count; i++)
        {
            Destroy(throwableInstances[i].gameObject);
        }
        throwableInstances.Clear();
    }

    [ContextMenu("Throw Dice")] 
    public void ThrowDice()
    {
        //TODO
    }
}
