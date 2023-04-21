using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Edible
{
    void Eat();
    void OnTriggerEnter2D(Collider2D other);
}