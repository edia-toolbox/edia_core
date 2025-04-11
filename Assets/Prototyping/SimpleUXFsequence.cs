using System;
using UnityEngine;
using UXF;

public class SimpleUXFsequence : MonoBehaviour
{
        // set this to reference your UXF Session in the inspector
        UXF.Session session;

        private void Awake() {
            session = Session.instance;
        }

        private void Start() {
            GenerateAndRun();
        }

        // assign this method to the Session OnSessionBegin UnityEvent in its inspector
        public void GenerateAndRun() 
        {       
            // Creating a block of 10 trials
            var myBlock = Session.instance.CreateBlock(10);

            // Add a new setting to trial 1, here just as an example we will apply a setting of "color" to "red" 
            myBlock.trials[0].settings.SetValue("color", "red");

            // Start the session!
            
            Session.instance.Begin("","",1);
            
            // session.FirstTrial.Begin();
        }


    }
