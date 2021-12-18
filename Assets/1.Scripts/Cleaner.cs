using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet")) other.gameObject.SetActive(false);
    }
}
