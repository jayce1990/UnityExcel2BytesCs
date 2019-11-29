using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class readBytesTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<weapon> weapons = weapon.LoadBytes();
        for (int i = 0; i < weapons.Count; i++)
        {
            weapon w = weapons[i];
            Debug.Log(w.id + "," + w.name + "," + w.prefabName + "," + w.nums + "," + w.desc);
        }
    }
}
