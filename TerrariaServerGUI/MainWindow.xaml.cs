using System;
using System.Windows;

namespace TerrariaServerGUI
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      /// <summary>
      /// Our ViewModel where all the stuff actually happens.
      /// </summary>
      private TerrariaServerVM ViewModel;

      /// <summary>
      /// Indicates that we are trying to close the application but are waiting for the
      /// server command to finish
      /// </summary>
      private bool FormClosing;

      /// <summary>
      /// 
      /// </summary>
      public MainWindow()
      {
         InitializeComponent();

         FormClosing = false;

         // Create our View Model
         ViewModel = new TerrariaServerVM(Dispatcher);

         // Bind our message list view to our message collection
         MessageList.DataContext = ViewModel;
         MessageList.ItemsSource = ViewModel.Messages;

         PlayingList.DataContext = ViewModel;
         PlayingList.ItemsSource = ViewModel.Players;

         // Get notifid of message changes
         ViewModel.PropertyChanged += ViewModel_PropertyChanged;
         ViewModel.ServerClosed += ViewModel_ServerClosed;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ViewModel_ServerClosed(object sender, EventArgs e)
      {
         CommandText.IsEnabled = false;
         CommandSend.IsEnabled = false;

         if (FormClosing)
         {
            // We've indicated that we want to close the application.
            // Server is closed, we're good to go now
            Close();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         ViewModel.StartServer();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         // If the server is running, then confirm the close
         if (ViewModel.ServerRunning)
         {
            if (!FormClosing)
            {
               // Notify the server to close and wait for it
               FormClosing = true;
               ViewModel.StopServer();
               e.Cancel = true;
            }
            else if (ViewModel.ServerRunning)
            {
               if (MessageBox.Show("Waiting for the server to stop.  Do you want to force it closed?", "Shutdown", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
               {
                  // Force the close
                  ViewModel.KillServer();
               }
               else
               {
                  // Wait for the server
                  e.Cancel = true;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         ViewModel.ProcessCommand(CommandText.Text);
         CommandText.Text = "";
      }
   }
}
