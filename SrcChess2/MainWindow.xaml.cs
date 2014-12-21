using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace SrcChess2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ChessBoardControl.IMoveListUI {

        #region Types
        
        /// <summary>Getting computer against computer playing statistic</summary>
        private class ComputerPlayingStat {
            public  ComputerPlayingStat() { m_timeSpanMethod1 = TimeSpan.Zero; m_timeSpanMethod2 = TimeSpan.Zero; m_eResult = ChessBoard.MoveResultE.NoRepeat; m_iMethod1MoveCount = 0; m_iMethod2MoveCount = 0; m_bUserCancel = false; }
            public  TimeSpan                m_timeSpanMethod1;
            public  TimeSpan                m_timeSpanMethod2;
            public  ChessBoard.MoveResultE  m_eResult;
            public  int                     m_iMethod1MoveCount;
            public  int                     m_iMethod2MoveCount;
            public  bool                    m_bUserCancel;
        };
        
        /// <summary>Use for computer move</summary>
        public enum MessageModeE {
            /// <summary>No message</summary>
            Silent      = 0,
            /// <summary>Only messages for move which are terminating the game</summary>
            CallEndGame = 1,
            /// <summary>All messages</summary>
            Verbose     = 2
        };
        
        /// <summary>Current playing mode</summary>
        public enum PlayingModeE {
            /// <summary>Player plays against another player</summary>
            PlayerAgainstPlayer,
            /// <summary>Player plays against computer</summary>
            PlayerAgainstComputer,
            /// <summary>Computer play against computer</summary>
            ComputerAgainstComputer,
            /// <summary>Design mode.</summary>
            DesignMode
        };
        
        #endregion

        #region Command
        /// <summary>Command: New Game</summary>
        public static readonly RoutedUICommand              NewGameCommand              = new RoutedUICommand("_New Game",                      "NewGame",              typeof(MainWindow));
        /// <summary>Command: Load Game</summary>
        public static readonly RoutedUICommand              LoadGameCommand             = new RoutedUICommand("_Load Game",                     "LoadGame",             typeof(MainWindow));
        /// <summary>Command: Create Game</summary>
        public static readonly RoutedUICommand              CreateGameCommand           = new RoutedUICommand("_Create Game",                   "CreateGame",           typeof(MainWindow));
        /// <summary>Command: Save Game</summary>
        public static readonly RoutedUICommand              SaveGameCommand             = new RoutedUICommand("_Save Game",                     "SaveGame",             typeof(MainWindow));
        /// <summary>Command: Save Game in PGN</summary>
        public static readonly RoutedUICommand              SaveGameInPGNCommand        = new RoutedUICommand("Save Game _To PGN",              "SaveGameToPGN",        typeof(MainWindow));
        /// <summary>Command: Quit</summary>
        public static readonly RoutedUICommand              QuitCommand                 = new RoutedUICommand("_Quit",                          "Quit",                 typeof(MainWindow));

        /// <summary>Command: Hint</summary>
        public static readonly RoutedUICommand              HintCommand                 = new RoutedUICommand("_Hint",                          "Hint",                 typeof(MainWindow));
        /// <summary>Command: Undo</summary>
        public static readonly RoutedUICommand              UndoCommand                 = new RoutedUICommand("_Undo",                          "Undo",                 typeof(MainWindow));
        /// <summary>Command: Redo</summary>
        public static readonly RoutedUICommand              RedoCommand                 = new RoutedUICommand("_Redo",                          "Redo",                 typeof(MainWindow));
        /// <summary>Command: Revert Board</summary>
        public static readonly RoutedUICommand              RevertBoardCommand          = new RoutedUICommand("Revert _Board",                  "RevertBoard",          typeof(MainWindow));
        /// <summary>Command: Player Against Player</summary>
        public static readonly RoutedUICommand              PlayerAgainstPlayerCommand  = new RoutedUICommand("_Player Against Player",         "PlayerAgainstPlayer",  typeof(MainWindow));
        /// <summary>Command: Automatic Play</summary>
        public static readonly RoutedUICommand              AutomaticPlayCommand        = new RoutedUICommand("_Automatic Play",                "AutomaticPlay",        typeof(MainWindow));
        /// <summary>Command: Fast Automatic Play</summary>
        public static readonly RoutedUICommand              FastAutomaticPlayCommand    = new RoutedUICommand("_Fast Automatic Play",           "FastAutomaticPlay",    typeof(MainWindow));
        /// <summary>Command: Cancel Play</summary>
        public static readonly RoutedUICommand              CancelPlayCommand           = new RoutedUICommand("_Cancel Play",                   "CancelPlay",           typeof(MainWindow));
        /// <summary>Command: Design Mode</summary>
        public static readonly RoutedUICommand              DesignModeCommand           = new RoutedUICommand("_Design Mode",                   "DesignMode",           typeof(MainWindow));

        /// <summary>Command: Search Mode</summary>
        public static readonly RoutedUICommand              SearchModeCommand           = new RoutedUICommand("_Search Mode...",                "SearchMode",           typeof(MainWindow));
        /// <summary>Command: Flash Piece</summary>
        public static readonly RoutedUICommand              FlashPieceCommand           = new RoutedUICommand("_Flash Piece",                   "FlashPiece",           typeof(MainWindow));
        /// <summary>Command: PGN Notation</summary>
        public static readonly RoutedUICommand              PGNNotationCommand          = new RoutedUICommand("_PGN Notation",                  "PGNNotation",          typeof(MainWindow));
        /// <summary>Command: Board Settings</summary>
        public static readonly RoutedUICommand              BoardSettingCommand         = new RoutedUICommand("_Board Settings...",             "BoardSettings",         typeof(MainWindow));
        
        /// <summary>Command: Create a Book</summary>
        public static readonly RoutedUICommand              CreateBookCommand           = new RoutedUICommand("_Create a Book...",              "CreateBook",           typeof(MainWindow));
        /// <summary>Command: Filter a PGN File</summary>
        public static readonly RoutedUICommand              FilterPGNFileCommand        = new RoutedUICommand("_Filter a PGN File...",          "FilterPGNFile",        typeof(MainWindow));
        /// <summary>Command: Test Board Evaluation</summary>
        public static readonly RoutedUICommand              TestBoardEvaluationCommand  = new RoutedUICommand("_Test Board Evaluation...",      "TestBoardEvaluation",  typeof(MainWindow));

        /// <summary>Command: Test Board Evaluation</summary>
        public static readonly RoutedUICommand              AboutCommand                = new RoutedUICommand("_About...",                      "About",                typeof(MainWindow));

        /// <summary>List of all supported commands</summary>
        private static readonly RoutedUICommand[]           m_arrCommands = new RoutedUICommand[] { NewGameCommand,
                                                                                                    LoadGameCommand,
                                                                                                    CreateGameCommand,
                                                                                                    SaveGameCommand,
                                                                                                    SaveGameInPGNCommand,
                                                                                                    QuitCommand,
                                                                                                    HintCommand,
                                                                                                    UndoCommand,
                                                                                                    RedoCommand,
                                                                                                    RevertBoardCommand,
                                                                                                    PlayerAgainstPlayerCommand,
                                                                                                    AutomaticPlayCommand,
                                                                                                    FastAutomaticPlayCommand,
                                                                                                    CancelPlayCommand,
                                                                                                    DesignModeCommand,
                                                                                                    SearchModeCommand,
                                                                                                    FlashPieceCommand,
                                                                                                    PGNNotationCommand,
                                                                                                    BoardSettingCommand,
                                                                                                    CreateBookCommand,
                                                                                                    FilterPGNFileCommand,
                                                                                                    TestBoardEvaluationCommand,
                                                                                                    AboutCommand };
        #endregion

        #region Members        
        /// <summary>Playing mode (player vs player, player vs computer, computer vs computer</summary>
        private PlayingModeE                m_ePlayingMode;
        /// <summary>Color played by the computer</summary>
        public ChessBoard.PlayerColorE      m_eComputerPlayingColor;
        /// <summary>true if a secondary thread is busy computing a move</summary>
        private bool                        m_bSecondThreadBusy;
        /// <summary>Search mode</summary>
        private SearchEngine.SearchMode     m_searchMode;
        /// <summary>Utility class to handle board evaluation objects</summary>
        private BoardEvaluationUtil         m_boardEvalUtil;
        /// <summary>List of piece sets</summary>
        private SortedList<string,PieceSet> m_listPieceSet;
        /// <summary>Currently selected piece set</summary>
        private PieceSet                    m_pieceSet;
        /// <summary>Color use to create the background brush</summary>
        private Color                       m_colorBackground;
        /// <summary>Dispatcher timer</summary>
        private DispatcherTimer             m_dispatcherTimer;
        #endregion

        #region Ctor

        /// <summary>
        /// Static Ctor
        /// </summary>
        static MainWindow() {
            NewGameCommand.InputGestures.Add(               new KeyGesture(Key.N,           ModifierKeys.Control));
            LoadGameCommand.InputGestures.Add(              new KeyGesture(Key.O,           ModifierKeys.Control));
            SaveGameCommand.InputGestures.Add(              new KeyGesture(Key.S,           ModifierKeys.Control));
            QuitCommand.InputGestures.Add(                  new KeyGesture(Key.F4,          ModifierKeys.Alt));
            HintCommand.InputGestures.Add(                  new KeyGesture(Key.H,           ModifierKeys.Control));
            UndoCommand.InputGestures.Add(                  new KeyGesture(Key.Z,           ModifierKeys.Control));
            RedoCommand.InputGestures.Add(                  new KeyGesture(Key.Y,           ModifierKeys.Control));
            RevertBoardCommand.InputGestures.Add(           new KeyGesture(Key.B,           ModifierKeys.Control));
            PlayerAgainstPlayerCommand.InputGestures.Add(   new KeyGesture(Key.P,           ModifierKeys.Control));
            AutomaticPlayCommand.InputGestures.Add(         new KeyGesture(Key.F2,          ModifierKeys.Control));
            FastAutomaticPlayCommand.InputGestures.Add(     new KeyGesture(Key.F3,          ModifierKeys.Control));
            CancelPlayCommand.InputGestures.Add(            new KeyGesture(Key.C,           ModifierKeys.Control));
            DesignModeCommand.InputGestures.Add(            new KeyGesture(Key.D,           ModifierKeys.Control));
            SearchModeCommand.InputGestures.Add(            new KeyGesture(Key.M,           ModifierKeys.Control));
            AboutCommand.InputGestures.Add(                 new KeyGesture(Key.F1));
        }

        /// <summary>
        /// Class Ctor
        /// </summary>
        public MainWindow() {
            SrcChess2.Properties.Settings   settings;
            ExecutedRoutedEventHandler      onExecutedCmd;
            CanExecuteRoutedEventHandler    onCanExecuteCmd;
            PieceSet                        pieceSet;

            InitializeComponent();
            settings                            = SrcChess2.Properties.Settings.Default;
            m_listPieceSet                      = PieceSetStandard.LoadPieceSetFromResource();
            pieceSet                            = m_listPieceSet[settings.PieceSet];
            m_chessCtl.Father                   = this;
            m_chessCtl.LiteCellColor            = NameToColor(settings.LiteCellColor);
            m_chessCtl.DarkCellColor            = NameToColor(settings.DarkCellColor);
            m_chessCtl.WhitePieceColor          = NameToColor(settings.WhitePieceColor);
            m_chessCtl.BlackPieceColor          = NameToColor(settings.BlackPieceColor);
            m_colorBackground                   = NameToColor(settings.BackgroundColor);
            Background                          = new SolidColorBrush(m_colorBackground);
            m_boardEvalUtil                     = new BoardEvaluationUtil();
            SetSearchModeFromSetting(settings);
            m_chessCtl.UpdateCmdState          += new EventHandler(m_chessCtl_UpdateCmdState);
            PlayingMode                         = PlayingModeE.PlayerAgainstComputer;
            m_eComputerPlayingColor             = ChessBoard.PlayerColorE.Black;
            m_lostPieceBlack.ChessBoardControl  = m_chessCtl;
            m_lostPieceBlack.Color              = true;
            m_lostPieceWhite.ChessBoardControl  = m_chessCtl;
            m_lostPieceWhite.Color              = false;
            m_moveViewer.NewMoveSelected       += new MoveViewer.NewMoveSelectedHandler(m_moveViewer_NewMoveSelected);
            m_moveViewer.DisplayMode            = (settings.MoveNotation == 0) ? MoveViewer.DisplayModeE.MovePos : MoveViewer.DisplayModeE.PGN;
            m_chessCtl.MoveListUI               = this;
            m_chessCtl.MoveSelected            += new ChessBoardControl.MoveSelectedEventHandler(m_chessCtl_MoveSelected);
            m_chessCtl.QueryPiece              += new ChessBoardControl.QueryPieceEventHandler(m_chessCtl_QueryPiece);
            m_chessCtl.QueryPawnPromotionType  += new ChessBoardControl.QueryPawnPromotionTypeEventHandler(m_chessCtl_QueryPawnPromotionType);
            m_bSecondThreadBusy                 = false;
            m_dispatcherTimer                   = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, new EventHandler(dispatcherTimer_Tick), Dispatcher);
            m_dispatcherTimer.Start();
            SetCmdState();
            ShowSearchMode();
            mnuOptionFlashPiece.IsChecked       = settings.FlashPiece;
            mnuOptionPGNNotation.IsChecked      = (m_moveViewer.DisplayMode == MoveViewer.DisplayModeE.PGN);
            PieceSet                            = pieceSet;
            onExecutedCmd                       = new ExecutedRoutedEventHandler(OnExecutedCmd);
            onCanExecuteCmd                     = new CanExecuteRoutedEventHandler(OnCanExecuteCmd);
            foreach (RoutedUICommand cmd in m_arrCommands) {
                CommandBindings.Add(new CommandBinding(cmd, onExecutedCmd, onCanExecuteCmd));
            }
        }

        /// <summary>
        /// Set the searching mode using the specified setting
        /// </summary>
        /// <param name="settings"> User setting</param>
        private void SetSearchModeFromSetting(SrcChess2.Properties.Settings settings) {
            SearchEngine.SearchMode.OptionE         eOption;
            SearchEngine.SearchMode.ThreadingModeE  eThreadingMode;
            int                                     iTransTableSize;
            IBoardEvaluation                        boardEvalWhite;
            IBoardEvaluation                        boardEvalBlack;

            iTransTableSize                 = (settings.TransTableSize < 5 || settings.TransTableSize > 256) ? 32 : settings.TransTableSize;
            TransTable.TranslationTableSize = iTransTableSize / 32 * 1000000;
            eOption                         = settings.UseAlphaBeta ? SearchEngine.SearchMode.OptionE.UseAlphaBeta : SearchEngine.SearchMode.OptionE.UseMinMax;
            if (settings.UseBook) {
                eOption |= SearchEngine.SearchMode.OptionE.UseBook;
            }
            if (settings.UseTransTable) {
                eOption |= SearchEngine.SearchMode.OptionE.UseTransTable;
            }
            if (settings.UsePlyCountIterative) {
                eOption |= SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch;
            }
            switch(settings.UseThread) {
            case 0:
                eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.Off;
                break;
            case 1:
                eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch;
                break;
            default:
                eThreadingMode = SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch;
                break;
            }
            boardEvalWhite = m_boardEvalUtil.FindBoardEvaluator(settings.WhiteBoardEval);
            if (boardEvalWhite == null) {
                boardEvalWhite = m_boardEvalUtil.BoardEvaluators[0];
            }
            boardEvalBlack = m_boardEvalUtil.FindBoardEvaluator(settings.BlackBoardEval);
            if (boardEvalBlack == null) {
                boardEvalBlack = m_boardEvalUtil.BoardEvaluators[0];
            }
            m_searchMode = new SearchEngine.SearchMode(boardEvalWhite,
                                                       boardEvalBlack,
                                                       eOption,
                                                       eThreadingMode,
                                                       settings.UsePlyCount | settings.UsePlyCountIterative ? ((settings.PlyCount > 1 && settings.PlyCount < 9) ? settings.PlyCount : 6) : 0,  // Maximum depth
                                                       settings.UsePlyCount | settings.UsePlyCountIterative ? 0 : (settings.AverageTime > 0 && settings.AverageTime < 1000) ? settings.AverageTime : 15,
                                                       (settings.RandomMode >= 0 && settings.RandomMode <= 2) ? (SearchEngine.SearchMode.RandomModeE)settings.RandomMode : SearchEngine.SearchMode.RandomModeE.On); // Average time
        }

        /// <summary>
        /// Convert a color name to a color
        /// </summary>
        /// <param name="strName">  Name of the color or hexa representation of the color</param>
        /// <returns>
        /// Color
        /// </returns>
        private Color NameToColor(string strName) {
            Color   colRetVal;
            int     iVal;
            
            if (strName.Length == 8 && (Char.IsLower(strName[0]) || Char.IsDigit(strName[0])) &&
                Int32.TryParse(strName, System.Globalization.NumberStyles.HexNumber, null, out iVal)) { 
                colRetVal = Color.FromArgb((byte)((iVal >> 24) & 255), (byte)((iVal >> 16) & 255), (byte)((iVal >> 8) & 255), (byte)(iVal & 255));
            } else {
                colRetVal = (Color)ColorConverter.ConvertFromString(strName);
            }
            return(colRetVal);    
        }

        #endregion

        #region Command Handling
        /// <summary>
        /// Executes the specified command
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Routed event argument</param>
        public virtual void OnExecutedCmd(object sender, ExecutedRoutedEventArgs e) {
            if (e.Command == NewGameCommand) {
                NewGame();
            } else if (e.Command == LoadGameCommand) {
                LoadGame();
            } else if (e.Command == CreateGameCommand) {
                CreateGame();
            } else if (e.Command == SaveGameCommand) {
                m_chessCtl.SaveToFile();
            } else if (e.Command == SaveGameInPGNCommand) {
                m_chessCtl.SavePGNToFile();
            } else if (e.Command == QuitCommand) {
                Close();
            } else if (e.Command == HintCommand) {
                ShowHint();
            } else if (e.Command == UndoCommand) {
                Undo((PlayingMode != PlayingModeE.PlayerAgainstPlayer));
            } else if (e.Command == RedoCommand) {
                Redo();
            } else if (e.Command == RevertBoardCommand) {
                RevertBoard();
            } else if (e.Command == PlayerAgainstPlayerCommand) {
                TogglePlayerAgainstPlayer();
            } else if (e.Command == AutomaticPlayCommand) {
                PlayComputerAgainstComputer(true);
            } else if (e.Command == FastAutomaticPlayCommand) {
                PlayComputerAgainstComputer(false);
            } else if (e.Command == CancelPlayCommand) {
                CancelAutoPlay();
            } else if (e.Command == DesignModeCommand) {
                ToggleDesignMode();
            } else if (e.Command == SearchModeCommand) {
                SetSearchMode();
            } else if (e.Command == FlashPieceCommand) {
                ToggleFlashPiece();
            } else if (e.Command == PGNNotationCommand) {
                TogglePGNNotation();
            } else if (e.Command == BoardSettingCommand) {
                ChooseBoardSetting();
            } else if (e.Command == CreateBookCommand) {
                m_chessCtl.CreateBookFromFiles();
            } else if (e.Command == FilterPGNFileCommand) {
                FilterPGNFile();
            } else if (e.Command == TestBoardEvaluationCommand) {
                TestBoardEvaluation();
            } else if (e.Command == AboutCommand) {
                ShowAbout();
            } else {
                e.Handled   = false;
            }
        }

        /// <summary>
        /// Determine if a command can be executed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Routed event argument</param>
        public virtual void OnCanExecuteCmd(object sender, CanExecuteRoutedEventArgs e) {
            bool    bDesignMode;

            bDesignMode = (PlayingMode == PlayingModeE.DesignMode);
            if (e.Command == NewGameCommand                     ||
                e.Command == CreateGameCommand                  ||
                e.Command == LoadGameCommand                    ||
                e.Command == SaveGameCommand                    ||
                e.Command == SaveGameInPGNCommand               ||
                e.Command == HintCommand                        ||
                e.Command == PlayerAgainstPlayerCommand         ||
                e.Command == AutomaticPlayCommand               ||
                e.Command == FastAutomaticPlayCommand           ||
                e.Command == RevertBoardCommand                 ||
                e.Command == CreateBookCommand                  ||
                e.Command == FilterPGNFileCommand) {
                e.CanExecute    = !(m_bSecondThreadBusy || bDesignMode);
            } else if (e.Command == QuitCommand                 ||
                       e.Command == SearchModeCommand           ||
                       e.Command == FlashPieceCommand           ||
                       e.Command == DesignModeCommand           ||
                       e.Command == PGNNotationCommand          ||
                       e.Command == BoardSettingCommand         ||
                       e.Command == TestBoardEvaluationCommand  ||
                       e.Command == AboutCommand) {
                e.CanExecute    = !m_bSecondThreadBusy;
            } else if (e.Command == CancelPlayCommand) {
                e.CanExecute    = m_bSecondThreadBusy;
            } else if (e.Command == UndoCommand) {
                e.CanExecute    = (!m_bSecondThreadBusy && !bDesignMode && m_chessCtl.UndoCount >= ((m_ePlayingMode == PlayingModeE.PlayerAgainstPlayer) ? 1 : 2));
            } else if (e.Command == RedoCommand) {
                e.CanExecute    = (!m_bSecondThreadBusy && !bDesignMode && m_chessCtl.RedoCount != 0);
            } else {
                e.Handled   = false;
            }
        }
        #endregion

        /// <summary>
        /// Used piece set
        /// </summary>
        public PieceSet PieceSet {
            get {
                return(m_pieceSet);
            }
            set {
                if (m_pieceSet != value) {
                    m_pieceSet                  = value;
                    m_chessCtl.PieceSet         = value;
                    m_lostPieceBlack.PieceSet   = value;
                    m_lostPieceWhite.PieceSet   = value;
                }
            }
        }

        /// <summary>
        /// Current playing mode (player vs player, player vs computer or computer vs computer)
        /// </summary>
        public PlayingModeE PlayingMode {
            get {
                return(m_ePlayingMode);
            }
            set {
                m_ePlayingMode                          = value;
                mnuEditPlayerAgainstPlayer.IsChecked    = (m_ePlayingMode == PlayingModeE.PlayerAgainstPlayer);
                switch(m_ePlayingMode) {
                case PlayingModeE.PlayerAgainstPlayer:
                    m_chessCtl.WhitePlayerType = PgnParser.PlayerTypeE.Human;
                    m_chessCtl.BlackPlayerType = PgnParser.PlayerTypeE.Human;
                    break;
                case PlayingModeE.PlayerAgainstComputer:
                    if (m_eComputerPlayingColor == ChessBoard.PlayerColorE.White) {
                        m_chessCtl.WhitePlayerType = PgnParser.PlayerTypeE.Program;
                        m_chessCtl.BlackPlayerType = PgnParser.PlayerTypeE.Human;
                    } else {
                        m_chessCtl.WhitePlayerType = PgnParser.PlayerTypeE.Human;
                        m_chessCtl.BlackPlayerType = PgnParser.PlayerTypeE.Program;
                    }
                    break;
                default:
                    m_chessCtl.WhitePlayerType = PgnParser.PlayerTypeE.Program;
                    m_chessCtl.BlackPlayerType = PgnParser.PlayerTypeE.Program;
                    break;
                }
            }
        }

        /// <summary>
        /// Set the current playing mode. Defined as a method so it can be called by a delegate
        /// </summary>
        /// <param name="ePlayingMode"> Playing mode</param>
        private void SetPlayingMode(PlayingModeE ePlayingMode) {
            PlayingMode = ePlayingMode;
        }

        /// <summary>
        /// Start asynchronous computing
        /// </summary>
        private void StartAsyncComputing() {
            bool    bDifferentThreadForUI;
            
            if (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch) {
                bDifferentThreadForUI   = true;
            } else if (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch) {
                bDifferentThreadForUI   = true;
            } else {
                bDifferentThreadForUI   = false;
            }
            if (bDifferentThreadForUI) {
                m_bSecondThreadBusy = true;
                SetCmdState();
            }
            m_statusLabelMove.Content           = "Finding Best Move...";
            m_statusLabelPermutation.Content    = "";
            Cursor = Cursors.Wait;
        }

        /// <summary>
        /// Show a move in status bar
        /// </summary>
        /// <param name="ePlayerColor"> Color of the move</param>
        /// <param name="move">         Move</param>
        /// <param name="iPermCount">   Permutation analyzed. 0 for none. -1 for book</param>
        /// <param name="iDepth">       Depth of the search (-1 if none)</param>
        /// <param name="iCacheHit">    Nb of permutation found in the translation table</param>
        private void ShowMoveInStatusBar(ChessBoard.PlayerColorE ePlayerColor, ChessBoard.MovePosS move, int iPermCount, int iDepth, int iCacheHit) {
            string                              strPermCount;
            System.Globalization.CultureInfo    ci;
            
            ci = new System.Globalization.CultureInfo("en-US");
            switch(iPermCount) {
            case -1:
                strPermCount = "Found in Book.";
                break;
            case 0:
                strPermCount = "---";
                break;
            default:
                strPermCount = iPermCount.ToString("C0", ci).Replace("$", "") + " permutations evaluated. " + iCacheHit.ToString("C0", ci).Replace("$","") + " found in cache.";
                break;
            }
            if (iDepth != -1) {
                strPermCount += " " + iDepth.ToString() + " ply.";
            }
            strPermCount                       += " " + m_chessCtl.LastFindBestMoveTimeSpan.TotalSeconds.ToString() + " sec(s).";
            m_statusLabelMove.Content           = ((ePlayerColor == ChessBoard.PlayerColorE.Black) ? "Black " : "White ") + ChessBoard.GetHumanPos(move);
            m_statusLabelPermutation.Content    = strPermCount;
        }

        /// <summary>
        /// Show the current searching parameters in the status bar
        /// </summary>
        private void ShowSearchMode() {
            string  str;
            
            if ((m_searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseAlphaBeta) == SearchEngine.SearchMode.OptionE.UseAlphaBeta) {
                str = "Alpha-Beta ";
            } else {
                str = "Min-Max ";
            }
            if (m_searchMode.m_iSearchDepth == 0) {
                str += "(Iterative " + m_searchMode.m_iTimeOutInSec.ToString() + " secs) ";
            } else if ((m_searchMode.m_eOption & SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch) == SearchEngine.SearchMode.OptionE.UseIterativeDepthSearch) {
                str += "(Iterative " + m_searchMode.m_iSearchDepth.ToString() + " ply) ";
            } else {
                str += "(" + m_searchMode.m_iSearchDepth.ToString() + " ply) ";
            }
            if (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch) {
                str += "using " + Environment.ProcessorCount.ToString() + " processor";
                if (Environment.ProcessorCount > 1) {
                    str += "s";
                }
                str += ". ";
            } else {
                str += "using 1 processor. ";
            }
            m_statusLabelSearchMode.Content = str;
        }

        /// <summary>
        /// Remove all moves from the list
        /// </summary>
        /// <param name="chessBoard">       Starting chess board</param>
        void ChessBoardControl.IMoveListUI.Reset(ChessBoard chessBoard) {
            m_moveViewer.Reset(chessBoard);
        }

        /// <summary>
        /// Append a new move to the list
        /// </summary>
        /// <param name="iPermCount">   Permutation analyzed. 0 for none. -1 for book</param>
        /// <param name="iDepth">       Depth</param>
        /// <param name="iCacheHit">    Nb of permutation found in the translation table</param>

        void ChessBoardControl.IMoveListUI.NewMoveDone(int iPermCount, int iDepth, int iCacheHit) {
            ChessBoard.MovePosS     movePos;
            ChessBoard.PlayerColorE eMoveColor;
            
            m_moveViewer.NewMoveDone(iPermCount, iDepth, iCacheHit);
            movePos     = m_chessCtl.ChessBoard.MovePosStack.CurrentMove;
            eMoveColor  = m_chessCtl.ChessBoard.CurrentMoveColor;
            ShowMoveInStatusBar(eMoveColor, movePos, iPermCount, iDepth, iCacheHit);
        }

        /// <summary>
        /// Remove the last move from the list
        /// </summary>
        void ChessBoardControl.IMoveListUI.RedoPosChanged() {
            m_moveViewer.RedoPosChanged();
        }

        /// <summary>
        /// Display a message related to the MoveStateE
        /// </summary>
        /// <param name="eMoveResult">  Move result</param>
        /// <param name="eMessageMode"> Message mode</param>
        /// <returns>
        /// true if it's the end of the game. false if not
        /// </returns>
        private bool DisplayMessage(ChessBoard.MoveResultE eMoveResult, MessageModeE eMessageMode) {
            bool    bRetVal;
            string  strOpponent;
            
            if (m_ePlayingMode == PlayingModeE.PlayerAgainstComputer) {
                if (m_chessCtl.ChessBoard.NextMoveColor == m_eComputerPlayingColor) {
                    strOpponent = "Computer is ";
                } else {
                    strOpponent = "You are ";
                }
            } else {
                strOpponent = (m_chessCtl.ChessBoard.NextMoveColor == ChessBoard.PlayerColorE.White) ? "White player is " : "Black player is ";
            }
            switch(eMoveResult) {
            case ChessBoard.MoveResultE.NoRepeat:
                bRetVal = false;
                break;
            case ChessBoard.MoveResultE.TieNoMove:
                if (eMessageMode != MessageModeE.Silent) {
                    MessageBox.Show("Draw. " + strOpponent + "unable to move.");
                }
                bRetVal = true;
                break;
            case ChessBoard.MoveResultE.TieNoMatePossible:
                if (eMessageMode != MessageModeE.Silent) {
                    MessageBox.Show("Draw. Not enough pieces to make a checkmate.");
                }
                bRetVal = true;
                break;
            case ChessBoard.MoveResultE.ThreeFoldRepeat:
                if (eMessageMode != MessageModeE.Silent) {
                    MessageBox.Show("Draw. 3 times the same board.");
                }
                bRetVal = true;
                break;
            case ChessBoard.MoveResultE.FiftyRuleRepeat:
                if (eMessageMode != MessageModeE.Silent) {
                    MessageBox.Show("Draw. 50 moves without moving a pawn or eating a piece.");
                }
                bRetVal = true;
                break;
            case ChessBoard.MoveResultE.Check:
                if (eMessageMode == MessageModeE.Verbose) {
                    MessageBox.Show(strOpponent + "in check.");
                }
                bRetVal = false;
                break;
            case ChessBoard.MoveResultE.Mate:
                if (eMessageMode != MessageModeE.Silent) {
                    MessageBox.Show(strOpponent + "checkmate.");
                }
                bRetVal = true;
                break;
            default:
                bRetVal = false;
                break;
            }
            return(bRetVal);
        }

        /// <summary>
        /// Called when a move is selected from the MoveViewer
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        void m_moveViewer_NewMoveSelected(object sender, MoveViewer.NewMoveSelectedEventArg e) {
            ChessBoard.MoveResultE  eResult;
            bool                    bSucceed;
            
            if (PlayingMode == PlayingModeE.PlayerAgainstPlayer) {
                eResult = m_chessCtl.SelectMove(e.NewIndex, out bSucceed);
                DisplayMessage(eResult, MessageModeE.Verbose);
                e.Cancel = !bSucceed;
            } else {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Called when the state of the commands need to be refreshed
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        private void m_chessCtl_UpdateCmdState(object sender, EventArgs e) {
            m_lostPieceBlack.Refresh();
            m_lostPieceWhite.Refresh();
            SetCmdState();
        }

        /// <summary>
        /// Reset the board.
        /// </summary>
        private void ResetBoard() {
            m_chessCtl.ResetBoard();
            SetCmdState();
        }

        /// <summary>
        /// Determine which menu item is enabled
        /// </summary>
        public void SetCmdState() {
            m_chessCtl.AutoSelection    = !m_bSecondThreadBusy;
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Unlock the chess board when asynchronous computing is finished
        /// </summary>
        private void UnlockBoard() {
            m_bSecondThreadBusy = false;
            Cursor              = Cursors.Arrow;
            SetCmdState();
        }

        /// <summary>
        /// Play the computer move found by the search.
        /// </summary>
        /// <param name="bFlash">       true to flash moving position</param>
        /// <param name="eMessageMode"> Which message to show</param>
        /// <param name="move">         Best move</param>
        /// <param name="iPermCount">   Permutation count</param>
        /// <param name="iDepth">       Depth of the search</param>
        /// <param name="iCacheHit">    Number of moves found in the translation table cache</param>
        /// <param name="stat">         Playing stat. Can be null</param>
        /// <returns>
        /// true if end of game, false if not
        /// </returns>
        private bool PlayComputerMove(bool bFlash, MessageModeE eMessageMode, ChessBoard.MovePosS move, int iPermCount, int iDepth, int iCacheHit, ComputerPlayingStat stat) {
            bool                        bRetVal;
            ChessBoard.MoveResultE      eResult;
            ChessBoard.PlayerColorE     eColorToPlay;
                                        
            eColorToPlay    = m_chessCtl.NextMoveColor;
            eResult         = m_chessCtl.DoMove(move,
                                                bFlash,
                                                iPermCount,
                                                iDepth,
                                                iCacheHit);
            if (stat != null) {
                stat.m_eResult = eResult;
            }
            bRetVal = DisplayMessage(eResult, eMessageMode);
            return(bRetVal);
        }

        /// <summary>
        /// Computer play a move. Can be called asynchronously by a secondary thread.
        /// </summary>
        /// <param name="bFlash">           true to use flash when moving pieces</param>
        /// <param name="searchMode">       Search mode</param>
        private void PlayComputerAsync(bool bFlash, SearchEngine.SearchMode searchMode) {
            Func<bool>                      delPlayComputerMove;
            Action                          delUnlockBoard;
            Action                          delSetPlayingMode;
            ChessBoard                      chessBoard;
            ChessBoard.MovePosS             move;
            int                             iPermCount;
            int                             iCacheHit;
            int                             iMaxDepth;
            bool                            bEndOfGame;
            bool                            bMoveFound;
            bool                            bMultipleThread;

            chessBoard          = m_chessCtl.ChessBoard.Clone();
            bMultipleThread     = (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                                   searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            bMoveFound          = m_chessCtl.FindBestMove(searchMode,
                                                          chessBoard,
                                                          out move,
                                                          out iPermCount,
                                                          out iCacheHit,
                                                          out iMaxDepth);
            if (bMoveFound) {
                if (bMultipleThread) {
                    delPlayComputerMove = () => PlayComputerMove(bFlash, MessageModeE.CallEndGame, move, iPermCount, iMaxDepth, iCacheHit, null);
                    bEndOfGame          = (bool)Dispatcher.Invoke(delPlayComputerMove);
                } else {
                    bEndOfGame          = PlayComputerMove(bFlash, MessageModeE.CallEndGame, move, iPermCount, iMaxDepth, iCacheHit, null);
                }
            } else {
                bEndOfGame = DisplayMessage(m_chessCtl.ChessBoard.CheckNextMove(), MessageModeE.CallEndGame);
            }
            if (bMultipleThread) {
                delUnlockBoard = () => UnlockBoard();
                Dispatcher.Invoke(delUnlockBoard);
                if (bEndOfGame) {
                    delSetPlayingMode = () => SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                    Dispatcher.Invoke(delSetPlayingMode);
                }
            } else {
                UnlockBoard();
                if (bEndOfGame) {
                    SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                }
            }
        }

        /// <summary>
        /// Computer find a best move and play it. If the book can be find in the opening book,
        /// the move is played directly. If not, the best move is found and played from a 
        /// secondary thread (if option is enabled).
        /// </summary>
        /// <param name="bFlash">           true to flash moving position</param>
        /// <param name="bSilent">          true to be silent up to the end of the game</param>
        /// <returns>
        /// true if computer is able to play, false if not
        /// </returns>
        private void PlayComputer(bool bFlash, bool bSilent) {
            bool    bMultipleThread;
            
            bMultipleThread = (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                               m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            StartAsyncComputing();
            if (bMultipleThread) {
                Task.Factory.StartNew(() => PlayComputerAsync(bFlash, m_searchMode));
            } else {
                PlayComputerAsync(bFlash, m_searchMode);
            }
        }

        /// <summary>
        /// Let's the computer play against itself. Can be called asynchronously by a secondary thread.
        /// </summary>
        /// <param name="bFlash">           true to flash the moving piece</param>
        /// <param name="searchMode">       Search mode</param>
        private void PlayComputerAgainstComputerAsync(bool bFlash, SearchEngine.SearchMode searchMode) {
            bool                                        bEndOfGame;
            int                                         iCount;
            Func<ChessBoard.MovePosS,int,int,int,bool>  delPlayComputerMove = null;
            Action                                      delUnlockBoard;
            Action                                      delSetPlayingMode;
            ChessBoard                                  chessBoard;
            ChessBoard.MovePosS                         move;
            int                                         iPermCount;
            int                                         iCacheHit;
            int                                         iMaxDepth;
            bool                                        bMoveFound;
            bool                                        bMultipleThread;
            
            bMultipleThread = (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                               searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            if (bMultipleThread) {
                delPlayComputerMove = (a1,a2,a3,a4) => { return(PlayComputerMove(bFlash, MessageModeE.CallEndGame, a1, a2, a3, a4, null)); };
            }
            iCount = 0;
            do {
                chessBoard          = m_chessCtl.ChessBoard.Clone();
                bMoveFound          = m_chessCtl.FindBestMove(searchMode,
                                                              chessBoard,
                                                              out move,
                                                              out iPermCount,
                                                              out iCacheHit,
                                                              out iMaxDepth);
                if (bMoveFound) {
                    if (bMultipleThread) {
                        bEndOfGame  = (bool)Dispatcher.Invoke(delPlayComputerMove, new object[] { move, iPermCount, iMaxDepth, iCacheHit });
                    } else {
                        bEndOfGame  = PlayComputerMove(bFlash, MessageModeE.CallEndGame, move, iPermCount, iMaxDepth, iCacheHit, null);
                    }
                } else {
                    bEndOfGame = DisplayMessage(m_chessCtl.ChessBoard.CheckNextMove(), MessageModeE.Verbose);
                }
                iCount++;
            } while (!bEndOfGame && PlayingMode == PlayingModeE.ComputerAgainstComputer && iCount < 500);
            if (PlayingMode != PlayingModeE.ComputerAgainstComputer) {
                MessageBox.Show("Automatic play canceled");
            } else {
                if (bMultipleThread) {
                    delSetPlayingMode = () => SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                    Dispatcher.Invoke(delSetPlayingMode);
                } else {
                    SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                }
                if (iCount >= 500) {
                    MessageBox.Show("Tie!");
                }
            }
            if (bMultipleThread) {
                delUnlockBoard = () => UnlockBoard();
                Dispatcher.Invoke(delUnlockBoard);
            } else {
                UnlockBoard();
            }
        }

        /// <summary>
        /// Let's the computer play against itself
        /// </summary>
        /// <param name="bFlash">           true to flash pieces whem moving</param>
        private void PlayComputerAgainstComputer(bool bFlash) {
            bool    bMultipleThread;
            
            bMultipleThread     = (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                                   m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            PlayingMode         = PlayingModeE.ComputerAgainstComputer;
            StartAsyncComputing();
            if (bMultipleThread) {
                Task.Factory.StartNew(() => PlayComputerAgainstComputerAsync(bFlash, m_searchMode));
            } else {
                PlayComputerAgainstComputerAsync(bFlash, m_searchMode);
            }
        }

        /// <summary>
        /// Show the test result of a computer playing against a computer
        /// </summary>
        /// <param name="iGameCount">       Number of games played.</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="stat">             Statistic.</param>
        /// <param name="iMethod1Win">      Number of games won by method #1</param>
        /// <param name="iMethod2Win">      Number of games won by method #2</param>
        private void TestShowResult(int iGameCount, SearchEngine.SearchMode searchMode, ComputerPlayingStat stat, int iMethod1Win, int iMethod2Win) {
            string  strMsg;
            string  strMethod1;
            string  strMethod2;
            int     iTimeMethod1;
            int     iTimeMethod2;
            
            strMethod1      = searchMode.m_boardEvaluationWhite.Name;
            strMethod2      = searchMode.m_boardEvaluationBlack.Name;
            
            iTimeMethod1    = (stat.m_iMethod1MoveCount == 0) ? 0 : stat.m_timeSpanMethod1.Milliseconds / stat.m_iMethod1MoveCount;
            iTimeMethod2    = (stat.m_iMethod2MoveCount == 0) ? 0 : stat.m_timeSpanMethod2.Milliseconds / stat.m_iMethod2MoveCount;
            strMsg          = iGameCount.ToString() + " game(s) played.\r\n" +
                              iMethod1Win.ToString() + " win(s) for method #1 (" + strMethod1 + "). Average time = " + iMethod1Win.ToString() + " ms per move.\r\n" + 
                              iMethod2Win.ToString() + " win(s) for method #2 (" + strMethod2 + "). Average time = " + iMethod2Win.ToString() + " ms per move.\r\n" + 
                              (iGameCount - iMethod1Win - iMethod2Win).ToString() + " draw(s).";
            MessageBox.Show(strMsg);
        }

        /// <summary>
        /// Tests the computer playing against itself. Can be called asynchronously by a secondary thread.
        /// </summary>
        /// <param name="iGameCount">       Number of games to play.</param>
        /// <param name="searchMode">       Search mode</param>
        private void TestComputerAgainstComputerAsync(int iGameCount, SearchEngine.SearchMode searchMode) {
            int                                                             iCount;
            Func<ChessBoard.MovePosS,int,int,int,ComputerPlayingStat,bool>  delPlayComputerMove = null;
            Action                                                          delResetBoard = null;
            Action                                                          delUnlockBoard;
            Action                                                          delSetPlayingMode;
            Action                                                          delShowResultDel;
            ChessBoard                                                      chessBoard;
            ChessBoard.MovePosS                                             move;
            int                                                             iPermCount;
            int                                                             iCacheHit;
            int                                                             iMaxDepth;
            int                                                             iGameIndex;
            int                                                             iMethod1Win = 0;
            int                                                             iMethod2Win = 0;
            DateTime                                                        dateTime;
            bool                                                            bMoveFound;
            bool                                                            bMultipleThread;
            bool                                                            bEven;
            bool                                                            bEndOfGame;
            IBoardEvaluation                                                boardEvaluation1;
            IBoardEvaluation                                                boardEvaluation2;
            ComputerPlayingStat                                             stat;

            stat             = new ComputerPlayingStat();
            bMultipleThread  = (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                                searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            boardEvaluation1 = searchMode.m_boardEvaluationWhite;
            boardEvaluation2 = searchMode.m_boardEvaluationBlack;
            if (bMultipleThread) {
                delPlayComputerMove = (a1, a2, a3, a4, a5) => PlayComputerMove(false, MessageModeE.Silent, a1, a2, a3, a4, a5);
                delResetBoard       = () => ResetBoard();
            }
            iGameIndex = 0;
            while (iGameIndex < iGameCount && !stat.m_bUserCancel) {
                bEven = ((iGameIndex & 1) == 0);
                searchMode.m_boardEvaluationWhite   = bEven ? boardEvaluation1 : boardEvaluation2;
                searchMode.m_boardEvaluationBlack   = bEven ? boardEvaluation2 : boardEvaluation1;
                if (bMultipleThread) {
                    Dispatcher.Invoke(delResetBoard);
                } else {
                    ResetBoard();
                }
                iCount = 0;
                do {
                    chessBoard  = m_chessCtl.ChessBoard.Clone();
                    dateTime    = DateTime.Now;
                    bMoveFound  = m_chessCtl.FindBestMove(searchMode,
                                                          chessBoard,
                                                          out move,
                                                          out iPermCount,
                                                          out iCacheHit,
                                                          out iMaxDepth);
                    if (bMoveFound) {
                        if ((m_chessCtl.NextMoveColor == ChessBoard.PlayerColorE.White && bEven) ||
                            (m_chessCtl.NextMoveColor == ChessBoard.PlayerColorE.Black && !bEven)) {
                            stat.m_timeSpanMethod1 += DateTime.Now - dateTime;
                            stat.m_iMethod1MoveCount++;
                        } else {
                            stat.m_timeSpanMethod2 += DateTime.Now - dateTime;
                            stat.m_iMethod2MoveCount++;
                        }
                        if (bMultipleThread) {
                            bEndOfGame = (bool)Dispatcher.Invoke(delPlayComputerMove, new object[] { move, iPermCount, iMaxDepth, iCacheHit, stat } );
                        } else {
                            bEndOfGame = PlayComputerMove(false, MessageModeE.Silent, move, iPermCount, iMaxDepth, iCacheHit, stat);
                        }
                    } else {
                        bEndOfGame = true;
                    }
                    iCount++;
                } while (!bEndOfGame && PlayingMode == PlayingModeE.ComputerAgainstComputer && iCount < 250);
                if (PlayingMode != PlayingModeE.ComputerAgainstComputer) {
                    stat.m_bUserCancel = true;
                } else if (iCount < 250) {
                    if (stat.m_eResult == ChessBoard.MoveResultE.Mate) {
                        if ((m_chessCtl.NextMoveColor == ChessBoard.PlayerColorE.Black && bEven) ||
                            (m_chessCtl.NextMoveColor == ChessBoard.PlayerColorE.White && !bEven)) {
                            iMethod1Win++;
                        } else {
                            iMethod2Win++;
                        }
                    }
                }
                iGameIndex++;
            }
            searchMode.m_boardEvaluationWhite = boardEvaluation1;
            searchMode.m_boardEvaluationBlack = boardEvaluation2;
            if (bMultipleThread) {
                delSetPlayingMode   = () => SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                delUnlockBoard      = () => UnlockBoard();
                delShowResultDel    = () => TestShowResult(iGameIndex, searchMode, stat, iMethod1Win, iMethod2Win);
                Dispatcher.Invoke(delShowResultDel);
                Dispatcher.Invoke(delSetPlayingMode);
                Dispatcher.Invoke(delUnlockBoard);
            } else {
                TestShowResult(iGameIndex, searchMode, stat, iMethod1Win, iMethod2Win);
                SetPlayingMode(PlayingModeE.PlayerAgainstPlayer);
                UnlockBoard();
            }
        }

        /// <summary>
        /// Test the computer play against itself
        /// </summary>
        /// <param name="iGameCount">   Number of games to play</param>
        /// <param name="searchMode">   Searching mode</param>
        private void TestComputerAgainstComputer(int iGameCount, SearchEngine.SearchMode searchMode) {
            bool    bMultipleThread;
            
            bMultipleThread = (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                               m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            PlayingMode     = PlayingModeE.ComputerAgainstComputer;
            StartAsyncComputing();
            if (bMultipleThread) {
                Task.Factory.StartNew(() => TestComputerAgainstComputerAsync(iGameCount, searchMode));
            } else {
                TestComputerAgainstComputerAsync(iGameCount, searchMode);
            }
        }

        /// <summary>
        /// Flash a hint
        /// </summary>
        /// <param name="move">         Move to show</param>
        /// <param name="iPermCount">   Permutation count</param>
        /// <param name="iDepth">       Search depth</param>
        /// <param name="iCacheHit">    Cache hit</param>
        private void ShowHintMove(ChessBoard.MovePosS move, int iPermCount, int iDepth, int iCacheHit) {
            ShowMoveInStatusBar(m_chessCtl.NextMoveColor, move, iPermCount, iDepth, iCacheHit);
            m_chessCtl.ShowHintMove(move);
        }       

        /// <summary>
        /// Show a hint. Can be called asynchronously by a secondary thread.
        /// </summary>
        /// <param name="searchMode">       Search mode</param>
        private void ShowHintAsync(SearchEngine.SearchMode searchMode) {
            Action              delShowHintMove;
            Action              delUnlockBoard;
            ChessBoard          chessBoard;
            ChessBoard.MovePosS move;
            int                 iPermCount;
            int                 iCacheHit;
            int                 iMaxDepth;
            bool                bMoveFound;
            bool                bMultipleThread;

            chessBoard      = m_chessCtl.ChessBoard.Clone();
            bMultipleThread = (searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                               searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            bMoveFound      = m_chessCtl.FindBestMove(searchMode,
                                                      chessBoard,
                                                      out move,
                                                      out iPermCount,
                                                      out iCacheHit,
                                                      out iMaxDepth);
            if (bMultipleThread) {
                if (bMoveFound) {
                    delShowHintMove = () => ShowHintMove(move, iPermCount, iMaxDepth, iCacheHit); 
                    Dispatcher.Invoke(delShowHintMove);
                }
                delUnlockBoard = () => UnlockBoard();
                Dispatcher.Invoke(delUnlockBoard);
            } else {
                if (bMoveFound) {
                    ShowHintMove(move, iPermCount, iMaxDepth, iCacheHit);
                }
                UnlockBoard();
            }
        }

        /// <summary>
        /// Show a hint
        /// </summary>
        private void ShowHint() {
            bool    bMultipleThread;
            
            bMultipleThread = (m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.DifferentThreadForSearch ||
                               m_searchMode.m_eThreadingMode == SearchEngine.SearchMode.ThreadingModeE.OnePerProcessorForSearch);
            StartAsyncComputing();
            if (bMultipleThread) {
                Task.Factory.StartNew(() => ShowHintAsync(m_searchMode));
            } else {
                ShowHintAsync(m_searchMode);
            }
        }

        /// <summary>
        /// Undo a user and computer move
        /// </summary>
        /// <param name="bPlayerAgainstComputer">   true if playing against computer</param>
        private void Undo(bool bPlayerAgainstComputer) {
            bool    bFlash;
            
            bFlash = mnuOptionFlashPiece.IsChecked;
            if (bPlayerAgainstComputer) {
                if (m_chessCtl.UndoCount > 1) {
                    m_chessCtl.UndoMove(bFlash);  // Computer
                    m_chessCtl.UndoMove(bFlash);  // User
                }
            } else {
                if (m_chessCtl.UndoCount != 0) {
                    m_chessCtl.UndoMove(bFlash);
                }
            }
        }

        /// <summary>
        /// Redo a user and computer move
        /// </summary>
        /// <param name="bPlayerAgainstComputer">   true if playing against computer</param>
        /// <returns>
        /// NoRepeat, FiftyRuleRepeat, ThreeFoldRepeat, Tie, Check, Mate
        /// </returns>
        private ChessBoard.MoveResultE RedoMove(bool bPlayerAgainstComputer) {
            ChessBoard.MoveResultE  eRetVal = ChessBoard.MoveResultE.NoRepeat;
            bool                    bFlash;
            
            bFlash = mnuOptionFlashPiece.IsChecked;
            if (m_chessCtl.RedoCount != 0) {
                eRetVal = m_chessCtl.RedoMove(bFlash);  // Computer
            }
            if (bPlayerAgainstComputer && m_chessCtl.RedoCount != 0) {
                eRetVal = m_chessCtl.RedoMove(bFlash);  // User
            }
            return(eRetVal);
        }

        /// <summary>
        /// Check if it's the computer time to move
        /// </summary>
        private void CheckIfComputerMustPlay() {
            switch(PlayingMode) {
            case PlayingModeE.ComputerAgainstComputer:
                //Update(); TODO
                PlayComputerAgainstComputer(true);
                break;
            case PlayingModeE.PlayerAgainstComputer:
                if (m_chessCtl.NextMoveColor == m_eComputerPlayingColor) {
                    //Update(); TODO
                    PlayComputer(mnuOptionFlashPiece.IsChecked, false);
                }
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// Called when the user has selected a valid move
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        void m_chessCtl_MoveSelected(object sender, ChessBoardControl.MoveSelectedEventArgs e) {
            bool                    bFlash;
            bool                    bEndOfGame;
            ChessBoard.MoveResultE  eMoveResult;
            
            bFlash      = mnuOptionFlashPiece.IsChecked;
            eMoveResult = m_chessCtl.DoMove(e.Move, bFlash, -1, 0, 0);
            bEndOfGame  = DisplayMessage(eMoveResult, MessageModeE.Verbose);
            if (!bEndOfGame) {
                CheckIfComputerMustPlay();
            }
        }

        /// <summary>
        /// Toggle the design mode. In design mode, the user can create its own board
        /// </summary>
        private void ToggleDesignMode() {
            if (PlayingMode == PlayingModeE.DesignMode) {
                PlayingMode                     = PlayingModeE.PlayerAgainstPlayer;
                mnuEditDesignMode.IsCheckable   = false;
                if (frmGameParameter.AskGameParameter(this)) {
                    m_chessCtl.BoardDesignMode = false;
                    if (m_chessCtl.BoardDesignMode) {
                        PlayingMode = PlayingModeE.DesignMode;
                        MessageBox.Show("Invalid board configuration. Correct or reset.");
                    } else {
                        m_lostPieceBlack.BoardDesignMode    = false;
                        m_lostPieceWhite.Visibility         = System.Windows.Visibility.Visible;
                        CheckIfComputerMustPlay();
                    }
                } else {
                    PlayingMode = PlayingModeE.DesignMode;
                }
            } else {
                PlayingMode                         = PlayingModeE.DesignMode;
                mnuEditDesignMode.IsCheckable       = true;
                m_lostPieceBlack.BoardDesignMode    = true;
                m_lostPieceWhite.Visibility         = System.Windows.Visibility.Hidden;
                m_chessCtl.BoardDesignMode          = true;
            }
            mnuEditDesignMode.IsChecked = (PlayingMode == PlayingModeE.DesignMode);
            SetCmdState();
        }

        /// <summary>
        /// Called to gets the selected piece for design mode
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        private void m_chessCtl_QueryPiece(object sender, ChessBoardControl.QueryPieceEventArgs e) {
            e.Piece = m_lostPieceBlack.SelectedPiece;
        }

        /// <summary>
        /// Called to gets the type of pawn promotion for the current move
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        private void m_chessCtl_QueryPawnPromotionType(object sender, ChessBoardControl.QueryPawnPromotionTypeEventArgs e) {
            frmQueryPawnPromotionType   frm;
            
            frm         = new frmQueryPawnPromotionType(e.ValidPawnPromotion);
            frm.Owner   = this;
            frm.ShowDialog();
            e.PawnPromotionType = frm.PromotionType;
        }

        /// <summary>
        /// Called when the game need to be reinitialized
        /// </summary>
        private void NewGame() {
            if (frmGameParameter.AskGameParameter(this)) {
                ResetBoard();
                CheckIfComputerMustPlay();
            }
        }

        /// <summary>
        /// Redo the last undone move
        /// </summary>
        private void Redo() {
            ChessBoard.MoveResultE  eMoveResult;
            
            eMoveResult = RedoMove((PlayingMode != PlayingModeE.PlayerAgainstPlayer));
            DisplayMessage(eMoveResult, MessageModeE.Verbose);
        }

        /// <summary>
        /// Load a board
        /// </summary>
        private void LoadGame() {
            if (m_chessCtl.LoadFromFile()) {
                CheckIfComputerMustPlay();
            }
        }

        /// <summary>
        /// Creates a game from a PGN text
        /// </summary>
        private void CreateGame() {
            if (m_chessCtl.CreateFromPGNText()) {
                PlayingMode = PlayingModeE.PlayerAgainstPlayer;
            }
        }

        /// <summary>
        /// Cancel the auto-play
        /// </summary>
        private void CancelAutoPlay() {
            if (PlayingMode == PlayingModeE.ComputerAgainstComputer) {
                PlayingMode = PlayingModeE.PlayerAgainstPlayer;
            } else {
                m_chessCtl.CancelSearch();
            }
        }

        /// <summary>
        /// Toggle the player vs player mode.
        /// </summary>
        private void TogglePlayerAgainstPlayer() {
            if (mnuEditPlayerAgainstPlayer.IsChecked) {
                PlayingMode = PlayingModeE.PlayerAgainstComputer;
                if (frmGameParameter.AskGameParameter(this)) {
                    CheckIfComputerMustPlay();
                } else {
                    PlayingMode = PlayingModeE.PlayerAgainstPlayer;
                }
            } else {
                PlayingMode = PlayingModeE.PlayerAgainstPlayer;
            }
            mnuEditPlayerAgainstPlayer.IsChecked = (PlayingMode == PlayingModeE.PlayerAgainstPlayer);
        }

        /// <summary>
        /// Revert the board so the computer play the actual user pieces
        /// </summary>
        private void RevertBoard() {
            m_eComputerPlayingColor = (m_eComputerPlayingColor == ChessBoard.PlayerColorE.Black) ? ChessBoard.PlayerColorE.White :
                                                                                                   ChessBoard.PlayerColorE.Black;
            CheckIfComputerMustPlay();
        }

        /// <summary>
        /// Filter the content of a PGN file
        /// </summary>
        private void FilterPGNFile() {
             PgnUtil    pgnUtil;
             
             pgnUtil = new PgnUtil();
             pgnUtil.CreatePGNSubset();
        }

        /// <summary>
        /// Show the About Dialog Box
        /// </summary>
        public void ShowAbout() {
            frmAbout    frm;

            frm         = new frmAbout();
            frm.Owner   = this;
            frm.ShowDialog();
        }

        /// <summary>
        /// Specifies the search mode
        /// </summary>
        private void SetSearchMode() {
            frmSearchMode   frm;

            frm         = new frmSearchMode(m_searchMode, m_boardEvalUtil);
            frm.Owner   = this;
            if (frm.ShowDialog() == true) {
                frm.UpdateSearchMode();
                ShowSearchMode();
            }
        }

        /// <summary>
        /// Test board evaluation routine
        /// </summary>
        private void TestBoardEvaluation() {
            frmTestBoardEval        frm;
            SearchEngine.SearchMode searchMode;
            int                     iGameCount;

            frm         = new frmTestBoardEval(m_boardEvalUtil, m_searchMode);
            frm.Owner   = this;
            if (frm.ShowDialog() == true) {
                searchMode  = frm.SearchMode;
                iGameCount  = frm.GameCount;
                TestComputerAgainstComputer(iGameCount, searchMode);
            }
        }

        /// <summary>
        /// Called each second for timer click
        /// </summary>
        /// <param name="sender">   Sender object</param>
        /// <param name="e">        Event handler</param>
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            GameTimer   gameTimer;
            
            gameTimer                               = m_chessCtl.GameTimer;
            m_toolbar.labelWhitePlayTime.Content    = GameTimer.GetHumanElapse(gameTimer.WhitePlayTime);
            m_toolbar.labelBlackPlayTime.Content    = GameTimer.GetHumanElapse(gameTimer.BlackPlayTime);
        }

        /// <summary>
        /// Toggle PGN/Move notation
        /// </summary>
        private void TogglePGNNotation() {
            SrcChess2.Properties.Settings   settings;
            bool                            bPGNNotationChecked;
            
            bPGNNotationChecked             = mnuOptionPGNNotation.IsChecked;
            m_moveViewer.DisplayMode        = (bPGNNotationChecked) ? MoveViewer.DisplayModeE.PGN : MoveViewer.DisplayModeE.MovePos;
            settings                        = SrcChess2.Properties.Settings.Default;
            settings.MoveNotation           = (m_moveViewer.DisplayMode == MoveViewer.DisplayModeE.MovePos) ? 0 : 1;
            settings.Save();
        }

        /// <summary>
        /// Toggle Flash piece
        /// </summary>
        private void ToggleFlashPiece() {
            SrcChess2.Properties.Settings   settings;
            bool                            bFlashPiece;

            bFlashPiece                     = mnuOptionFlashPiece.IsChecked;
            settings                        = SrcChess2.Properties.Settings.Default;
            settings.FlashPiece             = bFlashPiece;
            settings.Save();
        }

        /// <summary>
        /// Choose board setting
        /// </summary>
        private void ChooseBoardSetting() {
            frmBoardSetting         frm;
            Properties.Settings     settings;
            
            frm         = new frmBoardSetting(m_chessCtl.LiteCellColor, 
                                              m_chessCtl.DarkCellColor,
                                              m_chessCtl.WhitePieceColor,
                                              m_chessCtl.BlackPieceColor,
                                              m_colorBackground,
                                              m_listPieceSet,
                                              PieceSet);
            frm.Owner   = this;
            if (frm.ShowDialog() == true) {
                m_colorBackground           = frm.BackgroundColor;
                Background                  = new SolidColorBrush(m_colorBackground);
                m_chessCtl.LiteCellColor    = frm.LiteCellColor;
                m_chessCtl.DarkCellColor    = frm.DarkCellColor;
                m_chessCtl.WhitePieceColor  = frm.WhitePieceColor;
                m_chessCtl.BlackPieceColor  = frm.BlackPieceColor;
                PieceSet                    = frm.PieceSet;
                settings                    = Properties.Settings.Default;
                settings.PieceSet           = PieceSet.Name;
                settings.WhitePieceColor    = m_chessCtl.WhitePieceColor.ToString();
                settings.BlackPieceColor    = m_chessCtl.BlackPieceColor.ToString();
                settings.LiteCellColor      = m_chessCtl.LiteCellColor.ToString();
                settings.DarkCellColor      = m_chessCtl.DarkCellColor.ToString();
                settings.BackgroundColor    = m_colorBackground.ToString();
                settings.Save();
            }
        }
    } // Class MainWindow
} // Namespace
