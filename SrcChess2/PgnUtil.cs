using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SrcChess2 {
    /// <summary>
    /// Utility class to help handling PGN files
    /// </summary>
    public class PgnUtil {

        [Flags]
        private enum PGNAmbiguity {
            NotFound            = 0,
            Found               = 1,
            ColMustBeSpecify    = 2,
            RowMustBeSpecify    = 4
        }
        
        /// <summary>Information use to filter a PGN file</summary>
        public class FilterClause {
            /// <summary>All ELO included if true</summary>
            public  bool                        m_bAllRanges;
            /// <summary>Includes unrated games if true</summary>
            public  bool                        m_bIncludesUnrated;
            /// <summary>If not all EOL included, hash of all ELO which must be included. Each value represent a range (value, value+99)</summary>
            public  Dictionary<int, int>        m_hashRanges;
            /// <summary>All players included if true</summary>
            public  bool                        m_bAllPlayers;
            /// <summary>Hash of all players to include if not all included</summary>
            public  Dictionary<string,string>   m_hashPlayerList;
            /// <summary>Includes all ending if true</summary>
            public  bool                        m_bAllEnding;
            /// <summary>true to include game winned by white player</summary>
            public  bool                        m_bEndingWhiteWinning;
            /// <summary>true to include game winned by black player</summary>
            public  bool                        m_bEndingBlackWinning;
            /// <summary>true to include draws game </summary>
            public  bool                        m_bEndingDraws;
        }

        /// <summary>Item used to fill the description listbox so we can find the original index in the list after a sort</summary>
        public class PGNGameDescItem : IComparable<PGNGameDescItem> {
            private string  m_strDesc;
            private int     m_iIndex;

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="strDesc">  Item description</param>
            /// <param name="iIndex">   Item index</param>
            public PGNGameDescItem(string strDesc, int iIndex) {
                m_strDesc = strDesc;
                m_iIndex  = iIndex;
            }

            /// <summary>
            /// Description of the item
            /// </summary>
            public string Description {
                get {
                    return(m_strDesc);
                }
            }

            /// <summary>
            /// Index of the item
            /// </summary>
            public int Index {
                get {
                    return(m_iIndex);
                }
            }

            /// <summary>
            /// IComparable interface
            /// </summary>
            /// <param name="other">    Item to compare with</param>
            /// <returns>
            /// -1, 0, 1
            /// </returns>
            public int CompareTo(PGNGameDescItem other) {
                return(String.Compare(m_strDesc, other.m_strDesc));
            }

            /// <summary>
            /// Return the description
            /// </summary>
            /// <returns>
            /// Description
            /// </returns>
            public override string ToString() {
                return(m_strDesc);
            }
        } // Class PGNGameDescItem
        
        
        private string  m_strPushedLine = null;
        private int     m_iCurLine      = 0;

        /// <summary>
        /// Open an file for reading
        /// </summary>
        /// <param name="strInpFileName">   File name to open</param>
        /// <returns>
        /// Stream or null if unable to open the file.
        /// </returns>
        public static Stream OpenInpFile(string strInpFileName) {
            Stream  streamInp;
            
            try {
                streamInp = File.OpenRead(strInpFileName);
            } catch(System.Exception) {
                MessageBox.Show("Unable to open the file - " + strInpFileName);
                streamInp = null;
            }
            return(streamInp);
        }

        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <param name="strOutFileName">   Name of the file to create</param>
        /// <returns>
        /// Stream or null if unable to create the file.
        /// </returns>
        public static Stream CreateOutFile(string strOutFileName) {
            Stream  streamOut;
            
            try {
                streamOut = File.Create(strOutFileName);
            } catch(System.Exception) {
                MessageBox.Show("Unable to create the file - " + strOutFileName);
                streamOut = null;
            }
            return(streamOut);
        }

        /// <summary>
        /// Gets the next non emoty line. Support one line pushed ahead
        /// </summary>
        /// <param name="reader">   Text reader</param>
        /// <returns>
        /// Line or null if EOF
        /// </returns>
        private string GetNextNonEmptyLine(TextReader reader) {
            string  strRetVal;
            
            if (m_strPushedLine != null) {
                strRetVal       = m_strPushedLine;
                m_strPushedLine = null;
            } else {
                strRetVal = reader.ReadLine();
                m_iCurLine++;
                while (strRetVal != null && strRetVal == String.Empty) {
                    strRetVal = reader.ReadLine();
                    m_iCurLine++;
                }
            }
            return(strRetVal);
        }

        /// <summary>
        /// Extract the ELO value of the string
        /// </summary>
        /// <param name="str">      Input string</param>
        /// <returns>
        /// ELO value or -1 if not found
        /// </returns>
        private static int GetELOValue(string str) {
            int iRetVal;
            int iIndex;
            
            iIndex = str.IndexOf('"');
            if (iIndex < 1) {
                iRetVal = -1;
            } else {
                iRetVal = Int32.Parse(str.Substring(0, iIndex));
            }
            return(iRetVal);
        }

        /// <summary>
        /// Get the next PGN game from the file.
        /// </summary>
        /// <param name="reader">           Text reader</param>
        /// <param name="iGameStartPos">    Return the beginning of the game in the string array</param>
        /// <param name="iWhiteELO">        White player ELO</param>
        /// <param name="iBlackELO">        Black player ELO</param>
        /// <returns>
        /// Array of string describing the game or null if EOF
        /// </returns>
        private List<String> GetPGNGame(TextReader reader, out int iGameStartPos, out int iWhiteELO, out int iBlackELO) {
            List<string>    arrRetVal;
            string          strLine;
            
            iGameStartPos = -1;
            iWhiteELO     = -1;
            iBlackELO     = -1;
            arrRetVal     = new List<string>(32);
            strLine       = GetNextNonEmptyLine(reader);
            while (strLine != null && strLine[0] == '[') {
                arrRetVal.Add(strLine);
                if (strLine.Length > 10 && strLine[6] == 'E') {
                    if (String.Compare(strLine, 0, "[BlackElo \"", 0, 11) == 0) {
                        iBlackELO = GetELOValue(strLine.Substring(11));
                    } else if (String.Compare(strLine, 0, "[WhiteElo \"", 0, 11) == 0) {
                        iWhiteELO = GetELOValue(strLine.Substring(11));
                    }
                }
                strLine   = GetNextNonEmptyLine(reader);
            }
            if (strLine != null) {
                iGameStartPos = arrRetVal.Count;
                arrRetVal.Add(strLine);
                strLine = GetNextNonEmptyLine(reader);
                while (strLine != null && strLine[0] != '[') {
                    arrRetVal.Add(strLine);
                    strLine = GetNextNonEmptyLine(reader);
                }
                if (strLine != null) {
                    m_strPushedLine = strLine;
                }
            }
            if (arrRetVal.Count == 0) {
                arrRetVal = null;
            }
            return(arrRetVal);
        }

        /// <summary>
        /// Get the next PGN game from the file.
        /// </summary>
        /// <param name="reader">           Text reader</param>
        /// <param name="iGameStartPos">    Return the beginning of the game in the string array</param>
        /// <returns>
        /// Array of string describing the game or null if EOF
        /// </returns>
        private List<String> GetPGNGame(TextReader reader, out int iGameStartPos) {
            int     iWhiteELO;
            int     iBlackELO;
            
            return(GetPGNGame(reader, out iGameStartPos, out iWhiteELO, out iBlackELO));
        }

        /// <summary>
        /// Write a PGN game in the specified output stream
        /// </summary>
        /// <param name="writer">           Text writer</param>
        /// <param name="arrGame">          Array of string representing the PGN file</param>
        /// <param name="iGameStartPos">    Beginning of the game in the string array</param>
        private static void WritePGN(TextWriter writer, List<string> arrGame, int iGameStartPos) {
            for (int iIndex = 0; iIndex < iGameStartPos; iIndex++) {
                writer.WriteLine(arrGame[iIndex]);
            }
            writer.WriteLine("");
            for (int iIndex = iGameStartPos; iIndex < arrGame.Count; iIndex++) {
                writer.WriteLine(arrGame[iIndex]);
            }
            writer.WriteLine("");
        }

        /// <summary>
        /// Gets the information about a PGN game
        /// </summary>
        /// <param name="arrGame">          Array of string representing the PGN file</param>
        /// <param name="iGameStartPos">    Beginning of the game in the string array</param>
        /// <param name="strWhitePlayer">   Name of the white player</param>
        /// <param name="strBlackPlayer">   Name of the black player</param>
        /// <param name="strGameResult">    Result of the game</param>
        /// <param name="strGameDate">      Date of the game</param>
        /// <param name="iWhiteELO">        White player ELO</param>
        /// <param name="iBlackELO">        Black player ELO</param>
        private static void GetPGNGameInfo(List<string>    arrGame,
                                           int             iGameStartPos,
                                           out string      strWhitePlayer,
                                           out string      strBlackPlayer,
                                           out string      strGameResult,
                                           out string      strGameDate,
                                           out int         iWhiteELO,
                                           out int         iBlackELO) {
            string  str;
            
            strWhitePlayer  = null; 
            strBlackPlayer  = null;
            strGameResult   = null;
            strGameDate     = null;
            iWhiteELO       = -1;
            iBlackELO       = -1;
            for (int iIndex = 0; iIndex < iGameStartPos; iIndex++) {
                str = arrGame[iIndex];
                if (String.Compare(str, 0, "[Date \"", 0, 7, false) == 0) {
                    strGameDate = ExtractValue(str);
                } else if (String.Compare(str, 0, "[BlackElo \"", 0, 11, false) == 0) {
                    str = ExtractValue(str);
                    if (!Int32.TryParse(str, out iBlackELO)) {
                        iBlackELO = -1;
                    }
                } else if (String.Compare(str, 0, "[WhiteElo \"", 0, 11, false) == 0) {
                    str = ExtractValue(str);
                    if (!Int32.TryParse(str, out iWhiteELO)) {
                        iWhiteELO = -1;
                    }
                } else if (String.Compare(str, 0, "[Black \"", 0, 8, false) == 0) {
                    strBlackPlayer = ExtractValue(str);
                } else if (String.Compare(str, 0, "[White \"", 0, 8, false) == 0) {
                    strWhitePlayer = ExtractValue(str);
                } else if (String.Compare(str, 0, "[Result \"", 0, 9, false) == 0) {
                    strGameResult = ExtractValue(str);
                }
            }
        }

        /// <summary>
        /// Gets the information about a PGN game
        /// </summary>
        /// <param name="arrGame">          Array of string representing the PGN file</param>
        /// <param name="iGameStartPos">    Beginning of the game in the string array</param>
        /// <param name="iWhiteELO">        White player ELO</param>
        /// <param name="iBlackELO">        Black player ELO</param>
        private void GetPGNGameInfo(List<string>    arrGame,
                                    int             iGameStartPos,
                                    out int         iWhiteELO,
                                    out int         iBlackELO) {
            string      strWhitePlayer;
            string      strBlackPlayer;
            string      strGameResult;
            string      strGameDate;

            GetPGNGameInfo(arrGame,
                           iGameStartPos,
                           out strWhitePlayer,
                           out strBlackPlayer,
                           out strGameResult,
                           out strGameDate,
                           out iWhiteELO,
                           out iBlackELO);

        }

        /// <summary>
        /// Scan the PGN stream to retrieve some informations
        /// </summary>
        /// <param name="streamInp">        Stream containing the PGN file</param>
        /// <param name="hashPlayerList">   Hash list which will be filled with the players list</param>
        /// <param name="iMinELO">          Minimum ELO found in the games</param>
        /// <param name="iMaxELO">          Maximum ELO found in the games</param>
        /// <returns>
        /// Number of games found in the stream or -1 if error.
        /// </returns>
        private int ScanPGN(Stream streamInp, Dictionary<String, String> hashPlayerList, ref int iMinELO, ref int iMaxELO) {
            int             iRetVal;
            int             iWhiteELO;
            int             iBlackELO;
            string          strWhitePlayer;
            string          strBlackPlayer;
            string          strGameResult;
            string          strGameDate;
            List<String>    arrPGNGame;
            int             iGameStartPos;
            int             iAvgELO;
            TextReader      reader;
            
            streamInp.Seek(0, SeekOrigin.Begin);
            reader          = new StreamReader(streamInp);
            m_iCurLine      = 0;
            m_strPushedLine = null;
            iRetVal         = 0;
            try {
                arrPGNGame      = GetPGNGame(reader, out iGameStartPos);
                while (arrPGNGame != null) {
                    GetPGNGameInfo(arrPGNGame,
                                   iGameStartPos,
                                   out strWhitePlayer,
                                   out strBlackPlayer,
                                   out strGameResult,
                                   out strGameDate,
                                   out iWhiteELO,
                                   out iBlackELO);
                    if (hashPlayerList != null) {
                        if (strWhitePlayer != null && !hashPlayerList.ContainsKey(strWhitePlayer)) {
                            hashPlayerList.Add(strWhitePlayer, null);
                        }
                        if (strBlackPlayer != null && !hashPlayerList.ContainsKey(strBlackPlayer)) {
                            hashPlayerList.Add(strBlackPlayer, null);
                        }
                    }
                    if (iWhiteELO != -1 && iBlackELO != -1) {
                        iAvgELO = (iWhiteELO + iBlackELO) / 2;
                        if (iAvgELO > iMaxELO) {
                            iMaxELO = iAvgELO;
                        }
                        if (iAvgELO < iMinELO) {
                            iMinELO = iAvgELO;
                        }
                    }
                    iRetVal++;
                    arrPGNGame = GetPGNGame(reader, out iGameStartPos, out iWhiteELO, out iBlackELO);
                }
            } catch(System.Exception exc) {
                MessageBox.Show("Error around the line " + m_iCurLine.ToString() + " while parsing the PGN file.\r\n" + exc.Message);
                iRetVal = -1;
            }
            return(iRetVal);
        }

        /// <summary>
        /// Checks if the specified game must be retained accordingly to the specified filter
        /// </summary>
        /// <param name="arrGame">          Array of string representing the PGN file</param>
        /// <param name="iGameStartPos">    Beginning of the game in the string array</param>
        /// <param name="iAvgELO">          Game average ELO</param>
        /// <param name="filterClause">     Filter clause</param>
        /// <returns>
        /// true if must be retained
        /// </returns>
        private bool IsRetained(List<String> arrGame, int iGameStartPos, int iAvgELO, FilterClause filterClause) {
            bool    bRetVal = true;
            string  strWhitePlayer;
            string  strBlackPlayer;
            string  strGameResult;
            string  strGameDate;
            int     iDummy;
            
            if (iAvgELO == -1) {
                bRetVal = filterClause.m_bIncludesUnrated;
            } else if (filterClause.m_bAllRanges) {
                bRetVal = true;
            } else {
                iAvgELO = iAvgELO / 100 * 100;
                bRetVal = filterClause.m_hashRanges.ContainsKey(iAvgELO);
            }
            if (bRetVal) {
                if (!filterClause.m_bAllPlayers || !filterClause.m_bAllEnding) {
                    GetPGNGameInfo(arrGame,
                                   iGameStartPos,
                                   out strWhitePlayer,
                                   out strBlackPlayer,
                                   out strGameResult,
                                   out strGameDate,
                                   out iDummy,
                                   out iDummy);
                    if (!filterClause.m_bAllPlayers) {
                        if (!filterClause.m_hashPlayerList.ContainsKey(strBlackPlayer) &&
                            !filterClause.m_hashPlayerList.ContainsKey(strWhitePlayer)) {
                            bRetVal = false;
                        }
                    }
                    if (bRetVal && !filterClause.m_bAllEnding) {
                        if (strGameResult == "1-0") {
                            bRetVal = filterClause.m_bEndingWhiteWinning;
                        } else if (strGameResult == "0-1") {
                            bRetVal = filterClause.m_bEndingBlackWinning;
                        } else if (strGameResult == "1/2-1/2") {
                            bRetVal = filterClause.m_bEndingDraws;
                        } else {
                            bRetVal = false;
                        }
                    }
                }                
            }
            return(bRetVal);
        }

        /// <summary>
        /// Filter the content of the PGN file in the input stream to fill the output stream
        /// </summary>
        /// <param name="streamInp">    Inout stream</param>
        /// <param name="streamOut">    Output stream. If null, just run to determine the result count.</param>
        /// <param name="filterClause">     Filter clause</param>
        /// <returns>
        /// Number of resulting games.
        /// </returns>
        public int FilterPGN(Stream streamInp, Stream streamOut, FilterClause filterClause) {
            int             iRetVal;
            TextReader      reader;
            TextWriter      writer;
            List<string>    arrPGNGame;
            int             iGameCount;
            int             iWhiteELO;
            int             iBlackELO;
            int             iGameStartPos;
            int             iAvgElo;
            
            streamInp.Seek(0, SeekOrigin.Begin);
            if (streamOut != null) {
                streamOut.Seek(0, SeekOrigin.Begin);
            }
            reader          = new StreamReader(streamInp);
            writer          = (streamOut == null) ? null : new StreamWriter(streamOut);
            iGameCount      = 0;
            iRetVal         = 0;
            m_iCurLine      = 0;
            m_strPushedLine = null;
            try {
                arrPGNGame      = GetPGNGame(reader, out iGameStartPos, out iWhiteELO, out iBlackELO);
                while (arrPGNGame != null) {
                    iAvgElo = (iWhiteELO != -1 && iBlackELO != -1) ? (iWhiteELO + iBlackELO) / 2 : -1;
                    if (IsRetained(arrPGNGame, iGameStartPos, iAvgElo, filterClause)) {
                        if (writer != null) {
                            WritePGN(writer, arrPGNGame, iGameStartPos);
                        }
                        iRetVal++;
                    }
                    iGameCount++;
                    arrPGNGame = GetPGNGame(reader, out iGameStartPos, out iWhiteELO, out iBlackELO);
                }
            } catch(System.Exception exc) {
                MessageBox.Show("Error around the line " + m_iCurLine.ToString() + " while parsing the PGN file.\r\n" + exc.Message);
                iRetVal = 0;
            }
            if (writer != null) {
                writer.Flush();
            }
            return(iRetVal);
        }

        /// <summary>
        /// Extract the value from a PGN tag
        /// </summary>
        /// <param name="strTag">       Tag</param>
        /// <returns>
        /// Value or '???' if unable to decode.
        /// </returns>
        private static string ExtractValue(string strTag) {
            string  strRetVal = "???";
            int     iStart;
            int     iEnd;
            
            iStart = strTag.IndexOf('"');
            if (iStart != -1) {
                iEnd = strTag.LastIndexOf('"');
                if (iEnd != -1) {
                    strRetVal = strTag.Substring(iStart + 1, iEnd - iStart - 1);
                }
            }
            return(strRetVal);
        }

        /// <summary>
        /// Build a description for the specified game.
        /// </summary>
        /// <param name="arrGame">          Array of string representing the PGN file</param>
        /// <param name="iGameStartPos">    Beginning of the game in the string array</param>
        /// <returns>
        /// Description
        /// </returns>
        public string GetPGNGameDesc(List<string> arrGame, int iGameStartPos) {
            string  strRetVal;
            int     iWhiteELO;
            int     iBlackELO;
            string  strWhitePlayer;
            string  strBlackPlayer;
            string  strGameResult;
            string  strGameDate;
            
            GetPGNGameInfo(arrGame,
                           iGameStartPos,
                           out strWhitePlayer,
                           out strBlackPlayer,
                           out strGameResult,
                           out strGameDate,
                           out iWhiteELO,
                           out iBlackELO);
            strRetVal = ((strWhitePlayer == null) ? "???" : strWhitePlayer) + 
                        " against " + 
                        ((strBlackPlayer == null) ? "???" : strBlackPlayer) + 
                        " (" + 
                        ((iBlackELO == -1) ? "???" : iBlackELO.ToString()) + 
                        " / " + 
                        ((iWhiteELO == -1) ? "???" : iWhiteELO.ToString()) + 
                        ") played on " + 
                        ((strGameDate == null) ? "???" : strGameDate) + 
                        ". Result is " + 
                        ((strGameResult == null) ? "???" : strGameResult);
            return(strRetVal);
        }

        /// <summary>
        /// Creates a new PGN file as a subset of an existing one.
        /// </summary>
        public void CreatePGNSubset() {
            OpenFileDialog              openDlg;
            SaveFileDialog              saveDlg;
            Stream                      streamInp;
            Stream                      streamOut;
            Dictionary<string, string>  hashPlayer;
            String[]                    arrPlayer;
            int                         iMinELO;
            int                         iMaxELO;
            int                         iCount;
            frmPgnFilter                frm;
            PgnUtil.FilterClause        filterClause;
            
            openDlg = new OpenFileDialog();
            openDlg.AddExtension        = true;
            openDlg.CheckFileExists     = true;
            openDlg.CheckPathExists     = true;
            openDlg.DefaultExt          = "pgn";
            openDlg.Filter              = "Chess PGN Files (*.pgn)|*.pgn";
            openDlg.Multiselect         = false;
            openDlg.Title               = "Open Source PGN File"; 
            if (openDlg.ShowDialog() == true) {
                streamInp = OpenInpFile(openDlg.FileName);
                if (streamInp != null) {
                    using(streamInp) {
                        hashPlayer = new Dictionary<string,string>(4096);
                        iMinELO    = Int32.MaxValue;
                        iMaxELO    = Int32.MinValue;
                        iCount     = ScanPGN(streamInp, hashPlayer, ref iMinELO, ref iMaxELO);
                        if (iCount == 0) {
                            MessageBox.Show("No games found in the file.");
                        } else if (iCount != -1) {
                            arrPlayer = new string[hashPlayer.Keys.Count];
                            hashPlayer.Keys.CopyTo(arrPlayer, 0);
                            Array.Sort(arrPlayer);
                            frm = new frmPgnFilter(this, streamInp, iMinELO, iMaxELO, arrPlayer, openDlg.FileName, iCount);
                            if (frm.ShowDialog() == true) {
                                filterClause = frm.FilteringClause;
                                saveDlg = new SaveFileDialog();
                                saveDlg.AddExtension        = true;
                                saveDlg.CheckPathExists     = true;
                                saveDlg.DefaultExt          = "pgn";
                                saveDlg.Filter              = "Chess PGN Files (*.pgn)|*.pgn";
                                saveDlg.OverwritePrompt     = true;
                                saveDlg.Title               = "PGN File to Create";
                                if (saveDlg.ShowDialog() == true) {
                                    streamOut = CreateOutFile(saveDlg.FileName);
                                    if (streamOut != null) {
                                        using(streamOut) {
                                            iCount = FilterPGN(streamInp, streamOut, filterClause);
                                            MessageBox.Show("The file '" + saveDlg.FileName + "' has been created with " + iCount.ToString() + " game(s)");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fill a listbox with the description of the games include in a PGN stream
        /// </summary>
        /// <param name="streamInp">        Stream containing the PGN file</param>
        /// <param name="itemsControl">     Items control</param>
        /// <returns>
        /// Number of games found in the stream or -1 if error.
        /// </returns>
        public int FillListBoxWithDesc(Stream streamInp, ItemsControl itemsControl) {
            int             iRetVal;
            List<String>    arrPGNGame;
            int             iGameStartPos;
            int             iDummy;
            TextReader      reader;
            string          strDesc;
            
            streamInp.Seek(0, SeekOrigin.Begin);
            reader          = new StreamReader(streamInp);
            m_iCurLine      = 0;
            m_strPushedLine = null;
            iRetVal         = 0;
            try {
                arrPGNGame = GetPGNGame(reader, out iGameStartPos);
                while (arrPGNGame != null) {
                    GetPGNGameInfo(arrPGNGame,
                                   iGameStartPos,
                                   out iDummy,
                                   out iDummy);
                    strDesc = (iRetVal + 1).ToString().PadLeft(5, '0') + " - " + GetPGNGameDesc(arrPGNGame, iGameStartPos);
                    itemsControl.Items.Add(new PGNGameDescItem(strDesc, iRetVal));
                    iRetVal++;
                    arrPGNGame = GetPGNGame(reader, out iGameStartPos, out iDummy, out iDummy);
                }
            } catch(System.Exception exc) {
                MessageBox.Show("Error around the line " + m_iCurLine.ToString() + " while parsing the PGN file.\r\n" + exc.Message);
                iRetVal = -1;
            }
            return(iRetVal);
        }

        /// <summary>
        /// Get the Nth game
        /// </summary>
        /// <param name="streamInp">        Stream containing the PGN file</param>
        /// <param name="iIndex">           Game index (0-max)</param>
        /// <returns>
        /// Game or null if error
        /// </returns>
        public string GetNGame(Stream streamInp, int iIndex) {
            StringBuilder   strBuilder;
            List<String>    arrPGNGame;
            int             iGameStartPos;
            TextReader      reader;
            
            streamInp.Seek(0, SeekOrigin.Begin);
            reader          = new StreamReader(streamInp);
            m_iCurLine      = 0;
            m_strPushedLine = null;
            try {
                arrPGNGame      = GetPGNGame(reader, out iGameStartPos);
                while (arrPGNGame != null && iIndex > 0) {
                    arrPGNGame = GetPGNGame(reader, out iGameStartPos);
                    iIndex--;
                }
                if (arrPGNGame == null) {
                    strBuilder = null;
                } else {
                    strBuilder = new StringBuilder(8192);
                    for (int i = 0; i < iGameStartPos; i++) {
                        strBuilder.AppendLine(arrPGNGame[i]);
                    }
                    strBuilder.AppendLine();
                    for (int i = iGameStartPos; i < arrPGNGame.Count; i++) {
                        strBuilder.AppendLine(arrPGNGame[i]);
                    }
                }
            } catch(System.Exception exc) {
                MessageBox.Show("Error around the line " + m_iCurLine.ToString() + " while parsing the PGN file.\r\n" + exc.Message);
                strBuilder = null;
            }
            return((strBuilder == null) ? null : strBuilder.ToString());
        }

        /// <summary>
        /// Gets Square Id from the PGN representation
        /// </summary>
        /// <param name="strMove">  PGN square representation.</param>
        /// <returns>
        /// square id (0-63)
        /// PGN representation
        /// </returns>
        public static int GetSquareIDFromPGN(string strMove) {
            int     iRetVal;
            Char    cChr1;
            Char    cChr2;
            
            if (strMove.Length != 2) {
                iRetVal = -1;
            } else {
                cChr1 = strMove.ToLower()[0];
                cChr2 = strMove[1];
                if (cChr1 < 'a' || cChr1 > 'h' || cChr2 < '1' || cChr2 > '8') {
                    iRetVal = -1;
                } else {
                    iRetVal = 7 - (cChr1 - 'a') + ((cChr2 - '0') << 3);
                }
            }            
            return(iRetVal);
        }

        /// <summary>
        /// Gets the PGN representation of a square
        /// </summary>
        /// <param name="iPos">         Absolute position of the square.</param>
        /// <returns>
        /// PGN representation
        /// </returns>
        public static string GetPGNSquareID(int iPos) {
            string  strRetVal;
            
            strRetVal = ((Char)('a' + 7 - (iPos & 7))).ToString() + ((Char)((iPos >> 3) + '1')).ToString();
            return(strRetVal);
        }

        /// <summary>
        /// Find all moves which end to the same position which can create ambiguity
        /// </summary>
        /// <param name="chessBoard">   Chessboard before the move has been done.</param>
        /// <param name="move">         Move to convert</param>
        /// <param name="eMovePlayer">  Player making the move</param>
        /// <returns>
        /// PGN move
        /// </returns>
        private static PGNAmbiguity FindMoveAmbiguity(ChessBoard chessBoard, ChessBoard.MovePosS move, ChessBoard.PlayerColorE eMovePlayer) {
            PGNAmbiguity                eRetVal = PGNAmbiguity.NotFound;
            ChessBoard.PieceE           ePieceMove;
            List<ChessBoard.MovePosS>   moveList;
            
            moveList   = chessBoard.EnumMoveList(eMovePlayer);
            ePieceMove = chessBoard[move.StartPos];
            foreach (ChessBoard.MovePosS moveTest in moveList) {
                if (moveTest.EndPos == move.EndPos) {
                    if (moveTest.StartPos == move.StartPos) {
                        if (moveTest.Type == move.Type) {
                            eRetVal |= PGNAmbiguity.Found;
                        }
                    } else {
                        if (chessBoard[moveTest.StartPos] == ePieceMove) {
                            if ((moveTest.StartPos & 7) != (move.StartPos & 7)) {
                                eRetVal |= PGNAmbiguity.ColMustBeSpecify;
                            } else {
                                eRetVal |= PGNAmbiguity.RowMustBeSpecify;
                            }
                        }
                    }
                }
            }
            return(eRetVal);
        }

        /// <summary>
        /// Gets a PGN move from a MovePosS structure and a chessboard.
        /// </summary>
        /// <param name="chessBoard">       Chessboard before the move has been done.</param>
        /// <param name="move">             Move to convert</param>
        /// <param name="bIncludeEnding">   true to include ending</param>
        /// <returns>
        /// PGN move
        /// </returns>
        public static string GetPGNMoveFromMove(ChessBoard chessBoard, ChessBoard.MovePosS move, bool bIncludeEnding) {
            string                      strRetVal;
            string                      strStartPos;
            ChessBoard.PieceE           ePiece;
            PGNAmbiguity                eAmbiguity;
            ChessBoard.PlayerColorE     ePlayerToMove;
            
            if (move.Type == ChessBoard.MoveTypeE.Castle) {
                strRetVal = (move.EndPos == 1 || move.EndPos == 57) ? "O-O" : "O-O-O";
            } else {
                ePiece          = chessBoard[move.StartPos] & ChessBoard.PieceE.PieceMask;
                ePlayerToMove   = chessBoard.NextMoveColor;
                eAmbiguity      = FindMoveAmbiguity(chessBoard, move, ePlayerToMove);
                switch(ePiece) {
                case ChessBoard.PieceE.King:
                    strRetVal = "K";
                    break;
                case ChessBoard.PieceE.Queen:
                    strRetVal = "Q";
                    break;
                case ChessBoard.PieceE.Rook:
                    strRetVal = "R";
                    break;
                case ChessBoard.PieceE.Bishop:
                    strRetVal = "B";
                    break;
                case ChessBoard.PieceE.Knight:
                    strRetVal = "N";
                    break;
                case ChessBoard.PieceE.Pawn:
                    strRetVal = "";
                    break;
                default:
                    strRetVal = "";
                    break;
                }
                strStartPos = GetPGNSquareID(move.StartPos);
                if ((eAmbiguity & PGNAmbiguity.ColMustBeSpecify) == PGNAmbiguity.ColMustBeSpecify) {
                    strRetVal += strStartPos[0];
                }
                if ((eAmbiguity & PGNAmbiguity.RowMustBeSpecify) == PGNAmbiguity.RowMustBeSpecify) {
                    strRetVal += strStartPos[1];
                }
                if ((move.Type & ChessBoard.MoveTypeE.PieceEaten) == ChessBoard.MoveTypeE.PieceEaten) {
                    if (ePiece == ChessBoard.PieceE.Pawn && 
                        (eAmbiguity & PGNAmbiguity.ColMustBeSpecify) == (PGNAmbiguity)0 &&
                        (eAmbiguity & PGNAmbiguity.RowMustBeSpecify) == (PGNAmbiguity)0) {
                        strRetVal += strStartPos[0];
                    }
                    strRetVal += 'x';
                }
                strRetVal += GetPGNSquareID(move.EndPos);
                switch(move.Type & ChessBoard.MoveTypeE.MoveTypeMask) {
                case ChessBoard.MoveTypeE.PawnPromotionToQueen:
                    strRetVal += "=Q";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToRook:
                    strRetVal += "=R";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToBishop:
                    strRetVal += "=B";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToKnight:
                    strRetVal += "=N";
                    break;
                case ChessBoard.MoveTypeE.PawnPromotionToPawn:
                    strRetVal += "=P";
                    break;
                default:
                    break;
                }
            }
            chessBoard.DoMoveNoLog(move);
            switch(chessBoard.CheckNextMove()) {
            case ChessBoard.MoveResultE.NoRepeat:
                break;
            case ChessBoard.MoveResultE.Check:
                strRetVal += "+";
                break;
            case ChessBoard.MoveResultE.Mate:
                strRetVal += "#";
                if (bIncludeEnding) {
                    if (chessBoard.NextMoveColor == ChessBoard.PlayerColorE.Black) {
                        strRetVal += " 1-0";
                    } else {
                        strRetVal += " 0-1";
                    }
                }
                break;
            case ChessBoard.MoveResultE.ThreeFoldRepeat:
            case ChessBoard.MoveResultE.FiftyRuleRepeat:
            case ChessBoard.MoveResultE.TieNoMove:
            case ChessBoard.MoveResultE.TieNoMatePossible:
                if (bIncludeEnding) {
                    strRetVal += " 1/2-1/2";
                }
                break;
            default:
                break;
            }
            chessBoard.UndoMoveNoLog(move);
            return(strRetVal);
        }

        /// <summary>
        /// Generates FEN
        /// </summary>
        /// <param name="chessBoard">       Actual chess board (after the move)</param>
        /// <returns>
        /// PGN representation of the game
        /// </returns>
        public static string GetFENFromBoard(ChessBoard chessBoard) {
            StringBuilder               strBuilder;
            int                         iEmptyCount;
            ChessBoard.PieceE           ePiece;
            Char                        cPiece;
            ChessBoard.PlayerColorE     eNextMoveColor;
            ChessBoard.BoardStateMaskE  eBoardStateMask;
            int                         iEnPassant;
            bool                        bCastling;
            
            strBuilder      = new StringBuilder(512);
            eNextMoveColor  = chessBoard.NextMoveColor;
            eBoardStateMask = chessBoard.ComputeBoardExtraInfo(eNextMoveColor, false);
            iEnPassant      = (int)(eBoardStateMask & ChessBoard.BoardStateMaskE.EnPassant);
            for (int iRow = 7; iRow >= 0; iRow--) {
                iEmptyCount = 0;
                for (int iCol = 7; iCol >= 0; iCol--) {
                    ePiece = chessBoard[(iRow << 3) + iCol];
                    if (ePiece == ChessBoard.PieceE.None) {
                        iEmptyCount++;
                    } else {
                        if (iEmptyCount != 0) {
                            strBuilder.Append(iEmptyCount.ToString());
                            iEmptyCount = 0;
                        }
                        switch(ePiece & ChessBoard.PieceE.PieceMask) {
                        case ChessBoard.PieceE.King:
                            cPiece = 'K';
                            break;
                        case ChessBoard.PieceE.Queen:
                            cPiece = 'Q';
                            break;
                        case ChessBoard.PieceE.Rook:
                            cPiece = 'R';
                            break;
                        case ChessBoard.PieceE.Bishop:
                            cPiece = 'B';
                            break;
                        case ChessBoard.PieceE.Knight:
                            cPiece = 'N';
                            break;
                        case ChessBoard.PieceE.Pawn:
                            cPiece = 'P';
                            break;
                        default:
                            cPiece = '?';
                            break;
                        }
                        if ((ePiece & ChessBoard.PieceE.Black) == ChessBoard.PieceE.Black) {
                            cPiece = Char.ToLower(cPiece);
                        }
                        strBuilder.Append(cPiece);
                    }
                }
                if (iEmptyCount != 0) {
                    strBuilder.Append(iEmptyCount.ToString());
                }
                if (iRow != 0) {
                    strBuilder.Append('/');
                }
            }
            strBuilder.Append(' ');
            strBuilder.Append((eNextMoveColor == ChessBoard.PlayerColorE.White) ? 'w' : 'b');
            strBuilder.Append(' ');
            bCastling = false;
            if ((eBoardStateMask & ChessBoard.BoardStateMaskE.WRCastling) == ChessBoard.BoardStateMaskE.WRCastling) {
                strBuilder.Append('K');
                bCastling = true;
            }
            if ((eBoardStateMask & ChessBoard.BoardStateMaskE.WLCastling) == ChessBoard.BoardStateMaskE.WLCastling) {
                strBuilder.Append('Q');
                bCastling = true;
            }
            if ((eBoardStateMask & ChessBoard.BoardStateMaskE.BRCastling) == ChessBoard.BoardStateMaskE.BRCastling) {
                strBuilder.Append('k');
                bCastling = true;
            }
            if ((eBoardStateMask & ChessBoard.BoardStateMaskE.BLCastling) == ChessBoard.BoardStateMaskE.BLCastling) {
                strBuilder.Append('q');
                bCastling = true;
            }
            if (!bCastling) {
                strBuilder.Append('-');
            }
            strBuilder.Append(' ');
            if (iEnPassant == 0) {
                strBuilder.Append('-');
            } else {
                strBuilder.Append(GetPGNSquareID(iEnPassant));
            }
            strBuilder.Append(" 0 1");
            return(strBuilder.ToString());
        }

        /// <summary>
        /// Generates the PGN representation of the board
        /// </summary>
        /// <param name="chessBoard">       Actual chess board (after the move)</param>
        /// <param name="bIncludeRedoMove"> true to include redo move</param>
        /// <param name="strEvent">         Event tag</param>
        /// <param name="strSite">          Site tag</param>
        /// <param name="strDate">          Date tag</param>
        /// <param name="strRound">         Round tag</param>
        /// <param name="strWhitePlayer">   White player's name</param>
        /// <param name="strBlackPlayer">   Black player's name</param>
        /// <param name="eWhitePlayerType"> White player's type</param>
        /// <param name="eBlackPlayerType"> Black player's type</param>
        /// <param name="spanWhitePlayer">  Timer for the white</param>
        /// <param name="spanBlackPlayer">  Timer for the black</param>
        /// <returns>
        /// PGN representation of the game
        /// </returns>
        public static string GetPGNFromBoard(ChessBoard             chessBoard,
                                             bool                   bIncludeRedoMove,
                                             string                 strEvent,
                                             string                 strSite,
                                             string                 strDate,
                                             string                 strRound,
                                             string                 strWhitePlayer,
                                             string                 strBlackPlayer,
                                             PgnParser.PlayerTypeE  eWhitePlayerType,
                                             PgnParser.PlayerTypeE  eBlackPlayerType,
                                             TimeSpan               spanWhitePlayer,
                                             TimeSpan               spanBlackPlayer) {
            int                 iMoveIndex;
            StringBuilder       strBuilder;
            StringBuilder       strBuilderLine;
            int                 iOriIndex;
            int                 iMoveCount;
            MovePosStack        movePosStack;
            ChessBoard.MovePosS movePos;
            string              strResult;
            
            movePosStack    = chessBoard.MovePosStack;
            iOriIndex       = movePosStack.PositionInList;
            iMoveCount      = (bIncludeRedoMove) ? movePosStack.Count : iOriIndex + 1;
            strBuilder      = new StringBuilder(10 * iMoveCount + 256);
            strBuilderLine  = new StringBuilder(256);
            switch(chessBoard.CheckNextMove()) {
            case ChessBoard.MoveResultE.Check:
            case ChessBoard.MoveResultE.NoRepeat:
                strResult = "*";
                break;
            case ChessBoard.MoveResultE.Mate:
                strResult = (chessBoard.NextMoveColor == ChessBoard.PlayerColorE.White) ? "0-1" : "1-0";
                break;
            case ChessBoard.MoveResultE.FiftyRuleRepeat:
            case ChessBoard.MoveResultE.ThreeFoldRepeat:
            case ChessBoard.MoveResultE.TieNoMove:
            case ChessBoard.MoveResultE.TieNoMatePossible:
                strResult = "1/2-1/2";
                break;
            default:
                strResult = "*";
                break;
            }
            chessBoard.UndoAllMoves();
            strBuilder.Append("[Event \"" + strEvent + "\"]\n");
            strBuilder.Append("[Site \"" + strSite + "\"]\n");
            strBuilder.Append("[Date \"" + strDate + "\"]\n");
            strBuilder.Append("[Round \"" + strRound + "\"]\n");
            strBuilder.Append("[White \"" + strWhitePlayer + "\"]\n");
            strBuilder.Append("[Black \"" + strBlackPlayer + "\"]\n");
            strBuilder.Append("[Result \"" + strResult + "\"]\n");
            if (!chessBoard.StandardInitialBoard) {
                strBuilder.Append("[SetUp \"1\"]\n");
                strBuilder.Append("[FEN \"" + GetFENFromBoard(chessBoard) + "\"]\n");
            }
            strBuilder.Append("[WhiteType \"" + ((eWhitePlayerType == PgnParser.PlayerTypeE.Human) ? "human" : "program") + "\"]\n");
            strBuilder.Append("[BlackType \"" + ((eBlackPlayerType == PgnParser.PlayerTypeE.Human) ? "human" : "program") + "\"]\n");
            strBuilder.Append("[TimeControl \"?:" + spanWhitePlayer.Ticks.ToString() + ":" + spanBlackPlayer.Ticks.ToString() + "\"]\n");
            strBuilder.Append('\n');
            iMoveIndex              = 0;
            strBuilderLine.Length   = 0;
            for (iMoveIndex = 0; iMoveIndex < iMoveCount; iMoveIndex++) {
                if (strBuilderLine.Length > 60) {
                    strBuilder.Append(strBuilderLine);
                    strBuilder.Append("\n");
                    strBuilderLine.Length = 0;
                }
                movePos = movePosStack[iMoveIndex];
                if ((iMoveIndex & 1) == 0) {
                    strBuilderLine.Append(((iMoveIndex + 1) / 2 + 1).ToString());
                    strBuilderLine.Append(". ");
                }
                strBuilderLine.Append(GetPGNMoveFromMove(chessBoard, movePos, true) + " ");
                chessBoard.RedoMove();
            }
            strBuilder.Append(strBuilderLine);
            strBuilder.Append('\n');
            return(strBuilder.ToString());
        }

        /// <summary>
        /// Generates the PGN representation of a series of moves
        /// </summary>
        /// <param name="chessBoard">   Actual chess board.</param>
        /// <returns>
        /// PGN representation of the game
        /// </returns>
        public static string[] GetPGNArrayFromMoveList(ChessBoard chessBoard) {
            string[]        arrRetVal;
            int             iOriPos;
            int             iMoveIndex;
            MovePosStack    moveStack;
            
            iOriPos      = chessBoard.MovePosStack.PositionInList;
            chessBoard.UndoAllMoves();
            moveStack    = chessBoard.MovePosStack;
            arrRetVal    = new string[moveStack.Count];
            iMoveIndex   = 0;
            foreach (ChessBoard.MovePosS movePos in moveStack.List) {
                arrRetVal[iMoveIndex++] = GetPGNMoveFromMove(chessBoard, movePos, false);
                chessBoard.RedoMove();
            }
            chessBoard.SetUndoRedoPosition(iOriPos);
            return(arrRetVal);
        }
    } // Class PgnUtil
} // Namespace
