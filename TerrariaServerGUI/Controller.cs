using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace TerrariaServerGUI
{
   class Controller : IDisposable
   {
      private bool disposed = false;
      private bool ForeExit = false;

      private Process pController;

      private string sWorkingPath = string.Empty;
      private string sFileName = string.Empty;

      private BackgroundWorker bwHelper = new BackgroundWorker();
      private bool IsRunning = false;

      /// <summary>
      /// The command process where the server actually runs
      /// </summary>
      private ProcessIoManager ProcMan;

      private string BufferOut = string.Empty;
      private string ServerVersionText = string.Empty;
      private bool InWorldSelectBlock = false;
      private List<World> WorldOptions;

      /// <summary>
      /// 
      /// </summary>
      public string WorkingPath
      {
         set { this.sWorkingPath = value; }
      }

      /// <summary>
      /// 
      /// </summary>
      public string FileName
      {
         set { this.sFileName = this.sWorkingPath + value; }
      }

      /// <summary>
      /// Turn this into a method.  I don't like having assignments run commands
      /// </summary>
      public string Command
      {
         set
         {
            if (pController != null && this.IsRunning)
            {
               ProcMan.WriteStdin(value);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string Buffer
      {
         get { return BufferOut; }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsBusy
      {
         get { return bwHelper.IsBusy; }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool Running
      {
         get { return IsRunning; }
      }

      /// <summary>
      /// 
      /// </summary>
      public delegate void EventHandler();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="User"></param>
      public delegate void PlayerEventHandler(string PlayerName);

      /// <summary>
      /// 
      /// </summary>
      public event EventHandler Completed;

      /// <summary>
      /// 
      /// </summary>
      public event PlayerEventHandler PlayerJoined;

      /// <summary>
      /// 
      /// </summary>
      public event PlayerEventHandler PlayerLeft;

      /// <summary>
      /// 
      /// </summary>
      public event EventHandler ProgressChanged;

      /// <summary>
      /// 
      /// </summary>
      private void SetCompleted()
      {
         if (Completed != null && !this.ForeExit)
         {
            Completed();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void SetPlayerJoined(string PlayerName)
      {
         if (PlayerJoined != null && !ForeExit)
         {
            PlayerJoined(PlayerName);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void SetPlayerLeft(string PlayerName)
      {
         if (PlayerLeft != null && !ForeExit)
         {
            PlayerLeft(PlayerName);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void SetProgressChanged()
      {
         if (ProgressChanged != null && !ForeExit)
         {
            ProgressChanged();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bwHelper_DoWork(object sender, DoWorkEventArgs e)
      {
         DoJob();
      }

      /// <summary>
      /// report ui new data avaible
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bwHelper_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         SetCompleted();
      }

      /// <summary>
      /// 
      /// </summary>
      public Controller()
      {
         bwHelper.WorkerReportsProgress = true;
         bwHelper.DoWork += new DoWorkEventHandler(bwHelper_DoWork);
         bwHelper.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwHelper_RunWorkerCompleted);

         ProcMan = null;
      }

      /// <summary>
      /// 
      /// </summary>
      public void Init()
      {
         ProcMan = null;
      }

      /// <summary>
      /// 
      /// </summary>
      public void DoJobAsync()
      {
         if (!bwHelper.IsBusy)
         {
            bwHelper.RunWorkerAsync();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void DoJob()
      {
         this.ProcessController();
      }

      /// <summary>
      /// Execute a command on the server by writing to the standard input
      /// </summary>
      public void ExecuteCommand(string CommandText)
      {
         if (pController != null && IsRunning)
         {
            ProcMan.WriteStdin(CommandText);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void ProcessController()
      {
         WorldOptions = new List<World>();

         using (pController = new Process())
         {
            pController.EnableRaisingEvents = true;
            pController.StartInfo.UseShellExecute = false;
            pController.StartInfo.RedirectStandardError = true;
            pController.StartInfo.RedirectStandardOutput = true;
            pController.StartInfo.RedirectStandardInput = true;
            pController.StartInfo.CreateNoWindow = true;
            pController.StartInfo.FileName = sFileName;
            pController.StartInfo.WorkingDirectory = sWorkingPath;
            pController.Exited += new System.EventHandler(pController_Exited);
            pController.Start();

            this.IsRunning = true;

            // Create our process manager to handle the input/output to the server application
            ProcMan = new ProcessIoManager(pController);
            ProcMan.StderrTextRead += ProcMan_StderrTextRead;
            ProcMan.StdoutTextRead += ProcMan_StdoutTextRead;

            // Start the individual threads to monitor process text output
            ProcMan.StartProcessOutputRead();

            // Wait for the process to finish
            pController.WaitForExit();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      private void ProcMan_StdoutTextRead(string text)
      {
         if (text != null)
         {
            BufferOut = text;

            // The server version line is reported as one of the first things and is then
            // repeated a couple times.  We only care about the first one so ignore the rest
            if (ServerVersionText == "")
            {
               if (BufferOut.IndexOf("terraria server v", StringComparison.CurrentCultureIgnoreCase) >= 0)
               {
                  ServerVersionText = BufferOut;
                  InWorldSelectBlock = true;
               }
            }
            else if (BufferOut.Equals(ServerVersionText, StringComparison.CurrentCultureIgnoreCase))
            {
               // empty out duplicate version lines
               BufferOut = "";
            }

            if (InWorldSelectBlock)
            {
               if (BufferOut.IndexOf("Choose World:", StringComparison.CurrentCultureIgnoreCase) >= 0)
               {
                  InWorldSelectBlock = false;
               }
               else
               {
                  Match WorldMatch = Regex.Match(BufferOut, @"([1-9nd]+)\s+(\<number\>)?\s+(.+)");

                  if (WorldMatch.Success)
                  {
                     string WorldNum = WorldMatch.Groups[0].Value;
                     string WorldName = WorldMatch.Groups[2].Value;

                     if (WorldNum == "n")
                     {
                        // New world
                     }
                     else if (WorldNum == "d")
                     {
                        // Delete world
                     }
                     else
                     {
                        int Num;

                        if (int.TryParse(WorldNum, out Num))
                        {
                           World WorldOption = new World(Num, WorldName, "", 0, 0);
                           WorldOptions.Add(WorldOption);
                        }
                     }
                  }
               }
            }
            else
            {
               // Look for "<user> has joined." and add user to the current playing list
               Match UserAddMatch = Regex.Match(BufferOut, "(.+) has joined.");

               if (UserAddMatch.Success)
               {
                  SetPlayerJoined(UserAddMatch.Groups[1].Value);
               }

               // Look for "<user> has left." and remove from current playing list
               Match UserRemoveMatch = Regex.Match(BufferOut, "(.+) has left.");

               if (UserRemoveMatch.Success)
               {
                  SetPlayerLeft(UserRemoveMatch.Groups[1].Value);
               }
            }

            // Trim our output and if it's not empty, report an updated buffer
            // Since : is the command line prompt, ignore any lines that are
            // just this prompt
            BufferOut = BufferOut.Trim();

            if (BufferOut.Length > 0 && BufferOut != ":")
            {
               SetProgressChanged();
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      private void ProcMan_StderrTextRead(string text)
      {
         if (text != null)
         {
            this.BufferOut = (string.Format("ERROR: {0}", text));
            this.SetProgressChanged();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pController_Exited(object sender, EventArgs e)
      {
         ProcMan.StopMonitoringProcessOutput();
         this.IsRunning = false;
      }

      /// <summary>
      /// 
      /// </summary>
      public void RequestExit()
      {
         if (this.IsRunning)
         {
            this.Command = "1";

            Thread.Sleep(100);

            for (int i = 0; i < 5; i++)
            {
               this.Command = "";
               Thread.Sleep(100);
            }

            this.Command = "exit-nosave";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void KillServer()
      {
         pController.Kill();
      }

      /// <summary>
      /// Release unmanaged resources.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            if (disposing)
            {
               // Free other state (managed objects).
            }

            // Release unmanaged resources.
            // Set large fields to null.
            // Call Dispose on your base class.

            pController.Dispose();

            disposed = true;
         }
      }
   }
}
