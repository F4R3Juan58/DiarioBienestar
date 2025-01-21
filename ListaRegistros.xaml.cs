using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DiarioBienestar2
{
    // La clase ListaRegistros maneja la visualización y gestión de registros diarios del usuario
    public partial class ListaRegistros : ContentPage
    {
        // Cadena de conexión a la base de datos
        private const string ConnectionString = "Data Source=diario.db";

        // Propiedad que almacena la lista de registros
        public ObservableCollection<Registro> Registros { get; set; } = new ObservableCollection<Registro>();

        // Registro seleccionado por el usuario
        private Registro registroSeleccionado;
        public Registro RegistroSeleccionado
        {
            get => registroSeleccionado;
            set
            {
                // Si hay un registro previamente seleccionado, desmarcarlo
                if (registroSeleccionado != null)
                    registroSeleccionado.IsSelected = false;

                registroSeleccionado = value;

                // Si hay un nuevo registro seleccionado, marcarlo
                if (registroSeleccionado != null)
                    registroSeleccionado.IsSelected = true;

                OnPropertyChanged(nameof(RegistroSeleccionado));
            }
        }

        // Constructor de la página
        public ListaRegistros()
        {
            InitializeComponent();
            BindingContext = this;
            CargarRegistros(); // Cargar los registros al inicializar la página
        }

        // Método que se ejecuta cuando la página aparece, recarga los registros
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarRegistros();
        }

        // Método que carga los registros desde la base de datos
        private void CargarRegistros()
        {
            try
            {
                Registros.Clear(); // Limpiar la colección antes de cargar los nuevos registros

                // Abrir conexión a la base de datos
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();

                    // Consulta para obtener los registros diarios del usuario
                    string query = @"
                        SELECT id, DATE(fecha) AS fecha, detalles_dia, intensidad, nivel_energia
                        FROM registroDiario
                        WHERE id_usuario = @id_usuario
                        ORDER BY fecha DESC;";

                    // Ejecutar la consulta SQL
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_usuario", Identificador.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            // Leer los registros obtenidos
                            while (reader.Read())
                            {
                                var registro = new Registro
                                {
                                    Id = reader["id"].ToString(),
                                    Fecha = reader["fecha"].ToString(),
                                    Detalles = reader["detalles_dia"].ToString(),
                                    Intensidad = reader["intensidad"].ToString(),
                                    NivelEnergia = reader["nivel_energia"].ToString()
                                };

                                // Crear la descripción del registro
                                registro.Descripcion = $" --------------------\n| {registro.Fecha} |\n --------------------\nDetalles del día: {registro.Detalles}\nIntensidad: {registro.Intensidad}\nNivel de energía: {registro.NivelEnergia}";

                                // Agregar el registro a la lista
                                Registros.Add(registro);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar errores y mostrar un mensaje al usuario
                DisplayAlert("Error", $"Error al cargar registros: {ex.Message}", "OK");
            }
        }

        // Método para eliminar un registro
        private async void EliminarRegistro(object sender, EventArgs e)
        {
            // Verificar si hay un registro seleccionado
            if (RegistroSeleccionado != null)
            {
                // Confirmar la eliminación del registro
                bool confirm = await DisplayAlert("Confirmar", "¿Desea eliminar este registro?", "Sí", "No");
                if (confirm)
                {
                    try
                    {
                        // Eliminar el registro de la base de datos
                        using (var connection = new SqliteConnection(ConnectionString))
                        {
                            connection.Open();
                            string query = "DELETE FROM registroDiario WHERE id = @id";

                            using (var command = new SqliteCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id", RegistroSeleccionado.Id);
                                command.ExecuteNonQuery(); // Ejecutar la eliminación
                            }
                        }

                        // Eliminar el registro de la lista en la interfaz
                        Registros.Remove(RegistroSeleccionado);
                        RegistroSeleccionado = null; // Desmarcar el registro seleccionado
                    }
                    catch (Exception ex)
                    {
                        // Manejar errores y mostrar un mensaje al usuario
                        await DisplayAlert("Error", $"Error al eliminar registro: {ex.Message}", "OK");
                    }
                }
            }
        }

        // Método para modificar un registro
        private async void ModificarRegistro(object sender, EventArgs e)
        {
            // Verificar si hay un registro seleccionado
            if (RegistroSeleccionado != null)
            {
                // Navegar a la página de edición, pasando el registro seleccionado
                await Navigation.PushAsync(new EditarRegistroPage(RegistroSeleccionado));
            }
        }
    }

    // Clase que representa un registro diario
    public class Registro : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Fecha { get; set; }
        public string Detalles { get; set; }
        public string Intensidad { get; set; }
        public string NivelEnergia { get; set; }
        public string Descripcion { get; set; }

        // Propiedad que indica si el registro está seleccionado
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        // Evento para notificar cambios en las propiedades
        public event PropertyChangedEventHandler PropertyChanged;

        // Método que invoca el evento PropertyChanged
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
