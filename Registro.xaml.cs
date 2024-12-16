using Microsoft.Data.Sqlite;

namespace DiarioBienestar2;

public partial class Registro : ContentPage
{
    string connectionString = "Data Source=diario.db";

    public Registro()
	{
		InitializeComponent();
	}
        private async void LoginButton(object sender, EventArgs e)
    {
        if (password.Text == confirmpassword.Text)
        {

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO usuarios (usuario, email, password)
                    VALUES (@usuario, @email, @password);";

                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@usuario", usuario.Text);
                    command.Parameters.AddWithValue("@email", email.Text);
                    command.Parameters.AddWithValue("@password", HashPassword(password.Text));

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            await Navigation.PushAsync(new Login());
        }
        else
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
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
}