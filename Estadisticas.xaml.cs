using Microsoft.Data.Sqlite;

namespace DiarioBienestar2;

// La clase Estadisticas se encarga de mostrar las estadísticas de la actividad física y energía de la última semana.
public partial class Estadisticas : ContentPage
{
    // Constructor que inicializa los componentes y carga las estadísticas al inicio
    public Estadisticas()
    {
        InitializeComponent(); // Inicializar los componentes de la interfaz
        CargarEstadisticas(); // Llamar al método para cargar las estadísticas al cargar la página
    }

    // Método que se ejecuta cada vez que la página aparece, para recargar las estadísticas
    protected override void OnAppearing()
    {
        base.OnAppearing(); // Llamar a la implementación base
        CargarEstadisticas(); // Recargar las estadísticas cada vez que la página aparece
    }

    // Método que carga las estadísticas relacionadas con la actividad física y nivel de energía
    private void CargarEstadisticas()
    {
        // Variables locales para almacenar los valores de las estadísticas
        double promedioActividad = 0;
        double promedioEnergia = 0;
        int registrosTotales = 0;

        // Conexión a la base de datos SQLite
        using (var connection = new SqliteConnection("Data Source=diario.db"))
        {
            connection.Open(); // Abrir la conexión a la base de datos

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT AVG(intensidad) AS PromedioActividad, 
                       AVG(nivel_energia) AS PromedioEnergia, 
                       COUNT(*) AS TotalRegistros
                FROM registroDiario
                WHERE fecha >= date('now', '-7 days');"; // Consultar el promedio de actividad y energía, y el total de registros de los últimos 7 días

            // Ejecutar la consulta y leer los resultados
            using (var reader = command.ExecuteReader())
            {
                // Leer los resultados de la consulta
                if (reader.Read())
                {
                    // Asignar los valores obtenidos de la consulta a las variables
                    promedioActividad = reader["PromedioActividad"] != DBNull.Value
                        ? Convert.ToDouble(reader["PromedioActividad"]) // Si no es nulo, convertir a double
                        : 0; // Si es nulo, asignar 0

                    promedioEnergia = reader["PromedioEnergia"] != DBNull.Value
                        ? Convert.ToDouble(reader["PromedioEnergia"]) // Si no es nulo, convertir a double
                        : 0; // Si es nulo, asignar 0

                    registrosTotales = reader["TotalRegistros"] != DBNull.Value
                        ? Convert.ToInt32(reader["TotalRegistros"]) // Convertir el total de registros a entero
                        : 0; // Si es nulo, asignar 0
                }
            }
        }

        // Actualizar las etiquetas de la interfaz con los promedios obtenidos
        lblPromedioActividad.Text = $"Actividad física: {promedioActividad:F2}"; // Mostrar el promedio de actividad con 2 decimales
        lblPromedioEnergia.Text = $"Nivel de energía: {promedioEnergia:F2}"; // Mostrar el promedio de energía con 2 decimales

        // Calcular el progreso general como el promedio de la actividad física y energía
        double progreso = (promedioActividad + promedioEnergia) / 2.0 / 10.0; // Calcular el progreso como el promedio de los dos valores y normalizarlo a un valor entre 0 y 1

        // Actualizar el progreso general en la barra de progreso
        progressGeneral.Progress = progreso;

        // Actualizar la etiqueta de progreso mostrando el porcentaje
        lblProgreso.Text = $"{progreso * 100:F0}%"; // Mostrar el progreso en porcentaje con 0 decimales
    }
}
