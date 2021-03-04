using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;

namespace Alura.Coisas.AFazer.Testes
{
    public class ObtemCategoriaPorIDExecute
    {
        [Fact]
        public void QuandoPassarUmIdDeveChamarUmaUnicaVez()
        {

            var idCategoria = 20;

            var command = new ObtemCategoriaPorId(idCategoria);


            var mock = new Mock<IRepositorioTarefas>();

            var repo = mock.Object;

            var handler = new ObtemCategoriaPorIdHandler(repo);


            handler.Execute(command);



            mock.Verify(r => r.ObtemCategoriaPorId(idCategoria), Times.Once());
        }
    }
}
