using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;

namespace SrcChess2 {
    /// <summary>
    /// Interaction logic for frmPgnGamePicker.xaml
    /// </summary>
    public partial class frmPgnGamePicker : Window {
        /// <summary>List of moves for the current game</summary>
        public List<ChessBoard.MovePosS>    MoveList { get; private set; }
        /// <summary>Selected game</summary>
        public string                       SelectedGame { get; private set; }
        /// <summary>Starting board. Null if standard board</summary>
        public ChessBoard                   StartingChessBoard { get; private set; }
        /// <summary>Starting color</summary>
        public ChessBoard.PlayerColorE      StartingColor { get; private set; }
        /// <summary>White Player Name</summary>
        public string                       WhitePlayerName { get; private set; }
        /// <summary>Black Player Name</summary>
        public string                       BlackPlayerName { get; private set; }
        /// <summary>White Player Type</summary>
        public PgnParser.PlayerTypeE        WhitePlayerType { get; private set; }
        /// <summary>Black Player Type</summary>
        public PgnParser.PlayerTypeE        BlackPlayerType { get; private set; }
        /// <summary>White Timer</summary>
        public TimeSpan                     WhiteTimer { get; private set; }
        /// <summary>Black Timer</summary>
        public TimeSpan                     BlackTimer { get; private set; }
        /// <summary>Utility class</summary>
        private PgnUtil                     m_pgnUtil;
        /// <summary>Input stream</summary>
        private Stream                      m_streamInp;

        /// <summary>
        /// Class Ctor
        /// </summary>
        public frmPgnGamePicker() {
            InitializeComponent();
            m_pgnUtil               = new PgnUtil();
            SelectedGame            = null;
            StartingColor           = ChessBoard.PlayerColorE.White;
            StartingChessBoard      = null;
        }

        /// <summary>
        /// Get the selected game content
        /// </summary>
        /// <returns>
        /// Game or null if none selected
        /// </returns>
        private string GetSelectedGame() {
            string                  strRetVal;
            PgnUtil.PGNGameDescItem itemDesc;
            int                     iSelectedIndex;
            
            iSelectedIndex = listBoxGames.SelectedIndex;
            if (iSelectedIndex != -1) {
                itemDesc  = listBoxGames.SelectedItem as PgnUtil.PGNGameDescItem;
                strRetVal = m_pgnUtil.GetNGame(m_streamInp, itemDesc.Index);
            } else {
                strRetVal = null;
            }
            return(strRetVal);
        }

        /// <summary>
        /// Refresh the textbox containing the selected game content
        /// </summary>
        private void RefreshGameDisplay() {
            SelectedGame        = GetSelectedGame();
            textBoxGame.Text    = (SelectedGame == null) ? "" : SelectedGame;
        }

        /// <summary>
        /// Initialize the form with the content of the PGN file
        /// </summary>
        /// <param name="strFileName">  PGN file name</param>
        /// <returns>
        /// true if at least one game has been found.
        /// </returns>
        public bool InitForm(string strFileName) {
            bool    bRetVal;
            
            m_streamInp = PgnUtil.OpenInpFile(strFileName);
            if (m_streamInp == null) {
                bRetVal = false;
            } else {
                if (m_pgnUtil.FillListBoxWithDesc(m_streamInp, listBoxGames) < 1) {
                    MessageBox.Show("No games found in the PGN File '" + strFileName + "'");
                    bRetVal = false;
                } else {
                    listBoxGames.SelectedIndex = 0;
                    bRetVal                    = true;
                }
            }
            return(bRetVal);
        }

        private void GameSelected(bool bNoMove) {
            string                      strGame;
            PgnParser                   parser;
            List<ChessBoard.MovePosS>   listGame;
            int                         iSkip;
            int                         iTruncated;
            ChessBoard                  chessBoard;
            ChessBoard.PlayerColorE     eStartingColor;
            string                      strWhitePlayerName;
            string                      strBlackPlayerName;
            PgnParser.PlayerTypeE       eWhiteType;
            PgnParser.PlayerTypeE       eBlackType;
            TimeSpan                    spanWhitePlayer;
            TimeSpan                    spanBlackPlayer;
            
            strGame = GetSelectedGame();
            if (strGame != null) {
                listGame    = new List<ChessBoard.MovePosS>(256);
                parser      = new PgnParser(false);
                if (!parser.ParseSingle(strGame,
                                        bNoMove,
                                        listGame,
                                        out iSkip,
                                        out iTruncated,
                                        out chessBoard,
                                        out eStartingColor,
                                        out strWhitePlayerName,
                                        out strBlackPlayerName,
                                        out eWhiteType,
                                        out eBlackType,
                                        out spanWhitePlayer,
                                        out spanBlackPlayer)) {
                    MessageBox.Show("The specified board is invalid.");
                } else if (iSkip != 0) {
                    MessageBox.Show("The game is incomplete. Select another game.");
                } else if (iTruncated != 0) {
                    MessageBox.Show("The selected game includes an unsupported pawn promotion (only pawn promotion to queen is supported).");
                } else if (listGame.Count == 0 && chessBoard == null) {
                    MessageBox.Show("Game is empty.");
                } else {
                    StartingChessBoard  = chessBoard;
                    StartingColor       = eStartingColor;
                    WhitePlayerName     = strWhitePlayerName;
                    BlackPlayerName     = strBlackPlayerName;
                    WhitePlayerType     = eWhiteType;
                    BlackPlayerType     = eBlackType;
                    WhiteTimer          = spanWhitePlayer;
                    BlackTimer          = spanBlackPlayer;
                    MoveList            = listGame;
                    DialogResult        = true;
                    Close();
                }
            }
        }

        /// <summary>
        /// Accept the content of the form
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event argument</param>
        private void Button_Click(object sender, RoutedEventArgs e) {
            GameSelected(false /*bNoMove*/);
        }

        /// <summary>
        /// Accept the content of the form (but no move)
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event argument</param>
        private void Button_Click_1(object sender, RoutedEventArgs e) {
            GameSelected(true /*bNoMove*/);
        }

        /// <summary>
        /// Called when the game selection is changed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event argument</param>
        private void listBoxGames_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            RefreshGameDisplay();
        }
    } // Class frmPgnGamePicker
} // Namespace
