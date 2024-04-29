using UnityEngine;

namespace TLab.Android.WebView
{
    public class TouchEventManager : MonoBehaviour
    {
        [SerializeField] private RectTransform screenRect;
        [SerializeField] private TLabWebView tlabWebView;

        // rect transform
        private float[] screenEdge;
        private const int LEFT_IDX = 0;
        private const int RIGHT_IDX = 1;
        private const int BOTTOM_IDX = 2;
        private const int TOP_IDX = 3;
        private float[] screenSize;
        private const int VERTICAL_IDX = 0;
        private const int HORIZONTAL_IDX = 1;

        // event
        private const int TOUCH_DOWN = 0;
        private const int TOUCH_UP = 1;
        private const int TOUCH_MOVE = 2;

        // state
        private bool onTheWeb = false;

        void Start()
        {
            if (screenRect == null)
            {
                Debug.LogError("screenRect is null");
                return;
            }

            Vector3[] screenCorners = new Vector3[4];
            screenRect.GetWorldCorners(screenCorners);

            for (int i = 0; i < screenCorners.Length; i++)
            {
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, screenCorners[i]);
            }

            screenEdge = new float[4];
            screenEdge[LEFT_IDX] = screenCorners[0].x;
            screenEdge[RIGHT_IDX] = screenCorners[2].x;
            screenEdge[BOTTOM_IDX] = screenCorners[0].y;
            screenEdge[TOP_IDX] = screenCorners[1].y;

            screenSize = new float[2];
            screenSize[VERTICAL_IDX] = screenEdge[TOP_IDX] - screenEdge[BOTTOM_IDX];
            screenSize[HORIZONTAL_IDX] = screenEdge[RIGHT_IDX] - screenEdge[LEFT_IDX];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int TouchHorizontal(float x)
        {
            return (int)((x - screenEdge[LEFT_IDX]) / screenSize[HORIZONTAL_IDX] * tlabWebView.webWidth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int TouchVertical(float y)
        {
            return (int)((1.0f - (y - screenEdge[BOTTOM_IDX]) / screenSize[VERTICAL_IDX]) * tlabWebView.webHeight);
        }

        void Update()
        {
            if (tlabWebView == null)
            {
                return;
            }

#if UNITY_ANDROID
            foreach (Touch t in Input.touches)
            {
                int x = TouchHorizontal(t.position.x);
                int y = TouchVertical(t.position.y);

                int eventNum = (int)TouchPhase.Stationary;

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) eventNum = TOUCH_UP;
                else if (t.phase == TouchPhase.Began) eventNum = TOUCH_DOWN;
                else if (t.phase == TouchPhase.Moved) eventNum = TOUCH_MOVE;

                if (x > tlabWebView.webWidth || x < 0 || y > tlabWebView.webHeight || y < 0)
                {
                    if (onTheWeb == true && t.phase == TouchPhase.Moved)
                        eventNum = TOUCH_UP;
                    else
                        eventNum = (int)TouchPhase.Stationary;

                    onTheWeb = false;
                }
                else
                {
                    onTheWeb = true;
                }

                tlabWebView.TouchEvent(x, y, eventNum);
            }
#endif
        }
    }
}
