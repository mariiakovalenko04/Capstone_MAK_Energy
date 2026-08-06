using System;
using UnityEngine;
using AK.Wwise.Unity.Logging;

public static class AkDelegates
{
    /// <summary>
    /// Invokes a parameterless Action, safely handling null/destroyed UnityEngine.Object targets.
    /// Automatically unsubscribes destroyed Unity objects.
    /// </summary>
    /// <param name="action">The Action to invoke.</param>
    public static void InvokeUnitySafe(this System.Action action)
    {
        if (action == null)
            return;

        Delegate[] subscribers = action.GetInvocationList();

        foreach (Delegate del in subscribers)
        {
            if (del.Target is UnityEngine.Object unityTarget)
            {
                if (unityTarget == null)
                {
                    WwiseLogger.Log(
                        $"Removing stale delegate from Action. Method: {del.Method.Name}");

                    action -= (Action)del;
                    continue;
                }
            }

            try
            {
                ((Action)del).Invoke();
            }
            catch (MissingReferenceException missingReferenceException)
            {
                WwiseLogger.Error(
                    $"Missing Reference Exception caught during safe Invoke. Method: {del.Method.Name}. Error: {missingReferenceException.Message}");

                if (del.Target is UnityEngine.Object unityTargetOnException &&
                    unityTargetOnException == null)
                {
                    action -= (Action)del;
                }
            }
            catch (Exception ex)
            {
                WwiseLogger.Error(
                    $"Unexpected exception during safe Invoke. Method: {del.Method.Name}. Error: {ex.Message}");
            }
        }
    }
}