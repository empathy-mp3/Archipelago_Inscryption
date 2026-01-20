using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Archipelago_Inscryption.Utils
{
    internal static class FileSystem
    {
        private static bool usingHuey = false;

        private static MethodInfo hueyCreateDirectory;
        private static MethodInfo hueyDeleteDirectory;
        private static MethodInfo hueyDirectoryExists;
        private static MethodInfo hueyFileExists;
        private static MethodInfo hueyWriteAllText;
        private static MethodInfo hueyReadAllText;
        private static MethodInfo hueyFileSystemGetter;
        private static MethodInfo hueyMakePath;
        private static FieldInfo gameCoreStorageSystem;
        private static FieldInfo zioMemoryFileSystemField;
        private static MethodInfo zioFindNodeSafe;
        private static MethodInfo zioNodeChildrenGetter;
        private static MethodInfo zioGetLastWriteTime;
        private static MethodInfo zioNodeNameGetter;
        private static MethodInfo applicationDataPathGetter;

        private static Type directoryNodeType;

        internal static void Init()
        {
            // Check for the existence of Huey for Game Pass
            System.Type fileSystemType = AccessTools.TypeByName("Huey.Core.Utilities.FileSystemInMemory");

            if (fileSystemType == null)
            {
                ArchipelagoModPlugin.Log.LogInfo("Huey file system does not exist. This is likely because this is the Steam version of Inscryption. Using base file system.");
            }
            else
            {
                ArchipelagoModPlugin.Log.LogInfo("Huey file system found. This is likely because this is the Game Pass version of Inscryption. Using Huey file system.");
                usingHuey = true;
                InitHuey();
            }
        }

        private static void InitHuey()
        {
            Assembly hueyAssembly = AccessTools.AllAssemblies().FirstOrDefault(x => x.FullName.Contains("HueyCore"));

            if (hueyAssembly == null)
            {
                ArchipelagoModPlugin.Log.LogError("Couldn't find HueyCore assembly.");
                return;
            }

            Type fileType = hueyAssembly.GetType("File");
            Type directoryType = hueyAssembly.GetType("Directory");

            hueyCreateDirectory = AccessTools.Method(directoryType, "CreateDirectory");
            hueyDeleteDirectory = AccessTools.Method(directoryType, "Delete", [typeof(string), typeof(bool)]);
            hueyDirectoryExists = AccessTools.Method(directoryType, "Exists");
            hueyFileExists = AccessTools.Method(fileType, "Exists");
            hueyWriteAllText = AccessTools.Method(fileType, "WriteAllText", [typeof(string), typeof(string)]);
            hueyReadAllText = AccessTools.Method(fileType, "ReadAllText", [typeof(string)]);
            hueyFileSystemGetter = AccessTools.PropertyGetter(fileType, "HueyFS");

            gameCoreStorageSystem = AccessTools.Field(AccessTools.TypeByName("Huey.Core.GameCoreStorageSystem"), "_fileSystemInMemory");

            Type hueyFileSystem = AccessTools.TypeByName("Huey.Core.Utilities.FileSystemInMemory");

            zioMemoryFileSystemField = AccessTools.Field(hueyFileSystem, "_memoryFileSystem");
            hueyMakePath = AccessTools.Method(hueyFileSystem, "MakePath");

            Type zioMemoryFileSystemType = AccessTools.TypeByName("Zio.FileSystems.MemoryFileSystem");

            zioFindNodeSafe = AccessTools.Method(zioMemoryFileSystemType, "FindNodeSafe");
            zioGetLastWriteTime = AccessTools.Method(AccessTools.TypeByName("Zio.FileSystems.FileSystem"), "GetLastWriteTime");

            directoryNodeType = AccessTools.Inner(zioMemoryFileSystemType, "DirectoryNode");

            zioNodeChildrenGetter = AccessTools.PropertyGetter(directoryNodeType, "Children");
            zioNodeNameGetter = AccessTools.PropertyGetter(AccessTools.Inner(zioMemoryFileSystemType, "FileSystemNode"), "Name");

            applicationDataPathGetter = AccessTools.PropertyGetter(hueyAssembly.GetType("Application"), "dataPath");
        }

        public static void CreateDirectory(string path)
        {
            if (usingHuey)
            {
                hueyCreateDirectory.Invoke(null, [path]);
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (usingHuey)
            {
                hueyDeleteDirectory.Invoke(null, [path, true]);
            }
            else
            {
                Directory.Delete(path, true);
            }
        }

        public static bool DirectoryExists(string path)
        {
            if (usingHuey)
            {
                return (bool)hueyDirectoryExists.Invoke(null, [path]);
            }
            else
            {
                return Directory.Exists(path);
            }
        }

        public static bool FileExists(string path)
        {
            if (usingHuey)
            {
                return (bool)hueyFileExists.Invoke(null, [path]);
            }
            else
            {
                return File.Exists(path);
            }
        }

        public static string ReadAllText(string path)
        {
            if (usingHuey)
            {
                return (string)hueyReadAllText.Invoke(null, [path]);
            }
            else
            {
                return File.ReadAllText(path);
            }
        }

        public static void WriteAllText(string path, string content)
        {
            if (usingHuey)
            {
                hueyWriteAllText.Invoke(null, [path, content]);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }

        public static string[] GetDirectories(string path)
        {
            if (usingHuey)
            {
                object zioFileSystemInstance = GetZioMemoryFileSystemInstance();
                ArchipelagoModPlugin.Log.LogInfo("Finding node...");
                object pathNode = zioFindNodeSafe.Invoke(zioFileSystemInstance, [MakePath(path), false]);

                if (pathNode.GetType() == directoryNodeType)
                {
                    ArchipelagoModPlugin.Log.LogInfo("Node is directory. Getting children nodes...");
                    IDictionary children = (IDictionary)zioNodeChildrenGetter.Invoke(pathNode, null);

                    List<string> directories = new();

                    foreach (var child in children.Values)
                    {
                        ArchipelagoModPlugin.Log.LogInfo("Adding name of child node to list...");
                        directories.Add(Path.Combine(path, (string)zioNodeNameGetter.Invoke(child, null)));
                    }

                    return directories.ToArray();
                }

                return null;
            }
            else
            {
                return Directory.GetDirectories(path);
            }
        }

        public static DateTime GetLastWriteTime(string path)
        {
            if (usingHuey)
            {
                return (DateTime)zioGetLastWriteTime.Invoke(GetZioMemoryFileSystemInstance(), [MakePath(path)]);
            }
            else
            {
                return File.GetLastWriteTime(path);
            }
        }

        public static string GetDataPath()
        {
            if (usingHuey)
            {
                return (string)applicationDataPathGetter.Invoke(null, null);
            }
            else
            {
                return Application.dataPath;
            }
        }

        private static object GetZioMemoryFileSystemInstance()
        {
            return zioMemoryFileSystemField.GetValue(gameCoreStorageSystem.GetValue(hueyFileSystemGetter.Invoke(null, null)));
        }

        private static object MakePath(string path)
        {
            return hueyMakePath.Invoke(gameCoreStorageSystem.GetValue(hueyFileSystemGetter.Invoke(null, null)), [path]);
        }
    }
}
