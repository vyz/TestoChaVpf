using System.Windows;

namespace SrcChess2 {
    /// <summary>
    /// Pickup Game Parameter from the player
    /// </summary>
    public partial class frmGameParameter : Window {
        /// <summary>Father Window</summary>
        private MainWindow  Father { get; set; }

        /// <summary>
        /// Class Ctor
        /// </summary>
        public frmGameParameter() {
            InitializeComponent();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="father">       Father Window</param>
        private frmGameParameter(MainWindow father) : this() {
            Father = father;
            switch(Father.PlayingMode) {
            case MainWindow.PlayingModeE.DesignMode:
                throw new System.ApplicationException("Must not be called in design mode.");
            case MainWindow.PlayingModeE.PlayerAgainstComputer:
                radioButtonPlayerAgainstComputer.IsChecked = true;
                break;
            case MainWindow.PlayingModeE.PlayerAgainstPlayer:
                radioButtonPlayerAgainstPlayer.IsChecked = true;
                break;
            case MainWindow.PlayingModeE.ComputerAgainstComputer:
                radioButtonComputerAgainstComputer.IsChecked = true;
                break;
            }
            switch(Father.m_eComputerPlayingColor) {
            case ChessBoard.PlayerColorE.Black:
                radioButtonComputerPlayBlack.IsChecked = true;
                break;
            case ChessBoard.PlayerColorE.White:
                radioButtonComputerPlayWhite.IsChecked = true;
                break;
            }
            CheckState();
        }

        /// <summary>
        /// Check the state of the group box
        /// </summary>
        private void CheckState() {
            groupBoxComputerPlay.IsEnabled = radioButtonPlayerAgainstComputer.IsChecked.Value;
        }

        /// <summary>
        /// Called to accept the form
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event arguments</param>
        private void butOk_Click(object sender, RoutedEventArgs e) {
            if (radioButtonPlayerAgainstComputer.IsChecked == true) {
                Father.PlayingMode = MainWindow.PlayingModeE.PlayerAgainstComputer;
            } else if (radioButtonPlayerAgainstPlayer.IsChecked == true) {
                Father.PlayingMode = MainWindow.PlayingModeE.PlayerAgainstPlayer;
            } else if (radioButtonComputerAgainstComputer.IsChecked == true) {
                Father.PlayingMode = MainWindow.PlayingModeE.ComputerAgainstComputer;
            }
            Father.m_eComputerPlayingColor  = (radioButtonComputerPlayBlack.IsChecked == true) ? ChessBoard.PlayerColorE.Black : ChessBoard.PlayerColorE.White;
            DialogResult                    = true;
            Close();
        }

        /// <summary>
        /// Called when the radio button value is changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event arguments</param>
        private void radioButtonOpponent_CheckedChanged(object sender, RoutedEventArgs e) {
            CheckState();
        }

        /// <summary>
        /// Ask for the game parameter
        /// </summary>
        /// <param name="father">   Father window</param>
        /// <returns>
        /// true if succeed
        /// </returns>
        public static bool AskGameParameter(MainWindow father) {
            bool                bRetVal;
            frmGameParameter    frm;
            
            frm         = new frmGameParameter(father);
            frm.Owner   = father;
            bRetVal     = (frm.ShowDialog() == true);
            return(bRetVal);
        }
    } // Class frmGameParameter
} // Namespace
