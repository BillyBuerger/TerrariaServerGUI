using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;
using System.Xml;

using Microsoft.Win32;

namespace TerrariaServerGUI
{
   /// <summary>
   /// 
   /// </summary>
   public class TerrariaServerVM : INotifyPropertyChanged
   {
      /// <summary>
      /// The server controller
      /// </summary>
      private Controller TServerControl;

      /// <summary>
      /// 
      /// </summary>
      public string LogFileName;

      /// <summary>
      /// List of messages from the controller
      /// </summary>
      public ObservableCollection<Message> Messages;

      /// <summary>
      /// List of current players
      /// </summary>
      public ObservableCollection<Player> Players;

      /// <summary>
      /// 
      /// </summary>
      public World CurrentWorld;

      /// <summary>
      /// Need the dispatcher from the UI thread in order to safely pass along
      /// messages from the server thread.  Not sure of a better way to deal
      /// with this.
      /// </summary>
      public Dispatcher Dispatcher;

      /// <summary>
      /// 
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      /// <summary>
      /// 
      /// </summary>
      public event EventHandler ServerClosed;

      /// <summary>
      /// 
      /// </summary>
      public TerrariaServerVM(Dispatcher Dispatcher)
      {
         // Assign the UI dispatcher
         this.Dispatcher = Dispatcher;

         // Create our list of messages
         Messages = new ObservableCollection<Message>();
         Players = new ObservableCollection<Player>();

         // Create our Log file
         LogFileName = string.Format("{0}\\TerrariaServerLog.txt", System.Environment.ExpandEnvironmentVariables("%TEMP%"));
      }

      /// <summary>
      /// This actually fires before the collection is updated.  Or at leat with binding, it happens before 
      /// the binding causing the list view to update.  So passing this event to our UI to handle stuff
      /// after the collection has been updated doesn't work.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         //if (PropertyChanged != null)
         //{
         //   PropertyChanged(this, new PropertyChangedEventArgs("Messages"));
         //}
      }

      public void StartServer()
      {
         string ServerPath = ".\\";

         // First, check in the current working directory
         if (!File.Exists(ServerPath + "TerrariaServer.exe"))
         {
            ServerPath = "";

            // TODO: Load server path from config if already set previously

            if (ServerPath == "")
            {
               // No configured path, try to detect it in the normal locations
               string ProgramX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

               if (Directory.Exists(ProgramX86 + "\\Steam\\SteamApps\\common\\Terraria\\"))
               {
                  ServerPath = ProgramX86 + "\\Steam\\SteamApps\\common\\Terraria\\";
               }
               else
               {
                  ServerPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "").ToString();

                  if (!String.IsNullOrEmpty(ServerPath))
                  {
                     ServerPath = ServerPath.Replace("/", "\\") + "\\SteamApps\\common\\Terraria\\";
                  }
               }

               if (!File.Exists(ServerPath + "TerrariaServer.exe"))
               {
                  ServerPath = "";
               }
            }
         }

         if (ServerPath == "")
         {
            // TODO: Server path not found, prompt for it
         }

         if (ServerPath != "")
         {
            // Start the server
            TServerControl = new Controller();
            TServerControl.ProgressChanged += TServerControl_ProgressChanged;
            TServerControl.PlayerJoined += TServerControl_PlayerJoined;
            TServerControl.PlayerLeft += TServerControl_PlayerLeft;
            TServerControl.Completed += TServerControl_Completed;
            TServerControl.Init();

            if (ServerPath != ".\\")
            {
               TServerControl.WorkingPath = ServerPath;
            }

            TServerControl.FileName = "TerrariaServer.exe";
            TServerControl.DoJobAsync();
         }
         else
         {
            throw (new Exception("Server not found"));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void StopServer()
      {
         if (TServerControl != null)
         {
            if (TServerControl.Running)
            {
               TServerControl.Command = "exit";
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void KillServer()
      {
         if (TServerControl != null)
         {
            TServerControl.KillServer();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="CommandText"></param>
      public void ProcessCommand(string CommandText)
      {
         if (CommandText.Trim() != "" && CommandText.Trim() != ":")
         {
            AddMessage(": " + CommandText);
         }

         TServerControl.ExecuteCommand(CommandText);
      }

      /// <summary>
      /// 
      /// </summary>
      void TServerControl_Completed()
      {
         Dispatcher.BeginInvoke(DispatcherPriority.Background, new ParameterizedThreadStart(SetServerClosed), null);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="PlayerName"></param>
      private void TServerControl_PlayerJoined(string PlayerName)
      {
         Dispatcher.BeginInvoke(DispatcherPriority.Background, new ParameterizedThreadStart(PlayerJoined), PlayerName);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="PlayerName"></param>
      private void TServerControl_PlayerLeft(string PlayerName)
      {
         Dispatcher.BeginInvoke(DispatcherPriority.Background, new ParameterizedThreadStart(PlayerLeft), PlayerName);
      }

      /// <summary>
      /// 
      /// </summary>
      void TServerControl_ProgressChanged()
      {
         Dispatcher.BeginInvoke(DispatcherPriority.Background, new ParameterizedThreadStart(AddMessage), TServerControl.Buffer);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      private void SetServerClosed(object data)
      {
         AddMessage("Server terminated");
         AddMessage("");

         // Notify view that the server is closed
         if (ServerClosed != null)
         {
            ServerClosed(this, null);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool ServerRunning
      {
         get
         {
            return (TServerControl.Running);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="User"></param>
      /// <param name="Message"></param>
      private void AddMessage(object data)
      {
         string MessageText = data.ToString();
         Message NewMessage = new Message(MessageText);

         StreamWriter LogFile = File.AppendText(LogFileName);
         try
         {
            LogFile.WriteLine("{0} {1}\t{2}", NewMessage.Time.ToShortDateString(), NewMessage.Time.ToLongTimeString(), MessageText);
         }
         finally
         {
            LogFile.Close();
         }

         // Look for messages reporting a status such as saving.  Update the message with the new
         // percent complete so that we have just the one line in the list
         bool UpdateLast = false;

         if (Messages.Count > 0 && NewMessage.User == "")
         {
            Message LastMessage = Messages.Last();
            Match PercentMatch = Regex.Match(NewMessage.Text, @"(.*)(\s[0-9\.]+%)");
            Match LastMatch = Regex.Match(LastMessage.Text, @"(.*)(\s[0-9\.]+%)");

            if (PercentMatch.Success && LastMatch.Success)
            {
               // Look for a second matching percentage
               Match PercentMatch2 = Regex.Match(PercentMatch.Groups[1].Value, @"([0-9\.]+%)(.*)");
               Match LastMatch2 = Regex.Match(LastMatch.Groups[1].Value, @"([0-9\.]+%)(.*)");

               if (PercentMatch2.Success && LastMatch2.Success)
               {
                  if (PercentMatch2.Groups[2].Value == LastMatch2.Groups[2].Value)
                  {
                     // Update the last item
                     LastMessage.Text = NewMessage.Text;
                     UpdateLast = true;
                  }
               }
               else
               {
                  if (PercentMatch.Groups[1].Value == LastMatch.Groups[1].Value)
                  {
                     // Update the last item
                     LastMessage.Text = NewMessage.Text;
                     UpdateLast = true;
                  }
               }
            }
         }

         if (!UpdateLast)
         {
            Messages.Add(NewMessage);
         }

         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs("Messages"));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      private void PlayerJoined(object data)
      {
         string PlayerName = data.ToString();
         bool Found = false;

         foreach (Player User in Players)
         {
            if (User.Name.Equals(PlayerName))
            {
               Found = true;
            }
         }

         if (!Found)
         {
            Players.Add(new Player(PlayerName, string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString())));

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Players"));
            }

            WritePlayersLog();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      private void PlayerLeft(object data)
      {
         string PlayerName = data.ToString();
         Player Found = null;

         foreach (Player User in Players)
         {
            if (User.Name.Equals(PlayerName))
            {
               Found = User;
            }
         }

         if (Found != null)
         {
            Players.Remove(Found);

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Players"));
            }

            WritePlayersLog();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private void WritePlayersLog()
      {
         XmlDocument LogDoc = new XmlDocument();
         XmlDeclaration Dec = LogDoc.CreateXmlDeclaration("1.0", "utf-8", "");
         //XmlProcessingInstruction PI = LogDoc.CreateProcessingInstruction("xml-stylesheet", "href=\"gconflate_log.xsl\" type=\"text/xsl\"");
         LogDoc.AppendChild(Dec);
         //LogDoc.AppendChild(PI);

         // Create our main nodes
         XmlElement RootNode = LogDoc.CreateElement("Players");

         foreach (Player User in Players)
         {
            XmlElement PlayerNode = LogDoc.CreateElement("Player");
            PlayerNode.SetAttribute("Name", User.Name);
            PlayerNode.SetAttribute("Joined", User.Joined);
            RootNode.AppendChild(PlayerNode);
         }

         LogDoc.AppendChild(RootNode);

         LogDoc.Save("TerrariaServer_Playing.xml");
      }

      /// <summary>
      /// 
      /// </summary>
      private void AppendMessagesLog()
      {
         //XmlDocument LogDoc = new XmlDocument();

         //XmlDeclaration Dec = LogDoc.CreateXmlDeclaration("1.0", "utf-8", "");
         ////XmlProcessingInstruction PI = LogDoc.CreateProcessingInstruction("xml-stylesheet", "href=\"gconflate_log.xsl\" type=\"text/xsl\"");
         //LogDoc.AppendChild(Dec);
         ////LogDoc.AppendChild(PI);

         //// Create our main nodes
         //XmlElement RootNode = LogDoc.CreateElement("Players");

         //foreach (Player User in Players)
         //{
         //   XmlElement PlayerNode = LogDoc.CreateElement("Player");
         //   PlayerNode.SetAttribute("Name", User.Name);
         //   PlayerNode.SetAttribute("Joined", User.Joined);
         //   RootNode.AppendChild(PlayerNode);
         //}

         //LogDoc.AppendChild(RootNode);

         //LogDoc.Save("TerrariaServer_Messages.xml");
      }
   }
}
