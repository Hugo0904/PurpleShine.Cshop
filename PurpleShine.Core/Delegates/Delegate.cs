using PurpleShine.Core.Delegates.Args;

namespace PurpleShine.Core.Delegates
{
    /// <summary>
    /// �x�� Delegate
    /// </summary>
    /// <typeparam name="T">�ǻ����x���Ѽ�</typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void GenericEventHandler<T>(object sender, ValueArgs<T> e);
}