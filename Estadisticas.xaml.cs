using Microsoft.Data.Sqlite;

namespace DiarioBienestar2;

public partial class Estadisticas : ContentPage
{
    public Estadisticas()
    {
        InitializeComponent();
        CargarEstadisticas();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarEstadisticas();
    }

    private void CargarEstadisticas()
    {
        double promedioActividad = 0;
        double promedioEnergia = 0;
        int registrosTotales = 0;

        using (var connection = new SqliteConnection("Data Source=diario.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                    SELECT AVG(intensidad) AS PromedioActividad, 
                           AVG(nivel_energia) AS PromedioEnergia, 
                           COUNT(*) AS TotalRegistros
                    FROM registro_diario
                    WHERE fecha >= date('now', '-7 days');
                ";

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    promedioActividad = reader["PromedioActividad"] != DBNull.Value
                        ? Convert.ToDouble(reader["PromedioActividad"])
                        : 0;

                    promedioEnergia = reader["PromedioEnergia"] != DBNull.Value
                        ? Convert.ToDouble(reader["PromedioEnergia"])
                        : 0;

                    registrosTotales = reader["TotalRegistros"] != DBNull.Value
                        ? Convert.ToInt32(reader["TotalRegistros"])
                        : 0;
                }
            }
        }

        lblPromedioActividad.Text = $"Actividad física: {promedioActividad:F2}";
        lblPromedioEnergia.Text = $"Nivel de energía: {promedioEnergia:F2}";

        double progreso = (promedioActividad + promedioEnergia) / 2.0 / 10.0;
        progressGeneral.Progress = progreso;
        lblProgreso.Text = $"{progreso * 100:F0}%";
    }
}
