using System;
using System.Collections.Generic;

namespace SrcChess2 {
    /// <summary>Base class for Search Engine</summary>
    public sealed class SearchEngineMinMax : SearchEngine {

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="trace">    Trace object or null</param>
        /// <param name="rnd">      Random object</param>
        /// <param name="rndRep">   Repetitive random object</param>
        public SearchEngineMinMax(ITrace trace, Random rnd, Random rndRep) : base(trace, rnd, rndRep) {
        }

        /// <summary>
        /// Minimum/maximum depth first search
        /// </summary>
        /// <param name="chessBoard">       Chess board</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="iDepth">           Actual search depth</param>
        /// <param name="iWhiteMoveCount">  Number of moves white can do</param>
        /// <param name="iBlackMoveCount">  Number of moves black can do</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <returns>
        /// Points to give for this move
        /// </returns>
        private int MinMax(ChessBoard chessBoard, SearchMode searchMode, ChessBoard.PlayerColorE ePlayerColor, int iDepth, int iWhiteMoveCount, int iBlackMoveCount, ref int iPermCount) {
            int                         iRetVal;
            int                         iMoveCount;
            int                         iPts;
            List<ChessBoard.MovePosS>   moveList;
            ChessBoard.RepeatResultE    eResult;
            
            if (chessBoard.IsEnoughPieceForCheckMate()) {
                if (iDepth == 0 || m_bCancelSearch) {
                    iRetVal = (ePlayerColor == ChessBoard.PlayerColorE.Black) ? -chessBoard.Points(searchMode, ePlayerColor, 0, iWhiteMoveCount - iBlackMoveCount, ChessBoard.s_posInfoNull, ChessBoard.s_posInfoNull) :
                                                                                 chessBoard.Points(searchMode, ePlayerColor, 0, iWhiteMoveCount - iBlackMoveCount, ChessBoard.s_posInfoNull, ChessBoard.s_posInfoNull);
                    iPermCount++;
                } else {
                    moveList    = chessBoard.EnumMoveList(ePlayerColor);
                    iMoveCount  = moveList.Count;
                    if (ePlayerColor == ChessBoard.PlayerColorE.White) {
                        iWhiteMoveCount = iMoveCount;
                    } else {
                        iBlackMoveCount = iMoveCount;
                    }
                    if (iMoveCount == 0) {
                        if (chessBoard.IsCheck(ePlayerColor)) {
                            iRetVal = -1000000 - iDepth;
                        } else {
                            iRetVal = 0; // Draw
                        }
                    } else {
                        iRetVal  = Int32.MinValue;
                        foreach (ChessBoard.MovePosS move in moveList) {
                            eResult = chessBoard.DoMoveNoLog(move);
                            if (eResult == ChessBoard.RepeatResultE.NoRepeat) {
                                iPts = -MinMax(chessBoard,
                                               searchMode,
                                               (ePlayerColor == ChessBoard.PlayerColorE.Black) ? ChessBoard.PlayerColorE.White : ChessBoard.PlayerColorE.Black,
                                               iDepth - 1,
                                               iWhiteMoveCount,
                                               iBlackMoveCount,
                                               ref iPermCount);
                            } else {
                                iPts = 0;
                            }
                            chessBoard.UndoMoveNoLog(move);
                            if (iPts > iRetVal) {
                                iRetVal = iPts;
                            }
                        }
                    }
                }
            } else {
                iRetVal = 0;
            }
            return(iRetVal);
        }

        /// <summary>
        /// Find the best move for a player using minmax search
        /// </summary>
        /// <param name="chessBoard">       Chess board</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="moveList">         Move list</param>
        /// <param name="arrIndex">         Order of evaluation of the moves</param>
        /// <param name="iDepth">           Maximum depth</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Total permutation evaluated</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        private bool FindBestMoveUsingMinMaxAtDepth(ChessBoard chessBoard, SearchMode searchMode, ChessBoard.PlayerColorE ePlayerColor, List<ChessBoard.MovePosS> moveList, int[] arrIndex, int iDepth, ref ChessBoard.MovePosS moveBest, out int iPermCount) {
            bool                        bRetVal = false;
            ChessBoard.MovePosS         move;
            int                         iPts;
            int                         iWhiteMoveCount;
            int                         iBlackMoveCount;
            int                         iBestPts;
            ChessBoard.RepeatResultE    eResult;
            
            iPermCount  = 0;
            iBestPts    = Int32.MinValue;
            if (ePlayerColor == ChessBoard.PlayerColorE.White) {
                iWhiteMoveCount = moveList.Count;
                iBlackMoveCount = 0;
            } else {
                iWhiteMoveCount = 0;
                iBlackMoveCount = moveList.Count;
            }
            foreach (int iIndex in arrIndex) {
                move    = moveList[iIndex];
                eResult = chessBoard.DoMoveNoLog(move);
                if (eResult == ChessBoard.RepeatResultE.NoRepeat) {
                    iPts = -MinMax(chessBoard,
                                   searchMode,
                                   (ePlayerColor == ChessBoard.PlayerColorE.Black) ? ChessBoard.PlayerColorE.White : ChessBoard.PlayerColorE.Black,
                                   iDepth - 1,
                                   iWhiteMoveCount,
                                   iBlackMoveCount,
                                   ref iPermCount);
                } else {
                    iPts = 0;
                }                                   
                chessBoard.UndoMoveNoLog(move);
                if (iPts > iBestPts) {
                    TraceSearch(iDepth, ePlayerColor, move, iPts);
                    iBestPts = iPts;
                    moveBest = move;
                    bRetVal  = true;
                }
            }
            return(bRetVal);
        }

        /// <summary>
        /// Find the best move for a player using minmax search
        /// </summary>
        /// <param name="chessBoard">       Chess board</param>
        /// <param name="searchMode">       Search mode</param>
        /// <param name="ePlayerColor">     Color doing the move</param>
        /// <param name="moveList">         Move list</param>
        /// <param name="arrIndex">         Order of evaluation of the moves</param>
        /// <param name="posInfo">          Information about pieces attacks</param>
        /// <param name="moveBest">         Best move found</param>
        /// <param name="iPermCount">       Nb of permutations evaluated</param>
        /// <param name="iCacheHit">        Nb of cache hit</param>
        /// <param name="iMaxDepth">        Maximum depth evaluated</param>
        /// <returns>
        /// true if a move has been found
        /// </returns>
        protected override bool FindBestMove(ChessBoard                 chessBoard,
                                             SearchEngine.SearchMode    searchMode,
                                             ChessBoard.PlayerColorE    ePlayerColor,
                                             List<ChessBoard.MovePosS>  moveList, 
                                             int[]                      arrIndex,
                                             ChessBoard.PosInfoS        posInfo,
                                             ref ChessBoard.MovePosS    moveBest,
                                             out int                    iPermCount,
                                             out int                    iCacheHit,
                                             out int                    iMaxDepth) {
            bool                bRetVal = false;
            DateTime            dtTimeOut;
            int                 iDepth;
            int                 iPermCountAtLevel;
            
            iPermCount  = 0;
            iCacheHit   = 0;
            if (searchMode.m_iSearchDepth == 0) {
                dtTimeOut   = DateTime.Now + TimeSpan.FromSeconds(searchMode.m_iTimeOutInSec);
                iDepth      = 0;
                do {
                    bRetVal     = FindBestMoveUsingMinMaxAtDepth(chessBoard, searchMode, ePlayerColor, moveList, arrIndex, iDepth + 1, ref moveBest, out iPermCountAtLevel);
                    iPermCount += iPermCountAtLevel;
                    iDepth++;
                } while (DateTime.Now < dtTimeOut);
                iMaxDepth = iDepth;
            } else {
                iMaxDepth = searchMode.m_iSearchDepth;
                bRetVal   = FindBestMoveUsingMinMaxAtDepth(chessBoard, searchMode, ePlayerColor, moveList, arrIndex, iMaxDepth, ref moveBest, out iPermCount);
            }
            return(bRetVal);
        }
    } // Class SearchEngineMinMax
} // Namespace
