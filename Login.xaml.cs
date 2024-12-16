using Microsoft.Data.Sqlite;
using static DiarioBienestar2.Login;

namespace DiarioBienestar2;

public partial class Login : ContentPage
{ 
    string connectionString = "Data Source=diario.db";
    public Login()
	{
		InitializeComponent();
        createDataBase();

    }

    private void createDataBase()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS usuarios  (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        usuario TEXT NOT NULL,
                        email TEXT UNIQUE NOT NULL,
                        password TEXT NOT NULL
                    );";

            string createTableQuery2 = @"
                CREATE TABLE IF NOT EXISTS registro_diario (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    id_usuario INTEGER NOT NULL,
                    estado_animo TEXT NOT NULL,
                    actividades TEXT NOT NULL,
                    detalles_dia TEXT,
                    intensidad REAL NOT NULL CHECK(intensidad BETWEEN 0 AND 10),
                    nivel_energia INTEGER NOT NULL CHECK(nivel_energia BETWEEN 0 AND 5),
                    fecha DATE NOT NULL,
                    FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE
                );";

            using (var command = new SqliteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createTableQuery2, connection))
            {
                command.ExecuteNonQuery();
            }

            connection.Close();
        }
    }

    private async void onLoginClick(object sender, EventArgs e)
    {
        bool credencialesValidas = false;

        if (usuario.Text != "" && password.Text != "")
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT id,password FROM usuarios
                    WHERE usuario= @Usuario;";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Usuario", usuario.Text);
                    using (var reader = command.ExecuteReader())

                        if (reader.Read())
                        {
                        string passwordAlmacenada = reader["password"]?.ToString();
                            Identificador.Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0;
                            if (passwordAlmacenada == HashPassword(password.Text))
                            {
                                credencialesValidas = true;
                            }
                    }
                }

                connection.Close();
            }


            if (credencialesValidas)
            {
                await Shell.Current.GoToAsync("//Inicio");
            }
            else
            {
                await DisplayAlert("Error", "Nombre de usuario o contraseña incorrectos", "OK");
            }
        }
        else
        {
           await DisplayAlert("ERROR","Los campos Usuario o Constraseña no puede estar vacios","Okey");
        }
    }

    private string HashPassword(string password)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    private async void onRegistroClick(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Registro());
    }

}

