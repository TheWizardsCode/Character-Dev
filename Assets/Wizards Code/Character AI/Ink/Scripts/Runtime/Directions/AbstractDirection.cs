using UnityEngine;
using WizardsCode.Character;

namespace WizardsCode.Ink
{
    /// <summary>
    /// A direction is an Ink instruction that is denoted by `>>>` in the text.
    /// They have the format `>>> Name: Parameters` where `name` is the name of the direction and `parameters are a comma separated list of parameters for that direction.
    /// When the InkManager encounters a direction it will look for a class that implements this interface and call the method on that class.
    /// 
    /// Direction classes are identified by their name, which is the name of the direction followed by `Direction`, for example `NameDirection`.
    /// Once found the `Execute` method is called with the parameter string passed in.
    /// All Direction classes are discovered when the InkManager is loaded, using reflection. This means that you can create your own directions by creating a class that implements this abstract class.
    /// </summary>
    public abstract class AbstractDirection
    {
        static InkManager _manager;
        internal static string SecondayObjectsPrefix = "GM_";

        internal static InkManager Manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = GameObject.FindObjectOfType<InkManager>();
                }
                return _manager;
            }
        }

        abstract public void Execute(string[] parameters);

        internal void LogError(string message, string[] parameters)
        {
            Debug.LogError($"{message}\nCaused by Ink direction command implemented by {this}\n\nParameters: {string.Join(", ", parameters)}");
        }
        internal void LogWarning(string message, string[] parameters)
        {
            Debug.LogWarning($"{message}\nCaused by Ink direction command implemented by {this}\n\nParameters: {string.Join(", ", parameters)}");
        }

        internal bool ValidateArgumentCount(string[] args, int minRequiredCount, int maxRequiredCount = 0)
        {
            string error = "";
            string warning = "";

            if (args.Length < minRequiredCount)
            {
                error = "Too few arguments in Direction. There should be at least " + minRequiredCount + ". Ignoring direction: ";
            }
            else if (maxRequiredCount > 0)
            {
                if (args.Length > maxRequiredCount)
                {
                    warning = "Incorrect number of arguments in Direction. There should be between " + minRequiredCount + " and " + maxRequiredCount + " Ignoring the additional arguments: ";
                }
            }
            else
            {
                if (args.Length > minRequiredCount)
                {
                    warning = "Incorrect number of arguments in Direction. There should " + minRequiredCount + ". Ignoring the additional arguments: ";
                }
            }

            string msg = "";
            if (!string.IsNullOrEmpty(error))
            {
                msg = error + msg;
                LogError(msg, args);
                if (!string.IsNullOrEmpty(warning))
                {
                    msg = warning + msg;
                    LogWarning(msg, args);
                }
                return false;
            }
            else if (!string.IsNullOrEmpty(warning))
            {
                msg = warning + msg;
                LogWarning(msg, args);
            }

            return true;
        }
    }
}
