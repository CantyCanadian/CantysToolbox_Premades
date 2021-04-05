//***************************************************************
//
//		Game Management System
//		By CantyCanadian
//
//***************************************************************

Here's a quick readme on what this is and what it can be used for.
Just like other premades, this should be used at your own risk, as it might not fit the needs of your project.
However, it was still developped with flexibility in mind.

The game management system handles the process that is first to start after a game boots. It handles the initialization
of everything that is needed for the game to work properly. While Unity offers a variety of options like Awake and Start
for initialization, there are cases, like scenes and player spawning, that should be handled manually to prevent bugs
and to make sure everything is sequenced properly. 

This management system works via the following scripts :

> EventSystem/WorldEventDispatcher.cs
>		Acts as the main event dispatcher for the game-layer where game-scoped events happen. It is also the singleton 
>		for this system as a way to allow any script to send world events.

> EventSystem/WorldEventListenerBase.cs
>		Acts as the parent class for most of the controllers here, since each of them needs a back-and-forth link with
>		the dispatcher.

> EventSystem/Events
>		Every game-layer events, inheriting from WorldEventBase.

> WorldGameControllerBase.cs
>		Handles the initialization and reset of the game. Contains the main Awake() that starts the whole process.

> WorldSceneControllerBase.cs <ScenesType>
>		Handles scene transitions for the game. However, note that it only handles the transitions under the hood 
>		and thus might need support from a visual transition (like fade to black) to make it look less jarring.
>		Must be inherited from and given an enum containing every game scene, as a way to associate them with
>		SceneReference objects and to allow scene changes. Said dictionary must also be initialized manually with
>		the abstract function due to Unity inspector shenanigans.

> IGameSceneController.cs
>		Acts as the entry point of any scene loaded by the WorldSceneControllerBase. If you want stuff to run during
>		OnEnter and OnExit of a scene, this is your guy. Note, one must be present in any scene minus the initial scene.

To use it for your own game, simply create a child class of the EventDispatcher, GameController and SceneController,
then you may create your own listeners / events to start your own processes at any point in this initialization pipeline.