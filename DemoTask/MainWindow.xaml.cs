using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DemoTask
{
  public partial class MainWindow : Window
  {


    // Pattern observateur
    // 
    private CancellationTokenSource? cancellationTokenSource;



    public MainWindow()
    {
      InitializeComponent();
    }
    private async void buttonStart_Click(object sender, RoutedEventArgs e)
    {
      // instanciation à chaque tâche - usage unique
      // le CancellationTokenSource est l'émetteur du signal d'annulation
      cancellationTokenSource = new CancellationTokenSource();
      // le CancellationToken est le récepteur du signal
      // Le CancellationToken est utilisable sur N tâches
      CancellationToken cancellationToken = cancellationTokenSource.Token;
      progressBar1.IsIndeterminate = true;
      buttonStart.IsEnabled = false;
      buttonCancel.IsEnabled = true;
      try
      {
        string msg = await ReadAsync(cancellationToken);
        textBlockMsg.Text = msg;
      }
      catch (TaskCanceledException ex)
      {
        // tout va bien 
        Debug.WriteLine("Annulation de la tâche par l'utilisateur");
      }
      // Freezant :
      //string msg = ReadAsync().Result;
      progressBar1.IsIndeterminate = false;
      buttonStart.IsEnabled = true;
      buttonCancel.IsEnabled = false;
    }
    private async Task<string> ReadAsync(CancellationToken cancellationToken)
    {
      // Bloquant
      //Thread.Sleep(3000);
      // Bloquant
      //Task.Delay(3000).Wait();

      // Code simulant une requête sur un serveur
      //await Task.Delay(3000, cancellationToken);

      HttpClient client = new HttpClient { BaseAddress = new Uri("http://google.com") };
      HttpResponseMessage resp = await client.GetAsync("", cancellationToken);
      resp.EnsureSuccessStatusCode();
      // Tache longue annulable
      //for (int i = 0; i < 100_000; i++)
      //{
      //  if (cancellationToken.IsCancellationRequested)
      //    throw new TaskCanceledException();
      //}

      Task t1 = Task.Factory.StartNew(() => Thread.Sleep(3000));
      // Thank you Sébastien and John Thiriet
      var taskCompletionSource = new TaskCompletionSource<decimal>();
      // Registering a lambda into the cancellationToken
      cancellationToken.Register(() =>
      {
        // We received a cancellation message, cancel the TaskCompletionSource.Task
        taskCompletionSource.TrySetCanceled();
      });
      // Les lambdas excelllent à :
      // 1° Capturer des variables
      // 2° Etre des adaptateurs de fonction (ajouter ou enlever des params)
      Action action = () => LongComputation(cancellationToken);
      Task longTask = Task.Run(action);
      await longTask;
      //Task t2 = Task.Run( );
      await Task.WhenAny(t1, taskCompletionSource.Task);
      return "Hello world";
    }
    private async void LongComputation(CancellationToken cancellationToken)
    {
      for (int i = 0; i < 10; i++)
      {
        await Task.Delay(300, cancellationToken);
        textBlockMsg.Text = "Etape " +(i+1);
      }
    }
    private Task<string> ReadAsync2()
    {
      // pas ok sans async
      //return "Hello world";
      // Task.FromResult<T> est utile pour wrapper un résultat
      // dans une tâche lorque la fonction n'est pas async
      return Task.FromResult("Hello world");
    }
    private void buttonCancel_Click(object sender, RoutedEventArgs e)
    {
      // Emission signal annulation
      cancellationTokenSource?.Cancel();
    }
  }
}
