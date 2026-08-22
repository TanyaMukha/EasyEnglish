using EasyPeasy.Core.Interfaces.Cache;

namespace EasyPeasy.App
{
    public partial class App : Application
    {
        public App(IWordCacheService wordCache, ICurrentUnitCacheService unitCache)
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                await Task.WhenAll(
                    wordCache.InitializeAsync(),
                    unitCache.InitializeAsync()
                );
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "EasyPeasy.App" };
        }
    }
}
