#region Using
using System;
using System.Text;
using System.Windows;
using System.Speech.Recognition;
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
        private MessiahAction ACTION_STOP_LISTENING     = new MessiahAction(1, "stop listening");                           // Switch to MODE_IDLE
        private MessiahAction ACTION_START_LISTENING    = new MessiahAction(2, "start listening");                          // Switch back to MODE_BROWSER
        private MessiahAction ACTION_DICTATE_CHARACTERS = new MessiahAction(3, "dictate", "dictate characters");            // Switch to MODE_CHARACTERS
        private MessiahAction ACTION_DICTATE_WORDS      = new MessiahAction(4, "dictate words", "dictate sentences");       // Switch to MODE_WORDS
        private MessiahAction ACTION_STOP_DICTATING     = new MessiahAction(5, "stop dictating");                           // Switch back to MODE_BROWSER

        private MessiahAction ACTION_ENTER              = new MessiahAction(21, "enter", "confirm");                        // Enter
        private MessiahAction ACTION_BACKSPACE          = new MessiahAction(22, "backspace");                               // Backspace
        private MessiahAction ACTION_DELETE             = new MessiahAction(23, "delete");                                  // Delete
        private MessiahAction ACTION_SELECT_ALL         = new MessiahAction(24, "select all");                              // Ctrl+A
        private MessiahAction ACTION_DELETE_ALL         = new MessiahAction(25, "delete all");                              // Ctrl+A and Delete

        private MessiahAction ACTION_LEFT               = new MessiahAction(31, "left");                                    // Press left arrow
        private MessiahAction ACTION_RIGHT              = new MessiahAction(32, "right");                                   // Press right arrow
        private MessiahAction ACTION_UP                 = new MessiahAction(33, "up");                                      // Press up arrow
        private MessiahAction ACTION_DOWN               = new MessiahAction(34, "down");                                    // Press down arrow

        private MessiahAction ACTION_ELEMENT_PREVIOUS   = new MessiahAction(101, "select previous", "previous element");    // Focus previous element
        private MessiahAction ACTION_ELEMENT_NEXT       = new MessiahAction(102, "select next", "next element");            // Focus next element
        private MessiahAction ACTION_PAGE_BACK          = new MessiahAction(103, "page back", "go back", "previous page");  // Go to the previous page
        private MessiahAction ACTION_PAGE_FORWARD       = new MessiahAction(104, "page forward", "go forward", "next page");// Go to the next page
        private MessiahAction ACTION_START_BROWSER      = new MessiahAction(105, "open browser", "start browser");          // Open new browser window
        private MessiahAction ACTION_TAB_NEW            = new MessiahAction(106, "new tab", "open new tab");                // Open new tab
        private MessiahAction ACTION_TAB_CLOSE          = new MessiahAction(107, "close tab", "close active tab");          // Close active tab
        private MessiahAction ACTION_TAB_REOPEN         = new MessiahAction(108, "reopen tab", "open last closed tab");     // Open the most recently closed tab
        private MessiahAction ACTION_TAB_PREVIOUS       = new MessiahAction(109, "previous tab", "switch to previous tab"); // Open the most recently closed tab
        private MessiahAction ACTION_TAB_NEXT           = new MessiahAction(110, "next tab", "switch to next tab", "switch tab"); // Open the most recently closed tab
        private MessiahAction ACTION_RELOAD_PAGE        = new MessiahAction(111, "reload page", "refresh page");            // Reload the page in active tab
        private MessiahAction ACTION_SCROLL_UP          = new MessiahAction(112, "scroll up", "scroll up");                 // Scroll the page up
        private MessiahAction ACTION_SCROLL_DOWN        = new MessiahAction(113, "scroll down", "page down");               // Scroll the page down

        private MessiahAction ACTION_DELETE_LAST        = new MessiahAction(301, "delete last", "delete recent");           // Delete the most recently written word
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
        #endregion
        #region Variables
        private SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
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

            PrepareModes();
            LoadRecognitionMode(MODE_IDLE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Make sure the program doesn't continue listening after the window was closed
            recognizer.RecognizeAsyncCancel();
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
            Display("Confidence: " + e.Result.Confidence.ToString() + Environment.NewLine +
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
                            ACTION_SCROLL_UP
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
            modes[MODE_IDLE] = new RecognitionMode(MODE_IDLE, 0.9, ACTION_START_LISTENING.Alternatives);
            #endregion
            #region Load common actions to all modes except for IDLE
            for (int i = 1; i < modes.Length; i++)
            {
                AddAction(i,
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
                if (IsAction(input, ACTION_STOP_LISTENING))
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
