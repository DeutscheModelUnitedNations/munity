namespace MUNity.BlazorServer
{
    public static class DevTooling
    {
        public static event EventHandler DevToolsChanged;

        private static bool _enableDevTools = false;
        public static bool EnableDevTools
        {
            get => _enableDevTools;
            set
            {
                if (_enableDevTools != value)
                {
                    _enableDevTools = value;
                    DevToolsChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
    }
}
