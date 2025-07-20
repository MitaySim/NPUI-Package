using System;
using UnityEngine;
using System.Reflection; // Needed for Assembly.GetType

/// <summary>
/// Serializable wrapper for System.Type, allowing it to be exposed in the Inspector.
/// Stores the assembly-qualified name of the type.
/// </summary>
[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [SerializeField]
    private string _assemblyQualifiedName;

    private Type _type;

    // Public property to get the actual System.Type
    public Type Type
    {
        get
        {
            // If the type hasn't been resolved yet or the name changed, resolve it.
            if (_type == null && !string.IsNullOrEmpty(_assemblyQualifiedName))
            {
                _type = Type.GetType(_assemblyQualifiedName);
                if (_type == null)
                {
                    Debug.LogWarning($"Could not find type with assembly qualified name: {_assemblyQualifiedName}");
                }
            }
            return _type;
        }
        set
        {
            _type = value;
            if (value != null)
            {
                _assemblyQualifiedName = value.AssemblyQualifiedName;
            }
            else
            {
                _assemblyQualifiedName = null;
            }
        }
    }

    // Constructor to easily create from a Type
    public SerializableType(Type type)
    {
        Type = type;
    }

    // Implicit conversion from Type to SerializableType
    public static implicit operator SerializableType(Type type)
    {
        return new SerializableType(type);
    }

    // Implicit conversion from SerializableType to Type
    public static implicit operator Type(SerializableType serializableType)
    {
        return serializableType?.Type;
    }

    // ISerializationCallbackReceiver methods (important for serialization)
    public void OnBeforeSerialize()
    {
        // Nothing special to do here, _assemblyQualifiedName is already set by the setter
    }

    public void OnAfterDeserialize()
    {
        // Clear the cached _type so it gets re-resolved when accessed after deserialization
        _type = null; 
    }

    public override string ToString()
    {
        return Type != null ? Type.Name : "None (Type)";
    }
}