using UnityEngine;
using System.Collections;
using System;

namespace eDIA
{
    public class Comment : MonoBehaviour
    {
        public string comment;

        void Start()
        {
            if (!Application.isEditor) Destroy(this);
        }
    }
}