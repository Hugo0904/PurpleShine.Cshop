using System;
using System.Threading;
using System.Threading.Tasks;

namespace PurpleShine.Core.Helpers
{
    public static class Threads
    {
        /// <summary>
        /// 產生新的Thread 執行action
        /// </summary>
        /// <param name="e"></param>
        public static void Excute(ThreadStart e)
        {
            Thread thread = new Thread(e) { IsBackground = true };
            thread.Start();
        }


        /// <summary>
        /// 異步處理上下文
        /// </summary>
        /// <param name="actions"></param>
        public static void ContextAsync(Action<IProgress<Action>> actions)
        {
            Progress<Action> progress = new Progress<Action>();
            progress.ProgressChanged += (object sender, Action e) => e.Invoke();
            Task.Run(() => actions.Invoke(progress));
        }
    }
}
