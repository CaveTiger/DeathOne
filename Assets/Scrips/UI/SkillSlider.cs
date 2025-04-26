using UnityEngine;

public class SlideBarDetector : MonoBehaviour
{
    private static GameObject[] slideBars;

    void Awake()
    {
        slideBars = GameObject.FindGameObjectsWithTag("SlideBar");
    }

    public static GameObject GetNearestSlot(Vector3 screenPosition)
    {
        foreach (var bar in slideBars)
        {
            RectTransform rect = bar.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition))
                return bar;
        }
        return null;
    }
}
