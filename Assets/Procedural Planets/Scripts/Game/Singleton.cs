/*  
    Class: ProceduralPlanets.Singleton
    Version: 0.1.1 (initial alpha release)
    Date: 2018-01-10
    Author: Stefan Persson
    (C) Imphenzia AB

    Singleton class that the Manager derives from to ensure only one peristent instance exists of the Manager.

*/

using UnityEngine;

namespace ProceduralPlanets
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
#if !UNITY_EDITOR
            DontDestroyOnLoad(this.gameObject);
#endif
            }
            else
            {
#if !UNITY_EDITOR
            Destroy(gameObject);
#endif
            }
        }
    }
}
