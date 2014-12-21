using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace SrcChess2 {
    /// <summary>
    /// Interaction logic for frmCreatePgnGame.xaml
    /// </summary>
    public partial class frmCreatePgnGame : Window {
        /// <summary>Array of move list</summary>
        public List<ChessBoard.MovePosS>    MoveList { get; private set; }
        /// <summary>Board starting position</summary>
        public ChessBoard                   StartingChessBoard { get; private set; }
        /// <summary>Starting Color</summary>
        public ChessBoard.PlayerColorE      StartingColor { get; private set; }
        /// <summary>Name of the player playing white</summary>
        public string                       WhitePlayerName { get; private set; }
        /// <summary>Name of the player playing black</summary>
        public string                       BlackPlayerName { get; private set; }
        /// <summary>Player type (computer or human)</summary>
        public PgnParser.PlayerTypeE        WhitePlayerType { get; private set; }
        /// <summary>Player type (computer or human)</summary>
        public PgnParser.PlayerTypeE        BlackPlayerType { get; private set; }
        /// <summary>White player playing time</summary>
        public TimeSpan                     WhiteTimer { get; private set; }
        /// <summary>Black player playing time</summary>
        public TimeSpan                     BlackTimer { get; private set; }

        /// <summary>
        /// Class Ctor
        /// </summary>
        public frmCreatePgnGame() {
            InitializeComponent();
            StartingColor   = ChessBoard.PlayerColorE.White;
        }

        /// <summary>
        /// Accept the content of the form
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event argument</param>
        private void butOk_Click(object sender, RoutedEventArgs e) {
            string                      strGame;
            PgnParser                   parser;
            List<ChessBoard.MovePosS>   listGame;
            int                         iSkip;
            int                         iTruncated;
            ChessBoard                  chessBoardStarting;
            ChessBoard.PlayerColorE     eStartingColor;
            string                      strWhitePlayerName;
            string                      strBlackPlayerName;
            PgnParser.PlayerTypeE       eWhiteType;
            PgnParser.PlayerTypeE       eBlackType;
            TimeSpan                    spanWhitePlayer;
            TimeSpan                    spanBlackPlayer;
            
            strGame = textBox1.Text;
            if (String.IsNullOrEmpty(strGame)) {
                MessageBox.Show("No PGN text has been pasted.");
            } else {
                listGame    = new List<ChessBoard.MovePosS>(256);
                parser      = new PgnParser(false);
                if (!parser.ParseSingle(strGame,
                                        false,
                                        listGame,
                                        out iSkip,
                                        out iTruncated,
                                        out chessBoardStarting,
                                        out eStartingColor,
                                        out strWhitePlayerName,
                                        out strBlackPlayerName,
                                        out eWhiteType,
                                        out eBlackType,
                                        out spanWhitePlayer,
                                        out spanBlackPlayer)) {
                    MessageBox.Show("The specified board is invalid.");
                } else if (iSkip != 0) {
                    MessageBox.Show("The game is incomplete. Paste another game.");
                } else if (iTruncated != 0) {
                    MessageBox.Show("The selected game includes an unsupported pawn promotion (only pawn promotion to queen is supported).");
                } else if (listGame.Count == 0 && chessBoardStarting == null) {
                    MessageBox.Show("Game is empty.");
                } else {
                    MoveList            = listGame;
                    StartingChessBoard  = chessBoardStarting;
                    StartingColor       = eStartingColor;
                    WhitePlayerName     = strWhitePlayerName;
                    BlackPlayerName     = strBlackPlayerName;
                    WhitePlayerType     = eWhiteType;
                    BlackPlayerType     = eBlackType;
                    WhiteTimer          = spanWhitePlayer;
                    BlackTimer          = spanBlackPlayer;
                    DialogResult        = true;
                    Close();
                }
            }
        }
    } // Class frmCreatePgnGame
} // Namespace
