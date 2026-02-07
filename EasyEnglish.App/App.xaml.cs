using EasyEnglish.Core.Interfaces.Cache;

namespace EasyEnglish.App
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
            return new Window(new MainPage()) { Title = "EasyEnglish.App" };
        }
    }
}
