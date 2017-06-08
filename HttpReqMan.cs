using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class HttpReqMan : MonoBehaviour {

	public class Request {
		public WWW www { get; protected set; }
		public float eol_timestamp { get; protected set; }
		public UnityAction<WWW> on_success { get; protected set; }
		public UnityAction<WWW> on_error { get; protected set; }

		public Request(WWW _www, float _eol_timestamp, UnityAction<WWW> _on_success,
			UnityAction<WWW> _on_error) {

			www = _www;
			eol_timestamp = _eol_timestamp;
			on_success = _on_success;
			on_error = _on_error;
		}

		public void do_on_error() {
			if (on_error != null)
				on_error (www);
		}

		public void do_on_success() {
			if (on_success != null)
				on_success (www);
		}

		public void Cancel() {
			HttpReqMan.CancelRequest (this);
		}
	};

	List<Request> _Queue = new List<Request>();

	static HttpReqMan _Singleton;
	static GameObject _GameObject;

	public static HttpReqMan Singleton {
		get {
			if (_Singleton == null) {
				_GameObject = new GameObject ("_httpReqMan");
				_Singleton = _GameObject.AddComponent<HttpReqMan> ();
			}
			return _Singleton;
		}
	}

	public static Request MakeReqest(string url,
		byte[] data,
		Dictionary<string, string> headers,
		float timeout,
		UnityAction<WWW> on_success,
		UnityAction<WWW> on_error) {


		WWW www;
		if (data != null && headers != null) {
			www = new WWW (url, data, headers);
		} else if (data != null) {
			www = new WWW (url, data);
		} else {
			www = new WWW (url);
		}

		Request req_1 = new Request (www, Time.time + timeout, on_success, on_error);
		Singleton._Queue.Add (req_1);

		return req_1;
	}

	public static void CancelRequest(Request req) {
		req.www.Dispose ();
		Singleton._Queue.Remove (req);
		Debug.Log ("Request cancelled");
	}

	void Update() {
		float t = Time.time;
		List<Request> _Pending = new List<Request> ();
		foreach (Request req in _Queue) {
			if (t > req.eol_timestamp && req.www.isDone == false) {
				// timeout
				req.do_on_error();
			} else if (req.www.isDone) {
				// completed
				if (req.www.error == null) {
					// with success
					req.do_on_success();
				} else {
					// with built-in timeout or error
					req.do_on_error();
				}
			} else {
				// still pending
				_Pending.Add(req);
			}
		}
		_Queue = _Pending;
	}

	void Awake() {
		GameObject.DontDestroyOnLoad (_GameObject);
	}
}
