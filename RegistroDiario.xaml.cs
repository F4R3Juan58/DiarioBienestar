using Microsoft.Data.Sqlite;

namespace DiarioBienestar2;

public partial class RegistroDiario : ContentPage
{
    string connectionString = "Data Source=diario.db";

    public RegistroDiario()
	{
		InitializeComponent();
	}

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        progressBar.Progress = slider.Value/10;
    }

    private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        stepperLabel.Text= stepper.Value.ToString();
    }

    private void onGuardarClicker(object sender, EventArgs e)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string insertQuery = @"
                INSERT INTO registro_diario (
                    id_usuario, estado_animo, actividades, detalles_dia, intensidad, nivel_energia, fecha
                ) VALUES (
                    @id_usuario, @estado_animo, @actividades, @detalles_dia, @intensidad, @nivel_energia, @fecha
                );";

            using (var command = new SqliteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@id_usuario", Identificador.Id); 
                command.Parameters.AddWithValue("@estado_animo", estadoAnimo.Text);
                command.Parameters.AddWithValue("@actividades", actividades.Text);
                command.Parameters.AddWithValue("@detalles_dia", detallesDia.Text);
                command.Parameters.AddWithValue("@intensidad", slider.Value);
                command.Parameters.AddWithValue("@nivel_energia", stepper.Value);
                command.Parameters.AddWithValue("@fecha", fecha.Date);

                command.ExecuteNonQuery();
            }

            estadoAnimo.Text= null;
            actividades.Text= null;
            detallesDia.Text= null;
            slider.Value= 0;
            stepper.Value = 0;

            connection.Close();
        }
    }
}