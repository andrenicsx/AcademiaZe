
// Salve este arquivo como:
// AcademiaDoZe.Presentation.AppMaui/ViewModels/MatriculaViewModel.cs

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    [QueryProperty(nameof(MatriculaId), "Id")]
    public partial class MatriculaViewModel : BaseViewModel
    {
        private readonly IMatriculaService _matriculaService;
        private readonly IAlunoService _alunoService;

        // Propriedade principal, usando seu DTO
        [ObservableProperty]
        private MatriculaDTO _matricula = new()
        {
            // --- Campos da Matrícula ---
            Plano = EAppMatriculaPlano.Mensal,
            DataInicio = DateOnly.FromDateTime(DateTime.Today),
            DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            Objetivo = string.Empty,
            RestricoesMedicas = EAppMatriculaRestricoes.None,

            // --- Campos 'required' do AlunoDTO ---
            AlunoMatricula = new AlunoDTO
            {
                Id = 0,
                Nome = string.Empty,
                Cpf = string.Empty,
                DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                Telefone = string.Empty,
                Email = string.Empty,
                Endereco = new LogradouroDTO
                {
                    Id = 0,
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = string.Empty
                },
                Numero = string.Empty,
                Complemento = string.Empty,
                Senha = string.Empty,
                Foto = null
            }
        };

        [ObservableProperty]
        private int _matriculaId;

        [ObservableProperty]
        private bool _isEditMode;

        // Propriedades para a UI
        [ObservableProperty]
        private string _cpfBusca = string.Empty;

        [ObservableProperty]
        private string _textoBotaoLaudo = "Anexar Laudo";

        // ----- PROPRIEDADES PARA OS ENUMS -----
        public ObservableCollection<EAppMatriculaPlano> PlanosOpcoes { get; }
        public ObservableCollection<RestricaoWrapper> RestricoesOpcoes { get; }

        // ----- CONSTRUTOR -----
        public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
        {
            _matriculaService = matriculaService;
            _alunoService = alunoService;
            Title = "Nova Matrícula";

            PlanosOpcoes = new ObservableCollection<EAppMatriculaPlano>(
              Enum.GetValues(typeof(EAppMatriculaPlano)).Cast<EAppMatriculaPlano>()
            );

            RestricoesOpcoes = new ObservableCollection<RestricaoWrapper>();

            // 🔥 CORREÇÃO: Chama LoadRestricoes no construtor para garantir que a CollectionView
            // tenha dados para renderizar assim que a página for criada.
            LoadRestricoes();
        }

        // ----- INICIALIZAÇÃO -----
        public async Task InitializeAsync()
        {
            if (MatriculaId > 0)
            {
                IsEditMode = true;
                Title = "Editar Matrícula";
                await LoadMatriculaAsync();
            }
            else
            {
                IsEditMode = false;
                Title = "Nova Matrícula";

                Matricula = new() // Reseta o DTO com todos os required
                {
                    Plano = EAppMatriculaPlano.Mensal,
                    DataInicio = DateOnly.FromDateTime(DateTime.Today),
                    DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    Objetivo = string.Empty,
                    RestricoesMedicas = EAppMatriculaRestricoes.None,
                    AlunoMatricula = new AlunoDTO
                    {
                        Id = 0,
                        Nome = string.Empty,
                        Cpf = string.Empty,
                        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                        Telefone = string.Empty,
                        Email = string.Empty,
                        Endereco = new LogradouroDTO
                        {
                            Id = 0,
                            Cep = string.Empty,
                            Nome = string.Empty,
                            Bairro = string.Empty,
                            Cidade = string.Empty,
                            Estado = string.Empty,
                            Pais = string.Empty
                        },
                        Numero = string.Empty,
                        Complemento = string.Empty,
                        Senha = string.Empty,
                        Foto = null
                    }
                };

                CpfBusca = string.Empty;
                TextoBotaoLaudo = "Anexar Laudo";

                // Removemos LoadRestricoes daqui pois já está no construtor
                // e só é necessário no LoadMatriculaAsync para carregar o estado
                // ou se for resetar o DTO (que já é feito no LoadMatriculaAsync e no construtor).
            }
        }

        [RelayCommand]
        private async Task LoadMatriculaAsync()
        {
            if (MatriculaId <= 0) return;
            try
            {
                IsBusy = true;
                var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId);
                if (matriculaData != null)
                {
                    Matricula = matriculaData;
                    if (Matricula.LaudoMedico != null)
                        TextoBotaoLaudo = "Laudo Anexado";

                    LoadRestricoes(); // Carrega os checkboxes com os dados salvos
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ----- COMANDOS (BOTÕES) -----

        [RelayCommand]
        private async Task SearchAlunoByCpfAsync()
        {
            if (string.IsNullOrWhiteSpace(CpfBusca))
            {
                await Shell.Current.DisplayAlert("Atenção", "Digite um CPF para buscar.", "OK");
                return;
            }
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var aluno = await _alunoService.ObterPorCpfAsync(CpfBusca);

                if (aluno != null)
                {
                    Matricula.AlunoMatricula = aluno;
                    OnPropertyChanged(nameof(Matricula)); // Notifica a UI
                }
                else
                {
                    await Shell.Current.DisplayAlert("Não Encontrado", "Nenhum aluno encontrado com este CPF.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar aluno: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveMatriculaAsync()
        {
            if (IsBusy) return;
            UpdateRestricoesEnum();

            if (!await ValidateMatricula(Matricula)) return;

            try
            {
                IsBusy = true;
                if (IsEditMode)
                {
                    var dtoAtualizado = await _matriculaService.AtualizarAsync(Matricula);
                    await Shell.Current.DisplayAlert("Sucesso", "Matrícula atualizada com sucesso!", "OK");
                }
                else
                {
                    var dtoAdicionado = await _matriculaService.AdicionarAsync(Matricula);
                    await Shell.Current.DisplayAlert("Sucesso", "Matrícula criada com sucesso!", "OK");
                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro ao Salvar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelecionarLaudoAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(
                  new Dictionary<DevicePlatform, IEnumerable<string>>
                  {
                    { DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf" } },
                    { DevicePlatform.Android, new[] { "image/*", "application/pdf" } },
                    { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf" } },
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "public.image", "com.adobe.pdf" } },
                  });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o Laudo (Imagem ou PDF)",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    Matricula.LaudoMedico = new ArquivoDTO { Conteudo = ms.ToArray() };
                    OnPropertyChanged(nameof(Matricula));
                    TextoBotaoLaudo = result.FileName;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar laudo: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        // ----- REGRAS DE NEGÓCIO (VALIDAÇÃO) -----
        private async Task<bool> ValidateMatricula(MatriculaDTO matricula)
        {
            const string validationTitle = "Validação";

            if (matricula.AlunoMatricula == null || matricula.AlunoMatricula.Id <= 0)
            {
                await Shell.Current.DisplayAlert(validationTitle, "Você precisa buscar e selecionar um aluno.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(matricula.Objetivo))
            {
                await Shell.Current.DisplayAlert(validationTitle, "O campo Objetivo é obrigatório.", "OK");
                return false;
            }

            if (matricula.DataFim <= matricula.DataInicio)
            {
                await Shell.Current.DisplayAlert(validationTitle, "A Data Final deve ser maior que a Data de Início.", "OK");
                return false;
            }

            if (!IsEditMode)
            {
                var matriculasAtivas = await _matriculaService.ObterAtivasAsync(matricula.AlunoMatricula.Id);
                if (matriculasAtivas.Any())
                {
                    await Shell.Current.DisplayAlert(validationTitle, "Este aluno já possui uma matrícula ativa.", "OK");
                    return false;
                }
            }

            var hoje = DateTime.Today;
            var idade = hoje.Year - matricula.AlunoMatricula.DataNascimento.Year;
            if (matricula.AlunoMatricula.DataNascimento.ToDateTime(TimeOnly.MinValue) > hoje.AddYears(-idade))
                idade--;

            bool laudoObrigatorioPorIdade = (idade >= 12 && idade <= 16);
            bool laudoObrigatorioPorRestricao = (matricula.RestricoesMedicas != EAppMatriculaRestricoes.None);

            if ((laudoObrigatorioPorIdade || laudoObrigatorioPorRestricao) && (matricula.LaudoMedico == null || matricula.LaudoMedico.Conteudo.Length == 0))
            {
                string motivo = laudoObrigatorioPorIdade ? "por ter entre 12 e 16 anos." : "pois foram informadas restrições de saúde.";
                await Shell.Current.DisplayAlert(validationTitle, $"O Laudo Médico é obrigatório. Motivo: {motivo}", "OK");
                return false;
            }

            return true;
        }

        // ----- Lógica para os CheckBoxes [Flags] -----

        // ESTA É A VERSÃO CORRETA E ÚNICA de LoadRestricoes
        private void LoadRestricoes()
        {
            // O Clear é fundamental para evitar duplicatas em caso de LoadMatriculaAsync repetido
            RestricoesOpcoes.Clear();
            var currentRestricoes = Matricula.RestricoesMedicas;

            // Adicionamos manualmente para garantir que os nomes apareçam

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Diabetes",
                Valor = EAppMatriculaRestricoes.Diabetes,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.Diabetes)
            });

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Pressão Alta",
                Valor = EAppMatriculaRestricoes.PressaoAlta,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.PressaoAlta)
            });

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Labirintite",
                Valor = EAppMatriculaRestricoes.Labirintite,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.Labirintite)
            });

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Alergias",
                Valor = EAppMatriculaRestricoes.Alergias,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.Alergias)
            });

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Problemas Respiratórios",
                Valor = EAppMatriculaRestricoes.ProblemasRespiratorios,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.ProblemasRespiratorios)
            });

            RestricoesOpcoes.Add(new RestricaoWrapper
            {
                Nome = "Remédio Contínuo",
                Valor = EAppMatriculaRestricoes.RemedioContinuo,
                IsChecked = currentRestricoes.HasFlag(EAppMatriculaRestricoes.RemedioContinuo)
            });
        }

        private void UpdateRestricoesEnum()
        {
            var restricoesSelecionadas = EAppMatriculaRestricoes.None;
            foreach (var wrapper in RestricoesOpcoes.Where(w => w.IsChecked))
            {
                restricoesSelecionadas |= wrapper.Valor; // Combina os [Flags]
            }
            Matricula.RestricoesMedicas = restricoesSelecionadas;
        }
    }

    /// <summary>
    /// Classe auxiliar para o binding dos CheckBoxes
    /// </summary>
    public partial class RestricaoWrapper : ObservableObject
    {
        // O Community Toolkit gera o evento de notificação para a UI (INotifyPropertyChanged)
        [ObservableProperty]
        private bool _isChecked;

        // Nome não precisa de ObservableProperty, pois só é lido (não muda)
        public string Nome { get; set; } = string.Empty;
        public EAppMatriculaRestricoes Valor { get; set; }
    }
}
