#region Using
using System;
using System.Text;
using System.Windows;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows.Forms;
using NatiTools;
#endregion

namespace Messiah
{
    public partial class MainWindow : Window
    {
        #region Constants
        #region Modes
        private const int MODE_IDLE = 0;
        private const int MODE_BROWSER = 1;
        private const int MODE_CHARACTERS = 2;
        private const int MODE_WORDS = 3;
        #endregion
        #region Actions
        #region Global
        private MessiahAction ACTION_EXIT               = new MessiahAction("exit", "exit Messiah", "quit", "quit Messiah");// Switch to MODE_IDLE
        private MessiahAction ACTION_STOP_LISTENING     = new MessiahAction("stop listening");                              // Switch to MODE_IDLE
        private MessiahAction ACTION_START_LISTENING    = new MessiahAction("start listening");                             // Switch back to MODE_BROWSER
        private MessiahAction ACTION_DICTATE_CHARACTERS = new MessiahAction("dictate", "dictate characters");               // Switch to MODE_CHARACTERS
        private MessiahAction ACTION_DICTATE_WORDS      = new MessiahAction("dictate words", "dictate sentences");          // Switch to MODE_WORDS
        private MessiahAction ACTION_STOP_DICTATING     = new MessiahAction("stop dictating");                              // Switch back to MODE_BROWSER

        private MessiahAction ACTION_ENTER              = new MessiahAction("enter", "confirm");                            // Enter
        private MessiahAction ACTION_BACKSPACE          = new MessiahAction("backspace");                                   // Backspace
        private MessiahAction ACTION_DELETE             = new MessiahAction("delete");                                      // Delete
        private MessiahAction ACTION_SELECT_ALL         = new MessiahAction("select all");                                  // Ctrl+A
        private MessiahAction ACTION_DELETE_ALL         = new MessiahAction("delete all");                                  // Ctrl+A and Delete

        private MessiahAction ACTION_LEFT               = new MessiahAction("left");                                        // Press left arrow
        private MessiahAction ACTION_RIGHT              = new MessiahAction("right");                                       // Press right arrow
        private MessiahAction ACTION_UP                 = new MessiahAction("up");                                          // Press up arrow
        private MessiahAction ACTION_DOWN               = new MessiahAction("down");                                        // Press down arrow
        #endregion
        #region Browser
        private MessiahAction ACTION_ELEMENT_PREVIOUS   = new MessiahAction("select previous", "previous element");         // Focus previous element
        private MessiahAction ACTION_ELEMENT_NEXT       = new MessiahAction("select next", "next element");                 // Focus next element
        private MessiahAction ACTION_PAGE_BACK          = new MessiahAction("page back", "go back", "previous page");       // Go to the previous page
        private MessiahAction ACTION_PAGE_FORWARD       = new MessiahAction("page forward", "go forward", "next page");     // Go to the next page
        private MessiahAction ACTION_START_BROWSER      = new MessiahAction("open browser", "start browser");               // Open new browser window
        private MessiahAction ACTION_TAB_NEW            = new MessiahAction("new tab", "open new tab");                     // Open new tab
        private MessiahAction ACTION_TAB_CLOSE          = new MessiahAction("close tab", "close active tab");               // Close active tab
        private MessiahAction ACTION_TAB_REOPEN         = new MessiahAction("reopen tab", "open last closed tab");          // Open the most recently closed tab
        private MessiahAction ACTION_TAB_PREVIOUS       = new MessiahAction("previous tab", "switch to previous tab");      // Open the most recently closed tab
        private MessiahAction ACTION_TAB_NEXT           = new MessiahAction("next tab", "switch to next tab", "switch tab");// Open the most recently closed tab
        private MessiahAction ACTION_RELOAD_PAGE        = new MessiahAction("reload page", "refresh page");                 // Reload the page in active tab
        private MessiahAction ACTION_SCROLL_UP          = new MessiahAction("scroll up", "scroll up");                      // Scroll the page up
        private MessiahAction ACTION_SCROLL_DOWN        = new MessiahAction("scroll down", "page down");                    // Scroll the page down

        private MessiahAction ACTION_MOUSE_CLICK_LEFT   = new MessiahAction("click left", "left click");                    // Press the left mouse button
        private MessiahAction ACTION_MOUSE_CLICK_RIGHT  = new MessiahAction("click right", "right click");                  // Press the right mouse button
        private MessiahAction ACTION_MOUSE_CLICK_MIDDLE = new MessiahAction("click middle", "middle click", "scroll click");// Press the middle mouse button

        private MessiahAction ACTION_MOUSE_MOVE;
        #endregion
        #region Words
        private MessiahAction ACTION_DELETE_LAST        = new MessiahAction("delete last", "delete recent");                // Delete the most recently written word
        #endregion
        #endregion
        #region Characters
        private Character[] CHARACTERS = new Character[] {
                new Character(" ",  "space"),
                new Character(".",  "period", "dot"),
                new Character(",",  "comma"),
                new Character("%",  "percent"),
                new Character("\"", "quote", "double quote", "quotation mark"),
                new Character("'",  "single quote"),
                new Character("@",  "at sign"),
                new Character("$",  "dollar", "dollar sign"),
                new Character("€",  "euro", "euro sign"),
                new Character("?",  "question mark"),
                new Character("!",  "exclamation mark"),
                new Character("+",  "plus", "plus sign"),
                new Character("-",  "hyphen", "minus", "minus sign"),
                new Character("=",  "equals sign", "equality sign"),
                new Character(":",  "colon"),
                new Character(";",  "semicolon"),
                new Character("^",  "caret", "caret sign"),
                new Character("*",  "asterisk"),
                new Character("&",  "ampersand"),
                new Character("`",  "apostrophe"),
                new Character("/",  "slash", "forward slash"),
                new Character("\\", "backslash"),
                new Character("[",  "left bracket"),
                new Character("]",  "left bracket"),
                new Character("(",  "left parenthesis"),
                new Character(")",  "left parenthesis"),
                new Character("{",  "left brace"),
                new Character("}",  "left brace"),
                new Character("<",  "left chevron", "less than", "lower than"),
                new Character(">",  "left chevron", "more than", "higher than")
            };
        #endregion
        #region Mouse Movement
        private int[] INT_MOUSE_SPEEDS = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        private string[] STRING_MOUSE_DIRECTIONS = { "up", "right", "down", "left" };
        #endregion
        #endregion
        #region Variables
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer synthesizer;
        private RecognitionMode[] modes = new RecognitionMode[4];
        private int mode = -1;
        private int lastWordLength = 0;
        #endregion

        #region Window Events
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateSRE(ref recognizer, "en-US");
            recognizer.LoadGrammarCompleted += Recognizer_LoadGrammarCompleted;
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();

            #region Mouse Speed Action
            
            string[] strMouseSpeedAlternatives = new string[INT_MOUSE_SPEEDS.Length * STRING_MOUSE_DIRECTIONS.Length];

            for (int i = 0; i < INT_MOUSE_SPEEDS.Length; i++)
            {
                int iPtr = i * STRING_MOUSE_DIRECTIONS.Length;

                for (int j = 0; j < STRING_MOUSE_DIRECTIONS.Length; j++)
                {
                    strMouseSpeedAlternatives[iPtr + j] = "move " + STRING_MOUSE_DIRECTIONS[j] + " " + INT_MOUSE_SPEEDS[i].ToString();
                }
            }

            ACTION_MOUSE_MOVE = new MessiahAction(strMouseSpeedAlternatives);
            #endregion

            PrepareModes();
            LoadRecognitionMode(MODE_IDLE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Make sure the program doesn't continue listening after the window was closed
            recognizer.RecognizeAsyncCancel();

            // Say goodbye to the user and cancer all other speaking
            synthesizer.SpeakAsyncCancelAll();
            synthesizer.Speak("Goodbye!");
        }
        #endregion
        #region SRE Events
        private void Recognizer_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            // When all the grammar is successfully loaded, the program is ready to start listening
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // FOR DEBUGGING PURPOSES
            Display("Mode: " + mode.ToString() + Environment.NewLine +
                    "Confidence: " + e.Result.Confidence.ToString() + Environment.NewLine +
                    "Text: " + e.Result.Text);

            // If the required confirence was met, process the input text
            if (e.Result.Confidence >= modes[mode].RequiredConfidence)
                Process(e.Result.Text);
        }
        #endregion

        #region void PrepareModes()
        /// <summary>
        /// Prepares modes for future use.
        /// </summary>
        private void PrepareModes()
        {
            #region BROWSER
            modes[MODE_BROWSER] = new RecognitionMode(MODE_BROWSER, 0.9);
            AddAction(MODE_BROWSER,
                            ACTION_DICTATE_CHARACTERS,
                            ACTION_DICTATE_WORDS,

                            ACTION_ELEMENT_PREVIOUS,
                            ACTION_ELEMENT_NEXT,
                            ACTION_PAGE_BACK,
                            ACTION_PAGE_FORWARD,
                            ACTION_START_BROWSER,
                            ACTION_TAB_NEW,
                            ACTION_TAB_CLOSE,
                            ACTION_TAB_REOPEN,
                            ACTION_TAB_PREVIOUS,
                            ACTION_TAB_NEXT,
                            ACTION_RELOAD_PAGE,
                            ACTION_SCROLL_DOWN,
                            ACTION_SCROLL_UP,

                            ACTION_MOUSE_CLICK_LEFT,
                            ACTION_MOUSE_CLICK_RIGHT,
                            ACTION_MOUSE_CLICK_MIDDLE,

                            ACTION_MOUSE_MOVE
                            );
            #endregion
            #region CHARACTERS
            int specialCharsLen = CHARACTERS.Length;
            const int standardCharsLen = 62;
            const int lettersLen = 26;
            const int lettersTotal = lettersLen * 2;
            const int firstLetterIndex = 97;
            Array.Resize(ref CHARACTERS, specialCharsLen + standardCharsLen);

            // Generate letters (both lowercase and uppercase)
            for (int i = 0; i < lettersLen; i++)
            {
                string currChar = Encoding.ASCII.GetChars(new byte[1] { (byte)(firstLetterIndex + i) })[0].ToString();

                CHARACTERS[specialCharsLen + i] = new Character(currChar);
                CHARACTERS[specialCharsLen + lettersLen + i] = new Character(currChar.ToUpper(), "capital " + currChar);
            }

            // Generate numbers
            for (int i = 0; i < 10; i++)
                CHARACTERS[specialCharsLen + lettersTotal + i] = new Character(i.ToString());

            // Prepare new RecognitionMode object
            modes[MODE_CHARACTERS] = new RecognitionMode(MODE_CHARACTERS, 0.9);

            // Load characters into mode's dictionary
            foreach (Character ch in CHARACTERS)
            { modes[MODE_CHARACTERS].AddPhrases(ch.Alternatives); }

            AddAction(MODE_CHARACTERS,
                            ACTION_STOP_DICTATING
                            );
            #endregion
            #region WORDS
            modes[MODE_WORDS] = new RecognitionMode(MODE_WORDS, 0.8);
            AddAction(MODE_WORDS,
                            ACTION_STOP_DICTATING,
                            ACTION_DELETE_LAST
                            );
            #endregion
            #region IDLE
            modes[MODE_IDLE] = new RecognitionMode(MODE_IDLE, 0.90, ACTION_START_LISTENING.Alternatives);
            #endregion
            #region Load common actions to all modes except for IDLE
            for (int i = 1; i < modes.Length; i++)
            {
                AddAction(i,
                            ACTION_EXIT,
                            ACTION_STOP_LISTENING,

                            ACTION_ENTER,
                            ACTION_BACKSPACE,
                            ACTION_DELETE,
                            ACTION_SELECT_ALL,
                            ACTION_DELETE_ALL,

                            ACTION_LEFT,
                            ACTION_RIGHT,
                            ACTION_UP,
                            ACTION_DOWN
                            );
            }
            #endregion
        }
        #endregion
        #region void AddAction(int modeID, params MessiahAction[] actArr)
        /// <summary>
        /// Adds alternatives from specified action to mode's dictionary.
        /// </summary>
        /// <param name="modeID">mode to be added to</param>
        /// <param name="act">actions to be added</param>
        private void AddAction(int modeID, params MessiahAction[] actArr)
        {
            foreach(MessiahAction act in actArr)
            {
                modes[modeID].AddPhrases(act.Alternatives);
            }
        }
        #endregion
        #region void CreateSRE(ref SpeechRecognitionEngine sre, string preferredCulture)
        /// <summary>
        /// Creates a new SpeechRecognitionEngine object and initializes it with specified culture when available.
        /// </summary>
        /// <param name="sre">reference to the SRE object</param>
        /// <param name="preferredCulture">preferred culture</param>
        private void CreateSRE(ref SpeechRecognitionEngine sre, string preferredCulture)
        {
            // Wipe the old SRE
            sre = null;

            // If it's possible to use the preffered culture, create a new SRE using it
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (config.Culture.ToString() == preferredCulture)
                {
                    sre = new SpeechRecognitionEngine(config);
                    break;
                }
            }

            // If the requested culture wasn't found on the system, use the default culture available
            if(sre == null) sre = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);

            // Make sure the program uses the correct input device
            sre.SetInputToDefaultAudioDevice();
        }
        #endregion
        #region void LoadRecognitionMode(int sreMode)
        /// <summary>
        /// Loads specified recognition mode.
        /// </summary>
        /// <param name="sreMode">requested mode</param>
        private void LoadRecognitionMode(int sreMode)
        {
            if (mode == sreMode) return;
            if (sreMode < 0 || sreMode >= modes.Length) throw new ArgumentOutOfRangeException();

            // Stop listening
            recognizer.RecognizeAsyncCancel();
            // Remove all previously loaded grammars
            recognizer.UnloadAllGrammars();
            // In word dication mode, in addition to program's own grammar, dictation vocabulary is also loaded
            if (sreMode == MODE_WORDS) recognizer.LoadGrammar(new DictationGrammar());
            // Load program's dictionary
            recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(modes[sreMode].Phrases))));
            // Set the active mode to desired mode
            mode = sreMode;

            if (synthesizer != null)
            {
                string strModeString = string.Empty;

                switch(sreMode)
                {
                    case 0:
                        strModeString = "idle";
                        break;

                    case 1:
                        strModeString = "browser";
                        break;

                    case 2:
                        strModeString = "character dictation";
                        break;

                    case 3:
                        strModeString = "word dictation";
                        break;

                    default:
                        break;
                }

                synthesizer.SpeakAsync(strModeString + " mode");
            }
        }
        #endregion
        
        #region void Display(string strToDisplay)
        /// <summary>
        /// Displays a text string in the main window.
        /// </summary>
        /// <param name="strToDisplay">string to be displayed</param>
        private void Display(string strToDisplay)
        {
            textBoxInput.Text = strToDisplay;
        }
        #endregion
        #region bool IsAction(string str, MessiahAction act)
        /// <summary>
        /// Decides if the input string represents specific action or not.
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="act">action to be checked</param>
        /// <returns>Returns true if input string belongs to the action.</returns>
        private bool IsAction(string str, MessiahAction act)
        {
            return NatiStringTools.ArrayContains(act.Alternatives, str, false);
        }
        #endregion
        #region void FitPointOnScreen(ref System.Drawing.Point p)
        /// <summary>
        /// Move the Point p to fit within the screen boundaries.
        /// </summary>
        /// <param name="p">Point to move</param>
        private void FitPointOnScreen(ref System.Drawing.Point p)
        {
            System.Drawing.Rectangle rectScreen = Screen.PrimaryScreen.Bounds;

            if (p.X < rectScreen.Left)
                p.X = rectScreen.Left;
            else if (p.X > rectScreen.Right)
                p.X = rectScreen.Right;

            if (p.Y < rectScreen.Top)
                p.Y = rectScreen.Top;
            else if (p.Y > rectScreen.Bottom)
                p.Y = rectScreen.Bottom;
        }
        #endregion

        #region void SelectAll()
        /// <summary>
        /// Selects all the text within active element.
        /// </summary>
        private void SelectAll()
        {
            NatiKeyboard.Down(Keys.LControlKey);
            NatiKeyboard.Press(Keys.A);
            NatiKeyboard.Up(Keys.LControlKey);
        }
        #endregion
        #region void OpenNewTab()
        /// <summary>
        /// Opens a new browser tab.
        /// </summary>
        private void OpenNewTab()
        {
            NatiKeyboard.Down(Keys.LControlKey);
            NatiKeyboard.Press(Keys.T);
            NatiKeyboard.Up(Keys.LControlKey);
        }
        #endregion
        #region void SwitchTab()
        /// <summary>
        /// Switches to next browser tab.
        /// </summary>
        private void SwitchTab()
        {
            NatiKeyboard.Down(Keys.LControlKey);
            NatiKeyboard.Press(Keys.Tab);
            NatiKeyboard.Up(Keys.LControlKey);
        }
        #endregion
        #region void MoveCursor(int x, int y)
        /// <summary>
        /// Moves the cursor in desired direction.
        /// </summary>
        /// <param name="x">x direction</param>
        /// <param name="y">y direction</param>
        private void MoveCursor(int x, int y)
        {
            System.Drawing.Point pCursor = System.Windows.Forms.Cursor.Position;

            pCursor.X += x;
            pCursor.Y += y;

            FitPointOnScreen(ref pCursor);

            System.Windows.Forms.Cursor.Position = pCursor;
        }
        #endregion

        #region void Process(string input)
        /// <summary>
        /// Processes user input and calls interaction functions when requested.
        /// </summary>
        /// <param name="input">user input string</param>
        private void Process(string input)
        {
            if (mode != MODE_IDLE)
            {
                #region Global actions (independent on active mode)
                if (IsAction(input, ACTION_EXIT))
                    Close();
                else if (IsAction(input, ACTION_STOP_LISTENING))
                    LoadRecognitionMode(MODE_IDLE);
                else if (IsAction(input, ACTION_ENTER))
                    NatiKeyboard.Press(Keys.Enter);
                else if (IsAction(input, ACTION_BACKSPACE))
                    NatiKeyboard.Press(Keys.Back);
                else if (IsAction(input, ACTION_DELETE))
                    NatiKeyboard.Press(Keys.Delete);
                else if (IsAction(input, ACTION_SELECT_ALL))
                    SelectAll();
                else if (IsAction(input, ACTION_DELETE_ALL))
                {
                    SelectAll();
                    NatiKeyboard.Press(Keys.Delete);
                }
                else if (IsAction(input, ACTION_LEFT))
                    NatiKeyboard.Press(Keys.Left);
                else if (IsAction(input, ACTION_RIGHT))
                    NatiKeyboard.Press(Keys.Right);
                else if (IsAction(input, ACTION_UP))
                    NatiKeyboard.Press(Keys.Up);
                else if (IsAction(input, ACTION_DOWN))
                    NatiKeyboard.Press(Keys.Down);
                #endregion
                else
                {
                    #region BROWSER
                    if (mode == MODE_BROWSER)
                    {
                        if (IsAction(input, ACTION_DICTATE_CHARACTERS))
                            LoadRecognitionMode(MODE_CHARACTERS);
                        else if (IsAction(input, ACTION_DICTATE_WORDS))
                            LoadRecognitionMode(MODE_WORDS);
                        else if (IsAction(input, ACTION_ELEMENT_PREVIOUS))
                        {
                            NatiKeyboard.Down(Keys.LShiftKey);
                            NatiKeyboard.Press(Keys.Tab);
                            NatiKeyboard.Up(Keys.LShiftKey);
                        }
                        else if (IsAction(input, ACTION_ELEMENT_NEXT))
                            NatiKeyboard.Press(Keys.Tab);
                        else if (IsAction(input, ACTION_PAGE_BACK))
                        {
                            NatiKeyboard.Down(System.Windows.Input.Key.LeftAlt);
                            NatiKeyboard.Press(Keys.Left);
                            NatiKeyboard.Up(System.Windows.Input.Key.LeftAlt);
                        }
                        else if (IsAction(input, ACTION_PAGE_FORWARD))
                        {
                            NatiKeyboard.Down(System.Windows.Input.Key.LeftAlt);
                            NatiKeyboard.Press(Keys.Right);
                            NatiKeyboard.Up(System.Windows.Input.Key.LeftAlt);
                        }
                        else if (IsAction(input, ACTION_START_BROWSER))
                            System.Diagnostics.Process.Start("chrome");
                        else if (IsAction(input, ACTION_TAB_NEW))
                            OpenNewTab();
                        else if (IsAction(input, ACTION_TAB_CLOSE))
                        {
                            NatiKeyboard.Down(Keys.LControlKey);
                            NatiKeyboard.Press(Keys.W);
                            NatiKeyboard.Up(Keys.LControlKey);
                        }
                        else if (IsAction(input, ACTION_TAB_REOPEN))
                        {
                            NatiKeyboard.Down(Keys.LShiftKey);
                            OpenNewTab();
                            NatiKeyboard.Up(Keys.LShiftKey);
                        }
                        else if (IsAction(input, ACTION_TAB_PREVIOUS))
                        {
                            NatiKeyboard.Down(Keys.LShiftKey);
                            SwitchTab();
                            NatiKeyboard.Up(Keys.LShiftKey);
                        }
                        else if (IsAction(input, ACTION_TAB_NEXT))
                            SwitchTab();
                        else if (IsAction(input, ACTION_RELOAD_PAGE))
                            NatiKeyboard.Press(Keys.F5);
                        else if (IsAction(input, ACTION_SCROLL_UP))
                            NatiKeyboard.Press(Keys.PageUp);
                        else if (IsAction(input, ACTION_SCROLL_DOWN))
                            NatiKeyboard.Press(Keys.PageDown);
                        else if (IsAction(input, ACTION_MOUSE_CLICK_LEFT))
                            NatiMouse.LeftClick();
                        else if (IsAction(input, ACTION_MOUSE_CLICK_RIGHT))
                            NatiMouse.RightClick();
                        else if (IsAction(input, ACTION_MOUSE_CLICK_MIDDLE))
                            NatiMouse.MiddleClick();
                        else if (IsAction(input, ACTION_MOUSE_MOVE))
                        {
                            string[] arrInput = input.Replace("move ", string.Empty).Split(' ');

                            if(arrInput.Length == 2)
                            {
                                int x = 0, y = 0, distance = 0;

                                switch(arrInput[0])
                                {
                                    case "top":
                                        y = -1;
                                        break;

                                    case "right":
                                        x = 1;
                                        break;

                                    case "bottom":
                                        y = 1;
                                        break;

                                    case "left":
                                        x = -1;
                                        break;

                                    default:
                                        break;
                                }

                                if(int.TryParse(arrInput[1], out distance))
                                {
                                    MoveCursor(x * distance, y * distance);
                                }
                            }
                        }
                    }
                    #endregion
                    #region CHARACTERS
                    else if (mode == MODE_CHARACTERS)
                    {
                        if (IsAction(input, ACTION_STOP_DICTATING))
                            LoadRecognitionMode(MODE_BROWSER);
                        else
                        {
                            foreach (Character ch in CHARACTERS)
                            {
                                if (NatiStringTools.ArrayContains(ch.Alternatives, input, false))
                                {
                                    SendKeys.SendWait(ch.CharString);
                                    return;
                                }
                            }
                        }
                    }
                    #endregion
                    #region WORDS
                    else if (mode == MODE_WORDS)
                    {
                        if (IsAction(input, ACTION_STOP_DICTATING))
                            LoadRecognitionMode(MODE_BROWSER);
                        else if (IsAction(input, ACTION_DELETE_LAST))
                        {
                            for (int i = 0; i < lastWordLength; i++)
                            {
                                NatiKeyboard.Press(Keys.Back);
                            }
                        }
                        else
                        {
                            SendKeys.SendWait(input + " ");
                            lastWordLength = input.Length + 1;
                            return;
                        }

                        // Make sure the word length is only stored after a word dictation
                        lastWordLength = 0;
                    }
                    #endregion
                }
            }
            #region IDLE
            else
            {
                if (IsAction(input, ACTION_START_LISTENING))
                    LoadRecognitionMode(MODE_BROWSER);
            }
            #endregion
        }
        #endregion
    }
}
