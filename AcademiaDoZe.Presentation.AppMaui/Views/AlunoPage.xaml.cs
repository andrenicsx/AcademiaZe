using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage
{
    private CancellationTokenSource? _searchCts;

    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AlunoViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        if (SenhaEntry is not null)
        {
            // Switch.IsToggled == true -> mostrar senha -> IsPassword = false
            SenhaEntry.IsPassword = !e.Value;
        }
    }

    private async void OnEmailUnfocused(object sender, FocusEventArgs e)
    {
        if (BindingContext is AlunoViewModel viewModel && !string.IsNullOrWhiteSpace(EmailEntry?.Text))
        {
            var email = EmailEntry.Text.Trim();
            // Validação simples de email
            var isValid = System.Text.RegularExpressions.Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            EmailErrorLabel.IsVisible = !isValid;
            if (isValid)
            {
                viewModel.Aluno.Email = email;
            }
        }
    }

    // Debounce para evitar buscar a cada tecla pressionada
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

            if (BindingContext is AlunoViewModel vm)
            {
                //await vm.SearchAlunosCommand.ExecuteAsync(null);
            }
        }
        catch (TaskCanceledException)
        {
            // ignorar
        }
    }
}
