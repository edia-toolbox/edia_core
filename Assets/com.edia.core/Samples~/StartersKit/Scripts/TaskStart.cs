using System.Collections.Generic;
using Edia;
using UnityEngine;
using UXF;

namespace StartersKit {

    public class TaskStart : XBlock {

        [Header("Refs")]
        public GameObject StimuliHolder;
        public Material   StimuliMaterial;
        public GameObject UserInputPanel;

        // Trial related properties
        private readonly List<Color> _trialColors = new();
        private          Color       _selectedColor;
        private          Color       _currentStimuliColor;

        // Trial data to collect 
        private float UserResponseTime = 0f;
        private float StepStartTime    = 0;

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                Each step is a standalone part of the experiment task.
                I.e. Show a message to the user, show a fixation cross, wait for user input, etc.
                In order to use the steps, they need to be defined in the trial sequence.

                A common approach within a trial is as follows:
            */

            AddToTrialSequence(ShowStimuli); //      #1 Set up environment / Generate Stimuli / etc
            AddToTrialSequence(UserInput); //     #2 Input from the user
            AddToTrialSequence(ValidateUserInput); //   #3 Check input and log results

            // Prepare scene
            StimuliHolder.SetActive(false);
            UserInputPanel.SetActive(false);
        }

#region TASK STEPS

        /*
            The framework provides a set of tools to make it easier to implement the task.
            For example:
            * XRManager.Instance.xxxxx => All things XR related
            * Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

            See documentation for more details.
        */

        // #1
        /// <summary>Prepare and show stimuli</summary>
        void ShowStimuli() {
            /*
                For example:
                - Choose a random stimulus from a list of stimuli.
                - Generate a random number between 0 and 100.
                - Choose a color and assign it to the stimuli materials.
                - ...
            */

            ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color_code"), out Color parsedColor);
            _currentStimuliColor  = parsedColor;
            StimuliMaterial.color = parsedColor;

            StimuliHolder.SetActive(true);

            /*
                Continue to the next step follows this principe:
                1. Put experiment statemachine in 'wait' mode
                2. Proceed experiment statemachine with a 'proceed' call from somewhere (script/button/timer/etc)
            */

            Experiment.Instance.WaitOnProceed();

            /*
             * Then, either:
             *   - Directly: Experiment.Instance.Proceed();
             *   - Delayed: Experiment.Instance.ProceedWithDelay (seconds as float)
             *   - User dependent: I.e. when a button is pressed in VR -> proceed by calling Experiment.Instance.Proceed() in a callback method.
             */

            /*
             * In this case we use a setting from the currentblock to determine the time to show the stimuli called 'timer_showcube'.
             * This value is defined in the config file for this block `task-start_0.json`.
             * At runtime these settings from the config files, are converted into `session`, `block` or `trial` settings.
             * This is part of the UXF framework.
             */
            Experiment.Instance.ProceedWithDelay(Session.instance.CurrentBlock.settings.GetFloat("timer_showcube"));
        }

        // #2
        /// <summary>Hide stimuli and wait on user input</summary>
        void UserInput() {
            // Hide stimuli 
            StimuliHolder.SetActive(false);

            /*
             * Enable user interaction.
             * There are two available ways of interacting:
             *   1. Pointing a ray. This works for near and far interactions. ( XRManager.Instance.EnableRayInteraction )
             *   2. Poke / touch. This works for near interaction. ( XRManager.Instance.EnablePokeInteraction() )
             * Both work with controllers and hands.
             * See Edia documentation for more details.
             */

            XRManager.Instance.EnableRayInteraction(true);
            UserInputPanel.SetActive(true);

            // Remember the time when the user started inputting.
            StepStartTime = Time.time;

            // Tell the system to wait on button press. This will also enable the 'proceed' button on the experimenter controllerpanel 
            Experiment.Instance.WaitOnProceed();
        }

        /// <summary> Provides the user selected color. </summary>
        /// <param name="selectedColorIdx"></param>
        public void UserInputBtnPressed(int selectedColorIdx) {
            /*
             * !! If the user does not click a button and the experimenter uses 'proceed', this method is skipped.
             * Be aware of what code executes in which step in your task.
             */

            // Get provided color
            _selectedColor = _trialColors[selectedColorIdx];

            // Store the time 
            UserResponseTime = Time.time - StepStartTime;

            // As we already have put the system in 'wait' mode in the previous step, we can proceed directly.
            Experiment.Instance.Proceed();
        }

        // #3
        /// <summary>Validate and log data</summary>
        void ValidateUserInput() {
            // Hide panel
            UserInputPanel.SetActive(false);

            // Check if the user selected the correct color.
            bool isCorrect = _selectedColor == StimuliMaterial.color;

            // Add results and relevant data to trial results
            Experiment.Instance.AddToTrialResults("correct", isCorrect.ToString());
            Experiment.Instance.AddToTrialResults("response_time", UserResponseTime.ToString());

            /*
             * Continue to next step
             * The message panel to the user has a 'proceed' button by default.
             */
            Experiment.Instance.ShowMessageToUser(">" + (isCorrect ? "Correct" : "Wrong") + "!<\n\nPress 'proceed' to continue.");
            Experiment.Instance.WaitOnProceed();
        }

#endregion
#region ------------------------------ STATEMACHINE OVERRIDES

        /*
            Opional overrides
        */

        public override void OnBlockStart() {
        }

        public override void OnStartTrial() {
            // Get the colors for this trial from the settings
            foreach (var colorCode in Session.instance.CurrentTrial.settings.GetStringList("trial_colors")) {
                ColorUtility.TryParseHtmlString(colorCode, out Color color);
                _trialColors.Add(color);
            }
        }

        public override void OnEndTrial() {
            _trialColors.Clear();
        }

        public override void OnBetweenSteps() {
            // Disable XR interaction from the user inbetween steps so we don't have to worry about that.
            XRManager.Instance.EnableAllInteraction(false);
        }

        public override void OnBlockOutro() {
        }

        public override void OnBlockEnd() {
        }

#endregion
    }

}