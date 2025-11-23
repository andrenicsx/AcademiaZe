

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AcademiaDoZe.Presentation.AppMaui.Views;


namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    public partial class MatriculaListViewModel : BaseViewModel
    {
        private readonly IMatriculaService _matriculaService;
        private List<MatriculaDTO> _allMatriculas; // Lista de backup para o filtro

        public MatriculaListViewModel(IMatriculaService matriculaService)
        {
            _matriculaService = matriculaService;
            Title = "Matrículas";

            _allMatriculas = new List<MatriculaDTO>();
            Matriculas = new ObservableCollection<MatriculaDTO>();

            // Opções do Picker de filtro
            FilterTypes = new ObservableCollection<string>
            {
                "Nome do Aluno",
                "CPF do Aluno"
            };
            SelectedFilterType = FilterTypes.First(); // Define "Nome do Aluno" como padrão
        }

        // ----- Propriedades de Binding -----

        [ObservableProperty]
        private ObservableCollection<MatriculaDTO> _matriculas;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilterType;

        public ObservableCollection<string> FilterTypes { get; }

        // ----- Comandos -----

        // Comando chamado pelo OnAppearing e pelo RefreshView
        [RelayCommand]
        private async Task LoadMatriculasAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Carrega todos os dados do serviço
                var matriculasData = await _matriculaService.ObterTodasAsync();
                _allMatriculas = matriculasData.ToList();

                // Atualiza a coleção observável (a lista na tela)
                Matriculas.Clear();
                foreach (var matricula in _allMatriculas)
                {
                    Matriculas.Add(matricula);
                }

                // Se houver texto na busca, aplica o filtro
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchMatriculas();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Falha ao carregar matrículas: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // Comando para o RefreshView
        [RelayCommand]
        private async Task RefreshAsync()
        {
            // Limpa a busca e recarrega tudo
            SearchText = string.Empty;
            await LoadMatriculasAsync();
        }

        // Comando do botão "Buscar"
        [RelayCommand]
        private void SearchMatriculas()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Se a busca está vazia, mostra todos
                Matriculas.Clear();
                foreach (var m in _allMatriculas)
                {
                    Matriculas.Add(m);
                }
                return;
            }

            // Filtra a lista _allMatriculas baseado no texto e no filtro
            List<MatriculaDTO> filteredList;

            if (SelectedFilterType == "Nome do Aluno")
            {
                filteredList = _allMatriculas
                    .Where(m => m.AlunoMatricula.Nome.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else // "CPF do Aluno"
            {
                filteredList = _allMatriculas
                    .Where(m => m.AlunoMatricula.Cpf.Contains(SearchText))
                    .ToList();
            }

            // Atualiza a coleção observável
            Matriculas.Clear();
            foreach (var m in filteredList)
            {
                Matriculas.Add(m);
            }
        }

        // Comando do botão "+" (FAB) e do "Adicionar Primeiro"
        [RelayCommand]
        private async Task AddMatriculaAsync()
        {
            if (IsBusy) return;
            // Navega para a página de detalhes (que já criamos)
            await Shell.Current.GoToAsync(nameof(MatriculaPage));
        }

        // Comando chamado pelo code-behind (OnEditButtonClicked)
        [RelayCommand]
        private async Task EditMatriculaAsync(MatriculaDTO matricula)
        {
            if (matricula == null) return;

            // Navega para a página de detalhes, passando o ID da matrícula
            await Shell.Current.GoToAsync($"{nameof(MatriculaPage)}?Id={matricula.Id}");
        }

        // Comando chamado pelo code-behind (OnDeleteButtonClicked)
        [RelayCommand]
        private async Task DeleteMatriculaAsync(MatriculaDTO matricula)
        {
            if (matricula == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Excluir Matrícula",
                $"Tem certeza que deseja excluir a matrícula de {matricula.AlunoMatricula.Nome}?",
                "Sim", "Não");

            if (!confirm) return;

            if (IsBusy) return;
            IsBusy = true;

            try
            {
                await _matriculaService.RemoverAsync(matricula.Id);
                await Shell.Current.DisplayAlert("Sucesso", "Matrícula excluída.", "OK");

                // Remove da lista local (para não ter que recarregar do banco)
                _allMatriculas.Remove(matricula);
                Matriculas.Remove(matricula);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Falha ao excluir matrícula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}