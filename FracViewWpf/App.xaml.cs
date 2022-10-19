using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using FracView.Gfx;
using FracViewWpf.ViewModels;
using FracViewWpf.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FracViewWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            ServiceCollection serviceCollection;
            IConfiguration configuration;
            ServiceProvider serviceProvider;

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

                configuration = builder.Build();

                serviceCollection = new ServiceCollection();

                ConfigureServices(serviceCollection, configuration);

                serviceProvider = serviceCollection.BuildServiceProvider();

                Workspace.CreateInstance(configuration, serviceProvider);
            }
            catch (Exception ex)
            {
                ShowUnhandledStartupException(ex.Message);
            }

            Dispatcher.UnhandledException += (s, e) =>
            {
                e.Handled = true;
                ShowUnhandledException(e.Exception, "Dispatcher.UnhandledException");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                ShowUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            };

            MainWindowViewModel vm = (MainWindowViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(MainWindowViewModel))!;

            vm.LoadSessionJson();

            MainWindow mainWindow = (MainWindow)Workspace.Instance.ServiceProvider.GetService(typeof(MainWindow))!;

            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<ColorWindow>();
            services.AddTransient<IScene, Scene>();
        }

        /// <summary>
        /// Shows error window for uncaught exceptions. Closes application once
        /// the error window is closed.
        /// </summary>
        /// <param name="ex">Exception to display.</param>
        /// <param name="source">Source of exception.</param>
        private void ShowUnhandledException(Exception ex, string source)
        {
            var ewvm = new ErrorWindowViewModel($"Fatal: Unhandled exception in application: {source}", ex)
            {
                ExitOnClose = true,
                ButtonText = "Exit",
            };

            Workspace.Instance.RecreateSingletonWindow<ErrorWindow>(ewvm);
        }

        private void ShowUnhandledStartupException(string message)
        {
            System.Windows.MessageBox.Show(message);
            Environment.Exit(1);
        }
    }
}
