using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceCrushAnimation : MonoBehaviour
{
    [SerializeField]
    List<Material> materials = new List<Material>();

    List<MeshRenderer> childMaterials = new List<MeshRenderer>();

    bool isParentSet = false;


    public void SetMaterial(int colorIndex)
    {
        if(!isParentSet)
        {
            isParentSet = true;
            transform.SetParent(transform.parent.parent);
        }
        
        if(childMaterials.Count == 0)
        {
            for (int i = 0; i < 20; i++)
            {
                childMaterials.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
            }
        }

        for (int i = 0; i < childMaterials.Count; i++)
        {
            childMaterials[i].material = materials[colorIndex];
        }
    }
}