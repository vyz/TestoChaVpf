using SrcChess2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestoMy
{
    
    
    /// <summary>
    ///Это класс теста для ChessBoard_MovePosSTest, в котором должны
    ///находиться все модульные тесты ChessBoard_MovePosSTest
    ///</summary>
    [TestClass()]
    public class ChessBoard_MovePosSTest
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
        /// Тест для ToString
        /// Заложено 26 июля 2014 года
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            ChessBoard.MovePosS target = new ChessBoard.MovePosS();
            target.OriginalPiece = ChessBoard.PieceE.Knight | ChessBoard.PieceE.Black;
            target.StartPos = 57;
            target.EndPos = 42;
            target.Type = ChessBoard.MoveTypeE.Normal;
            string expected = "Чёрный конь g8-f6";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}
