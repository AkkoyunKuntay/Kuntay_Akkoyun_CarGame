using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CanvasType { Menu, Game, Win }

public class CanvasManager : MonoSingleton<CanvasManager>
{
    [SerializeField] CanvasVisibilityController startCanvas;
    [SerializeField] CanvasVisibilityController gameCanvas;
    [SerializeField] CanvasVisibilityController winCanvas;

    [Header("Debug")]
    [SerializeField] CanvasVisibilityController selectedCanvas;

    private void Start()
    {
        selectedCanvas = startCanvas;
        GameManager.instance.LevelStartedEvent += OnLevelStarted;
        GameManager.instance.LevelSuccessEvent += OnLevelSuccessfull;
    }
    private void OnLevelStarted()
    {
        SetCurrentCanvas(CanvasType.Game);
    }
    private void OnLevelSuccessfull()
    {
        SetCurrentCanvas(CanvasType.Win);
    }

    public void SetCurrentCanvas(CanvasType type)
    {
        selectedCanvas.Hide();
        selectedCanvas.gameObject.SetActive(false);

        switch (type)
        {
            case CanvasType.Menu:
                selectedCanvas = startCanvas;
                break;
            case CanvasType.Game:
                selectedCanvas = gameCanvas;
                break;
            case CanvasType.Win:
                selectedCanvas = winCanvas;
                break;
            default:
                break;
        }
        selectedCanvas.gameObject.SetActive(true);
        selectedCanvas.Show();
    }
    public void HideorShowGameCanvas(bool isActive)
    {
        if (isActive) gameCanvas.Show();
        else gameCanvas.Hide();
    }
}