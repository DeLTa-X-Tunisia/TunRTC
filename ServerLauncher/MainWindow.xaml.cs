using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TunRTC.Launcher
{
    public partial class MainWindow : Window
    {
        private Process? serverProcess;
        private DispatcherTimer healthCheckTimer = null!;
        private string serverUrl = "http://localhost:5000";
        private string projectPath = null!;

        public MainWindow()
        {
            InitializeComponent();
            InitializePaths();
            InitializeHealthCheck();
            LogMessage("✅ Application lancée");
            LogMessage($"📂 Dossier du projet: {projectPath}");
        }

        private void InitializePaths()
        {
            // Get the project root directory (one level up from ServerLauncher)
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            projectPath = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", ".."));

            // Verify the Server directory exists
            string serverProjectPath = Path.Combine(projectPath, "Server", "TunRTC.Server.csproj");
            if (!File.Exists(serverProjectPath))
            {
                LogMessage($"⚠️ ERREUR: Projet serveur introuvable: {serverProjectPath}");
                LogMessage("💡 Assurez-vous que le launcher est dans le dossier ServerLauncher/");
            }
        }

        private void InitializeHealthCheck()
        {
            healthCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            healthCheckTimer.Tick += HealthCheckTimer_Tick;
            healthCheckTimer.Start();
        }

        private async void HealthCheckTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.GetAsync($"{serverUrl}/health");

                if (response.IsSuccessStatusCode)
                {
                    UpdateServerStatus(true);
                }
                else
                {
                    UpdateServerStatus(false);
                }
            }
            catch
            {
                UpdateServerStatus(false);
            }
        }

        private void UpdateServerStatus(bool isRunning)
        {
            Dispatcher.Invoke(() =>
            {
                if (isRunning)
                {
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    StatusText.Text = "En ligne";
                    ServerUrlText.Text = $"🌐 {serverUrl}";
                    ServerUrlText.Visibility = Visibility.Visible;

                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    RestartButton.IsEnabled = true;
                    OpenSwaggerButton.IsEnabled = true;
                    OpenTestButton.IsEnabled = true;
                }
                else
                {
                    StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    StatusText.Text = "Arrêté";
                    ServerUrlText.Visibility = Visibility.Collapsed;

                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    RestartButton.IsEnabled = false;
                    OpenSwaggerButton.IsEnabled = false;
                    OpenTestButton.IsEnabled = false;
                }
            });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogMessage("🚀 Démarrage du serveur TunRTC...");

                string serverProjectPath = Path.Combine(projectPath, "Server", "TunRTC.Server.csproj");

                if (!File.Exists(serverProjectPath))
                {
                    LogMessage($"❌ ERREUR: Projet introuvable: {serverProjectPath}");
                    MessageBox.Show($"Le projet serveur est introuvable:\n{serverProjectPath}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{serverProjectPath}\"",
                        WorkingDirectory = projectPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                serverProcess.OutputDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        LogMessage($"[INFO] {args.Data}");
                    }
                };

                serverProcess.ErrorDataReceived += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        LogMessage($"[ERROR] {args.Data}");
                    }
                };

                serverProcess.Start();
                serverProcess.BeginOutputReadLine();
                serverProcess.BeginErrorReadLine();

                LogMessage($"✅ Serveur démarré (PID: {serverProcess.Id})");
                LogMessage($"🌐 URL: {serverUrl}");
                LogMessage("⏳ Vérification de la santé du serveur...");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ ERREUR lors du démarrage: {ex.Message}");
                MessageBox.Show($"Impossible de démarrer le serveur:\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogMessage("⏹️ Arrêt du serveur...");

                if (serverProcess != null && !serverProcess.HasExited)
                {
                    serverProcess.Kill(true);
                    serverProcess.WaitForExit(5000);
                    LogMessage($"✅ Serveur arrêté (PID: {serverProcess.Id})");
                }
                else
                {
                    LogMessage("⚠️ Aucun processus serveur actif trouvé");
                }

                serverProcess?.Dispose();
                serverProcess = null;
                UpdateServerStatus(false);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ ERREUR lors de l'arrêt: {ex.Message}");
                MessageBox.Show($"Erreur lors de l'arrêt du serveur:\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("🔄 Redémarrage du serveur...");
            StopButton_Click(sender, e);
            System.Threading.Thread.Sleep(2000);
            StartButton_Click(sender, e);
        }

        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
            LogMessage("🗑️ Logs effacés");
        }

        private void OpenSwaggerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = serverUrl, // Swagger is at root
                    UseShellExecute = true
                });
                LogMessage("📖 Swagger UI ouvert dans le navigateur");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur: {ex.Message}");
            }
        }

        private void OpenTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string testPath = Path.Combine(projectPath, "Tests", "signalr-test.html");
                if (File.Exists(testPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = testPath,
                        UseShellExecute = true
                    });
                    LogMessage("🧪 Tests SignalR ouverts dans le navigateur");
                }
                else
                {
                    LogMessage($"⚠️ Fichier de tests introuvable: {testPath}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur: {ex.Message}");
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = projectPath,
                    UseShellExecute = true
                });
                LogMessage("📁 Dossier du projet ouvert");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                LogTextBox.AppendText($"[{timestamp}] {message}\n");
                LogScrollViewer.ScrollToEnd();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            healthCheckTimer?.Stop();

            if (serverProcess != null && !serverProcess.HasExited)
            {
                var result = MessageBox.Show(
                    "Le serveur est toujours en cours d'exécution.\nVoulez-vous l'arrêter avant de quitter?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        serverProcess.Kill(true);
                        serverProcess.WaitForExit(5000);
                    }
                    catch { }
                }
            }

            serverProcess?.Dispose();
            base.OnClosed(e);
        }
    }
}
