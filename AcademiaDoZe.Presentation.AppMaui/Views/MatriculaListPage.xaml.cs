


using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;


public partial class MatriculaListPage : ContentPage
{
    // IMPORTANTE: Injete a nova ViewModel
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // IMPORTANTE: Chame o comando de carregar Matrículas
        if (BindingContext is MatriculaListViewModel viewModel)
        {
            await viewModel.LoadMatriculasCommand.ExecuteAsync(null);
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // IMPORTANTE: Use 'MatriculaDTO' e 'MatriculaListViewModel'
            if (sender is Button button && button.BindingContext is MatriculaDTO matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.EditMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex) { await DisplayAlert("Erro", $"Erro ao editar matrícula: {ex.Message}", "OK"); }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        try
        {
            // IMPORTANTE: Use 'MatriculaDTO' e 'MatriculaListViewModel'
            if (sender is Button button && button.BindingContext is MatriculaDTO matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.DeleteMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex) { await DisplayAlert("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK"); }
    }
}
