using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTools
{
    public static Transform GetHeirarchyParent(Transform target)
    {
        while (target.parent != null)
            target = target.parent;

        return target;
    }
}
