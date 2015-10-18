using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WarWorldInfServer
{
	public class TaskQueue
	{
		private List<Action> _actions = new List<Action>();
		private List<Action> _currentActions = new List<Action>();
		private Dictionary<string, AsyncTask> _asyncTasks = new Dictionary<string, AsyncTask> ();
		private static TaskQueue _instance;

		public TaskQueue ()
		{
			_instance = this;
		}

		public void Update(){
			if (_actions.Count > 0) {
				lock (_actions) {
					_currentActions.Clear ();
					_currentActions.AddRange (_actions);
					_actions.Clear ();
				}
				for (int i = 0; i < _currentActions.Count; i++) {
					try {
						_currentActions [i] ();
						_currentActions [i] = null;
					}
					catch(Exception e){
						Logger.LogError("Queue: {0}", e.Message);
						Logger.LogError(e.StackTrace);
						_currentActions[i] = null;
					}
				}
			}
		}

		public static void QueueMain(Action action) {
			if (_instance != null && _instance._actions != null) {
				lock (_instance._actions){
					_instance._actions.Add(action);
				}
			}
		}

		public static void QeueAsync(string thread, Action e){
			if (_instance != null && _instance._asyncTasks != null) {
				lock(_instance._asyncTasks){
					if (_instance._asyncTasks.ContainsKey(thread)){
						_instance._asyncTasks[thread].AddTask(e);
					}
					else{
						AddAsyncQueue(thread);
						QeueAsync(thread, e);
					}
				}
			}
		}

		public static void AddAsyncQueue(string thread){
			if (_instance != null && _instance._asyncTasks != null) {
				lock (_instance._asyncTasks){
					if (!_instance._asyncTasks.ContainsKey(thread)){
						AsyncTask task = new AsyncTask(thread);
						_instance._asyncTasks.Add(thread, task);
					}
				}
			}
		}

		public static bool ThreadExists(string thread) {
			return _instance._asyncTasks.ContainsKey(thread);
		}

		public static Thread GetThreadRef(string thread){
			if (_instance != null && _instance._asyncTasks != null) {
				lock (_instance._asyncTasks){
					if  (_instance._asyncTasks.ContainsKey(thread)){
						return _instance._asyncTasks[thread].thread;
					}
					else return null;
				}
			}
			return null;
		}
	}
}

