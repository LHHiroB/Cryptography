using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace IOApp.Features
{
    internal class AppGrid : Grid
    {
        public bool IsCursorHided { get; private set; }

        public void ShowCursor(bool isVisible)
        {
            if (isVisible)
            {
                ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                IsCursorHided = false;
            }   
            else
            {
                if (ProtectedCursor == null)
                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

                ProtectedCursor.Dispose();
                IsCursorHided = true;
            }
        }
    }
}