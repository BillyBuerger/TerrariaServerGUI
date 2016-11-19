using System;
using System.Windows.Input;

// TODO: Not currently being used.  Attempted to setup MVVM for button handling but can't really
//       get everything working for MVVM anyways with the multiple processes running in the background
//       So this can probably be removed from the project.


namespace TerrariaServerGUI
{
   public class ModelCommand : ICommand
   {
      public ModelCommand(Action<object> execute)
          : this(execute, null)
      { }

      public ModelCommand(Action<object> execute, Predicate<object> canExecute)
      {
         _execute = execute;
         _canExecute = canExecute;
      }

      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
         return _canExecute != null ? _canExecute(parameter) : true;
      }

      public void Execute(object parameter)
      {
         if (_execute != null)
            _execute(parameter);
      }

      public void OnCanExecuteChanged()
      {
         CanExecuteChanged(this, EventArgs.Empty);
      }

      private readonly Action<object> _execute = null;
      private readonly Predicate<object> _canExecute = null;
   }
}
