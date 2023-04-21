using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPassage : MonoBehaviour
{
    public Transform exit;

    private void OnCollisionEnter2D(Collision2D other) {
        Debug.Log("Passge hit");
        Vector3 position = other.transform.position;
        position.x = this.exit.position.x;
        position.y = this.exit.position.y;
        other.transform.position = position;
        // Vector3 position = exit.position;
        // position.z = other.transform.position.z;
        // other.transform.position = position;
    }
}
