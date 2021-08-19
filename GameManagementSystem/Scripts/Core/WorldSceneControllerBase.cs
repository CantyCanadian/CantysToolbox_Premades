using Canty.EventSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Canty.GameManagementSystem
{
    /// <summary>
    /// Base class for your scene controller. Must be implemented in order to specify which enum to use as the Scenes enum.
    /// The enum is implemented as a nullable so that you can set it to null to specify when no scene is loaded.
    /// </summary>
    /// <typeparam name="ScenesType"></typeparam>
    public abstract class WorldSceneControllerBase<ScenesType> : WorldEventListenerBase
        where ScenesType : struct, Enum
    {
        [SerializeField] protected float _sceneTransitionDelay = 0.25f;

        [SerializeField] private bool _showDebugLogs = false;
        [SerializeField, ConditionalField("_showDebugLogs")] private Color _debugLogColor = Color.black;

        protected Dictionary<ScenesType?, SceneReference> _internalSceneDictionary = new Dictionary<ScenesType?, SceneReference>();

        private Queue<(ScenesType? Type, LoadSceneMode Mode)> _sceneQueue = new Queue<(ScenesType? Type, LoadSceneMode Mode)>();
        private (ScenesType? Type, LoadSceneMode Mode) _currentScene = (null, LoadSceneMode.Additive);
        private IGameSceneController _currentSceneController = null;

        private WorldSceneTransitionEvent _sceneTransitionEvent = new WorldSceneTransitionEvent("WorldSceneController");
        private WorldSceneChangedEvent<ScenesType> _sceneChangedEvent = new WorldSceneChangedEvent<ScenesType>("WorldSceneController");

        [EventReceiver]
        protected void OnChangeSceneEvent(WorldChangeSceneEvent<ScenesType> eventObject)
        {
            _sceneQueue.Enqueue((eventObject.Scene, eventObject.Mode));
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            while(true)
            {
                yield return new WaitUntil(() => _sceneQueue.Count > 0);

                _dispatcher.SendEvent(_sceneTransitionEvent);
                OnSceneTransitionStarted();
                yield return new WaitForSeconds(_sceneTransitionDelay);

                if (_currentScene.Type != null)
                {
                    _currentSceneController.OnExit();
                    _currentSceneController = null;

                    AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath);
                    yield return new WaitUntil(() => asyncOperation.isDone);

                    if (_showDebugLogs)
                        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nOld loaded scene unloaded successfully.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));
                }
                else
                {
                    if (_showDebugLogs)
                        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nNo scene was loaded, thus, no unloading was done.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));
                }

                _currentScene = _sceneQueue.Dequeue();

                if (_currentScene.Type != null)
                {
                    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath, _currentScene.Mode);
                    yield return new WaitUntil(() => asyncOperation.isDone);

                    if (_showDebugLogs)
                        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nNewly requested scene loaded successfully.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));

                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);

                        if (scene.path != _internalSceneDictionary[_currentScene.Type].ScenePath)
                            continue;

                        _currentSceneController = scene.GetRootGameObjects()
                            .Select(obj => obj.GetComponent<IGameSceneController>())
                            .Where(controller => controller != null).First();
                    }

                    if (_currentSceneController != null)
                    {
                        _currentSceneController.OnEnter();

                        if (_showDebugLogs)
                            Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nGameSceneController found in newly loaded scene.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));

                        _sceneChangedEvent.Reset(_currentScene.Type.Value, _currentSceneController);
                        _dispatcher.SendEvent(_sceneChangedEvent);

                        OnSceneTransitionEnded();
                    }
                    else
                    {
                        if (_showDebugLogs)
                            Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nGameSceneController not found in newly loaded scene. Now unloading.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));

                        asyncOperation = SceneManager.UnloadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath);
                        yield return new WaitUntil(() => asyncOperation.isDone);

                        _currentScene = (default(ScenesType), LoadSceneMode.Additive);
                    }
                }
                else
                {
                    if (_showDebugLogs)
                        Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}ff>[WorldSceneController]</color>\nNewly requested scene is null. No loading done.", (int)(_debugLogColor.r * 255), (int)(_debugLogColor.g * 255), (int)(_debugLogColor.b * 255)));
                }
            }
        }

        protected virtual void OnSceneTransitionStarted() { }
        protected virtual void OnSceneTransitionEnded() { }

        // A dirty solution to a problem. Simply put, I cannot find how to display a 2 value list in the inspector, whether as a dictionary, tuple or subclass.
        // It would normally be easy, but in the case where one of the value is a generic enum, any attempt has failed. Thus, each implementation should handle populating the dictionary by itself.
        protected abstract void InitializeSceneDictionary();

        protected override void Awake()
        {
            base.Awake();

            InitializeSceneDictionary();
            StartCoroutine(SceneTransitionCoroutine());
        }
    }
}