using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace HelperMethods
{
    public class ProgramDifferences
    {
        #region Fields

        private string response;
        private string compiledOn;
        private string sourceFile;
        private string tempSourceFile;
        private string tempCompiledOn;
        private string sourceFileName = @"\NVRAM\SourceFile.txt";
        private string compiledFileName = @"\NVRAM\CompiledDate.txt";

        private bool programIsNew;

        private int programNumber;

        #endregion

        #region Properties

        public bool Debug { get; set; }
        /// <summary>
        /// Returns If program is new or not.
        /// </summary>
        public bool ProgramIsNew { get { return programIsNew; } }
        /// <summary>
        /// Returns the compiled on Date
        /// </summary>
        public string CompiledDate { get { return compiledOn; } }
        /// <summary>
        /// Will return the current source file
        /// </summary>
        public string SourceFile { get { return sourceFile; } }

        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public ProgramDifferences(int program)
        {
            try
            {
                programNumber = program;

                if (File.Exists(sourceFileName))
                {
                    using (StreamReader sr = new StreamReader(sourceFileName))
                    {
                        sourceFile = sr.ReadToEnd();
                    }
                }
                else
                {
                    sourceFile = "";
                }
                if (File.Exists(compiledFileName))
                {
                    using (StreamReader sr = new StreamReader(compiledFileName))
                    {
                        compiledOn = sr.ReadToEnd();
                    }
                }
                else
                {
                    compiledOn = "";
                }
            }
            catch (Exception e)
            {
                SendDebug("\n Program Difference Error in Constructor is: " + e);
            }

        }

        #endregion

        #region Internal Methods

        private void SendDebug(string message)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("Program Difference Debug message is: " + message);
                ErrorLog.Error("Program Difference Error message is: " + message);
            }
        }

        private void ParseConsoleResponse()
        {
            try
            {
                if (response.Length > 0)
                {
                    tempSourceFile = GetParsedData("Source File: ",tempSourceFile);
                    SendDebug("Source file is " + sourceFile + " . and the temp is " + tempSourceFile);

                    tempCompiledOn = GetParsedData("Compiled On: ",tempCompiledOn);
                    SendDebug("Compiled Date is " + compiledOn + ". and the temp is " + tempCompiledOn);

                    if (tempSourceFile != sourceFile )
                    {
                        sourceFile = tempSourceFile;
                        programIsNew = true;
                    }
                    else if (tempCompiledOn != compiledOn)
                    {
                        compiledOn = tempCompiledOn;
                        programIsNew = true;
                    }
                    else
                    {
                        programIsNew = false;
                    }

                    WriteToFile(sourceFileName, sourceFile);
                    WriteToFile(compiledFileName, compiledOn);

               }
            }
            catch (Exception e)
            {
                SendDebug("\n ProgramDifferences error in parseConsoleResponse is: " + e);
            }


        }

        private string GetParsedData(string findString, string lockString )
        {
            try
            {
                    int pos1, pos2;
                    SendDebug("\n Response is: " + response);
                    if (response.Contains(findString))
                    {
                        pos1 = response.IndexOf(findString);
                        pos2 = response.IndexOf('\x0d', pos1 + 1);

                        if (pos1 > 0 && pos2 > 0)
                            return response.Substring(pos1, pos2 - pos1);
                        else
                            return "";
                    }
                    else
                        return "";
            }
            catch (Exception e)
            {
                SendDebug("\n Program Difference Error in GetParsedData() is: " + e);
                return "";
            }
        }

        private void WriteToFile(string fileName, string data)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    StreamReader sr = new StreamReader(fs);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        fs.SetLength(0);

                        sw.Write(data);
                    }
                }
            }
            catch (Exception e)
            {
                SendDebug("\n Error in Write to File is: " + e.Message);
            }

        }

        #endregion

        #region Public Methods

        public bool GetProgramData()
        {
            try
            {
                bool sent;
                string commandString = "PROGCOMMENTS:" + programNumber.ToString() + "\n";
                SendDebug("GetProgramData command being sent is: " + commandString);
                sent = CrestronConsole.SendControlSystemCommand(commandString, ref response);

                if (sent)
                    ParseConsoleResponse();
                else
                {
                    SendDebug("\n Program Differences error did not succesfully send GetprogramData()");
                    return false;
                }

                return programIsNew;

            }
            catch (Exception e)
            {
                SendDebug("\n Program Differences error is: " + e);
                return false;
            }

        }

        #endregion
    }
}