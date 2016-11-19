using System;
using System.Windows.Controls;

namespace TerrariaServerGUI
{
   public class AutosizeGridView : GridView
   {
      protected override void PrepareItem(ListViewItem item)
      {
         foreach (GridViewColumn col in Columns)
         {
            if (double.IsNaN(col.Width))
            {
               col.Width = col.ActualWidth;
            }

            col.Width = double.NaN;
         }

         base.PrepareItem(item);
      }
   }
}
