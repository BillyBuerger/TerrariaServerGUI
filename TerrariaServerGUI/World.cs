using System;
using System.ComponentModel;

namespace TerrariaServerGUI
{
   /// <summary>
   /// 
   /// </summary>
   public class World : INotifyPropertyChanged
   {
      /// <summary>
      /// 
      /// </summary>
      private string _Name;

      /// <summary>
      /// 
      /// </summary>
      private int _Number;

      /// <summary>
      /// 
      /// </summary>
      private string _Password;

      /// <summary>
      /// 
      /// </summary>
      private int _MaxPlayers;

      /// <summary>
      /// 
      /// </summary>
      private int _Port;

      /// <summary>
      /// 
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      public World()
      {
         _Number = 0;
         _Name = "";
         _MaxPlayers = 0;
         _Password = "";
         _Port = 0;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="Number"></param>
      /// <param name="Name"></param>
      public World(int Number, string Name, string Password, int MaxPlayers, int Port)
      {
         _Number = Number;
         _Name = Name;
         _Password = Password;
         _MaxPlayers = MaxPlayers;
         _Port = Port;
      }

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
      public int Number
      {
         get { return (_Number); }
         set
         {
            _Number = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Number"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string Password
      {
         get { return (_Password); }
         set
         {
            _Password = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Password"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int Port
      {
         get { return (_Port); }
         set
         {
            _Port = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("Port"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int MaxPlayers
      {
         get { return (_MaxPlayers); }
         set
         {
            _MaxPlayers = value;

            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs("MaxPlayers"));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return (string.Format("{0}: {1}", _Number, _Name));
      }
   }
}
