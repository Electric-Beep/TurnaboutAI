using TurnaboutAI;
using System;
using UnityEngine;

namespace Doorstop
{
    /// <summary>
    /// Doorstop entrypoint.
    /// </summary>
    public static class Entrypoint
    {
        private static bool _patched = false;

        public static void Start()
        {
            Plugin.LogInfo("Started");

            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            }
            catch(Exception ex)
            {
                Plugin.LogInfo(ex.ToString());
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Plugin.LogError(e.ExceptionObject);
        }

        private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            Plugin.LogInfo($"Unity Version: {Application.unityVersion}");

            if (!_patched)
            {
                _patched = true;
                try
                {
                    GameObject main = null;

                    foreach(var root in arg0.GetRootGameObjects())
                    {
                        if(root.name == "main")
                        {
                            main = root;
                            break;
                        }
                    }

                    Plugin.Init(main);
                }
                catch(Exception ex)
                {
                    Plugin.LogInfo(ex.ToString());
                }
            }
        }
    }
}
