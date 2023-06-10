using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WizardsCode.Ink;
using UnityEngine.UI;
using System;

namespace WizardsCode.Ink
{
    /// <summary>
    /// Set ah image element to a specific image.
    /// 
    /// Usage:
    /// 
    /// Display an image on an RawImage component attached to an ojbect named Background
    /// >>> Image: filepathRelativeToInkFile
    /// 
    /// Display an image on an RawImage component attached to an object with a supplied name
    /// >>> Image: filepathRelativeToInkFile, nameOfImageObject
    /// 
    /// Remove and hide the RawImage object named Background
    /// >>> Image: null
    /// 
    /// Remove and hide the RawImage object with a supplied name
    /// >>> Image: null, nameOfImageObject
    /// </summary>
    public class ImageDirection : AbstractDirection
    {
        private string lastImageObjectName;
        private RawImage image;
        private RawImage secondaryImage;
        private string lastImage;

        public override void Execute(string[] parameters)
        {
            string imageObjectName = "Background";
            if (parameters.Length > 1)
            {
                imageObjectName = parameters[1];
            }

            if (imageObjectName != lastImageObjectName)
            {
                lastImageObjectName = imageObjectName;
                GameObject imageObject = GameObject.Find(imageObjectName);
                if (imageObject == null)
                {
                    LogError("Could not find a game object with the name " + imageObjectName, parameters);
                    return;
                }
                image = imageObject.GetComponent<RawImage>();
                
                GameObject secondaryImageObject = GameObject.Find(SecondayObjectsPrefix + imageObjectName);
                if (secondaryImageObject != null)
                {
                    secondaryImage = secondaryImageObject.GetComponent<RawImage>();
                }
            }

            if (image != null)
            {
                Texture2D texture = null;

                if (parameters[0].Trim() == "null")
                {
                    lastImage = "null";
                    texture = null;
                } else if (lastImage != parameters[0])
                {
                    lastImage = parameters[0];

                    string path = parameters[0];
                    texture = Resources.Load<Texture2D>(path);

                    if (texture == null)
                    {
                        LogError($"Could not find the image requested at `{path}`.", parameters);
                    }
                }

                if (texture != null)
                {
                    image.texture = texture;
                    image.enabled = true;

                    if (secondaryImage != null)
                    {
                        secondaryImage.texture = texture;
                        secondaryImage.enabled = true;
                    }
                } else
                {
                    image.enabled = false;
                    if (secondaryImage != null)
                    {
                        secondaryImage.enabled = false;
                    }
                }
            }
        }
    }
}
