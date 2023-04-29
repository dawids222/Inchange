using LibLite.Inchange.Desktop.Services;
using LibLite.Inchange.Desktop.Views;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LibLite.Inchange.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProgressWindow _progresWindow = new();
        private readonly OpenFileDialog _fileDialog = new();

        private readonly IInvoiceService _invoiceService = new InvoiceService();
        private readonly IFileService _fileService = new FileService(new TesseractFileReader());

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            OpenFileDialog();
            ProcessFiles();
        }

        private void Initialize()
        {
            Visibility = Visibility.Hidden;

            _fileDialog.Multiselect = true;
            _fileDialog.Filter = "Invoices|*.pdf;*.jpg;*.jpeg;*.png";

            _progresWindow.OkClick += (_, _) =>
            {
                _progresWindow.Hide();
                Environment.Exit(0);
            };
        }

        public void OpenFileDialog()
        {
            if (_fileDialog.ShowDialog() != true)
            {
                Environment.Exit(0);
            }
        }

        public void ProcessFiles()
        {
            var context = SynchronizationContext.Current;
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                Synchronize(context, () => _progresWindow.Show());
                await Parallel.ForEachAsync(_fileDialog.FileNames,
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    async (file, token) =>
                    {
                        var document = await _fileService.GetDocumentAsync(file);
                        try
                        {
                            var name = _invoiceService.GenerateName(document);
                            _fileService.Copy(document, name);
                            Synchronize(context, () => _progresWindow.AddEntry($"[Success] : {document.Name} -> {name}"));
                        }
                        catch (Exception ex)
                        {
                            Synchronize(context, () => _progresWindow.AddEntry($"[Error] : {document.Name} -> {ex.Message}"));
                        }
                        Synchronize(context, () => _progresWindow.AddProgress(100 / _fileDialog.FileNames.Length));
                    });
                Synchronize(context, () => _progresWindow.SetProgress(100));
            });
        }

        private static void Synchronize(SynchronizationContext? context, Action action)
        {
            if (context is not null)
            {
                context.Post(_ => action.Invoke(), null);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}
