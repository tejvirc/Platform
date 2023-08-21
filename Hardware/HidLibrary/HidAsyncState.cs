namespace Vgt.Client12.Hardware.HidLibrary
{
    public class HidAsyncState
    {
        public HidAsyncState(object callerDelegate, object callbackDelegate)
        {
            CallerDelegate = callerDelegate;
            CallbackDelegate = callbackDelegate;
        }

        public object CallerDelegate { get; }

        public object CallbackDelegate { get; }
    }
}