using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.TimelineControl.SceneFX
{
    public class SceneFXController : MonoBehaviour
    {
        public static SceneFXController Instance { get; private set; }
        
        [Header("References")] [SerializeField]
        private Image fadeImage;

        [Header("Settings")] [SerializeField] private float fadeSpeed = 0.5f;
        [SerializeField] private bool startWithBlackScreen = false;

        private Coroutine currentFadeCoroutine;

        private void Awake()
        {
            // 单例初始化
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            // 初始化状态
            if (fadeImage != null)
            {
                fadeImage.enabled = startWithBlackScreen;
                fadeImage.gameObject.SetActive(startWithBlackScreen);
                fadeImage.color = new Color(0, 0, 0, startWithBlackScreen ? 1 : 0);
            }
        }

        /// <summary>
        /// 淡入效果（从黑屏到透明）
        /// </summary>
        public void FadeInFX(System.Action onComplete = null)
        {

            if (fadeImage == null) return;
            // 停止正在进行的淡入淡出
            if (currentFadeCoroutine != null)
            {
                SetBlackScreen();
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeRoutine(1, 0, onComplete));
        }

        /// <summary>
        /// 淡出效果（从透明到黑屏）
        /// </summary>
        public void FadeOutFX(System.Action onComplete = null)
        {
            if (fadeImage == null) return;

            // 停止正在进行的淡入淡出
            if (currentFadeCoroutine != null)
            {
                SetTransparentScreen();
                StopCoroutine(currentFadeCoroutine);
            }

            currentFadeCoroutine = StartCoroutine(FadeRoutine(0, 1, onComplete));
        }

        /// <summary>
        /// 立即设置为黑屏
        /// </summary>
        public void SetBlackScreen()
        {
            if (fadeImage == null) return;
            fadeImage.enabled = true;
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1);
        }

        /// <summary>
        /// 立即设置为透明
        /// </summary>
        public void SetTransparentScreen()
        {
            if (fadeImage == null) return;
            fadeImage.enabled = false;
            fadeImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, System.Action onComplete)
        {
            // 确保Image是激活状态
            fadeImage.enabled = true;
            fadeImage.gameObject.SetActive(true);

            Color currentColor = fadeImage.color;
            currentColor.a = startAlpha;
            fadeImage.color = currentColor;

            float progress = 0f;

            while (progress < 1f)
            {
                progress += Time.deltaTime * fadeSpeed;
                currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
                fadeImage.color = currentColor;
                yield return null;
            }

            // 确保达到目标值
            currentColor.a = targetAlpha;
            fadeImage.color = currentColor;

            // 如果淡入完成（透明度为0），隐藏Image
            if (targetAlpha == 0)
            {
                fadeImage.enabled = false;
                fadeImage.gameObject.SetActive(false);
            }

            currentFadeCoroutine = null;
            
            onComplete?.Invoke();
        }

        // 属性访问器
        public float FadeSpeed
        {
            get => fadeSpeed;
            set => fadeSpeed = Mathf.Max(0.1f, value); // 确保最小值
        }

        public bool StartWithBlackScreen
        {
            get => startWithBlackScreen;
            set
            {
                startWithBlackScreen = value;
                // 立即应用更改
                if (fadeImage != null)
                {
                    fadeImage.enabled = value;
                    fadeImage.gameObject.SetActive(value);
                    fadeImage.color = new Color(0, 0, 0, value ? 1 : 0);
                }
            }
        }
    }
}