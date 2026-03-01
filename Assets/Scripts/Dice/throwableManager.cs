using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class throwableManager : MonoBehaviour
{
    public GameObject ossiclePrefab;
    public GameObject dicePrefab;
    public Transform mainCamera;

    public Vector3 singleDiceScreenSpacePosition = new Vector3(0.2f,0.0f,0.5f);
    public float singleDiceScale = 1e-1f;

    public float height = 1.0f;
    public int numberOfOssiclesToThrow = 8;
    public float circleRadius = 0.25f;
    public float randomVelocityMagnitude = 0.5f;
    public float randomAngularMomentumMagnitude = 18.0f;

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
                if(    throwableInstances[i].position.x-transform.position.x < lowBound.x
                    || throwableInstances[i].position.y-transform.position.y < lowBound.y
                    || throwableInstances[i].position.z-transform.position.z < lowBound.z
                    || throwableInstances[i].position.x-transform.position.x > highBound.x
                    || throwableInstances[i].position.y-transform.position.y > highBound.y
                    || throwableInstances[i].position.z-transform.position.z > highBound.z
                )
                {
                    float theta = -Mathf.PI*2.0f*Mathf.InverseLerp(0,numberOfOssiclesToThrow,i);
                    throwableInstances[i].rotation = UnityEngine.Random.rotationUniform;
                    throwableInstances[i].position = new Vector3(Mathf.Cos(theta)*circleRadius, 2.0f, Mathf.Sin(theta)*circleRadius) + transform.position;
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
            Transform upT = throwableInstances[i].transform.GetChild(0);
            if(Vector3.Dot(Vector3.Normalize(upT.position-throwableInstances[i].transform.position),Vector3.up)>0.0)
            {
                upDog++;
            }
        }
        ossicleThrowFinished?.Invoke(upDog);
        Debug.Log(upDog);
    }

    IEnumerator DiceAnim()
    {
        GameObject dice = Instantiate(dicePrefab, mainCamera.position + mainCamera.right*singleDiceScreenSpacePosition.x + mainCamera.up*singleDiceScreenSpacePosition.y + mainCamera.forward*singleDiceScreenSpacePosition.z, Quaternion.identity); 
        dice.transform.localScale = new Vector3(singleDiceScale,singleDiceScale,singleDiceScale);

        int randomSide = UnityEngine.Random.Range(1,20);

        Transform side = dice.transform.GetChild(randomSide-1);

        Vector3 direction = Vector3.Normalize(dice.transform.position-side.position);
        Vector3 targetDirection = Vector3.Normalize(dice.transform.position-mainCamera.position);

        dice.transform.Rotate(Vector3.Cross(direction,targetDirection),Mathf.Acos(Vector3.Dot(direction,targetDirection))*Mathf.Rad2Deg);
        dice.transform.Rotate(targetDirection,UnityEngine.Random.Range(0.0f,360.0f),Space.World);

        Quaternion finalRot = dice.transform.rotation;
        Vector3 spinAxis = UnityEngine.Random.onUnitSphere;
        spinAxis = Vector3.Normalize(Vector3.Cross(spinAxis,targetDirection));

        for(int i = 0; i < 100; i++)
        {
            dice.transform.rotation = finalRot;
            dice.transform.Rotate(spinAxis,Mathf.Pow(Mathf.InverseLerp(99,0,i),2.0f)*1000.0f,Space.World);
            yield return new WaitForSeconds(0.01f);
        }

        for(int i = 0; i < 100; i++)
        {
            float scaleAnim = singleDiceScale*(1.0f+0.2f*Mathf.Exp(-Mathf.Pow(4.0f*(Mathf.Pow(Mathf.InverseLerp(99,0,i),1.5f)-0.5f),2.0f)));
            dice.transform.localScale = new Vector3(scaleAnim,scaleAnim,scaleAnim);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.25f);
        diceThrowFinished?.Invoke(randomSide);
        Destroy(dice);
    }

    [ContextMenu("Throw Ossicles")] 
    public void ThrowOssicles()
    {
        for (int i = 0; i < numberOfOssiclesToThrow; i++)
        {
            float theta = -Mathf.PI*2.0f*Mathf.InverseLerp(0,numberOfOssiclesToThrow,i);
            Rigidbody instance = Instantiate(ossiclePrefab, new Vector3(Mathf.Cos(theta)*circleRadius, 2.0f, Mathf.Sin(theta)*circleRadius) + transform.position, Quaternion.identity).GetComponent<Rigidbody>(); 
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
        StartCoroutine(DiceAnim());
    }
}
