using StegoApolloUI.Views;
using StegoLib.Services;
using System;
using System.Threading.Tasks;
using StegoLib.Utilities;

namespace StegoApolloUI.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view; // 介面
        private IStegoService _stegoService; // 演算法服務
        public MainPresenter(IMainView view, IStegoService stego)
        {
            _view = view; // 儲存 View 介面
            _stegoService = stego; // 儲存演算法服務

            _view.AlgorithmChanged += OnAlgorithmChanged; // 註冊演算法變更事件
            // 把 View 事件綁到對應的 handler
            _view.EmbedRequested += async (s, e) => {
                _view.ShowInfo("開始嵌入…");
                var progress = new Progress<int>(percent => _view.ShowProgress(percent));
                var result = await Task.Run(() => _stegoService.Embed(
                    ImageHelper.Load(_view.InputFilePath),
                    _view.MessageText, progress));
                if (result.Success) { _view.IsProcessed = true; _view.ShowImage(result.Image); LogManager.Instance.LogSuccess("嵌入程序成功!"); }
                else { _view.ShowError(result.ErrorMessage); LogManager.Instance.LogError("嵌入程序發生問題，動作沒有成功!"); }
            };

            _view.ExtractRequested += (s, e) => {
                _view.ShowInfo("開始提取…");
                var progress = new Progress<int>(percent => _view.ShowProgress(percent));
                var result = _stegoService.Extract(
                    ImageHelper.Load(_view.InputFilePath), progress);
                if (result.Success) { _view.IsProcessed = true; _view.ShowExtracted($"{result.Message}"); LogManager.Instance.LogSuccess("萃取程序成功!"); }
                else { _view.ShowError(result.ErrorMessage); _view.ShowExtracted(null); LogManager.Instance.LogError("萃取程序發生問題，動作沒有成功"); }
            };
        }
        private void OnAlgorithmChanged(object sender, AlgorithmChangedEventArgs e)
        {
            switch (e.Algorithm)
            {
                case "LSB 演算法":
                    LogManager.Instance.LogInfo("切換至 LSB 演算法");
                    _stegoService = new LsbStegoService();
                    break;
                case "DCT 演算法":
                    LogManager.Instance.LogInfo("切換至 DCT 演算法，但我不想做了");
                    throw new NotImplementedException("DCT 目前被放棄了。");
                case "HistShift 演算法":
                    LogManager.Instance.LogInfo("切換至 HistShift 演算法，但我不想做了");
                    _stegoService = new HistShiftStegoService();
                    break;
                case "QIM 演算法":
                    LogManager.Instance.LogInfo("切換至 QIM 演算法");
                    _stegoService = new QimStegoService(16);
                    break;
                default:
                    LogManager.Instance.LogError($"不支援的演算法：{e.Algorithm}");
                    _view.ShowError($"不支援的演算法：{e.Algorithm}");
                    break;
            }

            _view.ShowInfo($"已切換至演算法：{e.Algorithm}");
        }

    }
}
