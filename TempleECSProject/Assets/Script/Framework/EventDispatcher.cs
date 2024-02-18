using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemoObserver
{

    public class EventDispatcher : MonoBehaviour
    {
        #region Singleton
        static EventDispatcher s_instance;
        public static EventDispatcher Instance
        {
            get
            {
                // instance not exist, then create new one
                if (s_instance == null)
                {
                    // create new Gameobject, and add EventDispatcher component
                    GameObject singletonObject = new GameObject();
                    s_instance = singletonObject.AddComponent<EventDispatcher>();
                    singletonObject.name = "Singleton - EventDispatcher";
                    Common.Log("Create singleton : {0}", singletonObject.name);
                }
                return s_instance;
            }
            private set { }
        }

        public static bool HasInstance()
        {
            return s_instance != null;
        }

        void Awake()
        {
            // check if there's another instance already exist in scene
            if (s_instance != null && s_instance.GetInstanceID() != this.GetInstanceID())
            {
                // Destroy this instances because already exist the singleton of EventsDispatcer
                Common.Log("An instance of EventDispatcher already exist : <{1}>, So destroy this instance : <{2}>!!", s_instance.name, name);
                Destroy(gameObject);
            }
            else
            {
                // set instance
                s_instance = this as EventDispatcher;
            }
        }


        void OnDestroy()
        {
            // reset this static var to null if it's the singleton instance
            if (s_instance == this)
            {
                ClearAllListener();
                s_instance = null;
            }
        }
        #endregion


        #region Fields
        /// Store all "listener"
        Dictionary<EventID, Action<object>> _listeners = new Dictionary<EventID, Action<object>>();

        #endregion


        #region Add Listeners, Post events, Remove listener

        public void RegisterListener(EventID eventID, Action<object> callback)
        {
            Common.Assert(callback != null, "AddListener, event {0}, callback = null !!", eventID.ToString());
            Common.Assert(eventID != EventID.None, "RegisterListener, event = None !!");
            // check if listener exist in distionary
            if (_listeners.ContainsKey(eventID))
            {
                _listeners[eventID] += callback;
            }
            else
            {
                _listeners.Add(eventID, null);
                _listeners[eventID] += callback;
            }
        }

        public void PostEvent(EventID eventID, object param = null)
        {
            if (!_listeners.ContainsKey(eventID))
            {
                Common.Log("No listeners for this event : {0}", eventID);
                return;
            }

            // posting event
            var callbacks = _listeners[eventID];
            if (callbacks != null)
            {
                callbacks(param);
            }
            else
            {
                Common.Log("PostEvent {0}, but no listener remain, Remove this key", eventID);
                _listeners.Remove(eventID);
            }
        }

        public void RemoveListener(EventID eventID, Action<object> callback)
        {
            // checking params
            Common.Assert(callback != null, "RemoveListener, event {0}, callback = null !!", eventID.ToString());
            Common.Assert(eventID != EventID.None, "AddListener, event = None !!");

            if (_listeners.ContainsKey(eventID))
            {
                _listeners[eventID] -= callback;
            }
            else
            {
                Common.Warning(false, "RemoveListener, not found key : " + eventID);
            }
        }

        public void ClearAllListener()
        {
            _listeners.Clear();
        }
        #endregion
    }


    #region Extension class
    public static class EventDispatcherExtension
    {
        /// Use for registering with EventsManager
        public static void RegisterListener(this MonoBehaviour listener, EventID eventID, Action<object> callback)
        {
            // eventID = EventID.None;
            EventDispatcher.Instance.RegisterListener(eventID, callback);
        }

        /// Post event with param
        public static void PostEvent(this MonoBehaviour listener, EventID eventID, object param)
        {
            EventDispatcher.Instance.PostEvent(eventID, param);
        }

        /// Post event with no param (param = null)
        public static void PostEvent(this MonoBehaviour sender, EventID eventID)
        {
            EventDispatcher.Instance.PostEvent(eventID, null);
        }
    }
    #endregion
}