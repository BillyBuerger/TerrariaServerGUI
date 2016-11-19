using System;
using System.ComponentModel;

namespace TerrariaServerGUI
{
   public class Player : INotifyPropertyChanged
   {
      /// <summary>
      /// 
      /// </summary>
      private string _Name;

      /// <summary>
      /// 
      /// </summary>
      private string _Joined;

      /// <summary>
      /// 
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      /// <summary>
      /// 
      /// </summary>
      public string Name
      {
         get { return (_Name); }
         set
         {
            _Name = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string Joined
      {
         get { return (_Joined); }
         set
         {
            _Joined = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Joined"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Player()
      {
         _Name = "";
         _Joined = "";
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="Name"></param>
      /// <param name="Joined"></param>
      public Player(string Name, string Joined)
      {
         _Name = Name;
         _Joined = Joined;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return (string.Format("{0} joined at {1}", _Name, _Joined));
      }
   }
}
