using System.Collections.Generic;
using UnityEngine;

public class ElemetsIDSystem
{
    List<string> elemetsID = new List<string>();
    private readonly string _menuName;

    public ElemetsIDSystem(string menuName)
    {
        _menuName = menuName;
    }
    public void SetElementID(GenericUIData element)
    {
        string elementID;
        if (element == null)
        {
            Debug.Log("GetElementID: Element is null");
            return;
        }
        elementID = _menuName + nameof(element);
        if (elemetsID.Contains(elementID))
        {
            int numSuffix = 1;
            while (elemetsID.Contains(elementID + "_" + numSuffix))
            {
                numSuffix++;
            }
            elementID += "_" + numSuffix;
        }
        elemetsID.Add(elementID);
        element.ID = elementID;
    }
}
