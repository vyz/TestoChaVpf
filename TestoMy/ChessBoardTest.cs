using SrcChess2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace TestoMy
{
    
    
    /// <summary>
    ///Это класс теста для ChessBoardTest, в котором должны
    ///находиться все модульные тесты ChessBoardTest
    ///</summary>
    [TestClass()]
    public class ChessBoardTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Тест для Points
        ///</summary>
        [TestMethod()]
        public void PointsTest()
        {
            ChessBoard target = new ChessBoard(); // TODO: инициализация подходящего значения
            SearchEngine.SearchMode searchMode = null; // TODO: инициализация подходящего значения
            ChessBoard.PlayerColorE ePlayerToPlay = new ChessBoard.PlayerColorE(); // TODO: инициализация подходящего значения
            int iDepth = 0; // TODO: инициализация подходящего значения
            int iMoveCountDelta = 0; // TODO: инициализация подходящего значения
            ChessBoard.PosInfoS posInfoWhite = new ChessBoard.PosInfoS(); // TODO: инициализация подходящего значения
            ChessBoard.PosInfoS posInfoBlack = new ChessBoard.PosInfoS(); // TODO: инициализация подходящего значения
            int expected = 0; // TODO: инициализация подходящего значения
            int actual;
            actual = target.Points(searchMode, ePlayerToPlay, iDepth, iMoveCountDelta, posInfoWhite, posInfoBlack);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Проверьте правильность этого метода теста.");
        }

        /// <summary>
        ///Тест для LoadBoard
        ///</summary>
        [TestMethod()]
        public void LoadBoardTest()
        {
            ChessBoard target = new ChessBoard();
            BinaryReader reader = new BinaryReader(File.OpenRead(@"..\..\..\TestoMy\as1.che"));

            bool expected = true; 
            bool actual;
            //Первая строка для LocalChessBoardControl
            string a1 = reader.ReadString();
            string b1 = "SRCCHESS095";
            Assert.AreEqual(a1, b1);
            //Вторая строка для ChessBoardControl - после основного чтения еще читает инфо об игроках и времени
            //смотри: public virtual bool LoadGame(BinaryReader reader) 
            a1 = reader.ReadString();
            b1 = "SRCBC095";
            Assert.AreEqual(a1, b1);
            actual = target.LoadBoard(reader);
            Assert.AreEqual(expected, actual);
        }
    }
}
