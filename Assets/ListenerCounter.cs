using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class ListenerCounter
{
	public static int GetListenerNumber(this UnityEventBase unityEvent) {
		var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
		var invokeCallList = field.GetValue(unityEvent);
		var property = invokeCallList.GetType().GetProperty("Count");
		return (int)property.GetValue(invokeCallList);
	}
}
