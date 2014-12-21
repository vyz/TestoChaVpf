using System;
using System.Windows;
using System.Windows.Controls;

namespace SrcChess2 {
    /// <summary>
    /// Ask user about search mode
    /// </summary>
    public partial class frmSearchMode : Window {
        /// <summary>Source search mode object</summary>
        private SearchEngine.SearchMode m_searchMode;
        /// <summary>Board evaluation utility class</summary>
        private BoardEvaluationUtil     m_boardEvalUtil;

        /// <summary>
        /// Class Ctor
        /// </summary>
        public frmSearchMode() {
            InitializeComponent();
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="searchMode">       Actual search mode</param>
        /// <param name="boardEvalUtil">    Board Evaluation list</param>
        public frmSearchMode(SearchEngine.SearchMode searchMode, BoardEvaluationUtil boardEvalUtil) : this() {
            int     iPos;
            
            m_searchMode    = searchMode;
            m_boardEvalUtil = boardEvalUtil;
            foreach (IBoardEvaluation boardEval in m_boardEvalUtil.BoardEvaluators) {
                iPos = comboBoxWhiteBEval.Items.Add(boardEval.Name);
                if (searchMode.m_boardEvaluationWhite == boardEval) {
                    comboBoxWhiteBEval.SelectedIndex = iPos;
                }
                iPos = comboBoxBlackBEval.Items.Add(boardEval.Name);
                if (searchMode.m_boardEvaluationBlack == boardEval) {
                    comboBoxBlackBEval.SelectedIndex = iPos;
                }
            }
            checkBoxTransTable.IsChecked    = ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseTransTable) != 0);
            if (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch) {
                radioButtonOnePerProc.IsChecked = true;
            } else if (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch) {
                radioButtonOneForUI.IsChecked   = true;
            } else {
                radioButtonNoThread.IsChecked   = true;
            }
            checkBoxBookOpening.IsChecked       = ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseBook) != 0);
            if ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseAlphaBeta) != 0) {
                radioButtonAlphaBeta.IsChecked  = true;
            } else {
                radioButtonMinMax.IsChecked     = true;
                checkBoxTransTable.IsEnabled    = false;
            }
            if (searchMode.m_iSearchDepth == 0) {
                radioButtonAvgTime.IsChecked    = true;
                textBoxTimeInSec.Text           = searchMode.m_iTimeOutInSec.ToString();
                plyCount.Value                  = 6;
            } else {
                if ((searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch) == SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch) {
                    radioButtonFixDepthIterative.IsChecked = true;
                } else {
                    radioButtonFixDepth.IsChecked = true;
                }
                plyCount.Value              = searchMode.m_iSearchDepth;
                textBoxTimeInSec.Text       = "15";
            }
            plyCount2.Content   = plyCount.Value.ToString();
            switch(searchMode.m_eRandomMode) {
            case SearchEngine.SearchMode.RandomModeE.Off:
                radioButtonRndOff.IsChecked     = true;
                break;
            case SearchEngine.SearchMode.RandomModeE.OnRepetitive:
                radioButtonRndOnRep.IsChecked   = true;
                break;
            default:
                radioButtonRndOn.IsChecked      = true;
                break;
            }
            textBoxTransSize.Text = (TransTable.TranslationTableSize / 1000000 * 32).ToString();    // Roughly 32 bytes / entry
            plyCount.ValueChanged += new RoutedPropertyChangedEventHandler<double>(plyCount_ValueChanged);
        }

        void plyCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            plyCount2.Content = plyCount.Value.ToString();
        }

        /// <summary>
        /// Called when radioButtonAlphaBeta checked state has been changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event parameter</param>
        private void radioButtonAlphaBeta_CheckedChanged(object sender, RoutedEventArgs e) {
            checkBoxTransTable.IsEnabled = radioButtonAlphaBeta.IsChecked.Value;
        }

        /// <summary>
        /// Set the plyCount/avgTime control state
        /// </summary>
        private void SetPlyAvgTimeState() {
            if (radioButtonAvgTime.IsChecked == true) {
                plyCount.IsEnabled         = false;
                labelNumberOfPly.IsEnabled = false;
                textBoxTimeInSec.IsEnabled = true;
                labelAvgTime.IsEnabled     = true;
            } else {
                plyCount.IsEnabled         = true;
                labelNumberOfPly.IsEnabled = true;
                textBoxTimeInSec.IsEnabled = false;
                labelAvgTime.IsEnabled     = false;
            }
        }

        /// <summary>
        /// Called when radioButtonFixDepth checked state has been changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event parameter</param>
        private void radioButtonSearchType_CheckedChanged(object sender, RoutedEventArgs e) {
            SetPlyAvgTimeState();
        }

        /// <summary>
        /// Called when the time in second textbox changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event parameter</param>
        private void textBoxTimeInSec_TextChanged(object sender, TextChangedEventArgs e) {
            int iVal;
            
            butOk.IsEnabled = (Int32.TryParse(textBoxTimeInSec.Text, out iVal) &&
                               iVal > 0 &&
                               iVal < 999);
        }

        /// <summary>
        /// Called when the transposition table size is changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event parameter</param>
        private void textBoxTransSize_TextChanged(object sender, TextChangedEventArgs e) {
            int iVal;
            
            butOk.IsEnabled = (Int32.TryParse(textBoxTransSize.Text, out iVal) &&
                               iVal > 4 &&
                               iVal < 256);
        }

        /// <summary>
        /// Update the SearchMode object
        /// </summary>
        public void UpdateSearchMode() {
            Properties.Settings settings = Properties.Settings.Default;
            int                 iTransTableSize;
            IBoardEvaluation    boardEval;
            
            settings.UseAlphaBeta           = radioButtonAlphaBeta.IsChecked.Value;
            settings.UseBook                = checkBoxBookOpening.IsChecked.Value;
            settings.UseTransTable          = checkBoxTransTable.IsChecked.Value;
            settings.PlyCount               = (int)plyCount.Value;
            settings.AverageTime            = Int32.Parse(textBoxTimeInSec.Text);
            settings.UsePlyCount            = radioButtonFixDepth.IsChecked.Value;
            settings.UsePlyCountIterative   = radioButtonFixDepthIterative.IsChecked.Value;
            m_searchMode.m_eOption          = (radioButtonAlphaBeta.IsChecked == true) ? SearchEngine.SearchMode.OptionE.UseAlphaBeta :
                                                                                         SearchEngine.SearchMode.OptionE.UseMinMax;
            if (checkBoxBookOpening.IsChecked == true) {
                m_searchMode.m_eOption |= SearchEngine.SearchMode.OptionE.UseBook;
            }
            if (checkBoxTransTable.IsChecked == true) {
                m_searchMode.m_eOption |= SearchEngine.SearchMode.OptionE.UseTransTable;
            }
            if (radioButtonOnePerProc.IsChecked == true) {
                m_searchMode.m_eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch;
                settings.UseThread            = 2;
            } else if (radioButtonOneForUI.IsChecked == true) {
                m_searchMode.m_eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch;
                settings.UseThread            = 1;
            } else {
                m_searchMode.m_eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.Off;
                settings.UseThread            = 0;
            }
            if (radioButtonAvgTime.IsChecked == true) {
                m_searchMode.m_iSearchDepth     = 0;
                m_searchMode.m_iTimeOutInSec    = Int32.Parse(textBoxTimeInSec.Text);
            } else {
                m_searchMode.m_iSearchDepth     = (int)plyCount.Value;
                m_searchMode.m_iTimeOutInSec    = 0;
                if (radioButtonFixDepthIterative.IsChecked == true) {
                    m_searchMode.m_eOption |= SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch;
                }
            }
            if (radioButtonRndOff.IsChecked == true) {
                m_searchMode.m_eRandomMode  = SearchEngine.SearchMode.RandomModeE.Off;
            } else if (radioButtonRndOnRep.IsChecked == true) {
                m_searchMode.m_eRandomMode  = SearchEngine.SearchMode.RandomModeE.OnRepetitive;
            } else {
                m_searchMode.m_eRandomMode = SearchEngine.SearchMode.RandomModeE.On;
            }
            settings.RandomMode             = (int)m_searchMode.m_eRandomMode;
            iTransTableSize                 = Int32.Parse(textBoxTransSize.Text);
            settings.TransTableSize         = iTransTableSize;
            TransTable.TranslationTableSize = iTransTableSize / 32 * 1000000;
            
            boardEval = m_boardEvalUtil.FindBoardEvaluator(comboBoxWhiteBEval.SelectedItem.ToString());
            if (boardEval == null) {
                boardEval = m_boardEvalUtil.BoardEvaluators[0];
            }
            m_searchMode.m_boardEvaluationWhite = boardEval;
            settings.WhiteBoardEval             = boardEval.Name;
            boardEval = m_boardEvalUtil.FindBoardEvaluator(comboBoxBlackBEval.SelectedItem.ToString());
            if (boardEval == null) {
                boardEval = m_boardEvalUtil.BoardEvaluators[0];
            }
            m_searchMode.m_boardEvaluationBlack = boardEval;
            settings.BlackBoardEval             = boardEval.Name;
            settings.Save();
        }

        /// <summary>
        /// Called when the Ok button is clicked
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event parameter</param>
        private void butOk_Click(object sender, RoutedEventArgs e) {
            DialogResult    = true;
            Close();
        }
    } // Class frmSearchMode
} // Namespace
