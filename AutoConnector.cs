//
// Auto-connector a'la Qt
// Copyright (C) Stanislaw Adaszewski, 2016
// http://algoholic.eu
// License: 2-clause BSD
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Events;

public class AutoConnector {

	public static void autoFind(object o) {
		FieldInfo[] fields = o.GetType ().GetFields();
		for (int i = 0; i < fields.Length; i++) {
			FieldInfo fi = fields [i];
			GameObject go = GameObject.Find (fi.Name);
			if (go == null)
				continue;
			fi.SetValue (o, go);
			Debug.Log (string.Format ("Successfully auto-found {0}", fi.Name));
		}
	}
	
	public static void autoConnect(Object o) {
		MethodInfo[] methods = o.GetType ().GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		for (int i = 0; i < methods.Length; i++) {
			MethodInfo mi = methods [i];
			if (!mi.Name.StartsWith ("on_"))
				continue;

			string[] parts = mi.Name.Split ('_');
			if (parts.Length != 3)
				continue;
			string objName = parts [1];
			string eventName = parts [2];

			GameObject go = GameObject.Find (objName);
			if (go == null)
				continue;

			Object[] comps = go.GetComponents<Component>();
			int k;
			for (k = 0; k < comps.Length; k++) {
				if (comps[k].GetType ().GetProperty ("on" + eventName) != null)
					break;
			}

			if (k == comps.Length)
				continue;

			Object component = comps [k];

			PropertyInfo pi = component.GetType ().GetProperty ("on" + eventName);
			if (pi == null)
				continue;

			object eo = (object) pi.GetValue (component, null);
			MethodInfo ali = eo.GetType ().GetMethod ("AddListener");
			if (ali == null)
				continue;
			ali.Invoke (eo, new object[] { new UnityAction(() => {
				mi.Invoke(o, null);
			}) });

			Debug.Log (string.Format ("Successfully autoconnected {0}", mi.Name));
		}
	}
}
