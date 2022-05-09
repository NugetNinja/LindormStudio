using ICSharpCode.AvalonEdit.Document;
using LindormStudio.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LindormStudio
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly LindormClient _client;
        private Connection? selectedConnection;
        private DataTable? results;
        private string? messages;
        private int resultTabIndex;
        private string? selectedDatabase;
        private TextDocument? selectedDocument;

        public MainWindowViewModel()
        {
            _client = new LindormClient(new HttpClient());
            Connections = new ObservableCollection<Connection>();
            Databases = new ObservableCollection<string>();
            Documents = new ObservableCollection<KeyValuePair<string, TextDocument>>()
            {
            };
            ExecuteSqlCommand = new AsyncRelayCommand(ExecuteSqlAsync, () => SelectedConnection != null);
            NewQueryCommand = new RelayCommand(NewQuery);
            RemoveQueryCommand = new RelayCommand<string>(RemoveQuery);
            App.Current.Configuration.GetSection("Connections").Bind(Connections);
        }
        public ObservableCollection<KeyValuePair<string, TextDocument>> Documents { get; }
        public TextDocument? SelectedDocument { get => selectedDocument; set => SetProperty(ref selectedDocument, value); }
        public IRelayCommand ExecuteSqlCommand { get; }
        public IRelayCommand NewQueryCommand { get; }
        public IRelayCommand RemoveQueryCommand { get; }
        public ObservableCollection<Connection> Connections { get; }
        public ObservableCollection<string> Databases { get; }
        public string? SelectedDatabase { get => selectedDatabase; set => SetProperty(ref selectedDatabase, value); }
        public string? Sql { get => SelectedDocument?.Text; }
        public Connection? SelectedConnection
        {
            get => selectedConnection; set
            {
                SetProperty(ref selectedConnection, value);
                LoadDatabasesAsync();
                ExecuteSqlCommand.NotifyCanExecuteChanged();
            }
        }
        public int ResultTabIndex { get => resultTabIndex; set => SetProperty(ref resultTabIndex, value); }
        public DataTable? Results { get => results; set => SetProperty(ref results, value); }
        public string? Messages { get => messages; set => SetProperty(ref messages, value); }
        public async void LoadDatabasesAsync()
        {
            if (SelectedConnection == null)
            {
                return;
            }
            Databases.Clear();
            await foreach (var database in _client.ShowDatabasesAsync(SelectedConnection!.BaseAddress, SelectedConnection!.UserName, SelectedConnection!.Password))
            {
                Databases.Add(database);
            }
            SelectedDatabase = Databases[0];
        }
        public async Task ExecuteSqlAsync()
        {
            if (SelectedConnection == null || string.IsNullOrWhiteSpace(Sql))
            {
                return;
            }
            var dt = new DataTable();
            try
            {
                var resp = await _client.ExecuteSqlAsync(SelectedConnection!.BaseAddress, SelectedConnection!.UserName, SelectedConnection!.Password, Sql, SelectedDatabase);
                foreach (var col in resp!.Columns)
                {
                    dt.Columns.Add(col);
                }
                foreach (var row in resp!.Rows)
                {
                    var dtRow = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dtRow[i] = row[i];
                    }
                    dt.Rows.Add(dtRow);
                }
                Results = dt;
                Messages = string.Empty;
                ResultTabIndex = 0;
            }
            catch (LindormExecuteSqlException e)
            {
                Results = dt;
                Messages = e.ToString();
                ResultTabIndex = 1;
            }
        }

        public void NewQuery()
        {
            var doc = new KeyValuePair<string, TextDocument>($"Query {Documents.Count + 1}", new TextDocument());
            Documents.Add(doc);
            SelectedDocument = doc.Value;
        }
        public void RemoveQuery(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            var doc = Documents.First(x => x.Key == name);
            if (doc.Value == SelectedDocument)
            {
                SelectedDocument = null;
            }
            Documents.Remove(doc);
        }
    }
}
