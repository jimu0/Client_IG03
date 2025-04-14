using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Scripts.ControlAnimation
{
    public class ControlAnimation : MonoBehaviour
    {
        private Animation animationComponent;
        private List<string> clipName;
        public int i;
        public float speed = 1;
        private int iOld = -1;
        private void Awake()
        {
            animationComponent = GetComponent<Animation>();
            clipName = new List<string>();
            foreach (AnimationState clip in animationComponent)clipName.Add(clip.name);
        }
        void Update()
        {
            if (i == iOld) return;
            if (i >= clipName.Count || i < 0) return;
            animationComponent.Play(clipName[i]);
            animationComponent[clipName[i]].speed = speed;
            iOld = i;
        }
    } 
}

