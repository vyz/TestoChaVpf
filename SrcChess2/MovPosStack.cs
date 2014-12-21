using System.Collections.Generic;

namespace SrcChess2 {
    /// <summary>Maintains the list of moves which has been done on a board. The undo moves are kept up to when a new move is done.</summary>
    public class MovePosStack {
        /// <summary>List of move position</summary>
        private List<ChessBoard.MovePosS>   m_listMovePos;
        /// <summary>Position of the current move in the list</summary>
        private int                         m_iPosInList;

        /// <summary>
        /// Class constructor
        /// </summary>
        public MovePosStack() {
            m_listMovePos = new List<ChessBoard.MovePosS>(512);
            m_iPosInList  = -1;
        }

        /// <summary>
        /// Class constructor (copy constructor)
        /// </summary>
        private MovePosStack(MovePosStack movePosList) {
            m_listMovePos = new List<ChessBoard.MovePosS>(movePosList.m_listMovePos);
            m_iPosInList  = movePosList.m_iPosInList;
        }

        /// <summary>
        /// Clone the stack
        /// </summary>
        /// <returns>
        /// Move list
        /// </returns>
        public MovePosStack Clone() {
            return(new MovePosStack(this));
        }

        /// <summary>
        /// Save to the specified binary writer
        /// </summary>
        /// <param name="writer">   Binary writer</param>
        public void SaveToWriter(System.IO.BinaryWriter writer) {
            writer.Write(m_listMovePos.Count);
            writer.Write(m_iPosInList);
            foreach (ChessBoard.MovePosS movePos in m_listMovePos) {
                writer.Write((byte)movePos.OriginalPiece);
                writer.Write(movePos.StartPos);
                writer.Write(movePos.EndPos);
                writer.Write((byte)movePos.Type);
            }
        }

        /// <summary>
        /// Load from reader
        /// </summary>
        /// <param name="reader">   Binary Reader</param>
        public void LoadFromReader(System.IO.BinaryReader reader) {
            int                 iMoveCount;
            ChessBoard.MovePosS movePos;
            
            m_listMovePos.Clear();
            iMoveCount      = reader.ReadInt32();
            m_iPosInList    = reader.ReadInt32();
            for (int iIndex = 0; iIndex < iMoveCount; iIndex++) {
                movePos.OriginalPiece   = (ChessBoard.PieceE)reader.ReadByte();
                movePos.StartPos        = reader.ReadByte();
                movePos.EndPos          = reader.ReadByte();
                movePos.Type            = (ChessBoard.MoveTypeE)reader.ReadByte();
                m_listMovePos.Add(movePos);
            }
        }

        /// <summary>
        /// Count
        /// </summary>
        public int Count {
            get {
                return(m_listMovePos.Count);
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public ChessBoard.MovePosS this[int iIndex] {
            get {
                return(m_listMovePos[iIndex]);
            }
        }

        /// <summary>
        /// Get the list of moves
        /// </summary>
        public List<ChessBoard.MovePosS> List {
            get {
                return(m_listMovePos);
            }
        }

        /// <summary>
        /// Add a move to the stack. All redo move are discarded
        /// </summary>
        /// <param name="movePos">  New move</param>
        public void AddMove(ChessBoard.MovePosS movePos) {
            int     iCount;
            int     iPos;
            
            iCount = Count;
            iPos   = m_iPosInList + 1;
            while (iCount != iPos) {
                m_listMovePos.RemoveAt(--iCount);
            }
            m_listMovePos.Add(movePos);
            m_iPosInList = iPos;
        }

        /// <summary>
        /// Current move (last done move)
        /// </summary>
        public ChessBoard.MovePosS CurrentMove {
            get {
                return(this[m_iPosInList]);
            }
        }

        /// <summary>
        /// Next move in the redo list
        /// </summary>
        public ChessBoard.MovePosS NextMove {
            get {
                return(this[m_iPosInList+1]);
            }
        }

        /// <summary>
        /// Move to next move
        /// </summary>
        public void MoveToNext() {
            int iMaxPos;
            
            iMaxPos = Count - 1;
            if (m_iPosInList < iMaxPos) {
                m_iPosInList++;
            }
        }

        /// <summary>
        /// Move to previous move
        /// </summary>
        public void MoveToPrevious() {
            if (m_iPosInList > -1) {
                m_iPosInList--;
            }
        }

        /// <summary>
        /// Current move index
        /// </summary>
        public int PositionInList {
            get {
                return(m_iPosInList);
            }
        }

        /// <summary>
        /// Removes all move in the list
        /// </summary>
        public void Clear() {
            m_listMovePos.Clear();
            m_iPosInList = -1;
        }
    } // Class MovePosStack
} // Namespace
