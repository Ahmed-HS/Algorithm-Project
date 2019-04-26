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
                StartTask("Reading movies file.");
                ReadGraph.Start();
                ActorNames = ReadGraph.Result;
                FinishTask();
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
                StartTask("Reading queries file.");
                ReadQueries.Start();
                FinishTask(ReadQueries.Result);
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
            StartTask("Finding relation between two actors.");
            FindRelationTask.Start();
            FinishTask(FindRelationTask.Result);
        }

        private void FindAllRelations(object sender, RoutedEventArgs e)
        {
            string Source = FirstActor.Text;
            Task<string> FindOneToAll = new Task<string>(() =>
            {
                return Graph.GetOneToAllRelation(Source);
            });
            StartTask("Calculating distribution of shortest paths.");
            FindOneToAll.Start();
            FinishTask(FindOneToAll.Result);
        }

        private void FindStrongestPath(object sender, RoutedEventArgs e)
        {
            string Source = FirstActor.Text;
            string Target = SecondActor.Text;
            Task<string> StrongestPathTask = new Task<string>(() =>
            {
                return "To be Implemented";
            });
            StartTask("Finding strongest path between two actors.");
            StrongestPathTask.Start();
            FinishTask(StrongestPathTask.Result);
        }

        private void FindMST(object sender, RoutedEventArgs e)
        {
            string Source = FirstActor.Text;
            string Target = SecondActor.Text;
            Task<string> MSTTaske = new Task<string>(() =>
            {
                return "To be Implemented";
            });
            StartTask("Finding MST.");
            MSTTaske.Start();
            FinishTask(MSTTaske.Result);
        }

        private void StartTask(string Status)
        {
            StatusText.Text = Status;
            StartTaskTimer();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 0);
        }

        private void FinishTask(string Result = "")
        {
            string FinishTime = StopTaskTime();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            if (Result != "")
            {
                SetQueriesResultText(Result, FinishTime);
            }
        }

        private void StartTaskTimer()
        {
            TaskTimer.Reset();
            TaskTimer.Start();
        }

        private string StopTaskTime()
        {
            TaskTimer.Stop();
            StatusText.Text = "Finished in : \n" + TaskTimer.Elapsed.Minutes + " Minutes , " + TaskTimer.Elapsed.Seconds + " Seconds ";
            return TaskTimer.Elapsed.Minutes + " Minutes , " + TaskTimer.Elapsed.Seconds + " Seconds ";
        }

        private void SetQueriesResultText(string NewText,string FinishTime)
        {
            int MaxDisplaySize = 307620;
            if (NewText.Length > MaxDisplaySize)
            {
                ShowSaveDialog(NewText, "Result too large", "The result is too large to display, Save it to a text file.");
                return;
            }
            else if (NewText == "Please Enter Correct Actor Names")
            {
                return;
            }
            else
            {
                ShowSaveDialog(NewText, "Finished", "The task finished in " + FinishTime + ", Do you want to save the result to a text file ?");
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

        private async void ShowSaveDialog(string QueriesResult,string Heading,string Message)
        {
            ContentDialog SaveDialog = new ContentDialog
            {
                Title = Heading,
                Content = Message,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult Result = await SaveDialog.ShowAsync();

            if (Result == ContentDialogResult.Primary)
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
            if (ActorNames == null)
            {
                return null;
            }
            Suggestions = ActorNames.Where(x => x.Contains(text)).ToArray();
            return Suggestions;
        }

    }
}
