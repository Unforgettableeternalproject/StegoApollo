using System;
using System.Drawing;

namespace StegoApolloUI.Views
{
    public interface IMainView
    {
        string InputFilePath { get; }
        string MessageText { get; }

        void ShowProgress(int percent);
        void ShowResultImage(Bitmap bmp);
        void ShowError(string message);
        void ShowInfo(string message);

        event EventHandler EmbedRequested;
        event EventHandler ExtractRequested;
    }
}
