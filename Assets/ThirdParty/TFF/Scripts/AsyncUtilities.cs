using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace TFF {

    public static class AsyncUtils
    {
        public static async Task WaitForSecondsAsync(float _timeToWait)
            {
                await Task.Delay(TimeSpan.FromSeconds(_timeToWait));
                //Debug.Log("Finished waiting.");
            }
    }
}
