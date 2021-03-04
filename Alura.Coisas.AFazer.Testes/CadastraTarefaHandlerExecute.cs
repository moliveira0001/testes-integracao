using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Alura.Coisas.AFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void TarefaDadaInfoCorretaCadastraNoBD()
        {

            //arange
            var comando = new CadastraTarefa("Estudar XUnit",
                                              new Categoria("Estudo"),
                                              DateTime.Now.AddDays(1)
                                             );

            //var repositorio = new RepositorioTarefasFake();

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;


            var context = new DbTarefasContext(options);


            var repositorio = new RepositorioTarefa(context);

            var handler = new CadastraTarefaHandler(repositorio, mock.Object);

            //act 

            handler.Execute(comando);

            //assert

            var tarefa = repositorio.ObtemTarefas(t => t.Titulo == "Estudar XUnit");

            Assert.NotNull(tarefa);
        }


        delegate void CapturaMensagemLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInfoValidasDeveLogar()
        {
            //arrange
            var tituloTarefaEsperado = "Usar Moq para aprofundar conhecimento de API";
            var comando = new CadastraTarefa(tituloTarefaEsperado, new Categoria(100, "Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturaMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(), //nível de log => LogError
                    It.IsAny<EventId>(), //identificador do evento
                    It.IsAny<object>(), //objeto que será logado
                    It.IsAny<Exception>(),    //exceção que será logada
                    It.IsAny<Func<object, Exception, string>>() //função que converte objeto+exceção >> string)
                )).Callback(captura);

            var mock = new Mock<IRepositorioTarefas>();

            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //act
            handler.Execute(comando); //SUT >> CadastraTarefaHandlerExecute

            //assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains(tituloTarefaEsperado, mensagemCapturada);
        }


        [Fact]
        public void QuandoExceptionForLancadaIssSuccessDeveSerFalse()
        {

            //arange
            var comando = new CadastraTarefa("Estudar XUnit",
                                              new Categoria("Estudo"),
                                              DateTime.Now.AddDays(1)
                                             );



            var mock = new Mock<IRepositorioTarefas>();

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Erro ao Incluir Tarefa"));

            var repo = mock.Object;

         

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act 

           CommandResult result = handler.Execute(comando);

            //assert

            Assert.False(result.IsSuccess);

        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogar()
        {

            //arange
            var comando = new CadastraTarefa("Estudar XUnit",
                                              new Categoria("Estudo"),
                                              DateTime.Now.AddDays(1)
                                             );

            var Exception =new Exception("Erro ao Incluir Tarefa");

            var mock = new Mock<IRepositorioTarefas>();

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(Exception);

            var repo = mock.Object;



            var handler = new CadastraTarefaHandler(repo,mockLogger.Object);

            //act 

            CommandResult result = handler.Execute(comando);

            //assert

            mockLogger.Verify(l => l.Log(LogLevel.Error,
                                         It.IsAny<EventId>(),
                                         It.IsAny<object>(),
                                         Exception, 
                                         It.IsAny<Func<object,Exception,string>>()
                                         )
                           , Times.Once());

        }
    }
}
