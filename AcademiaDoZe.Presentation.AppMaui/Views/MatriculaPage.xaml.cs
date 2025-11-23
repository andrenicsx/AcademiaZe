
// Salve este arquivo como:
// AcademiaDoZe.Presentation.AppMaui/Views/MatriculaPage.xaml.cs

using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.Views
{
    public partial class MatriculaPage : ContentPage
    {
        private readonly MatriculaViewModel _viewModel; // Declaração para acesso

        // O construtor recebe a ViewModel por injeção de dependência
        public MatriculaPage(MatriculaViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel; // Armazena a referência
            BindingContext = viewModel; // Liga a View ao ViewModel
        }

        // ESTE MÉTODO É OBRIGATÓRIO: Chamaremos o InitializeAsync do ViewModel aqui
        // No code-behind MatriculaPage.xaml.cs
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.InitializeAsync(); // Chama LoadRestricoes
            }
        }
    }
}
