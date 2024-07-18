using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] VisualTreeAsset mainMenu, lobby;
    [SerializeField] GameObject go;

    void SwitchTo(VisualTreeAsset visualTreeAsset) =>
        document.visualTreeAsset = visualTreeAsset;
}
