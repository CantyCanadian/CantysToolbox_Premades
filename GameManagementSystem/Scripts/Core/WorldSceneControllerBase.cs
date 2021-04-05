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
    /// Please note that enum value 0 should represent a Null or equivalent, as it is used in the code as the "no scene" ID.
    /// </summary>
    /// <typeparam name="ScenesType"></typeparam>
    public abstract class WorldSceneControllerBase<ScenesType> : WorldEventListenerBase
        where ScenesType : Enum
    {
        protected Dictionary<ScenesType, SceneReference> _internalSceneDictionary = new Dictionary<ScenesType, SceneReference>();

        private Queue<(ScenesType Type, LoadSceneMode Mode)> _sceneQueue = new Queue<(ScenesType Type, LoadSceneMode Mode)>();
        private (ScenesType Type, LoadSceneMode Mode) _currentScene = (default(ScenesType), LoadSceneMode.Additive);
        private IGameSceneController _currentSceneController = null;

        private WorldSceneTransitionEvent _sceneTransitionEvent = new WorldSceneTransitionEvent("WorldSceneController");
        private WorldSceneChangedEvent<ScenesType> _sceneChangedEvent = new WorldSceneChangedEvent<ScenesType>("WorldSceneController");

        [EventReceiver]
        public void OnChangeSceneEvent(WorldChangeSceneEvent<ScenesType> eventObject)
        {
            _sceneQueue.Enqueue((eventObject.Scene, eventObject.Mode));
        }

        private IEnumerator SceneTransitionCoroutine()
        {
            while(true)
            {
                yield return new WaitUntil(() => _sceneQueue.Count > 0);

                m_Dispatcher.SendEvent(_sceneTransitionEvent);
                if (!EqualityComparer<ScenesType>.Default.Equals(_currentScene.Type, default(ScenesType)))
                {
                    _currentSceneController.OnExit();
                    _currentSceneController = null;

                    AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath);
                    yield return new WaitUntil(() => asyncOperation.isDone);
                }

                _currentScene = _sceneQueue.Dequeue();

                if (!EqualityComparer<ScenesType>.Default.Equals(_currentScene.Type, default(ScenesType)))
                {
                    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath, _currentScene.Mode);
                    yield return new WaitUntil(() => asyncOperation.isDone);
                    
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

                        _sceneChangedEvent.Reset(_currentScene.Type, _currentSceneController);
                        m_Dispatcher.SendEvent(_sceneChangedEvent);
                    }
                    else
                    {
                        asyncOperation = SceneManager.UnloadSceneAsync(_internalSceneDictionary[_currentScene.Type].ScenePath);
                        yield return new WaitUntil(() => asyncOperation.isDone);

                        _currentScene = (default(ScenesType), LoadSceneMode.Additive);
                    }
                }
            }
        }

        // A dirty solution to a problem. Simply put, I cannot find how to display a 2 value list in the inspector, whether as a dictionary, tuple or subclass.
        // It would normally be easy, but in the case where one of the value is a generic enum, any attempt has failed. Thus, each implementation should handle populating the dictionary by itself.
        protected abstract void InitializeSceneDictionary();

        protected override void Awake()
        {
            base.Awake();

            InitializeSceneDictionary();
            StartCoroutine(SceneTransitionCoroutine());
        }

        [Serializable] public class TestDictionary : SerializableDictionary<int, float> { }
        [Serializable] public class ScenesDictionary : SerializableDictionary<ScenesType, SceneReference> { }
    }
}