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
        private const int MODE_DEFAULT = 0;
        private const int MODE_CHARACTERS = 1;
        private const int MODE_WORDS = 2;
        #endregion
        #region Variables
        private RecognitionMode[] modes = new RecognitionMode[3];
        private int mode = MODE_DEFAULT;
        private SpeechRecognitionEngine recognizer = null;
        #endregion

        #region Window Events
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recognizer.LoadGrammarCompleted += Recognizer_LoadGrammarCompleted;
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            LoadRecognitionMode(MODE_DEFAULT);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            recognizer.RecognizeAsyncCancel();
        }
        #endregion
        #region SRE Events
        private void Recognizer_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
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
            modes[MODE_DEFAULT] = new RecognitionMode(MODE_DEFAULT, 0.9,
                                /*TODO: Commands*/string.Empty);

            modes[MODE_CHARACTERS] = new RecognitionMode(MODE_CHARACTERS, 0.9,
                                /*TODO: Commands*/string.Empty);

            modes[MODE_WORDS] = new RecognitionMode(MODE_WORDS, 0.9,
                                /*TODO: Commands*/string.Empty);
        }
        #endregion
        #region SpeechRecognitionEngine CreateSRE(ref SpeechRecognitionEngine sre, string preferredCulture)
        private SpeechRecognitionEngine CreateSRE(ref SpeechRecognitionEngine sre, string preferredCulture)
        {
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
            return (sre == null ? new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]) : sre);
        }
        #endregion
        #region void LoadRecognitionMode(int sreMode)
        private void LoadRecognitionMode(int sreMode)
        {
            recognizer.RecognizeAsyncCancel();
            CreateSRE(ref recognizer, "en-US");
            recognizer.LoadGrammarAsync(modes[sreMode].ModeGrammar);
            mode = sreMode;
        }
        #endregion
    }
}
