using System;
using System.Collections.Generic;

namespace SrcChess2 {
    /// <summary>Base class for Search Engine</summary>
    public abstract class SearchEngine {

        /// <summary>Interface to implement to do a search</summary>
        public interface ITrace {
            /// <summary>
            /// Search trace
            /// </summary>
            /// <param name="iDepth">       Depth of the move</param>
            /// <param name="ePlayerColor"> Player's color</param>
            /// <param name="movePos">      Move position</param>
            /// <param name="iPts">         Points for the board</param>
            void TraceSearch(int iDepth, ChessBoard.PlayerColorE ePlayerColor, ChessBoard.MovePosS movePos, int iPts);
        };
        


        /// <summary>Option for the search</summary>
        public class SearchMode {
            
            /// <summary>Threading mode</summary>
            public enum ThreadingModeE {
                /// <summary>No threading at all. User interface share the search one.</summary>
                Off                         = 0,
                /// <summary>Use a different thread for search and user interface</summary>
                DifferentThreadForSearch    = 1,
                /// <summary>Use one thread for each processor for search and one for user inetrface</summary>
                OnePerProcessorForSearch    = 2
            };
            
            /// <summary>Random mode</summary>
            public enum RandomModeE {
                /// <summary>No random</summary>
                Off                         = 0,
                /// <summary>Use a repetitive random</summary>
                OnRepetitive                = 1,
                /// <summary>Use random with time seed</summary>
                On                          = 2
            };
            
            /// <summary>Search options</summary>
            [Flags]
            public enum OptionE {
                /// <summary>Use MinMax search</summary>
                UseMinMax                   = 0,
                /// <summary>Use Alpha-Beta prunning function</summary>
                UseAlphaBeta                = 1,
                /// <summary>Use transposition table</summary>
                UseTransTable               = 2,
                /// <summary>Use book opening. No way to sub-class enum so it's defined here</summary>
                UseBook                     = 4,
                /// <summary>Use iterative depth-first search on a fix ply count</summary>
                UseIterativeDepthSearch     = 8
            };
            
            /// <summary>Board evaluation for the white</summary>
            public IBoardEvaluation m_boardEvaluationWhite;
            /// <summary>Board evaluation for the black</summary>
            public IBoardEvaluation m_boardEvaluationBlack;
            /// <summary>Search option</summary>
            public OptionE          m_eOption;
            /// <summary>Threading option</summary>
            public ThreadingModeE   m_eThreadingMode;
            /// <summary>Maximum search depth (or 0 to use iterative deepening depth-first search with time out)</summary>
            public int              m_iSearchDepth;
            /// <summary>Time out in second if using iterative deepening depth-first search</summary>
            public int              m_iTimeOutInSec;
            /// <summary>Random mode</summary>
            public RandomModeE      m_eRandomMode;
            /// <summary>Constructor</summary>
            public                  SearchMode(IBoardEvaluation boardEvalWhite, IBoardEvaluation boardEvalBlack, OptionE eOption, ThreadingModeE eThreadingMode, int iSearchDepth, int iTimeOutInSec, RandomModeE eRandomMode) { m_eOption = eOption; m_eThreadingMode = eThreadingMode; m_iSearchDepth = iSearchDepth; m_iTimeOutInSec = iTimeOutInSec; m_eRandomMode = eRandomMode; m_boardEvaluationWhite = boardEvalWhite; m_boardEvaluationBlack = boardEvalBlack; }
        }
        
        private struct IndexPoint : IComparable<IndexPoint> {
            public int     iIndex;
            public int     iPoints;

            public int CompareTo(IndexPoint Other) {
                int     iRetVal;
                
                if (iPoints < Other.iPoints) {
                    iRetVal = 1;
                } else if (iPoints > Other.iPoints) {
                    iRetVal = -1;
                } else {
                    iRetVal = (iIndex < Other.iIndex) ? -1 : 1;
                }
                return(iRetVal);
            }
        }
        
        /// <summary>true to cancel the search</summary>
        protected bool                          m_bCancelSearch;
        /// <summary>Object where to redirect the trace if any</summary>
        private ITrace                          m_trace;
        /// <summary>Random number generator</summary>
        private Random                          m_rnd;
        /// <summary>Random number generator (repetitive, seed = 0)</summary>
        private Random                          m_rndRep;

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="trace">    Trace object or null</param>
        /// <param name="rnd">      Random object</param>
        /// <param name="rndRep">   Repetitive random object</param>
        protected SearchEngine(ITrace trace, Random rnd, Random rndRep) {
            m_trace         =   trace;
            m_rnd           =   rnd;
            m_rndRep        =   rndRep;
        }

        /// <summary>
        /// Debugging routine
        /// </summary>
        /// <param name="iDepth">       Actual search depth</param>
        /// <param name="ePlayerColor"> Color doing the move</param>
        /// <param name="move">         Move</param>
        /// <param name="iPts">         Points for this move</param>
        protected void TraceSearch(int iDepth, ChessBoard.PlayerColorE ePlayerColor, ChessBoard.MovePosS move, int iPts) {
            if (m_trace != null) {
                m_trace.TraceSearch(iDepth, ePlayerColor, move, iPts);
            }
        }

        /// <summary>
        /// Cancel the search
        /// </summary>
        public void CancelSearch() {
            m_bCancelSearch = true;
        }

        /// <summary>
        /// Sort move list using the specified point array so the highest point move come first
        /// </summary>
        /// <param name="moveList"> Source move list to sort</param>
        /// <param name="arrPoints">Array of points for each move</param>
        /// <returns>
        /// Sorted move list
        /// </returns>
        protected static List<ChessBoard.MovePosS> SortMoveList(List<ChessBoard.MovePosS> moveList, int[] arrPoints) {
            List<ChessBoard.MovePosS>   moveListRetVal;
            IndexPoint[]                arrIndexPoint;
            
            moveListRetVal = new List<ChessBoard.MovePosS>(moveList.Count);
            arrIndexPoint  = new IndexPoint[arrPoints.Length];
            for (int iIndex = 0; iIndex < arrIndexPoint.Length; iIndex++) {
                arrIndexPoint[iIndex].iPoints = arrPoints[iIndex];
                arrIndexPoint[iIndex].iIndex  = iIndex;
            }
            Array.Reverse(arrIndexPoint);
            Array.Sort<IndexPoint>(arrIndexPoint);
            for (int iIndex = 0; iIndex < arrIndexPoint.Length; iIndex++) {
                moveListRetVal.Add(moveList[arrIndexPoint[iIndex].iIndex]);
            }
            return(moveListRetVal);
        }

        /// <summary>
        /// Find the best move using a specific search method
        /// </summary>
        /// <param name="chessBoard">       Chess board</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="moveList">         Move list</param>
        /// <param name="arrIndex">         Order of evaluation of the moves</param>
        /// <param name="posInfo">          Position information</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <param name="iCacheHit">        Number of moves found in the translation table cache</param>
        /// <param name="iMaxDepth">        Maximum depth to use</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        protected abstract bool FindBestMove(ChessBoard chessBoard, SearchMode searchMode, ChessBoard.PlayerColorE ePlayerColor, List<ChessBoard.MovePosS> moveList, int[]arrIndex, ChessBoard.PosInfoS posInfo, ref ChessBoard.MovePosS moveBest, out int iPermCount, out int iCacheHit, out int iMaxDepth);

        /// <summary>
        /// Find the best move for a player using a specific method
        /// </summary>
        /// <param name="chessBoard">       Chess board</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <param name="iCacheHit">        Number of moves found in the translation table cache</param>
        /// <param name="iMaxDepth">        Maximum depth reached</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        public bool FindBestMove(ChessBoard chessBoard, SearchMode searchMode, ChessBoard.PlayerColorE ePlayerColor, out ChessBoard.MovePosS moveBest, out int iPermCount, out int iCacheHit, out int iMaxDepth) {
            bool                        bRetVal = false;
            List<ChessBoard.MovePosS>   moveList;
            int[]                       arrIndex;
            int                         iSwapIndex;
            int                         iTmp;
            Random                      rnd;
            ChessBoard.PosInfoS         posInfo;
            
            iCacheHit               = 0;
            moveList                = chessBoard.EnumMoveList(ePlayerColor, true, out posInfo);
            arrIndex                = new int[moveList.Count];
            m_bCancelSearch         = false;
            for (int iIndex = 0; iIndex < moveList.Count; iIndex++) {
                arrIndex[iIndex] = iIndex;
            }
            if (searchMode.m_eRandomMode != SearchMode.RandomModeE.Off) {
                rnd = (searchMode.m_eRandomMode == SearchMode.RandomModeE.OnRepetitive) ? m_rndRep : m_rnd;
                for (int iIndex = 0; iIndex < moveList.Count; iIndex++) {
                    iSwapIndex           = rnd.Next(moveList.Count);
                    iTmp                 = arrIndex[iIndex];
                    arrIndex[iIndex]     = arrIndex[iSwapIndex];
                    arrIndex[iSwapIndex] = iTmp;
                }
            }
            moveBest.StartPos           = 0;
            moveBest.EndPos             = 0;
            moveBest.OriginalPiece      = ChessBoard.PieceE.None;
            moveBest.Type               = ChessBoard.MoveTypeE.Normal;
            bRetVal                     = FindBestMove(chessBoard, searchMode, ePlayerColor, moveList, arrIndex, posInfo, ref moveBest, out iPermCount, out iCacheHit, out iMaxDepth);
            return(bRetVal);
        }
    } // Class SearchEngine
} // Namespace
