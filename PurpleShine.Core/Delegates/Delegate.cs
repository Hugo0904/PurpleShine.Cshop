using PurpleShine.Core.Delegates.Args;

namespace PurpleShine.Core.Delegates
{
    /// <summary>
    /// 泛型 Delegate
    /// </summary>
    /// <typeparam name="T">傳遞的泛型參數</typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void GenericEventHandler<T>(object sender, ValueArgs<T> e);
}