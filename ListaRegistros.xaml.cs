using Microsoft.Data.Sqlite;

namespace DiarioBienestar2
{
    public partial class ListaRegistros : ContentPage
    {
        string connectionString = "Data Source=diario.db";

        public ListaRegistros()
        {
            InitializeComponent();
            CargarRegistros();
        }

        private void CargarRegistros()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, fecha, estado_animo
                    FROM registro_diario
                    WHERE id_usuario = @id_usuario
                    ORDER BY fecha DESC;";

                var registros = new List<string>();

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_usuario", Identificador.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fecha = reader["fecha"].ToString();
                            string estadoAnimo = reader["estado_animo"].ToString();
                            registros.Add($"{fecha}: {estadoAnimo}");
                        }
                    }
                }

                listaRegistros.ItemsSource = registros;
            }
        }

        private async void EliminarRegistro(object sender, EventArgs e)
        {
            string registro = (sender as MenuItem)?.CommandParameter as string;
            if (registro != null)
            {
                bool confirm = await DisplayAlert("Confirmar", "¿Desea eliminar este registro?", "Sí", "No");
                if (confirm)
                {
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM registroDiario WHERE id = @id";

                        using (var command = new SqliteCommand(query, connection))
                        {
                            string id = registro.Split(':')[0];
                            command.Parameters.AddWithValue("@id", id);
                            command.ExecuteNonQuery();
                        }
                    }

                    CargarRegistros();
                }
            }
        }
    }
}
