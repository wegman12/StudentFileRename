using StudentFileRename.Interface;
using StudentFileRename.Service;
using StudentFileRename.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Windows;
using StudentFileRename.View;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace StudentFileRename
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddNLog();
                })
                .AddSingleton<StartupPageViewModel>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<IFileDialogService, WindowsFileDialogService>()
                .AddSingleton<VistaFolderBrowserDialog>()
                .AddSingleton<OpenFileDialog>()
                .AddSingleton<IDialogService, MaterialDesignDialogService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
