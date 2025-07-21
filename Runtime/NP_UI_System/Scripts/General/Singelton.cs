using UnityEngine;


namespace NP_UI
{
    /// <summary>
    /// Automatic singleton pattern, use ONLY on objects that are always active.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        /// <summary>
        /// <para>Returns the instance of the <typeparamref name="T"/> singleton.</para>
        /// use ONLY on objects that are always active.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    //if (instance == null)
                    //{
                    //    Debug.LogWarning("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                    //}
                }

                return instance;
            }
        }
    }
}
