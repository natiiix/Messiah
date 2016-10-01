#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using NatiTools;
#endregion

namespace Messiah
{
    public partial class MainWindow : Window
    {
        #region Constants
        private const int MODE_IDLE = 0;
        private const int MODE_CHARACTERS = 1;
        private const int MODE_WORDS = 2;
        private const int MODE_BROWSER = 3;

        private const int MATCH_NONE = 0;
        private const int MATCH_EQUALS = 1;
        private const int MATCH_CONTAINS = 2;
        #endregion
        #region Variables
        private RecognitionMode[] modes = new RecognitionMode[4];
        private int mode = MODE_BROWSER;
        private SpeechRecognitionEngine recognizer = null;
        #endregion

        #region Window Events
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammarCompleted += Recognizer_LoadGrammarCompleted;
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

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
            throw new NotImplementedException();
        }
        #endregion

        #region void PrepareModes()
        private void PrepareModes()
        {
            modes[MODE_BROWSER] = new RecognitionMode(MODE_BROWSER, 0.9,
                                "stop listening", // Switch to MODE_IDLE
                                "dictate characters", "dictate letters", // Switch to MODE_CHARACTERS
                                "start dictating", "dictate", "dictate words", "dictate sentences" // Switch to MODE_WORDS
                                );

            #region MODE_CHARACTERS
            string[] chars = new string[52];
            int charsLenHalf = chars.Length / 2;
            const int firstChar = 97;

            for (int i = 0; i < charsLenHalf; i++)
            {
                chars[i] = ASCIIEncoding.ASCII.GetChars(new byte[1] { (byte)(firstChar + i) })[0].ToString();
                chars[i + charsLenHalf] = "capital " + chars[i];
            }

            modes[MODE_CHARACTERS] = new RecognitionMode(MODE_CHARACTERS, 0.9,
                                chars,
                                "period",
                                "comma",
                                "percent",
                                "quote",
                                "single quote",
                                "at sign",
                                "question mark",
                                "exclamation mark",
                                "plus", "plus sign",
                                "minus", "minus sign",
                                "hyphen",
                                "colon",
                                "semicolon",
                                "asterisk",
                                "ampersand",
                                "apostrophe",
                                "slash", "forward slash",
                                "backslash", "backward slash",
                                "left bracket", "right bracket",
                                "left parenthesis", "right parenthesis",
                                "left brace", "right brace",
                                "left chevron", "right chevron",
                                "new line", "enter", "return", "confirm",
                                "stop dictating" // Switch back to MODE_DEFAULT
                                );
            #endregion

            modes[MODE_WORDS] = new RecognitionMode(MODE_WORDS, 0.9,
                                "stop dictating" // Switch back to MODE_DEFAULT
                                );

            modes[MODE_IDLE] = new RecognitionMode(MODE_IDLE, 0.9,
                                "start listening" // Switch back to MODE_DEFAULT
                                );
        }
        #endregion
        #region SpeechRecognitionEngine CreateSRE(ref SpeechRecognitionEngine sre, string preferredCulture)
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
            // Create a new empty SRE using American English culture if available
            CreateSRE(ref recognizer, "en-US");
            // In word dication mode, in addition to program's own grammar, dictation vocabulary is also loaded
            if (sreMode == MODE_WORDS) recognizer.LoadGrammar(new DictationGrammar());
            // Load program's dictionary
            recognizer.LoadGrammarAsync(modes[sreMode].ModeGrammar);
            // Set the active mode to desired mode
            mode = sreMode;
        }
        #endregion

        #region int Compare(string str1, string str2)
        /// <summary>
        /// Compares two strings and determines the relation between them.
        /// </summary>
        /// <param name="str1">first string</param>
        /// <param name="str2">second string</param>
        /// <param name="caseSensitive">Should the comparsion be case sensitive?</param>
        /// <returns>Returns the type of relation between strings.</returns>
        private int Compare(string str1, string str2, bool caseSensitive = false)
        {
            string[] strArr = new string[2] {
                caseSensitive ? str1 : str1.ToLower(),
                caseSensitive ? str2 : str2.ToLower() };

            if (strArr[0] == strArr[1])
                return MATCH_EQUALS;
            else if (strArr[0].Contains(strArr[1]) || strArr[1].Contains(strArr[0]))
                return MATCH_CONTAINS;
            else
                return MATCH_NONE;
        }
        #endregion

        #region void Process(string input)
        /// <summary>
        /// Processes user input and calls interaction functions when requested.
        /// </summary>
        /// <param name="input">user input string</param>
        private void Process(string input)
        {
            
        }
        #endregion
    }
}
