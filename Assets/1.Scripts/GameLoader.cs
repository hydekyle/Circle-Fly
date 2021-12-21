using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    async void Start()
    {
        await UniTask.WaitUntil(() => AdManager.isInitialized == true);
        SceneManager.LoadScene(1);
    }
}
