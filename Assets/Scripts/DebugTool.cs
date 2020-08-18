using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class DebugTool
{
    //TODO: to replace by Debug.LogError 
    public static bool printError(string message,string gameObjectName="",string componentName="")
    {
        Debug.Log("Error - " +componentName + "/" + gameObjectName + " - " + message);
        return false;
    }


    //check if a gameobject have the GONameToFind in this children
    public static bool tryFindGOChildren(GameObject GOParent, string GONameToFind, out GameObject GOToReturn, LogType debugType = LogType.Warning)
    {
        GOToReturn = GOParent.transform.gameObject;
        try
        {
            GOToReturn = GOParent.transform.Find(GONameToFind).gameObject;
            
        }
        catch (NullReferenceException)
        {
            switch(debugType)
            {
                case (LogType.Log):
                    Debug.Log("Unable to find " + GONameToFind + " in " + GOParent.gameObject.name);
                    break;
                case (LogType.Warning):
                    Debug.LogWarning("Unable to find " + GONameToFind + " in " + GOParent.gameObject.name);
                    break;
                case (LogType.Error):
                    Debug.LogError("Unable to find " + GONameToFind + " in " + GOParent.gameObject.name);
                    break;
                default:
                    Debug.LogWarning("LogType undefined " + debugType);
                    Debug.LogWarning("Unable to find " + GONameToFind + " in " + GOParent.gameObject.name);
                    break;
            }
            return false;
        }
        return true;
    }

    public static void addGizmosText(Vector3 positionToAdd, string textToAdd)
    {
        
    }
}

