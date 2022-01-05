﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DemoTask
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    // Pattern observateur
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
      await Task.Delay(3000, cancellationToken);
      return "Hello world";
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
