using Microsoft.Data.Sqlite;
using static DiarioBienestar2.Login;

namespace DiarioBienestar2
{
    // La clase Login maneja la funcionalidad de inicio de sesión y registro de usuarios
    public partial class Login : ContentPage
    {
        // Cadena de conexión a la base de datos
        private const string ConnectionString = "Data Source=diario.db";

        // Constructor de la página que inicializa los componentes y crea la base de datos si no existe
        public Login()
        {
            InitializeComponent();
            CreateDataBase(); // Crear base de datos si no existe
        }

        // Método para crear la base de datos y sus tablas si no existen
        private void CreateDataBase()
        {
            // Usar 'using' para asegurar la correcta gestión de la conexión
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Crear tabla de usuarios si no existe
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS usuarios  (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        usuario TEXT NOT NULL,
                        email TEXT UNIQUE NOT NULL,
                        password TEXT NOT NULL
                    );";

                // Crear tabla de registros diarios si no existe
                string createTableQuery2 = @"
                    CREATE TABLE IF NOT EXISTS registroDiario (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        id_usuario INTEGER NOT NULL,
                        detalles_dia TEXT,
                        intensidad REAL NOT NULL CHECK(intensidad BETWEEN 0 AND 10),
                        nivel_energia INTEGER NOT NULL CHECK(nivel_energia BETWEEN 0 AND 5),
                        fecha DATE NOT NULL,
                        FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE
                    );";

                // Ejecutar las consultas para crear las tablas
                ExecuteQuery(createTableQuery, connection);
                ExecuteQuery(createTableQuery2, connection);

                connection.Close();
            }
        }

        // Método auxiliar para ejecutar una consulta SQL
        private void ExecuteQuery(string query, SqliteConnection connection)
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Método que maneja el evento de login, validando el usuario y la contraseña
        private async void LoginButton(object sender, EventArgs e)
        {
            bool credencialesValidas = false;

            // Verificar si los campos de usuario y contraseña no están vacíos
            if (usuarioLogin.Text != "" && passwordLogin.Text != "")
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT id, password FROM usuarios
                        WHERE usuario = @Usuario;";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Usuario", usuarioLogin.Text);
                        using (var reader = command.ExecuteReader())
                        {
                            // Verificar si el usuario existe
                            if (reader.Read())
                            {
                                string passwordAlmacenada = reader["password"]?.ToString();
                                Identificador.Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0;

                                // Validar si la contraseña ingresada coincide con la almacenada
                                if (passwordAlmacenada == HashPassword(passwordLogin.Text))
                                {
                                    credencialesValidas = true;
                                }
                            }
                        }
                    }

                    connection.Close();
                }

                // Si las credenciales son válidas, navegar a la página de inicio
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
                await DisplayAlert("Error", "Los campos Usuario o Contraseña no pueden estar vacíos", "OK");
            }
        }

        // Método para hashear la contraseña antes de almacenarla o compararla
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // Generar el hash de la contraseña
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Método para mostrar el formulario de registro y ocultar el de login
        private void OnRegistroMenuClick(object sender, EventArgs e)
        {
            MenuLogin.IsVisible = false;
            MenuRegistro.IsVisible = true;
        }

        // Método para mostrar el formulario de login y ocultar el de registro
        private void OnLoginMenuClick(object sender, EventArgs e)
        {
            MenuLogin.IsVisible = true;
            MenuRegistro.IsVisible = false;
        }

        // Método para registrar un nuevo usuario, validando las contraseñas
        private async void RegisterButton(object sender, EventArgs e)
        {
            // Verificar que las contraseñas coincidan
            if (passwordRegistro.Text == confirmpassword.Text)
            {
                // Usar 'using' para manejar la conexión a la base de datos
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();

                    // Consulta SQL para insertar un nuevo usuario
                    string insertQuery = @"
                        INSERT INTO usuarios (usuario, email, password)
                        VALUES (@usuario, @email, @password);";

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        // Agregar los parámetros a la consulta
                        command.Parameters.AddWithValue("@usuario", usuarioRegistro.Text);
                        command.Parameters.AddWithValue("@email", emailRegistro.Text);
                        command.Parameters.AddWithValue("@password", HashPassword(passwordRegistro.Text));

                        // Ejecutar la consulta
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                // Navegar a la pantalla de login después de registrar el usuario
                await Navigation.PushAsync(new Login());
            }
            else
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            }
        }
    }
}
