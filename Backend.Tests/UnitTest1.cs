using Xunit;
using Backend.Models;

namespace Backend.Tests
{
    public class DomainModelTests
    {
        [Fact]
        public void Pregunta_Initialization_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var pregunta = new Pregunta();

            // Assert
            Assert.True(pregunta.EsObligatoria);
            Assert.Equal("+57", pregunta.PrefijoPais);
            Assert.Equal(string.Empty, pregunta.Restriccion);
        }

        [Fact]
        public void Encuesta_Initialization_ShouldHaveEmptyQuestionsList()
        {
            // Arrange & Act
            var encuesta = new Encuesta();

            // Assert
            Assert.NotNull(encuesta.Preguntas);
            Assert.Empty(encuesta.Preguntas);
        }
    }
}
