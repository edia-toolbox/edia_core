using System.Collections.Generic;
using Edia;
using UnityEngine;
using UXF;
using UnityEngine.UI;

/*
 * Xblock order of execution:
 * 1. OnBlockStart
 * 2. OnTrialStart
 * 3. first step method
 * 4. InBetweenSteps
 * 5. next step method
 * 6. ...
 * 7. OnTrialEnd
 * 8. OnBlockEnd
 * 9. OnBlockOutro
 */

namespace StartersKit {

    public class TaskStartersKitFinished : XBlock {

        [Header("Far condition")]
        public GameObject  PlayAreaFar;
        public GameObject  StimuliHolderFar;
        public GameObject  FLoatingCanvasPanel;
        public List<Image> SelectionImages = new();

        [Space(10)]
        [Header("Near condition")]
        public GameObject PlayAreaNear;
        public GameObject StimuliHolderNear;

        [Tooltip("Plates on the table to place the colored box on.")]
        public List<SelectionSlot> SelectionSlots;

        public float SelectionSlotTimerDuration = 8f;

        // Trial related properties
        private readonly List<Color> _blockColors = new();
        private          Color       _selectedColor;
        private          Color       _currentStimuliColor;
        private          string      _currentTaskCondition;
        private          Vector3     _stimuliHolderNearInitialPosition;

        // Trial data to collect 
        private float UserResponseTime          = 0f;
        private float UserInputEnabledTimestamp = 0;

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                Each step is a standalone part of the experiment task.
                I.e. Show a message to the user, show a fixation cross, wait for user input, etc.
                In order to use the steps, they need to be defined in the trial sequence.

                A common approach within a trial is as follows:
            */
            

            AddToTrialSequence(SetupAndShowStimuli); //      #1 Set up environment / Generate Stimuli / etc
            AddToTrialSequence(UserInput); //     #2 Input from the user
            AddToTrialSequence(ValidateAndLogUserInput); //   #3 Check input and log results

            // Prepare scene
            PlayAreaFar.SetActive(false);
            PlayAreaNear.SetActive(false);
            StimuliHolderFar.SetActive(false);
            FLoatingCanvasPanel.SetActive(false);

            _stimuliHolderNearInitialPosition = StimuliHolderNear.transform.position;
        }

#region TASK STEPS

        /*
            The framework provides a set of tools to make it easier to implement the task.
            For example:
            * XRManager.Instance.xxxxx => All things XR related
            * Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

            See documentation for more details.
        */

        // -------------------------------------------------------------------------------------------
        // #1
        /// <summary>Prepare and show stimuli</summary>
        void SetupAndShowStimuli() {
            /*
                Some examples:
                - Choose a random stimulus from a list of stimuli.
                - Generate a random number between 0 and 100.
                - Choose a color and assign it to the stimuli materials.
                - ...
            */
            
            ColorUtility.TryParseHtmlString(Session.instance.CurrentTrial.settings.GetString("color_code"), out Color parsedColor);
            _currentStimuliColor  = parsedColor;

            /*
                Continue to the next step follows this principle:
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

            if (_currentTaskCondition == "far") {
                /*
                 * In the 'far' condition, we use a setting called 'timer_showcube' from the currentblock to determine the time to show the stimuli.
                 * This value is defined in the config file for this block `task-start_0.json` and `task-start_1.json`.
                 * At runtime these settings from the config files, are converted into `session`, `block` or `trial` settings.
                 * That is part of the UXF framework.
                 */

                // Set the colors for the 2D colored selections
                for (int i = 0; i < SelectionImages.Count; i++) {
                    SelectionImages[i].color = _blockColors[i];
                }

                StimuliHolderFar.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor(("_BaseColor"),_currentStimuliColor);            
                StimuliHolderFar.SetActive(true);
                Experiment.Instance.ProceedWithDelay(Session.instance.CurrentBlock.settings.GetFloat("timer_showcube"));
            }
            else {
                /*
                 * In the 'near' condition, we want to
                 */

                // Set the colors for the 3D colored selection slots
                for (int i = 0; i < SelectionSlots.Count; i++) {
                    SelectionSlots[i].Init(_blockColors[i], i, SelectionSlotTimerDuration);
                }
                
                StimuliHolderNear.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor(("_BaseColor"),_currentStimuliColor);
                StimuliHolderNear.SetActive(true);
                Experiment.Instance.Proceed();
            }
            
            /*
             * A step always ends with either a 'PROCEED' or 'WAITONPROCEED'
             */
        }

        // -------------------------------------------------------------------------------------------
        // #2
        /// <summary>Wait on user input</summary>
        void UserInput() {
            if (_currentTaskCondition == "far") {
                FLoatingCanvasPanel.SetActive(true);
                StimuliHolderFar.SetActive(false);
            }

            /*
             * Enable user interaction.
             * There are two available ways of interacting:
             *   1. Pointing a ray. This works for near and far interactions. ( XRManager.Instance.EnableRayInteraction )
             *   2. Poke / touch. This works for near interaction. ( XRManager.Instance.EnablePokeInteraction() )
             * Both work with controllers and hands.
             * See Edia documentation for more details.
             */

            XRManager.Instance.EnableRayInteraction(true); // This enabled a XR ray to interact FAR and NEAR, including grabbing

            // Remember the time when the user started inputting.
            UserInputEnabledTimestamp = Time.time;

            // Tell the system to wait on button press. This will also enable the 'proceed' button on the experimenter controllerpanel 
            Experiment.Instance.WaitOnProceed();
        }

        /// <summary> Provides the user selected color. </summary>
        /// <param name="selectedColorIdx"></param>
        public void UserInputDone(int selectedColorIdx) {
            /*
             * !! If the experimenter uses the 'proceed' button on the controllerpanel, this method is skipped.
             * Therefore be very aware of where to put essential parts of code.
             */

            // Get the selected color
            _selectedColor = _blockColors[selectedColorIdx];

            // Store the time 
            UserResponseTime = Time.time - UserInputEnabledTimestamp;

            // As we already have put the system in 'wait' mode in the previous step, we can proceed.
            Experiment.Instance.Proceed();
        }

        // -------------------------------------------------------------------------------------------
        // #3
        /// <summary>Validate and log data</summary>
        void ValidateAndLogUserInput() {
            FLoatingCanvasPanel.SetActive(false);
            StimuliHolderFar.SetActive(false);
            PlayAreaFar.SetActive(false);
            StimuliHolderNear.SetActive(false);
            foreach (var slot in SelectionSlots) {
                slot.gameObject.SetActive(false);
            }

            // Check if the user selected the correct color.
            bool isCorrect = _selectedColor == _currentStimuliColor;

            // Add results and relevant data to trial results
            Experiment.Instance.AddToTrialResults("correct", isCorrect.ToString());
            Experiment.Instance.AddToTrialResults("response_time", UserResponseTime.ToString());

            /*
             * Continue to next step
             * The message panel has a 'proceed' button by default.
             */
            Experiment.Instance.ShowMessageToUser(">" + (isCorrect ? "Correct" : "Wrong") + "!<\n\nPress 'proceed' to continue.");
            Experiment.Instance.WaitOnProceed();
        }

#endregion
#region STATEMACHINE OVERRIDES

        /*
            Statemachine calls
        */

        public override void OnBlockStart() {
            /*
             * Shows message to the user if there is a value defined in the configs with the key "_start"
             */

            // Get the playarea space setting for this block
            _currentTaskCondition = Session.instance.CurrentBlock.settings.GetString("task_space");
        }

        public override void OnStartTrial() {
            // Get the colors for this trial from the settings
            foreach (var colorCode in Session.instance.CurrentBlock.settings.GetStringList("block_colors")) {
                ColorUtility.TryParseHtmlString(colorCode, out Color color);
                _blockColors.Add(color);
            }

            PlayAreaFar.SetActive(_currentTaskCondition == "far");
            PlayAreaNear.SetActive(_currentTaskCondition == "near");
        }

        public override void OnEndTrial() {
            _blockColors.Clear();

            // Reset the stimuli holder position
            StimuliHolderNear.transform.position = _stimuliHolderNearInitialPosition;
        }

        public override void OnBetweenSteps() {
            // Disable XR interaction from the user inbetween steps so we don't have to worry about that.
            XRManager.Instance.EnableAllInteraction(false);
        }

        public override void OnBlockEnd() {
            PlayAreaFar.SetActive(false);
            PlayAreaNear.SetActive(false);
        }

        public override void OnBlockOutro() {
            /*
             * Shows message to the user if there is a value defined in the configs with the key "_end"
             */
        }

#endregion
    }

}