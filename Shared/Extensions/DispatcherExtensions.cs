using System;
using System.Threading;
using System.Windows.Threading;

namespace Shared.Extensions
{
    public static class DispatcherExtensions
    {
        #region Methods

        #region Public

        public static void DoEvents(this Dispatcher currentDispatcher)
        {
            DispatcherFrame frame = new DispatcherFrame();
            currentDispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object f)
            {
                ((DispatcherFrame)f).Continue = false;
                return null;
            },
                                          frame);
            Dispatcher.PushFrame(frame);
        }       

        public static void SynchronizeCall(this Dispatcher dispatcher, Action action)
        {
            if (Thread.CurrentThread != dispatcher.Thread)
            {
                dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        #endregion

        #endregion
    }
}