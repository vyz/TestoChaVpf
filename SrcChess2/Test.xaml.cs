using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SrcChess2 {
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : Window {
        private PieceSet            m_pieceSet;
        private ChessBoard.PieceE   m_ePiece;

        public Test() {
            InitializeComponent();
        }

        public Test(PieceSet pieceSet) : this() {
            m_pieceSet  = pieceSet;
            m_ePiece    = ChessBoard.PieceE.Pawn;
            ShowControl();
        }

        private void ShowControl() {
            UserControl     ctl;

            ctl             = m_pieceSet[m_ePiece];
            Container.Child = ctl;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (m_ePiece == ChessBoard.PieceE.King) {
                m_ePiece = ChessBoard.PieceE.Pawn;
            } else {
                m_ePiece++;
            }
            ShowControl();
        }
    }
}
