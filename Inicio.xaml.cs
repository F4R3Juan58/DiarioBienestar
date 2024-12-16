using Microsoft.Data.Sqlite;

namespace DiarioBienestar2
{
    public partial class Inicio : ContentPage
    {
        string connectionString = "Data Source=diario.db";

        public Inicio()
        {
            InitializeComponent();
            Bienvenida();

        }


        private void Bienvenida()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT usuario FROM usuarios
                    WHERE id= @id;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", Identificador.Id);
                    using (var reader = command.ExecuteReader())

                        if (reader.Read())
                        {
                            msgBienvenida.Text = $"Bienvenido {reader["usuario"]?.ToString()}";
                        }
                }

                connection.Close();
            }
        }

    }

}
