using Microsoft.Data.Sqlite;

namespace DiarioBienestar2
{
    // La clase Inicio maneja la pantalla principal donde se muestra el estado general del diario y las estadísticas.
    public partial class Inicio : ContentPage
    {
        // Cadena de conexión a la base de datos
        private const string ConnectionString = "Data Source=diario.db";

        // Constructor de la clase, inicializa la página y carga la información relevante
        public Inicio()
        {
            InitializeComponent();
            Bienvenida(); // Mostrar mensaje de bienvenida
            cargarDiario(); // Cargar información del diario del día actual
            CargarEstadisticasSemanal(); // Cargar estadísticas semanales
        }

        // Método que se ejecuta cada vez que la página aparece, recarga los datos
        protected override void OnAppearing()
        {
            base.OnAppearing();
            cargarDiario();
            CargarEstadisticasSemanal();
        }

        // Método que muestra un mensaje de bienvenida al usuario
        private void Bienvenida()
        {
            // Establecer conexión con la base de datos
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    SELECT usuario FROM usuarios
                    WHERE id= @id;"; // Consultar el nombre del usuario por su ID

                // Ejecutar la consulta SQL
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", Identificador.Id); // Usar el ID global del usuario

                    using (var reader = command.ExecuteReader())
                    {
                        // Si se encuentra el usuario, mostrar el mensaje de bienvenida
                        if (reader.Read())
                        {
                            msgBienvenida.Text = $"Bienvenido {reader["usuario"]?.ToString()}"; // Actualizar el texto con el nombre del usuario
                        }
                    }
                }

                connection.Close();
            }
        }

        // Método que carga los datos del diario del día actual (intensidad y energía)
        void cargarDiario()
        {
            try
            {
                // Conexión a la base de datos
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT intensidad, 
                               nivel_energia
                        FROM registroDiario
                        WHERE DATE(fecha)= DATE('now') AND id = @id;"; // Consultar los valores de intensidad y energía del día actual

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", Identificador.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Obtener y manejar la intensidad y energía
                                int intensidad = reader["intensidad"] != DBNull.Value
                                    ? Convert.ToInt32(reader["intensidad"])
                                    : 0;

                                int nivelEnergia = reader["nivel_energia"] != DBNull.Value
                                    ? Convert.ToInt32(reader["nivel_energia"])
                                    : 0;

                                // Actualizar los controles en la interfaz de usuario
                                intensity.Text = $" {intensidad.ToString()}";
                                energy.Text = $" {nivelEnergia.ToString()}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones en caso de error en la base de datos
                DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        // Método que carga las estadísticas semanales, comparando la semana actual con la anterior
        private void CargarEstadisticasSemanal()
        {
            double promedioActividadSemanaActual = 0;
            double promedioEnergiaSemanaActual = 0;
            double promedioActividadSemanaAnterior = 0;
            double promedioEnergiaSemanaAnterior = 0;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Obtener el promedio de la actividad y energía para la semana actual
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT AVG(intensidad) AS PromedioActividad, 
                           AVG(nivel_energia) AS PromedioEnergia
                    FROM registroDiario
                    WHERE fecha >= date('now', '-7 days') and id_usuario = @id;
                ";

                command.Parameters.AddWithValue("@id", Identificador.Id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Obtener promedios de la semana actual
                        promedioActividadSemanaActual = reader["PromedioActividad"] != DBNull.Value
                            ? Convert.ToDouble(reader["PromedioActividad"])
                            : 0;

                        promedioEnergiaSemanaActual = reader["PromedioEnergia"] != DBNull.Value
                            ? Convert.ToDouble(reader["PromedioEnergia"])
                            : 0;
                    }
                }

                // Obtener el promedio de la actividad y energía para la semana anterior
                command.CommandText = @"
                    SELECT AVG(intensidad) AS PromedioActividad, 
                           AVG(nivel_energia) AS PromedioEnergia
                    FROM registroDiario
                    WHERE fecha >= date('now', '-14 days') and fecha < date('now', '-7 days') and id_usuario = @id;
                ";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Obtener promedios de la semana anterior
                        promedioActividadSemanaAnterior = reader["PromedioActividad"] != DBNull.Value
                            ? Convert.ToDouble(reader["PromedioActividad"])
                            : 0;

                        promedioEnergiaSemanaAnterior = reader["PromedioEnergia"] != DBNull.Value
                            ? Convert.ToDouble(reader["PromedioEnergia"])
                            : 0;
                    }
                }
            }

            // Comparar los promedios de la semana actual y la anterior y actualizar la interfaz
            string mensajeComparacion = "Progreso esta semana: ";

            // Comparar actividad
            if (promedioActividadSemanaActual > promedioActividadSemanaAnterior)
            {
                mensajeComparacion += $"Mejor en actividad en un {promedioActividadSemanaActual - promedioActividadSemanaAnterior} más de promedio";
                progressActividad.ProgressColor = Colors.Green; // Color verde si es mejor
            }
            else if (promedioActividadSemanaActual < promedioActividadSemanaAnterior)
            {
                mensajeComparacion += $"Peor en actividad en un {promedioActividadSemanaAnterior - promedioActividadSemanaActual} menos de promedio";
                progressActividad.ProgressColor = Colors.Red; // Color rojo si es peor
            }
            else
            {
                mensajeComparacion += "Igual en actividad";
                progressActividad.ProgressColor = Colors.Gray; // Color gris si es igual
            }

            // Comparar energía
            if (promedioEnergiaSemanaActual > promedioEnergiaSemanaAnterior)
            {
                mensajeComparacion += $"\nMejor en energía en un {promedioEnergiaSemanaActual - promedioEnergiaSemanaAnterior} más de promedio";
                progressEnergia.ProgressColor = Colors.Green; // Color verde si es mejor
            }
            else if (promedioEnergiaSemanaActual < promedioEnergiaSemanaAnterior)
            {
                mensajeComparacion += $"\nPeor en energía en un {promedioActividadSemanaAnterior - promedioActividadSemanaActual} menos de promedio";
                progressEnergia.ProgressColor = Colors.Red; // Color rojo si es peor
            }
            else
            {
                mensajeComparacion += "\nIgual en energía";
                progressEnergia.ProgressColor = Colors.Gray; // Color gris si es igual
            }

            // Mostrar el mensaje de comparación
            labelComparacion.Text = mensajeComparacion;

            // Actualizar las barras de progreso con los valores de las estadísticas
            progressActividad.Progress = promedioActividadSemanaActual / 10; // Se asume que el máximo es 10
            progressEnergia.Progress = promedioEnergiaSemanaActual / 10; // Se asume que el máximo es 10
        }

        // Navegar a la página de registros
        private async void OnVerDiarioClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListaRegistros());
        }

        // Navegar a la página de registro diario
        private async void OnAgregarEntradaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroDiario());
        }

        // Navegar a la página de estadísticas
        private async void OnVerEstadisticasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Estadisticas());
        }
    }
}
