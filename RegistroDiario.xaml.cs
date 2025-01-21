using Microsoft.Data.Sqlite;

namespace DiarioBienestar2
{
    // La clase RegistroDiario maneja la funcionalidad de la interfaz para registrar datos diarios.
    public partial class RegistroDiario : ContentPage
    {
        // Cadena de conexión para la base de datos
        private const string ConnectionString = "Data Source=diario.db";

        // Constructor de la página
        public RegistroDiario()
        {
            InitializeComponent();
        }

        // Maneja el cambio del valor del Slider, ajustando el progreso de la barra
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Establecer el valor del progreso de la barra
            progressBar.Progress = slider.Value / 10;
        }

        // Maneja el cambio del valor del Stepper, actualizando el texto de la etiqueta correspondiente
        private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Actualizar el texto de la etiqueta con el valor del stepper
            stepperLabel.Text = stepper.Value.ToString();
        }

        // Maneja el evento de clic en el botón Guardar, insertando los datos en la base de datos
        private void OnGuardarClicker(object sender, EventArgs e)
        {
            // Usar 'using' para asegurarse de que la conexión se cierre correctamente
            using (var connection = new SqliteConnection(ConnectionString))
            {
                // Abrir la conexión a la base de datos
                connection.Open();

                // Definir la consulta SQL para insertar los datos
                string insertQuery = @"
                    INSERT INTO registroDiario (
                        id_usuario, detalles_dia, intensidad, nivel_energia, fecha
                    ) VALUES (
                        @id_usuario, @detalles_dia, @intensidad, @nivel_energia, @fecha
                    );";

                // Usar un comando SQL parametrizado para evitar inyecciones SQL
                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    // Agregar parámetros a la consulta
                    command.Parameters.AddWithValue("@id_usuario", Identificador.Id); // Suponiendo que Identificador.Id es el ID del usuario
                    command.Parameters.AddWithValue("@detalles_dia", detallesDia.Text);
                    command.Parameters.AddWithValue("@intensidad", slider.Value);
                    command.Parameters.AddWithValue("@nivel_energia", stepper.Value);
                    command.Parameters.AddWithValue("@fecha", fecha.Date);

                    // Ejecutar la consulta SQL
                    command.ExecuteNonQuery();
                }

                // Limpiar los campos después de guardar
                LimpiarCampos();

                // Cerrar la conexión
                connection.Close();
            }
        }

        // Método para limpiar los campos después de guardar
        private void LimpiarCampos()
        {
            detallesDia.Text = null; // Limpiar el campo de detalles del día
            slider.Value = 0;        // Restablecer el valor del slider
            stepper.Value = 0;       // Restablecer el valor del stepper
        }
    }
}
