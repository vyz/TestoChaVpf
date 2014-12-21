using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace SrcChess2 {
    /// <summary>Parser exception</summary>
    [Serializable]
    public class PgnParserException : System.Exception {
        /// <summary>Code which is in error</summary>
        public string   CodeInError;
        /// <summary>Array of move position</summary>
        public int[]    MoveList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        /// <param name="strCodeInError">   Code in error</param>
        /// <param name="ex">               Inner exception</param>
        public          PgnParserException(string strMsg, string strCodeInError, Exception ex) : base(strMsg, ex) { CodeInError = strCodeInError; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        /// <param name="strCodeInError">   Code in error</param>
        public          PgnParserException(string strMsg, string strCodeInError) : this(strMsg, strCodeInError, null) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        public          PgnParserException(string strMsg) : this(strMsg, "", null) {}

        /// <summary>
        /// Constructor
        /// </summary>
        public          PgnParserException() : this("", "", null) {}

        /// <summary>
        /// Unserialize additional data
        /// </summary>
        /// <param name="info">     Serialization Info</param>
        /// <param name="context">  Context Info</param>
        protected PgnParserException(SerializationInfo info, StreamingContext context) : base(info, context) {
            CodeInError = info.GetString("CodeInError");
            MoveList    = (int[])info.GetValue("MoveList", typeof(int[]));
        }

        /// <summary>
        /// Serialize the additional data
        /// </summary>
        /// <param name="info">     Serialization Info</param>
        /// <param name="context">  Context Info</param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("CodeInError",    CodeInError);
            info.AddValue("MoveList",       MoveList);
        }
    } // Class PgnParserException
     
    /// <summary>Class implementing the parsing of a PGN file. PGN is a standard way of recording chess games.</summary>
    public class PgnParser {
    
        /// <summary>Type of player (human of computer program)</summary>
        public enum PlayerTypeE {
            /// <summary>Player is a human</summary>
            Human,
            /// <summary>Player is a computer program</summary>
            Program
        };
        
        /// <summary>Text to parse</summary>
        private string      m_strText;
        /// <summary>Starting position of a game</summary>
        private int         m_iStartPos;
        /// <summary>Current position in text</summary>
        private int         m_iPos;
        /// <summary>Size of the text</summary>
        private int         m_iSize;
        /// <summary>Board use to play as we decode</summary>
        private ChessBoard  m_chessBoard;
        /// <summary>true to diagnose the parser. This generate exception when a move cannot be resolved</summary>
        private bool        m_bDiagnose;

        /// <summary>
        /// Class Ctor
        /// </summary>
        /// <param name="bDiagnose">    true to diagnose the parser</param>
        public PgnParser(bool bDiagnose) {
            m_chessBoard = new ChessBoard();
            m_bDiagnose  = bDiagnose;
        }

        /// <summary>
        /// Return the code of the current game
        /// </summary>
        /// <returns>
        /// Current game
        /// </returns>
        private string GetCodeInError() {
            return(m_strText.Substring(m_iStartPos, m_iPos - m_iStartPos + 1));
        }

        /// <summary>
        /// Peek the next character
        /// </summary>
        /// <returns>
        /// Character
        /// </returns>
        private Char PeekChr() {
            return((m_iPos < m_iSize) ? m_strText[m_iPos] : (Char)0);
        }

        /// <summary>
        /// Gets the next character
        /// </summary>
        /// <returns>
        /// Character
        /// </returns>
        private Char GetChr() {
            return((m_iPos < m_iSize) ? m_strText[m_iPos++] : (Char)0);
        }

        /// <summary>
        /// Skip whitespace
        /// </summary>
        private void SkipSpace() {
            while (m_iPos < m_iSize && Char.IsWhiteSpace(m_strText[m_iPos])) {
                m_iPos++;
            }
        }

        /// <summary>
        /// Skip a remark (...)
        /// </summary>
        private void SkipAltMoveAndRemark() {
            Char    cChr;
            Char    cSkipChr;
            Char    cEndSkip;
            
            SkipSpace();
            cSkipChr = PeekChr();
            while (cSkipChr == '(' || cSkipChr == '{') {
                cEndSkip = (cSkipChr == '(') ? ')' : '}';
                cChr = GetChr();
                while (cChr != cEndSkip && cChr != '\0') {
                    cChr = GetChr();
                }
                SkipSpace();
                cSkipChr = PeekChr();
            }
        }

        /// <summary>
        /// Decode a move
        /// </summary>
        /// <param name="strPos">       Position</param>
        /// <param name="iStartCol">    Returns the starting column found in move if specified (-1 if not)</param>
        /// <param name="iStartRow">    Returns the starting row found in move if specified (-1 if not)</param>
        /// <param name="iEndPos">      Returns the ending position of the move</param>
        private void DecodeMove(string strPos, out int iStartCol, out int iStartRow, out int iEndPos) {
            switch(strPos.Length) {
            case 2:
                if (strPos[0] < 'a' || strPos[0] > 'h' ||
                    strPos[1] < '1' || strPos[1] > '8') {
                    throw new PgnParserException("Unable to decode position", GetCodeInError());
                }
                iStartCol   = -1;
                iStartRow   = -1;
                iEndPos     = (7 - (strPos[0] - 'a')) + ((strPos[1] - '1') << 3);
                break;
            case 3:
                if (strPos[0] >= 'a' && strPos[0] <= 'h') {
                    iStartCol   = 7 - (strPos[0] - 'a');
                    iStartRow   = -1;
                } else if (strPos[0] >= '1' && strPos[0] <= '8') {
                    iStartCol   = -1;
                    iStartRow   = (strPos[0] - '1');
                } else {
                    throw new PgnParserException("Unable to decode position", GetCodeInError());
                }
                if (strPos[1] < 'a' || strPos[1] > 'h' ||
                    strPos[2] < '1' || strPos[2] > '8') {
                    throw new PgnParserException("Unable to decode position", GetCodeInError());
                }
                iEndPos     = (7 - (strPos[1] - 'a')) + ((strPos[2] - '1') << 3);
                break;
            case 4:
                if (strPos[0] < 'a' || strPos[0] > 'h' ||
                    strPos[1] < '1' || strPos[1] > '8' ||
                    strPos[2] < 'a' || strPos[2] > 'h' ||
                    strPos[3] < '1' || strPos[3] > '8') {
                    throw new PgnParserException("Unable to decode position", GetCodeInError());
                }
                iStartCol   = 7 - (strPos[0] - 'a');
                iStartRow   = (strPos[1] - '1');
                iEndPos     = (7 - (strPos[2] - 'a')) + ((strPos[3] - '1') << 3);
                break;
            default:
                throw new PgnParserException("Unable to decode position", GetCodeInError());
            }
        }

        /// <summary>
        /// Find a castle move
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="bShortCastling">   true for short, false for long</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="strMove">          Move</param>
        /// <param name="movePos">          Returned moved if found</param>
        /// <returns>
        /// Moving position or -1 if error
        /// </returns>
        private int FindCastling(ChessBoard.PlayerColorE ePlayerColor, bool bShortCastling, ref int iTruncated, string strMove, ref ChessBoard.MovePosS movePos) {
            int                         iRetVal = -1;
            int                         iWantedDelta;
            int                         iDelta;
            List<ChessBoard.MovePosS>   arrMovePos;

            arrMovePos      = m_chessBoard.EnumMoveList(ePlayerColor);
            iWantedDelta    = bShortCastling ? 2 : -2;
            foreach (ChessBoard.MovePosS move in arrMovePos) {
                if ((move.Type & ChessBoard.MoveTypeE.MoveTypeMask) == ChessBoard.MoveTypeE.Castle) {
                    iDelta = ((int)move.StartPos & 7) - ((int)move.EndPos & 7);
                    if (iDelta == iWantedDelta) {
                        iRetVal = (int)move.StartPos + ((int)move.EndPos << 8);
                        movePos = move;
                        m_chessBoard.DoMove(move);
                    }
                }
            }
            if (iRetVal == -1) {
                if (m_bDiagnose) {
                    throw new PgnParserException("Unable to find compatible move - " + strMove, GetCodeInError());
                }
                iTruncated++;
            }
            return(iRetVal);
        }

        /// <summary>
        /// Find a move using the specification
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="ePiece">           Piece moving</param>
        /// <param name="iStartCol">        Starting column of the move or -1 if not specified</param>
        /// <param name="iStartRow">        Starting row of the move or -1 if not specified</param>
        /// <param name="iEndPos">          Ending position of the move</param>
        /// <param name="eMoveType">        Type of move. Use for discriminating between different pawn promotion.</param>
        /// <param name="strMove">          Move</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="movePos">          Move position</param>
        /// <returns>
        /// Moving position or -1 if error
        /// </returns>
        private int FindPieceMove(ChessBoard.PlayerColorE ePlayerColor, ChessBoard.PieceE ePiece, int iStartCol, int iStartRow, int iEndPos, ChessBoard.MoveTypeE eMoveType, string strMove, ref int iTruncated, ref ChessBoard.MovePosS movePos) {
            int                         iRetVal = -1;
            List<ChessBoard.MovePosS>   arrMovePos;
            int                         iCol;
            int                         iRow;
            
            ePiece      = ePiece | ((ePlayerColor == ChessBoard.PlayerColorE.Black) ? ChessBoard.PieceE.Black : ChessBoard.PieceE.White);
            arrMovePos  = m_chessBoard.EnumMoveList(ePlayerColor);
            foreach (ChessBoard.MovePosS move in arrMovePos) {
                if ((int)move.EndPos == iEndPos && m_chessBoard[(int)move.StartPos] == ePiece) {
                    if (eMoveType == ChessBoard.MoveTypeE.Normal || (move.Type & ChessBoard.MoveTypeE.MoveTypeMask) == eMoveType) {
                        iCol = (int)move.StartPos & 7;
                        iRow = (int)move.StartPos >> 3;
                        if ((iStartCol == -1 || iStartCol == iCol) &&
                            (iStartRow == -1 || iStartRow == iRow)) {
                            if (iRetVal != -1) {
                                throw new PgnParserException("More then one piece found for this move - "  + strMove, GetCodeInError());
                            }
                            movePos = move;
                            iRetVal = (int)move.StartPos + ((int)move.EndPos << 8);
                            m_chessBoard.DoMove(move);
                        }
                    }
                }
            }            
            if (iRetVal == -1) {
                if (m_bDiagnose) {
                    throw new PgnParserException("Unable to find compatible move - " + strMove, GetCodeInError());
                }
                iTruncated++;
            }
            return(iRetVal);
        }

        /// <summary>
        /// Convert a PGN position into a moving position
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="strMove">          Move</param>
        /// <param name="iPos">             Returned moving position</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="movePos">          Move position</param>
        private void CnvRawMoveToPosMove(ChessBoard.PlayerColorE ePlayerColor, string strMove, out int iPos, ref int iTruncated, ref ChessBoard.MovePosS movePos) {
            string                  strPureMove;
            int                     iIndex;
            int                     iStartCol;
            int                     iStartRow;
            int                     iEndPos;
            int                     iOfs;
            ChessBoard.PieceE       ePiece;
            ChessBoard.MoveTypeE    eMoveType;
            
            eMoveType   = ChessBoard.MoveTypeE.Normal;
            iPos        = 0;
            strPureMove = strMove.Replace("x", "").Replace("#", "").Replace("ep","").Replace("+", "");
            iIndex      = strPureMove.IndexOf('=');
            if (iIndex != -1) {
                if (strPureMove.Length > iIndex + 1) {
                    switch(strPureMove[iIndex+1]) {
                    case 'Q':
                        eMoveType = ChessBoard.MoveTypeE.PawnPromotionToQueen;
                        break;
                    case 'R':
                        eMoveType = ChessBoard.MoveTypeE.PawnPromotionToRook;
                        break;
                    case 'B':
                        eMoveType = ChessBoard.MoveTypeE.PawnPromotionToBishop;
                        break;
                    case 'N':
                        eMoveType = ChessBoard.MoveTypeE.PawnPromotionToKnight;
                        break;
                    case 'P':
                        eMoveType = ChessBoard.MoveTypeE.PawnPromotionToPawn;
                        break;
                    default:
                        iPos = -1;
                        iTruncated++;
                        break;
                    }
                    if (iPos != -1) {
                        strPureMove = strPureMove.Substring(0, iIndex);
                    }
                } else {
                    iPos = -1;
                    iTruncated++;
                }
            }
            if (iPos == 0) {
                if (strPureMove == "O-O") {
                    iPos = FindCastling(ePlayerColor, true, ref iTruncated, strMove, ref movePos);
                } else if (strPureMove == "O-O-O") {
                    iPos = FindCastling(ePlayerColor, false, ref iTruncated, strMove, ref movePos);
                } else {
                    iOfs = 1;
                    switch(strPureMove[0]) {
                    case 'K':   // King
                        ePiece = ChessBoard.PieceE.King;
                        break;
                    case 'N':   // Knight
                        ePiece = ChessBoard.PieceE.Knight;
                        break;
                    case 'B':   // Bishop
                        ePiece = ChessBoard.PieceE.Bishop;
                        break;
                    case 'R':   // Rook
                        ePiece = ChessBoard.PieceE.Rook;
                        break;
                    case 'Q':   // Queen
                        ePiece = ChessBoard.PieceE.Queen;
                        break;
                    default:    // Pawn
                        ePiece = ChessBoard.PieceE.Pawn;
                        iOfs   = 0;
                        break;
                    }
                    DecodeMove(strPureMove.Substring(iOfs), out iStartCol, out iStartRow, out iEndPos);
                    iPos = FindPieceMove(ePlayerColor, ePiece, iStartCol, iStartRow, iEndPos, eMoveType, strMove, ref iTruncated, ref movePos);
                }
            }
        }

        /// <summary>
        /// Convert a list of PGN positions into a moving positions
        /// </summary>
        /// <param name="eColorToPlay">     Color to play</param>
        /// <param name="arrRawMove">       Array of PGN moves</param>
        /// <param name="piMoveList">       Returned array of moving position</param>
        /// <param name="listMovePos">      Returned the list of move if not null</param>
        /// <param name="iSkip">            Skipped count</param>
        /// <param name="iTruncated">       Truncated count</param>
        private void CnvRawMoveToPosMove(ChessBoard.PlayerColorE eColorToPlay, List<string> arrRawMove, out int[] piMoveList, List<ChessBoard.MovePosS> listMovePos, ref int iSkip, ref int iTruncated) {
            List<int>               arrMoveList;
            ChessBoard.MovePosS     movePos;
            int                     iPos;
            
            movePos          = new ChessBoard.MovePosS();
            movePos.StartPos = 0;
            movePos.EndPos   = 0;
            movePos.Type     = ChessBoard.MoveTypeE.Normal;
            arrMoveList      = new List<int>(256);
            try {
                foreach (string strMove in arrRawMove) {
                    if (strMove == "1-0" ||
                        strMove == "0-1" ||
                        strMove == "1/2-1/2" ||
                        strMove == "*" ||
                        strMove[0] == '(') {
                        break;
                    } else if (strMove == "..") {
                        iSkip++;
                        break;
                    }
                    CnvRawMoveToPosMove(eColorToPlay, strMove, out iPos, ref iTruncated, ref movePos);
                    if (iPos != -1) {
                        arrMoveList.Add(iPos);
                        if (listMovePos != null) {
                            listMovePos.Add(movePos);
                        }
                        eColorToPlay = (eColorToPlay == ChessBoard.PlayerColorE.Black) ? ChessBoard.PlayerColorE.White : ChessBoard.PlayerColorE.Black;
                    } else {
                        break;
                    }
                }
            } catch(PgnParserException ex) {
                ex.MoveList = arrMoveList.ToArray();
                throw;
            }
            piMoveList = arrMoveList.ToArray();
        }

        /// <summary>
        /// Parse an attribute [Name "Value"]
        /// </summary>
        /// <param name="dict">             Dictionary where to append the attribute</param>
        private void ParseAttr(Dictionary<string,string> dict) {
            StringBuilder   strbName;
            StringBuilder   strbValue;
            Char            cChr;
            
            strbName    = new StringBuilder(64);
            strbValue   = new StringBuilder(64);
            GetChr();
            cChr        = GetChr();
            while (!Char.IsWhiteSpace(cChr) && cChr != ']' && cChr != '\0') {
                strbName.Append(cChr);
                cChr = GetChr();
            }
            if (Char.IsWhiteSpace(cChr)) {
                SkipSpace();
                cChr = GetChr();
                if (cChr == '"') {
                    cChr = GetChr();
                    while (cChr != '"' && cChr != '\0') {
                        strbValue.Append(cChr);
                        cChr = GetChr();
                    }
                    SkipSpace();
                    cChr = GetChr();
                }
            }
            if (cChr != ']') {
                throw new PgnParserException("Syntax error", GetCodeInError());
            }
            dict.Add(strbName.ToString().ToUpper(), strbValue.ToString());
        }

        /// <summary>
        /// Parse a single PGN move
        /// </summary>
        /// <param name="iMoveIndex">       Index of the current move</param>
        /// <param name="strMove">          Returned PGN move</param>
        /// <returns>
        /// true if succeed, false if failed
        /// </returns>
        private bool ParseRawMove(int iMoveIndex, out string strMove) {
            bool            bRetVal = false;
            int             iCount;
            StringBuilder   strbMoveIndex;
            StringBuilder   strbMove;
            Char            cChr;
            
            strbMoveIndex   = new StringBuilder(64);
            strbMove        = new StringBuilder(64);
            strMove         = null;
            SkipAltMoveAndRemark();
            if (Char.IsDigit(PeekChr())) {
                cChr = GetChr();
                while (Char.IsDigit(cChr)) {
                    strbMoveIndex.Append(cChr);
                    cChr = GetChr();
                }
                if (iMoveIndex == Int32.Parse(strbMoveIndex.ToString()) && cChr == '.') {
                    SkipAltMoveAndRemark();
                    cChr    = GetChr();
                    bRetVal = true;
                    iCount  = 0;
                    for (int iIndex = 0; iIndex < 2; iIndex++) {
                        while (!Char.IsWhiteSpace(cChr) && cChr != '\0') {
                            strbMove.Append(cChr);
                            iCount++;
                            cChr = GetChr();
                        }
                        SkipAltMoveAndRemark();
                        if (iIndex == 0 && PeekChr() != '\0') {
                            strbMove.Append(' ');
                            cChr = GetChr();
                        }
                        if (iCount == 0) {
                            bRetVal = false;
                        }
                    }
                }
                strMove = strbMove.ToString();
            }
            return(bRetVal);
        }

        /// <summary>
        /// Parse FEN definition into a board representation
        /// </summary>
        /// <param name="strFEN">           FEN</param>
        /// <param name="eColorToMove">     Return the color to move</param>
        /// <param name="eBoardStateMask">  Return the mask of castling info</param>
        /// <param name="iEnPassant">       Return the en passant position or 0 if none</param>
        /// <returns>
        /// true if succeed, false if failed
        /// </returns>
        private bool ParseFEN(string strFEN, out ChessBoard.PlayerColorE eColorToMove, out ChessBoard.BoardStateMaskE eBoardStateMask, out int iEnPassant) {
            bool                bRetVal = true;
            string[]            arrCmd;
            string[]            arrRow;
            int                 iPos;
            int                 iLinePos;
            int                 iBlankCount;
            ChessBoard.PieceE   ePiece;
            
            eBoardStateMask = (ChessBoard.BoardStateMaskE)0;
            iEnPassant      = 0;
            eColorToMove    = ChessBoard.PlayerColorE.White;
            arrCmd          = strFEN.Split(' ');
            if (arrCmd.Length != 6) {
                bRetVal = false;
            } else {
                arrRow = arrCmd[0].Split('/');
                if (arrRow.Length != 8) {
                    bRetVal = false;
                } else {
                    iPos = 63;
                    foreach (string strRow in arrRow) {
                        iLinePos = 0;
                        foreach (char cChr in strRow) {
                            ePiece = ChessBoard.PieceE.None;
                            switch(cChr) {
                            case 'P':
                                ePiece = ChessBoard.PieceE.Pawn | ChessBoard.PieceE.White;
                                break;
                            case 'N':
                                ePiece = ChessBoard.PieceE.Knight | ChessBoard.PieceE.White;
                                break;
                            case 'B':
                                ePiece = ChessBoard.PieceE.Bishop | ChessBoard.PieceE.White;
                                break;
                            case 'R':
                                ePiece = ChessBoard.PieceE.Rook | ChessBoard.PieceE.White;
                                break;
                            case 'Q':
                                ePiece = ChessBoard.PieceE.Queen | ChessBoard.PieceE.White;
                                break;
                            case 'K':
                                ePiece = ChessBoard.PieceE.King | ChessBoard.PieceE.White;
                                break;
                            case 'p':
                                ePiece = ChessBoard.PieceE.Pawn | ChessBoard.PieceE.Black;
                                break;
                            case 'n':
                                ePiece = ChessBoard.PieceE.Knight | ChessBoard.PieceE.Black;
                                break;
                            case 'b':
                                ePiece = ChessBoard.PieceE.Bishop | ChessBoard.PieceE.Black;
                                break;
                            case 'r':
                                ePiece = ChessBoard.PieceE.Rook | ChessBoard.PieceE.Black;
                                break;
                            case 'q':
                                ePiece = ChessBoard.PieceE.Queen | ChessBoard.PieceE.Black;
                                break;
                            case 'k':
                                ePiece = ChessBoard.PieceE.King | ChessBoard.PieceE.Black;
                                break;
                            default:
                                if (cChr >= '1' && cChr <= '8') {
                                     iBlankCount = Int32.Parse(cChr.ToString());
                                     if (iBlankCount + iLinePos <= 8) {
                                        for (int iIndex = 0; iIndex < iBlankCount; iIndex++) {
                                            m_chessBoard[iPos--] = ChessBoard.PieceE.None;
                                        }
                                        iLinePos += iBlankCount;
                                    }
                                } else {
                                    bRetVal = false;
                                }
                                break;
                            }
                            if (bRetVal && ePiece != ChessBoard.PieceE.None) {
                                if (iLinePos < 8) {
                                    m_chessBoard[iPos--] = ePiece;
                                    iLinePos++;
                                } else {
                                    bRetVal = false;
                                }
                            }
                        }
                        if (iLinePos != 8) {
                            bRetVal = false;
                        }
                    }
                    if (bRetVal) {
                        if (arrCmd[1] == "w") {
                            eColorToMove = ChessBoard.PlayerColorE.White;
                        } else if (arrCmd[1] == "b") {
                            eColorToMove = ChessBoard.PlayerColorE.Black;
                        } else {
                            bRetVal = false;
                        }
                        if (arrCmd[2] != "-") {
                            for (int iIndex = 0; iIndex < arrCmd.Length; iIndex++) {
                                switch(arrCmd[2][iIndex]) {
                                case 'K':
                                    eBoardStateMask |= ChessBoard.BoardStateMaskE.WRCastling;
                                    break;
                                case 'Q':
                                    eBoardStateMask |= ChessBoard.BoardStateMaskE.WLCastling;
                                    break;
                                case 'k':
                                    eBoardStateMask |= ChessBoard.BoardStateMaskE.BRCastling;
                                    break;
                                case 'q':
                                    eBoardStateMask |= ChessBoard.BoardStateMaskE.BLCastling;
                                    break;
                                }
                            }
                        }
                        if (arrCmd[3] == "-") {
                            iEnPassant = 0;
                        } else {
                            iEnPassant = PgnUtil.GetSquareIDFromPGN(arrCmd[3]);
                            if (iEnPassant == -1) {
                                iEnPassant = 0;
                            }
                        }
                    }
                }
            }
            return(bRetVal);
        }

        /// <summary>
        /// Parse one game in PGN
        /// </summary>
        /// <param name="bIgnoreMoveListIfFEN"> Ignore the move list if FEN is found</param>
        /// <param name="piMoveList">           Returned move list</param>
        /// <param name="attrs">                Returned list of attributes for this game</param>
        /// <param name="listMovePos">          Returned the list of move if not null</param>
        /// <param name="iSkip">                Number of games skipped</param>
        /// <param name="iTruncated">           Number of games truncated</param>
        /// <param name="chessBoardStarting">   Starting board setting. If null, standard board used.</param>
        /// <param name="eStartingColor">       Starting color</param>
        /// <param name="strWhitePlayerName">   White pieces player name</param>
        /// <param name="strBlackPlayerName">   Black pieces player name</param>
        /// <param name="eWhitePlayerType">     White pieces player type</param>
        /// <param name="eBlackPlayerType">     Black pieces player type</param>
        /// <param name="spanWhitePlayer">      Time span for white player</param>
        /// <param name="spanBlackPlayer">      Time span for black player</param>
        /// <returns>
        /// false if invalid board
        /// </returns>
        /// <remarks>
        /// 
        /// The parser understand an extended version of the [TimeControl] tag:
        /// 
        ///     [TimeControl "?:123:456"]   where 123 = white tick count, 456 = black tick count (100 nano-sec unit)
        ///
        /// The parser also understand the following standard tags:
        /// 
        ///     [White] [Black] [FEN] [WhiteType] [BlackType]
        /// 
        /// </remarks>
        private bool ParseNextMoveList(bool                             bIgnoreMoveListIfFEN,
                                       out int[]                        piMoveList,
                                       out Dictionary<string, string>   attrs,
                                       List<ChessBoard.MovePosS>        listMovePos,
                                       ref int                          iSkip,
                                       ref int                          iTruncated,
                                       out ChessBoard                   chessBoardStarting,
                                       out ChessBoard.PlayerColorE      eStartingColor,
                                       out string                       strWhitePlayerName,
                                       out string                       strBlackPlayerName,
                                       out PlayerTypeE                  eWhitePlayerType,
                                       out PlayerTypeE                  eBlackPlayerType,
                                       out TimeSpan                     spanWhitePlayer,
                                       out TimeSpan                     spanBlackPlayer) {
            bool                        bRetVal = true;
            bool                        bEndOfMove;
            bool                        bFENFound;
            List<string>                arrRawMove;
            int                         iMoveIndex;
            string                      strMove;
            string                      strFEN;
            string[]                    arrStr;
            string[]                    arrTimeControl;
            ChessBoard.BoardStateMaskE  eBoardMask;
            int                         iEnPassant;
            int                         iTick1;
            int                         iTick2;
            
            attrs               = new Dictionary<string,string>(10);
            chessBoardStarting  = null;
            eStartingColor      = ChessBoard.PlayerColorE.White;
            piMoveList          = null;
            arrRawMove          = new List<string>(256);
            SkipAltMoveAndRemark();
            while (PeekChr() == '[') {
                ParseAttr(attrs);
                SkipAltMoveAndRemark();
            }
            strWhitePlayerName = (attrs.ContainsKey("WHITE")) ? attrs["WHITE"] : "Player 1";
            strBlackPlayerName = (attrs.ContainsKey("BLACK")) ? attrs["BLACK"] : "Player 2";
            if (attrs.ContainsKey("WHITETYPE")) {
                eWhitePlayerType = (attrs["WHITETYPE"].ToLower() == "program") ? PlayerTypeE.Program : PlayerTypeE.Human;
            } else {
                eWhitePlayerType = PlayerTypeE.Human;
            }
            if (attrs.ContainsKey("BLACKTYPE")) {
                eBlackPlayerType = (attrs["BLACKTYPE"].ToLower() == "program") ? PlayerTypeE.Program : PlayerTypeE.Human;
            } else {
                eBlackPlayerType = PlayerTypeE.Human;
            }
            arrTimeControl = (attrs.ContainsKey("TIMECONTROL")) ? attrs["TIMECONTROL"].Split(':') : null;
            if (arrTimeControl != null      &&
                arrTimeControl.Length == 3  &&
                arrTimeControl[0] == "?"    &&
                Int32.TryParse(arrTimeControl[1], out iTick1)   &&
                Int32.TryParse(arrTimeControl[2], out iTick2)) {
                spanWhitePlayer = new TimeSpan(iTick1);
                spanBlackPlayer = new TimeSpan(iTick2);
            } else {
                spanWhitePlayer = TimeSpan.Zero;
                spanBlackPlayer = TimeSpan.Zero;
            }
            bFENFound  = attrs.ContainsKey("FEN");
            m_chessBoard.ResetBoard();
            if (bFENFound) {
                strFEN = attrs["FEN"];
                m_chessBoard.OpenDesignMode();
                bRetVal = ParseFEN(strFEN, out eStartingColor, out eBoardMask, out iEnPassant);
                m_chessBoard.CloseDesignMode(eStartingColor, eBoardMask, iEnPassant);
                if (bRetVal) {
                    chessBoardStarting = m_chessBoard.Clone();
                }
            }
            if (bRetVal && !(bFENFound && bIgnoreMoveListIfFEN)) {
                iMoveIndex = 1;
                bEndOfMove = false;
                while (!bEndOfMove && ParseRawMove(iMoveIndex, out strMove)) {
                    arrStr = strMove.Split(' ');
                    if (arrStr.Length == 2) {
                        arrRawMove.Add(arrStr[0]);
                        arrRawMove.Add(arrStr[1]);
                    } else if (arrStr.Length == 1) {
                        arrRawMove.Add(arrStr[0]);
                    } else {
                        bEndOfMove = true;
                    }
                    iMoveIndex++;
                }
                if (arrRawMove.Count == 0) {
                    if (m_bDiagnose) {
                        throw new PgnParserException("Syntax error", GetCodeInError());
                    }
                    iSkip++;
                }
                CnvRawMoveToPosMove(eStartingColor, arrRawMove, out piMoveList, listMovePos, ref iSkip, ref iTruncated);
            }
            return(bRetVal);
        }

        /// <summary>
        /// Parse a PGN text file
        /// </summary>
        /// <param name="strText">          PGN Text</param>
        /// <param name="arrMoveList">      Move list array to fill</param>
        /// <param name="iSkip">            Number of games skipped</param>
        /// <param name="iTruncated">       Number of games truncated</param>
        public void Parse(string strText, List<int[]> arrMoveList, out int iSkip, out int iTruncated) {
            int[]                       piMoveList;
            Dictionary<string,string>   attrs;
            ChessBoard                  chessBoardStarting;
            ChessBoard.PlayerColorE     eStartingColor;
            string                      strWhitePlayerName;
            string                      strBlackPlayerName;
            PlayerTypeE                 eWhitePlayerType;
            PlayerTypeE                 eBlackPlayerType;
            TimeSpan                    spanWhitePlayer;
            TimeSpan                    spanBlackPlayer;
            
            m_strText   = strText;
            m_iPos      = 0;
            m_iSize     = strText.Length;
            iSkip       = 0;
            iTruncated  = 0;
            SkipAltMoveAndRemark();
            while (PeekChr() != '\0') {
                m_iStartPos = m_iPos;
                ParseNextMoveList(false,
                                  out piMoveList,
                                  out attrs,
                                  null,
                                  ref iSkip,
                                  ref iTruncated,
                                  out chessBoardStarting,
                                  out eStartingColor,
                                  out strWhitePlayerName,
                                  out strBlackPlayerName,
                                  out eWhitePlayerType,
                                  out eBlackPlayerType,
                                  out spanWhitePlayer,
                                  out spanBlackPlayer);
                arrMoveList.Add(piMoveList);
                while (PeekChr() != '[' && PeekChr() != '\0') {
                    if (PeekChr() == '{') {
                        SkipAltMoveAndRemark();
                    } else {
                        GetChr();
                    }
                }
            }
        }

        /// <summary>
        /// Parse a single PGN game
        /// </summary>
        /// <param name="strText">              PGN Text</param>
        /// <param name="bIgnoreMoveListIfFEN"> Ignore the move list if FEN is found</param>
        /// <param name="listMovePos">          Returned the list of move if not null</param>
        /// <param name="iSkip">                Number of games skipped</param>
        /// <param name="iTruncated">           Number of games truncated</param>
        /// <param name="chessBoardStarting">   Starting board setting. If null, standard board used.</param>
        /// <param name="eStartingColor">       Starting color</param>
        /// <param name="strWhitePlayerName">   White pieces player name</param>
        /// <param name="strBlackPlayerName">   Black pieces player name</param>
        /// <param name="eWhitePlayerType">     White pieces player type</param>
        /// <param name="eBlackPlayerType">     Black pieces player type</param>
        /// <param name="spanWhitePlayer">      Time span for white player</param>
        /// <param name="spanBlackPlayer">      Time span for black player</param>
        /// <returns>
        /// false if the board specified by FEN is invalid.
        /// </returns>
        public bool ParseSingle(string                      strText,
                                bool                        bIgnoreMoveListIfFEN,
                                List<ChessBoard.MovePosS>   listMovePos,
                                out int                     iSkip,
                                out int                     iTruncated,
                                out ChessBoard              chessBoardStarting,
                                out ChessBoard.PlayerColorE eStartingColor,
                                out string                  strWhitePlayerName,
                                out string                  strBlackPlayerName,
                                out PlayerTypeE             eWhitePlayerType,
                                out PlayerTypeE             eBlackPlayerType,
                                out TimeSpan                spanWhitePlayer,
                                out TimeSpan                spanBlackPlayer) {
            bool                        bRetVal = true;
            int[]                       piMoveList;
            Dictionary<string,string>   attrs;
            
            listMovePos.Clear();
            chessBoardStarting  = null;
            eStartingColor      = ChessBoard.PlayerColorE.White;
            strWhitePlayerName  = "Player 1";
            strBlackPlayerName  = "Player 2";
            eWhitePlayerType    = PlayerTypeE.Human;
            eBlackPlayerType    = PlayerTypeE.Human;
            spanWhitePlayer     = TimeSpan.Zero;
            spanBlackPlayer     = TimeSpan.Zero;
            m_strText           = strText;
            m_iPos              = 0;
            m_iSize             = strText.Length;
            iSkip               = 0;
            iTruncated          = 0;
            SkipAltMoveAndRemark();
            if (PeekChr() != '\0') {
                m_iStartPos = m_iPos;
                bRetVal = ParseNextMoveList(bIgnoreMoveListIfFEN,
                                            out piMoveList,
                                            out attrs,
                                            listMovePos,
                                            ref iSkip,
                                            ref iTruncated,
                                            out chessBoardStarting,
                                            out eStartingColor,
                                            out strWhitePlayerName,
                                            out strBlackPlayerName,
                                            out eWhitePlayerType,
                                            out eBlackPlayerType,
                                            out spanWhitePlayer,
                                            out spanBlackPlayer);
            }
            return(bRetVal);
        }
    } // Class PgnParser
} // Namespace
