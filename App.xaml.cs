using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TodaysAlphaReactor
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>二重起動防止用セマフォの名前</summary>
        private const string SemaphoreName = "TodaysAlphaReactor";

        /// <summary>二重起動防止用セマフォ</summary>
        private System.Threading.Semaphore _semaphore;

        /// <summary>
        /// 開始時処理
        /// セマフォを作成し、新規作成の場合のみ起動処理を行う。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _semaphore = new System.Threading.Semaphore(1, 1, SemaphoreName, out createdNew);
            // 新規作成なら通常起動、既にセマフォが作成されていた場合は二重起動なので終了させる
            if (createdNew)
            {
                base.OnStartup(e);
            }
            else
            {
                this.Shutdown();
            }
        }

        /// <summary>
        /// 終了時処理
        /// セマフォを解放する。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
#if DEBUG
            try
            {
                _semaphore?.Release();
            }
            catch (Exception ex) when (ex is System.Threading.SemaphoreFullException)
            {
                //
            }
#else
            _semaphore?.Release();
#endif
        }
    }
}
