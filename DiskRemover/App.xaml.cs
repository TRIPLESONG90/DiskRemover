using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Forms.Design;
using Serilog.Events;
using System.Diagnostics;
using System.IO;

namespace DiskRemover
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public class ActionSink : Serilog.Core.ILogEventSink
    {
        private readonly Action<LogEvent> _writeAction;

        public ActionSink(Action<LogEvent> writeAction)
        {
            _writeAction = writeAction;
        }

        public void Emit(LogEvent logEvent)
        {
            _writeAction(logEvent);
        }
    }

    public partial class App : Application
    {
        public static event EventHandler<string> LogEvent;
        public IServiceProvider ServiceProvider { get; private set; }
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {

            const string appName = "imageRemover"; // 고유한 애플리케이션 이름
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // 이미 실행 중인 인스턴스가 있으면 종료
                MessageBox.Show("앱이 이미 실행 중입니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }

            base.OnStartup(e);

            // DI 컨테이너 구성
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
           
            // MainWindow 초기화
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30) // 파일 로그
                .WriteTo.Sink(new ActionSink(logEvent =>
                {
                    var timeString = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    LogEvent?.Invoke(this, $"[{timeString}] [{logEvent.Level}] {logEvent.RenderMessage()}");
                }))
                .CreateLogger();
            services.AddLogging(builder =>
            {
                builder.ClearProviders(); // 기본 제공 로깅 제거
                builder.AddSerilog();    // Serilog로 ILogger 연결
            });
            // 뷰모델 및 서비스 등록
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<Remover>();
            services.AddSingleton<AppSetting>((sp) =>
            {
                return AppSetting.Load();
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }

    }

}
