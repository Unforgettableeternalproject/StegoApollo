using System;
using System.Drawing;

namespace StegoApolloUI.Views
{
    public interface IMainView
    {
        string InputFilePath { get; }
        string MessageText { get; }

        bool IsProcessed { get; set; }

        void ShowProgress(int percent);
        void ShowImage(Bitmap bmp);
        void ShowError(string message);
        void ShowInfo(string message);
        void ShowExtracted(string message);
        void Invoke(Action action);

        event EventHandler EmbedRequested;
        event EventHandler ExtractRequested;
        event EventHandler<AlgorithmChangedEventArgs> AlgorithmChanged;
    }
    public class AlgorithmChangedEventArgs : EventArgs
    {
        public string Algorithm { get; }

        public AlgorithmChangedEventArgs(string algorithm)
        {
            Algorithm = algorithm;
        }
    }
}
