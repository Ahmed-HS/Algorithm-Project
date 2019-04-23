using System;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AlgorithmProject;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace SmallWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        string[] ActorNames;
        static Stopwatch TaskTimer;

        public MainPage()
        {
            InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(800, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            TaskTimer = new Stopwatch();
            QueriesResult.MaxLength = int.MaxValue;
            QueriesResult.IsReadOnly = true;
      
        }

        private void StartTaskTimer()
        {
            TaskTimer.Reset();
            TaskTimer.Start();
        }
        private void SetTaskTime()
        {
            string Time = "";
            int Hours, Minutes, Seconds;
            TaskTimer.Stop();
            Seconds = TaskTimer.Elapsed.Seconds;
            Seconds %= 60;
            Minutes = Seconds / 60;
            Minutes %= 60;
            Hours = Minutes / 60;
            Time = "Finished in : " + Hours + " Hours , " + Minutes + " Minutes , " + Seconds + " Seconds ";
            TimerText.Text = Time;
        }

        private async void OpenMoviesFile(object sender, RoutedEventArgs e)
        {
            FileOpenPicker FilePicker = new FileOpenPicker();
            FilePicker.ViewMode = PickerViewMode.Thumbnail;
            FilePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            FilePicker.FileTypeFilter.Add(".txt");
            StorageFile ChosenFile = await FilePicker.PickSingleFileAsync();
            Task<string[]> ReadGraph = null;
            if (ChosenFile != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(ChosenFile);
                ReadGraph = new Task<string[]>(()=> 
                {
                    return Graph.ReadGraph(ChosenFile.Path);
                });
                StartTaskTimer();
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);
                ReadGraph.Start();
                ActorNames = ReadGraph.Result;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                SetTaskTime();
            }

        }

        private async void OpenQueriesFile(object sender, RoutedEventArgs e)
        {
            FileOpenPicker FilePicker = new FileOpenPicker();
            FilePicker.ViewMode = PickerViewMode.Thumbnail;
            FilePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            FilePicker.FileTypeFilter.Add(".txt");
            StorageFile ChosenFile = await FilePicker.PickSingleFileAsync();
            Task<string> ReadQueries = null;
            if (ChosenFile != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(ChosenFile);
                ReadQueries = new Task<string>(()=>
                {                  
                    return Graph.ReadQueries(ChosenFile.Path);
                });
                StartTaskTimer();
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);
                ReadQueries.Start();
                SetQueriesResultText(ReadQueries.Result);
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                SetTaskTime();
            }
          
        }

        private void SetQueriesResultText(string NewText)
        {
            int MaxDisplaySize = 457050;
            if (NewText.Length > MaxDisplaySize)
            {
                ShowSaveDialog(NewText);
                return;
            }
            QueriesResult.IsReadOnly = false;
            QueriesResult.Document.SetText(Windows.UI.Text.TextSetOptions.None,NewText);
            QueriesResult.IsReadOnly = true;
        }

        private async void ShowDialog(string Heading,string Message)
        {
            ContentDialog SavedFile = new ContentDialog
            {
                Title = Heading,
                Content = Message,
                CloseButtonText = "Ok"
            };

            ContentDialogResult Result = await SavedFile.ShowAsync();
        }

        private async void ShowSaveDialog(string QueriesResult)
        {
            ContentDialog TextTooLarge = new ContentDialog
            {
                Title = "Result too large",
                Content = "The result is too large to display, Save it to a text file.",
                CloseButtonText = "Ok"
            };

            ContentDialogResult Result = await TextTooLarge.ShowAsync();

            if (Result == ContentDialogResult.None)
            {
                FileSavePicker SavePicker = new FileSavePicker();
                SavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                SavePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                SavePicker.SuggestedFileName = "QueriesResult";

                StorageFile file = await SavePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteTextAsync(file, QueriesResult);

                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        ShowDialog("File Saved", "The result was saved successfully.");
                    }
                    else
                    {
                        ShowDialog("Save Failed", "The result couldn't be saved.");
                    }
                }
                else
                {
                    ShowDialog("Save Cancelled", "The save operation was cancelled.");
                }
            }
        }

        private void FindRelation(object sender, RoutedEventArgs e)
        {
            string Source = FirstActor.Text;
            string Target = SecondActor.Text;
            Task<string> FindRelationTask = new Task<string>(() =>
            {
                return Graph.GetTwoActorsRelation(Source,Target);
            });
            StartTaskTimer();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);
            FindRelationTask.Start();
            SetQueriesResultText(FindRelationTask.Result);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            SetTaskTime();
        }

        private void FindAllRelations(object sender, RoutedEventArgs e)
        {
            string Source = FirstActor.Text;
            Task<string> FindOneToAll = new Task<string>(() =>
            {
                return Graph.GetOneToAllRelation(Source);
            });
            StartTaskTimer();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);
            FindOneToAll.Start();
            SetQueriesResultText(FindOneToAll.Result);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            SetTaskTime();
        }

        private async void FilterActors(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string CurrentText = sender.Text;
                sender.ItemsSource = await Task.Run(() => { return GetSuggestions(CurrentText); });
            }
        }
        

        private string[] GetSuggestions(string text)
        {
            string[] Suggestions = null;

            Suggestions = ActorNames.Where(x => x.Contains(text)).ToArray();
            return Suggestions;
        }
    }
}
