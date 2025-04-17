using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlayStory(int id)
    {
        StoryManager.StartStory(id);
    }
}
