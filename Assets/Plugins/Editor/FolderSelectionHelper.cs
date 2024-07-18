using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

//Code by Albarnie

[InitializeOnLoad]
public class FolderSelectionHelper
{
    //The last object we had selected
    public static Object LastActiveObject;
    //Object to select the next frame
    public static Object ObjectToSelect;
    //Whether we have locked the inspector
    public static bool HasLocked;

    static FolderSelectionHelper()
    {
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnUpdate;

        //Restore folder lock status
        HasLocked = EditorPrefs.GetBool("FolderSelectionLocked", false);
    }

    static void OnSelectionChanged()
    {
        if (LastActiveObject != null && Selection.activeObject != null)
        {
            //If the selection has actually changed
            if (Selection.activeObject != LastActiveObject)
            {
                //If the new object is a folder, reselect our old object
                if (IsAssetAFolder(Selection.activeObject))
                {
                    //We have to select the object the next frame, otherwise it will not register
                    ObjectToSelect = LastActiveObject;
                }
                else
                {
                    UnLockFolders();
                    //Update the last object
                    LastActiveObject = Selection.activeObject;
                }
            }
        }
        else if (!IsAssetAFolder(Selection.activeObject))
        {
            LastActiveObject = Selection.activeObject;
            UnLockFolders();
        }

    }

    //We have to do selecting in the next editor update because Unity does not allow selecting another object in the same editor update
    static void OnUpdate()
    {
        //If the editor is locked then we don't care
        if (ObjectToSelect != null && !ActiveEditorTracker.sharedTracker.isLocked)
        {
            //Select the new object
            Selection.activeObject = ObjectToSelect;

            LockFolders();

            LastActiveObject = ObjectToSelect;
            ObjectToSelect = null;
        }
        else
        {
            ObjectToSelect = null;
        }
    }

    static void LockFolders()
    {
        ActiveEditorTracker.sharedTracker.isLocked = true;
        HasLocked = true;
        //We store the state so that if we compile or leave the editor while the folders are locked then the state is kept
        EditorPrefs.SetBool("FolderSelectionLocked", true);
    }

    static void UnLockFolders()
    {
        //Only unlock inspector if we are the one who locked it
        if (HasLocked)
        {
            ActiveEditorTracker.sharedTracker.isLocked = false;
            HasLocked = false;
            EditorPrefs.SetBool("FolderSelectionLocked", false);
        }
    }

    private static bool IsAssetAFolder(Object obj)
    {
        string path = "";

        if (obj == null)
        {
            return false;
        }

        //Get the path to the asset
        path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

        //If the asset is a directory (i.e a folder)
        if (path.Length > 0 && Directory.Exists(path))
        {
            return true;
        }

        return false;
    }

}