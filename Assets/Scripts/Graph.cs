using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    private List<GameObject> gameObjectList;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    // Start is called before the first frame update
    void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        gameObjectList = new List<GameObject>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.transform.localScale = Vector2.one * 0.2f;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    public void ShowGraph(List<float> valueList, Func<int,  string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }
        float graphHeight = graphContainer.sizeDelta.y;
        float graphLength = graphContainer.sizeDelta.x;

        float yMaximum = 0;
        foreach (float val  in valueList)
        {
            if (val > yMaximum)
            {
                yMaximum = val;
            }
        }
        yMaximum = yMaximum * 1.1f;
        float xSize = graphLength / valueList.Count * 0.92f;
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);

        }
        gameObjectList.Clear();
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = i * xSize + 5f;
            float yPosition = (valueList[i] / yMaximum) * graphHeight + 5f;
            GameObject circleGameObject = CreateCircle(new Vector2 (xPosition, yPosition));
            gameObjectList.Add(circleGameObject);
            if (lastCircleGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add (dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;

           
        }

        int separatorCountY = 10;
        for (int i = 0; i <= separatorCountY; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 0.95f / separatorCountY;
            labelY.anchoredPosition3D = new Vector3(-3f, normalizedValue * graphHeight + 5f, 0);
            labelY.transform.localScale = Vector2.one * 0.06f;
            labelY.GetComponent<Text>().text = getAxisLabelY(normalizedValue *  yMaximum);
            gameObjectList.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            float normalizedValue2 = i * 0.95f / separatorCountY;
            dashY.anchoredPosition3D = new Vector3(-2f, normalizedValue2 * graphHeight + 4f, 0);
            gameObjectList.Add(dashY.gameObject);
        }
        int separatorCountX = 15;
        for (int i = 0; i <= separatorCountX; i++)
        {
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            float normalizedValue = i * 0.95f / separatorCountX;
            labelX.anchoredPosition3D = new Vector3(normalizedValue * graphLength + 3f, -1f, 0);
            labelX.transform.localScale = Vector2.one * 0.06f;
            labelX.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(labelX.gameObject);

            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer, false);
            dashX.gameObject.SetActive(true);
            float normalizedValue2 = i * 0.95f / separatorCountX;
            dashX.anchoredPosition3D = new Vector3(normalizedValue2 * graphLength + 3f, -1f, 0);
            gameObjectList.Add(dashX.gameObject);
        }
    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance * 5f, 3f);
        gameObject.transform.localScale = Vector2.one * 0.2f;
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
        return gameObject;
    }
}
