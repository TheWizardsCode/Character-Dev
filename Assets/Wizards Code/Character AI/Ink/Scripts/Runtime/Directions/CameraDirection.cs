#if INK_PRESENT
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Ink;
using Cinemachine;

namespace WizardsCode.Ink
{
    /// <summary>
    /// Switch to a specific camera and optionally follow and or look at a named object.
    ///
    /// </summary>
    /// <param name="args">[CameraName] [FollowTargetName] [LookAtTargetName]</param>
    public class CameraDirection : AbstractDirection
    {
        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 3))
            {
                return;
            }
            
            CinemachineCamera newCamera;
            Transform camera = Manager.FindTarget(parameters[0].Trim());
            if (camera)
            {
                newCamera = camera.gameObject.GetComponent<CinemachineCamera>();
                if (Manager.cinemachine.ActiveVirtualCamera != (ICinemachineCamera)newCamera)
                {
                    Manager.cinemachine.ActiveVirtualCamera.Priority = 10;

                    if (Manager.m_FadeCamera)
                    {
                        Manager.StartCoroutine(Manager.CrossFadeCamerasCo(newCamera));
                    }
                    else
                    {
                        newCamera.Priority = 99;
                    }
                }
                
                Transform objectName;
                if (parameters.Length >= 2)
                {
                    objectName = Manager.FindTarget(parameters[1].Trim());
                    if (objectName)
                    {
                        if (parameters.Length == 2)
                        {
                            newCamera.Follow = objectName;
                            newCamera.LookAt = objectName;
                        }
                        else
                        {
                            Transform childObject = Manager.FindChild(objectName, parameters[2].Trim());
                            if (childObject)
                            {
                                newCamera.Follow = childObject;
                                newCamera.LookAt = childObject;
                            }
                            else
                            {
                                newCamera.Follow = objectName;
                                newCamera.LookAt = objectName;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Recieved direction to switch to camera called {parameters[0].Trim()}, however, no such camera could not be found: ");
            }
        }
    }
}
#endif