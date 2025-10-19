using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;
public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
        }
    }
    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is ColaboradorDTO colaborador && BindingContext is ColaboradorListViewModel viewModel)
            {
                await viewModel.EditColaboradorCommand.ExecuteAsync(colaborador);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao editar colaborador: {ex.Message}", "OK");
        }
    }
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is ColaboradorDTO colaborador && BindingContext is ColaboradorListViewModel viewModel)
            {
                await viewModel.DeleteColaboradorCommand.ExecuteAsync(colaborador);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir colaborador: {ex.Message}", "OK");
        }
    }

    // CancellationTokenSource � uma classe do .NET usada para controlar o cancelamento de opera��es ass�ncronas,
    // como tarefas (Task) ou m�todos async/await.
    // Ela permite que voc� envie um sinal de cancelamento para uma ou mais opera��es que estejam ouvindo esse sinal.
    private CancellationTokenSource? _searchCts;

    // Debounce � uma t�cnica usada para evitar que uma a��o seja executada repetidamente em alta frequ�ncia,
    // especialmente durante eventos que disparam v�rias vezes em sequ�ncia, como digita��o em um campo de busca.
    // No nosso caso, debounce serve para evitar que a busca seja feita a cada tecla pressionada.
    // Em vez disso, a busca s� � executada ap�s um pequeno intervalo sem novas digita��es, por exemplo, 300ms.
    // Se o usu�rio continuar digitando, o timer reinicia e a busca s� acontece quando ele parar de digitar por esse tempo.
    private async void OnSearchDebounceTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            // espera curta (300ms)
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;
            if (BindingContext is ColaboradorListViewModel vm)
            {
                await vm.SearchColaboradoresCommand.ExecuteAsync(null);
            }
        }
        catch (TaskCanceledException) { /* ignorar */ }
    }
}