﻿using SrcChess2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestoMy
{
    
    
    /// <summary>
    ///Это класс теста для PgnParserTest, в котором должны
    ///находиться все модульные тесты PgnParserTest
    ///</summary>
    [TestClass()]
    public class PgnParserTest
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
        ///Тест для ParseFEN
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SrcChess2.exe")]
        public void ParseFENTest()
        {
            // Сбой при создании частного метода доступа для "Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly"
            Assert.Inconclusive("Сбой при создании частного метода доступа для \"Microsoft.VisualStudio.TestTools.T" +
                    "ypesAndSymbols.Assembly\"");
        }
    }
}
