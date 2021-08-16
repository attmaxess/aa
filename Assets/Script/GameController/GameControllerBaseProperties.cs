using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerBaseProperties : MonoBehaviour
{
    public GameController gameController
    {
        get
        {
            if (_gameController == null) _gameController = GetComponent<GameController>();
            return _gameController;
        }
    }
    GameController _gameController;
    public GameLoadLevelAsync levelAsync
    {
        get
        {
            if (_levelAsync == null) _levelAsync = GetComponent<GameLoadLevelAsync>();
            return _levelAsync;
        }
    }
    GameLoadLevelAsync _levelAsync;
    public GameCaptureLevel gameCapture
    {
        get
        {
            if (_gameCapture == null) _gameCapture = GetComponent<GameCaptureLevel>();
            return _gameCapture;
        }
    }
    GameCaptureLevel _gameCapture;
}
