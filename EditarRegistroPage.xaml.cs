using Microsoft.Data.Sqlite;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace DiarioBienestar2
{
    // La clase EditarRegistroPage se encarga de la lógica para editar los detalles de un registro específico en el diario.
    public partial class EditarRegistroPage : ContentPage
    {
        // Cadena de conexión a la base de datos SQLite
        private string connectionString = "Data Source=diario.db";

        // Propiedad que contiene el registro que se va a editar
        public Registro Registro { get; set; }

        // Constructor que inicializa la página con los datos del registro seleccionado para su edición
        public EditarRegistroPage(Registro registro)
        {
            InitializeComponent(); // Inicializar los componentes de la interfaz de usuario
            Registro = registro; // Asignar el registro a la propiedad
            BindingContext = Registro; // Establecer el contexto de enlace de datos a Registro
        }

        // Evento que se ejecuta cuando el valor del slider de intensidad cambia
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Convertir el valor del slider a un entero
            int valorIntensidad = (int)intensidadSlider.Value;
            Registro.Intensidad = valorIntensidad.ToString(); // Actualizar la propiedad Intensidad del registro
            intendidadTxt.Text = valorIntensidad.ToString(); // Actualizar el texto mostrado en la interfaz
        }

        // Evento que se ejecuta cuando el valor del stepper de energía cambia
        private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Actualizar el texto del stepper con el valor actual
            stepperLabel.Text = stepper.Value.ToString();
        }

        // Método asincrónico que guarda los cambios realizados en el registro en la base de datos
        private async void GuardarCambios(object sender, EventArgs e)
        {
            try
            {
                // Conexión a la base de datos SQLite
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open(); // Abrir la conexión

                    // Consulta SQL para actualizar los datos del registro en la base de datos
                    string query = @"
                        UPDATE registroDiario
                        SET detalles_dia = @detalles,
                            intensidad = @intensidad,
                            nivel_energia = @nivelEnergia
                        WHERE id = @id";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        // Asignar los valores de los parámetros de la consulta
                        command.Parameters.AddWithValue("@detalles", Registro.Detalles);
                        command.Parameters.AddWithValue("@intensidad", Registro.Intensidad); // Guardar la intensidad como string
                        command.Parameters.AddWithValue("@nivelEnergia", Registro.NivelEnergia);
                        command.Parameters.AddWithValue("@id", Registro.Id);

                        command.ExecuteNonQuery(); // Ejecutar la consulta de actualización
                    }
                }

                // Mostrar un mensaje de éxito y volver a la página anterior
                await DisplayAlert("Éxito", "Registro actualizado correctamente.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                // Mostrar un mensaje de error si ocurre una excepción
                await DisplayAlert("Error", $"Error al guardar cambios: {ex.Message}", "OK");
            }
        }
    }
}
