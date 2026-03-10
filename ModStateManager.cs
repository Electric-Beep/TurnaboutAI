using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TurnaboutAI.Actions;
using TurnaboutAI.NeuroAPI;
using TurnaboutAI.Patches;
using TurnaboutAI.SaveLoad;
using TurnaboutAI.Utility;
using UnityEngine;

namespace TurnaboutAI
{
    /// <summary>
    /// Manages observation of game state and actions.
    /// </summary>
    public sealed class ModStateManager : MonoBehaviour
    {
        private static ModStateManager _instance;

        private readonly NeuroGame _neuro;
        private readonly Dictionary<string, CancellationTokenSource> _actionsCancels = new Dictionary<string, CancellationTokenSource>();

        private bool _started = false;
        private IEnumerator _clientLoop;

        private bool _textStarted;
        private guideCtrl.GuideType _lastGuideType = guideCtrl.GuideType.NO_GUIDE;

        private static ModSaveData SaveData;

        private ModStateManager()
        {
            _neuro = new NeuroGame("Phoenix Wright: Ace Attorney Trilogy");
        }

        public bool SuppressEvents { get; set; }

        void Awake()
        {
            _instance = this;

            string url;

            if (!string.IsNullOrEmpty(Plugin.Config.WebSocketUrl))
            {
                url = Plugin.Config.WebSocketUrl;
            }
            else
            {
                url = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL");
            }

            _clientLoop = _neuro.Start(url);
        }

        void Update()
        {
            if(!_started && coroutineCtrl.instance != null)
            {
                _started = true;
                coroutineCtrl.instance.Play(_clientLoop);
            }
        }

        void FixedUpdate()
        {
            KeyPresser.ReleaseKey();
        }

        /// <summary>
        /// Global manager instance.
        /// </summary>
        /// <remarks>Not thread safe.</remarks>
        public static ModStateManager Instance => _instance;

        /// <summary>
        /// Registers the action to start a new game.
        /// </summary>
        public void MainMenuNewGame()
        {
            UnregisterAllActions();
            RegisterAction(new NewGameAction());
        }

        /// <summary>
        /// Registers the action to continue a saved game.
        /// </summary>
        public void MainMenuContinueGame()
        {
            UnregisterAllActions();
            RegisterAction(new ContinueGameAction());
        }

        /// <summary>
        /// Called when a save is loaded.
        /// </summary>
        public void Load()
        {
            UnregisterAllActions();
            TextPatch.Init = false;
            _lastGuideType = guideCtrl.GuideType.NO_GUIDE;
            SaveData = null;
        }

        /// <summary>
        /// Sends context to Neuro.
        /// </summary>
        /// <param name="context">Some informative context about what's going on.</param>
        /// <param name="silent">If true, she shouldn't be prompted to respond, otherwise she might.</param>
        public void SendContext(string context, bool silent)
        {
            _neuro.SendContext(context, silent);
        }
        
        /// <summary>
        /// Gives move locations.
        /// </summary>
        public void Move(List<string> locations)
        {
            if (Plugin.Config.OnlyText) return;

            UnregisterAllActions();

            RegisterActions(
                new MoveToAction(locations),
                new CancelAction());
        }

        /// <summary>
        /// Starts the detect minigame.
        /// </summary>
        public void Detect(List<DetectPoint> points)
        {
            UnregisterAllActions();

            MaybeConsumeDialogue();

            if (Plugin.Config.OnlyText) return;

            RegisterAction(new DetectSpotAction(points));
        }

        /// <summary>
        /// Starts the point mini game handler.
        /// </summary>
        /// <param name="point">The correct choice.</param>
        public void PointMiniGame(GSPoint4 point)
        {
            UnregisterAllActions();

            MaybeConsumeDialogue();

            if (Plugin.Config.OnlyText) return;

            var action = new PointAction(point);
            RegisterAction(action);
        }

        /// <summary>
        /// Registers present actions.
        /// </summary>
        public void Present(List<string> evidence, List<string> profiles, bool canGoBack, bool canPresentProfile)
        {
            if (SuppressEvents) return;

            UnregisterAllActions();

            if (ShouldSave())
            {
                Utilities.Do(SaveLoadHelper.DoSave(SaveData), Do);
            }
            else
            {
                Do();
            }

            void Do()
            {
                MaybeConsumeDialogue();

                if (Plugin.Config.OnlyText) return;

                var actions = new List<IActionHandler>();

                if (evidence.Count > 0)
                {
                    actions.Add(new ReviewEvidenceAction());
                    actions.Add(new PresentEvidenceAction(evidence));
                }

                if (profiles.Count > 0 && canPresentProfile)
                {
                    actions.Add(new ReviewProfilesAction());
                    actions.Add(new PresentProfileAction(profiles));
                }

                if (canGoBack)
                {
                    actions.Add(new CancelAction());
                }

                RegisterActions(actions.ToArray());
            }
        }

        /// <summary>
        /// Registers examine actions.
        /// </summary>
        public void Inspect(List<ExamineSpot> spots, bool canGoBack)
        {
            UnregisterAllActions();

            MaybeConsumeDialogue();

            if (Plugin.Config.OnlyText) return;

            var actions = new List<IActionHandler>();

            if(spots.Count > 0)
            {
                actions.Add(new ExamineSpotAction(spots));
            }

            if(canGoBack)
            {
                actions.Add(new CancelAction());
            }

            RegisterActions(actions.ToArray());
        }

        /// <summary>
        /// Registers the appropriate investigation actions.
        /// </summary>
        /// <param name="type"></param>
        public void SetInvestigationMenu(int type)
        {
            if (SuppressEvents) return;

            if (Plugin.Config.OnlyText) return;

            UnregisterAllActions();

            _lastGuideType = guideCtrl.GuideType.NO_GUIDE;
            RegisterInvestigationActions(type);
        }

        /// <summary>
        /// Sets the talk options when a talk choice appears.
        /// </summary>
        public void SetTalkOptions(List<TalkOption> options, bool isSelect)
        {
            if(isSelect && ShouldSave())
            {
                Utilities.Do(SaveLoadHelper.DoSave(SaveData), Do);
            }
            else
            {
                Do();
            }

            void Do()
            {
                MaybeConsumeDialogue();

                if (Plugin.Config.OnlyText) return;

                UnregisterAllActions();

                if(isSelect)
                {
                    RegisterActions(
                        new PickChoiceAction(options),
                        new ReviewEvidenceAction(),
                        new ReviewProfilesAction());
                }
                else
                {
                    var cancelAction = new CancelAction();

                    if (options.All(o => o.Read))
                    {
                        RegisterAction(cancelAction);
                    }
                    else
                    {
                        var questionAction = new QuestionAction(options);

                        if (options.Any(o => o.PsyLock))
                        {
                            var magatama = new UseMagatamaAction();

                            RegisterActions(questionAction, magatama, cancelAction);
                        }
                        else
                        {
                            RegisterActions(questionAction, cancelAction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates dialogue has started.
        /// </summary>
        public void TextStarted()
        {
            if(!_textStarted)
            {
                _textStarted = true;
                UnregisterAllActions();
            }
            
        }

        /// <summary>
        /// Indicates dialogue has finished.
        /// </summary>
        /// <param name="text">Final dialogue.</param>
        public void TextFinished(string text, bool canGoBack, guideCtrl.GuideType guideType)
        {
            if (ShouldSave(guideType))
            {
                SaveData = SaveLoadHelper.GetSaveData();
            }

            _textStarted = false;
            _lastGuideType = guideType;

            SendContext(text, false);

            if (Plugin.Config.OnlyText) return;

            if (guideType == guideCtrl.GuideType.QUESTIONING)
            {
                List<IActionHandler> handlers = new List<IActionHandler>
                    {
                        new ReviewEvidenceAction(),
                        new ReviewProfilesAction(),
                        new PressAction(),
                        new PresentAction(true),
                        new NextStatementAction(),
                    };

                if (canGoBack)
                {
                    handlers.Add(new PreviousStatementAction());
                }

                //Cross Examine
                RegisterActions(handlers.ToArray());
            }
            else
            {
                var handler = new NextDialogueAction();

                RegisterAction(handler);
            }
        }

        /// <summary>
        /// Unregisters a specific action.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        public void UnregisterAction(string actionName)
        {
            _neuro.UnregisterAction(actionName);
        }

        /// <summary>
        /// Unregisters all actions.
        /// </summary>
        public void UnregisterAllActions()
        {
            foreach(var src in _actionsCancels.Values)
            {
                src.Cancel();
            }

            _actionsCancels.Clear();
            _neuro.UnregisterAllActions();
        }

        private void MaybeConsumeDialogue()
        {
            string dialogue = TextPatch.ConsumeDialogues();

            if (!string.IsNullOrEmpty(dialogue))
            {
                SendContext(dialogue, false);
            }

            _textStarted = false;
        }

        private void RegisterAction(IActionHandler handler)
        {
            try
            {
                CancellationTokenSource src = new CancellationTokenSource();
                handler.CancellationToken = src.Token;
                _actionsCancels[handler.Name] = src;
                _neuro.RegisterAction(handler);
            }
            catch (Exception ex)
            {
                Plugin.LogError(ex);
            }
        }

        private void RegisterActions(params IActionHandler[] handlers)
        {
            try
            {
                foreach (var handler in handlers)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    handler.CancellationToken = src.Token;
                    _actionsCancels[handler.Name] = src;
                }

                _neuro.RegisterActions(handlers);
            }
            catch(Exception ex)
            {
                Plugin.LogError(ex);
            }
        }

        private void RegisterInvestigationActions(int type)
        {
            // 0 = Examine, Move
            // 1 = Examine, Move, Talk, Present
            if (type == 0)
            {
                RegisterActions(
                    new ExamineAction(),
                    new MoveAction());
            }
            else if(type == 1)
            {
                RegisterActions(
                    new ExamineAction(),
                    new MoveAction(),
                    new TalkAction(),
                    new PresentAction(false));
            }
            else
            {
                return;
            }
        }

        private bool ShouldSave()
        {
            return ShouldSave(_lastGuideType) && SaveData != null;
        }

        private bool ShouldSave(guideCtrl.GuideType guideType)
        {
            if (!Plugin.Config.SafetySave) return false;

            return guideType == guideCtrl.GuideType.QUESTIONING || guideType == guideCtrl.GuideType.HOUTEI;
        }
    }
}
