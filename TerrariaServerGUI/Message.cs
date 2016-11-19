using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TerrariaServerGUI
{
   /// <summary>
   /// 
   /// </summary>
   public class Message : INotifyPropertyChanged
   {
      /// <summary>
      /// 
      /// </summary>
      public DateTime _Time;

      public DateTime Time
      {
         get
         {
            return (_Time);
         }

         set
         {
            _Time = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Time"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string _User;

      /// <summary>
      /// 
      /// </summary>
      public string User
      {
         get
         {
            return (_User);
         }

         set
         {
            _User = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("User"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string _Text;

      public string Text
      {
         get
         {
            return (_Text);
         }

         set
         {
            _Text = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Message()
      {
         _Time = DateTime.Now;
         _User = "";
         _Text = "";
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="Message"></param>
      public Message(string MessageText)
      {
         _Time = DateTime.Now;
         _User = "";
         _Text = "";

         // Look for a player name at the start of the message
         Match UserMatch = Regex.Match(MessageText, "^<(.+)>(.*)");

         if (UserMatch.Success)
         {
            _User = UserMatch.Groups[1].Value;
            _Text = UserMatch.Groups[2].Value;
         }
         else
         {
            _Text = MessageText;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="User"></param>
      /// <param name="Message"></param>
      public Message(string User, string MessageText)
      {
         _Time = DateTime.Now;
         _User = User;
         _Text = MessageText;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return (string.Format("{0} {1}: {2}{3}", _Time.ToShortDateString(), _Time.ToShortTimeString(), _User == "" ? "" : _User + ": ", _Text));
      }

      /// <summary>
      /// 
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;
   }
}
