using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class Generator : MonoBehaviour
{
    [SerializeField] public int numberOfSpheres = 100;
    [SerializeField] public int radius = 60;
    [SerializeField] public int speed = 20;
    
    public static readonly UnityEvent<DynamicObjectsTests> NewObjectAddedEvent = new UnityEvent<DynamicObjectsTests>();
    public static readonly UnityEvent<DynamicObjectsTests> ObjectDestroyedEvent = new UnityEvent<DynamicObjectsTests>();
    public static readonly UnityEvent DestroyEvent = new UnityEvent();
    
    public void CreateAllDynamicObjects()
    {
        for(int i = 0; i < numberOfSpheres; i++)
        {
            NewObjectAddedEvent.Invoke(CreateNewDynamicObject());
        }
    }

    private DynamicObjectsTests CreateNewDynamicObject()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 0);
        DynamicObjectsTests dynamicObjectsTests = cube.AddComponent<DynamicObjectsTests>();
        dynamicObjectsTests.speed = speed;
        dynamicObjectsTests.sphereRadius = radius;
        dynamicObjectsTests.AddListeners();
        return dynamicObjectsTests;
    }
}
