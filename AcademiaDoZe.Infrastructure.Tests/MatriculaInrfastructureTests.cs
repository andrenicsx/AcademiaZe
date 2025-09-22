// André Nícolas Granemann Coelho
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Infrastructure.Tests;
using System;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    [Fact]
    public async Task Matricula_Adicionar()
    {
        var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
        var logradouro = await repoLogradouro.ObterPorId(4);
        Assert.NotNull(logradouro);

        var arquivo = Arquivo.Criar(new byte[] { 1, 2, 3 });
        var random = new Random();
        var cpf = $"1234567{random.Next(1000, 9999)}"; // sempre 11 dígitos
        var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);
        Assert.False(await repoAluno.CpfJaExiste(cpf), "CPF já existe no banco.");

        var aluno = Aluno.Criar(
            1, 
            "Aluno Teste",
            cpf,
            new DateOnly(2010, 10, 09),
            "49999999999",
            "aluno@teste.com",
            logradouro!,
            "123",
            "Complemento casa",
            "Senha@123",
            arquivo
        );
        await repoAluno.Adicionar(aluno);

        var matricula = Matricula.Criar(
            1, 
            aluno,
            EMatriculaPlano.Semestral,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            "Emagrecer",
            EMatriculaRestricoes.Alergias,
            arquivo,
            "Sem observações"
        );

        var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);
        var matriculaInserida = await repoMatricula.Adicionar(matricula);

        Assert.NotNull(matriculaInserida);
        Assert.True(matriculaInserida.Id > 0);
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_Atualizar()
    {
        var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);
        var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

        var aluno = await repoAluno.ObterPorCpf("11140608981");
        Assert.NotNull(aluno);

        var matriculas = (await repoMatricula.ObterPorAluno(aluno!.Id)).ToList();
        Assert.NotEmpty(matriculas);

        var matricula = matriculas.First(); // pega a primeira matrícula

        var arquivo = Arquivo.Criar(new byte[] { 1, 2, 3 });
        var matriculaAtualizada = Matricula.Criar(
            1, 
            aluno,
            EMatriculaPlano.Anual,
            new DateOnly(2020, 05, 20),
            new DateOnly(2020, 05, 20).AddMonths(12),
            "Hipertrofia",
            EMatriculaRestricoes.Alergias,
            arquivo,
            "Observação atualizada"
        );

        typeof(Entity).GetProperty("Id")?.SetValue(matriculaAtualizada, matricula.Id);

        var resultado = await repoMatricula.Atualizar(matriculaAtualizada);

        Assert.NotNull(resultado);
        Assert.Equal("Hipertrofia", resultado.Objetivo);
        Assert.Equal("Observação atualizada", resultado.ObservacoesRestricoes);
        Assert.Equal(EMatriculaPlano.Anual, resultado.Plano);
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_Remover_ObterPorId()
    {
        var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);
        var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);

        var aluno = await repoAluno.ObterPorCpf("11140608981");
        Assert.NotNull(aluno);

        var matriculas = (await repoMatricula.ObterPorAluno(aluno!.Id)).ToList();
        Assert.NotEmpty(matriculas);

        var matricula = matriculas.First(); // pega a primeira matrícula
        
        // Remover
        var resultadoRemocao = await repoMatricula.Remover(matricula.Id);

        Assert.True(resultadoRemocao);

        // Verificar se foi removida
        var matriculaRemovida = await repoMatricula.ObterPorId(matricula.Id);
        Assert.Null(matriculaRemovida);
    }

    [Fact]
    public async Task Matricula_ObterTodos()
    {
        var repoLogradouro = new LogradouroRepository(ConnectionString, DatabaseType);
        var logradouro = await repoLogradouro.ObterPorId(4);
        Assert.NotNull(logradouro);

        var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);
        var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);

        // Arquivo para foto e laudo
        var arquivo = Arquivo.Criar(new byte[] { 1, 2, 3 });

        // CPF de teste
        var cpfAluno = "11140608981";

        // Apaga registros antigos de teste (evita conflito de CPF)
        var alunoExistente = await repoAluno.ObterPorCpf(cpfAluno);
        if (alunoExistente != null)
        {
            var matriculasAntigas = (await repoMatricula.ObterPorAluno(alunoExistente.Id)).ToList();
            foreach (var mat in matriculasAntigas)
            {
                await repoMatricula.Remover(mat.Id);
            }
            await repoAluno.Remover(alunoExistente.Id);
            alunoExistente = null;
        }

        // Cria o aluno de teste
        var aluno = Aluno.Criar(
            0,
            "Aluno Teste",
            cpfAluno,
            new DateOnly(2010, 10, 09), // menor de 16 anos
            "49999999999",
            "aluno@teste.com",
            logradouro!,
            "123",
            "Complemento casa",
            "Senha@123",
            arquivo // foto obrigatória
        );
        await repoAluno.Adicionar(aluno);

        // Cria matrícula de teste
        var matricula = Matricula.Criar(
            0,
            aluno,
            EMatriculaPlano.Semestral,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            "Emagrecer",
            EMatriculaRestricoes.Alergias,
            arquivo, // laudo obrigatório para menores de 16
            "Sem observações"
        );
        await repoMatricula.Adicionar(matricula);

        // Testa ObterTodos
        var resultado = await repoMatricula.ObterTodos();
        Assert.NotNull(resultado);
        Assert.NotEmpty(resultado);
    }

    [Fact]
    public async Task Matricula_ObterPorId()
    {
        var repoAluno = new AlunoRepository(ConnectionString, DatabaseType);
        var repoMatricula = new MatriculaRepository(ConnectionString, DatabaseType);
        var random = new Random();
        var cpf = $"1234567{random.Next(1000, 9999)}"; // sempre 11 dígitos

        // Cria o aluno menor de 16 anos com foto
        var aluno = Aluno.Criar(
            1,
            "Aluno Teste",
            cpf,
            new DateOnly(2010, 10, 09), // menor de 16 anos
            "49999999999",
            "aluno@teste.com",
            await new LogradouroRepository(ConnectionString, DatabaseType).ObterPorId(4),
            "123",
            "Complemento casa",
            "Senha@123",
            Arquivo.Criar(new byte[] { 1, 2, 3 }) // foto obrigatória
        );
        await repoAluno.Adicionar(aluno);

        // Cria a matrícula com o laudo médico obrigatório
        var matricula = Matricula.Criar(
            1,
            aluno,
            EMatriculaPlano.Semestral,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            "Emagrecer",
            EMatriculaRestricoes.Alergias,
            Arquivo.Criar(new byte[] { 1, 2, 3 }), // laudo médico obrigatório
            "Sem observações"
        );
        await repoMatricula.Adicionar(matricula);

        // Testa a obtenção da matrícula pelo ID
        var matriculaPorId = await repoMatricula.ObterPorId(matricula.Id);
        Assert.NotNull(matriculaPorId);
        Assert.Equal(matricula.Id, matriculaPorId.Id);
    }
}