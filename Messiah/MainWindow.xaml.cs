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
        private MessiahAction ACTION_NULL = null;
        private MessiahAction ACTION_STOP_LISTENING     = new MessiahAction(1, "stop listening");                           // Switch to MODE_IDLE
        private MessiahAction ACTION_START_LISTENING    = new MessiahAction(2, "start listening");                          // Switch back to MODE_BROWSER
        private MessiahAction ACTION_DICTATE_CHARACTERS = new MessiahAction(3, "dictate characters", "dictate letters");    // Switch to MODE_CHARACTERS
        private MessiahAction ACTION_DICTATE_WORDS      = new MessiahAction(4, "start dictating", "dictate", "dictate words", "dictate sentences"); // Switch to MODE_WORDS
        private MessiahAction ACTION_STOP_DICTATING     = new MessiahAction(5, "stop dictating");                           // Switch back to MODE_BROWSER
        private MessiahAction ACTION_ENTER              = new MessiahAction(6, "new line", "enter", "return", "confirm");   // Press enter

        private MessiahAction ACTION_BACKSPACE          = new MessiahAction(201, "backspace");                              // Press backspace

        private MessiahAction ACTION_DELETE_LAST        = new MessiahAction(301, "delete last", "delete recent");           // Deletes the most recently written word
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
        private RecognitionMode[] modes = new RecognitionMode[4];
        private int mode = 0;
        private SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
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
            LoadRecognitionMode(MODE_BROWSER);
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
            modes[MODE_BROWSER] = new RecognitionMode(MODE_BROWSER, 0.9);
            AddAction(MODE_BROWSER,
                            ACTION_STOP_LISTENING,
                            ACTION_DICTATE_CHARACTERS,
                            ACTION_DICTATE_WORDS,
                            ACTION_ENTER
                            );

            #region MODE_CHARACTERS
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
                            ACTION_STOP_LISTENING,
                            ACTION_STOP_DICTATING,
                            ACTION_ENTER,
                            ACTION_BACKSPACE
                            );
            #endregion

            modes[MODE_WORDS] = new RecognitionMode(MODE_WORDS, 0.9);
            AddAction(MODE_WORDS,
                            ACTION_STOP_LISTENING,
                            ACTION_STOP_DICTATING,
                            ACTION_DELETE_LAST
                            );

            modes[MODE_IDLE] = new RecognitionMode(MODE_IDLE, 0.9);
            AddAction(MODE_IDLE, ACTION_START_LISTENING);
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

        private MessiahAction GetAction(string input)
        {
            return ACTION_NULL;
        }

        private bool IsAction(string str, MessiahAction act)
        {
            return NatiStringTools.ArrayContains(act.Alternatives, str, false);
        }

        #region void Process(string input)
        /// <summary>
        /// Processes user input and calls interaction functions when requested.
        /// </summary>
        /// <param name="input">user input string</param>
        private void Process(string input)
        {
            // Global actions (independent on active mode)
            if(mode != MODE_IDLE)
            {
                if (IsAction(input, ACTION_ENTER))
                    NatiKeyboard.Press(Keys.Enter);
            }

            if(mode == MODE_BROWSER)
            {
                if (IsAction(input, ACTION_STOP_LISTENING))
                    LoadRecognitionMode(MODE_IDLE);
                else if (IsAction(input, ACTION_DICTATE_CHARACTERS))
                    LoadRecognitionMode(MODE_CHARACTERS);
                else if (IsAction(input, ACTION_DICTATE_WORDS))
                    LoadRecognitionMode(MODE_WORDS);
            }
            else if(mode == MODE_CHARACTERS)
            {
                if (IsAction(input, ACTION_STOP_DICTATING))
                    LoadRecognitionMode(MODE_BROWSER);
                else if (IsAction(input, ACTION_BACKSPACE))
                    NatiKeyboard.Press(Keys.Back);
                else
                {
                    foreach (Character ch in CHARACTERS)
                    {
                        if (NatiStringTools.ArrayContains(ch.Alternatives, input, false))
                        {
                            SendKeys.SendWait(ch.CharString);
                            break;
                        }
                    }
                }
            }
            else if (mode == MODE_WORDS)
            {
                if (IsAction(input, ACTION_STOP_DICTATING))
                    LoadRecognitionMode(MODE_BROWSER);
                else if (IsAction(input, ACTION_DELETE_LAST))
                {

                }
                else
                {
                    SendKeys.SendWait(input);
                }
            }
            else if (mode == MODE_IDLE)
            {
                if (IsAction(input, ACTION_START_LISTENING))
                    LoadRecognitionMode(MODE_BROWSER);
            }
        }
        #endregion
    }
}
