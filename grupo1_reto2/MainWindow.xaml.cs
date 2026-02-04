using grupo1_reto2.Models;
using grupo1_reto2.Services;
using grupo1_reto2.Ventanas;
using LiveCharts;
using LiveCharts.Wpf;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace grupo1_reto2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BBDDPostgresService _dbService;
       
        //Observable para que cargue automaticamente los datos que obtiene de la base de datos en el datagrid
        private ObservableCollection<Jugador> _jugadores;
        private ICollectionView _jugadoresView;

        private ObservableCollection<PartidaNomJugador> _partidas;
        private ICollectionView _partidasView;


        public MainWindow()
        {
            InitializeComponent();
            //llamada a la clase de ApiPostgresServicepara obtener sus funciones
            _dbService = new BBDDPostgresService();
            //carga la información dejugadores
            cargarJugadores();
            //carga la información de partidas
            cargarPartidas();
            //carga la información de raking
            CargarRanking();
        }


        private void cargarJugadores()
        {

            // se convierte la lista en ObservableCollection
            _jugadores = new ObservableCollection<Jugador>(
                _dbService.GetJugadores()
            );

            // se crea la vista filtrable
            _jugadoresView = CollectionViewSource.GetDefaultView(_jugadores);

            // se asigna a DataGrid los datos de jugadores
            dataGridJugadores.ItemsSource = _jugadoresView;
        }


        //Filtro de búsqueda del datagrid de Jugadores
        private void FiltroJugadores_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Si no obtiene ningún jugador no devuelve nada.
            if (_jugadoresView == null) return;

            //Obtiene la información desde los campos de diseño
            string filtroId = txtBuscarId.Text;
            string filtroNombre = txtBuscarNombre.Text.ToLower();
            string filtroEmail = txtBuscarEmail.Text.ToLower();

            //Filtra por cada sección que tiene 'JugadoresView' (en este caso: ID, nomrbe y email)
            _jugadoresView.Filter = item =>
            {
                Jugador j = item as Jugador;

                //Si el campo está vacío pasan todos los datos, pero si se ha detectado algo se filtrará por lo que se ha ingresado en este caso los ids
                bool coincideId =
                    string.IsNullOrWhiteSpace(filtroId) 
                    || j.Jugador_id.ToString().Contains(filtroId);

                //Si el campo está vacío pasan todos los datos, pero si se ha detectado algo se filtrará por lo que se ha ingresado en este casa los nombres
                bool coincideNombre =
                    string.IsNullOrWhiteSpace(filtroNombre)
                    || j.Nombre.ToLower().Contains(filtroNombre);

                //Si el campo está vacío pasan todos los datos, pero si se ha detectado algo se filtrará por lo que se ha ingresado en este casa los emails
                bool coincideEmail =
                    string.IsNullOrWhiteSpace(filtroEmail)
                    || j.Email.ToLower().Contains(filtroEmail);

                return coincideId && coincideNombre && coincideEmail; //si coinciden los filtros devuelve la respuesta 
            };
        }


        //Limpia los campos de Jugadores
        private void LimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarId.Text = "";
            txtBuscarNombre.Text = "";
            txtBuscarEmail.Text = "";
            _jugadoresView.Filter = null;
        }


        private void cargarPartidas()
        {
            //Obtiene la información desde la base de datos y las guarda en ObservableCollection que avisa a la interfaz cuando los datos cambian
            _partidas = new ObservableCollection<PartidaNomJugador>(
                _dbService.GetPartidaNomJugador()
            );

            //crea una vista de la colección y la asigna como origen de datos del DataGrid de partidas
            _partidasView = CollectionViewSource.GetDefaultView(_partidas);
            dataGridPartidas.ItemsSource = _partidasView;
        }


        //Búsqueda de la información de la datagrid de Partidas
        private void FiltroPartidas_TextChanged(object sender, EventArgs e)
        {
            //Si no obtiene información de partidas no devuelve nada
            if (_partidasView == null) return;

            //Obtiene la información de cada campo de búsqueda
            string filtroId = txtBuscarPartidaId.Text;
            string filtroNombreJugadorPartida = txtBuscarNombreJugadorPartida.Text;
            string filtroDuracion = txtBuscarDuracion.Text;
            string textoFecha = txtBuscarFecha.Text;

            _partidasView.Filter = item =>
            {
                PartidaNomJugador p = item as PartidaNomJugador;
                if (p == null) return false;

                bool coincideId =
                    string.IsNullOrWhiteSpace(filtroId)
                    || p.Partida_id.ToString().Contains(filtroId);

                bool coincideNomJugadorPartida =
                   string.IsNullOrWhiteSpace(filtroNombreJugadorPartida) ||
                   p.Jugador.ToLower().Contains(filtroNombreJugadorPartida.ToLower());


                bool coincideDuracion =
                    string.IsNullOrWhiteSpace(filtroDuracion)
                    || p.Duracion.ToString().Contains(filtroDuracion);

                bool coincideFecha = true;

                if (!string.IsNullOrWhiteSpace(txtBuscarFecha.Text))
                {
                    if (DateTime.TryParse(txtBuscarFecha.Text, out DateTime fechaManual))
                    {
                        //si coinciden la fecha manual y DateTime se guarda la información en 'coincideFecha' es true
                        coincideFecha = p.Fecha.Date == fechaManual.Date; 
                    }
                }

                //Si coinciden todos los filtrados devuelve el resultado
                return coincideId && coincideNomJugadorPartida && coincideDuracion && coincideFecha;
            };
        }


        //Se conectan el textbox de fecha con el DateTime para cuando seleccione una fecha aprezca en el textbox y vice versa
        private void txtBuscarFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBuscarFecha.Text.Length == 10 &&
                DateTime.TryParseExact(
                txtBuscarFecha.Text,
                "dd/MM/yyyy",
                null,
                System.Globalization.DateTimeStyles.None,
                out DateTime fecha))
            {
                dpBuscarFecha.SelectedDate = fecha;
            }
            else
            {
                dpBuscarFecha.SelectedDate = null; // Si no es válido, queda vacío
            }

            //Llama al filtro para actualizar el DataGrid
            FiltroPartidas_TextChanged(sender, e);
        }


        //se conecta DateTime con el textbox
        private void dpBuscarFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpBuscarFecha.SelectedDate.HasValue)
            {
                txtBuscarFecha.Text = dpBuscarFecha.SelectedDate.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                txtBuscarFecha.Text = "";
            }

            // Llama al filtro para actualizar el DataGrid
            FiltroPartidas_TextChanged(sender, e);
        }


        //Limpia los campos de Partidas
        private void LimpiarFiltrosPartidas_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarPartidaId.Text = "";
            txtBuscarDuracion.Text = "";
            dpBuscarFecha.SelectedDate = null;
            txtBuscarFecha.Text = "";
            _partidasView.Filter = null;
        }


        //Visualiza la información del Ranking
        private void CargarRanking()
        {
            //obtiene los datos de la clase dbService d ela función GetTop3Ranking
            var ranking = _dbService.GetTop3Ranking();

            //si menos de tres jugadores no devuelve nada
            if (ranking.Count < 3) return;

            // Ordenar por 2º 1º 3º
            var ordenVisual = new[]
            {
            ranking[1], // segundo → izquierda
            ranking[0], // primero → centro
            ranking[2]  // tercero → derecha
            };

            //Se crean las columnas que se van a visualizar
            rankingChart.Series = new SeriesCollection
            { 
             new ColumnSeries
             {
                 Title = ordenVisual[0].Nombre, //Nombre del jugador
                 Values = new ChartValues<int> { ordenVisual[0].ScoreTotal }, //puntuación total
                 Fill = Brushes.Silver, //color plata
                 DataLabels = true, //se meustra ell número encima
                 MaxColumnWidth = 90 //ancho máximo de 90


             },
                new ColumnSeries
                {
                    Title = ordenVisual[1].Nombre,
                    Values = new ChartValues<int> { ordenVisual[1].ScoreTotal },
                    Fill = Brushes.Gold, //color oro
                    DataLabels = true,
                    LabelPoint = point => point.Y + " 🏆", //Muestra la puntuación con un trofeo al ganador
                    MaxColumnWidth = 90

                },
                new ColumnSeries
                {
                    Title = ordenVisual[2].Nombre,
                    Values = new ChartValues<int> { ordenVisual[2].ScoreTotal },
                    Fill = Brushes.Peru, //color bronce
                    DataLabels = true,
                    MaxColumnWidth = 90

                }
            };

            //Muestra una etiqueta al eje horizontal 'Ranking'
            rankingChart.AxisX[0].Labels = new[]
            {
                "Ranking"
            };
        }


        //Para visualizar el fichero de ayuda
        private void MenuItemAyuda_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //Obtiene la ruta del fichero
                string rutaPdf = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Archivos",
                    "Ayuda.pdf"
                );

                //Si no existe el fichero salta un mensaje de error
                if (!System.IO.File.Exists(rutaPdf))
                {
                    MessageBox.Show("No se encontró el archivo de ayuda.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                //Si todo está correcto se procede a abrir el fichero
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaPdf, //indica que fichero visualizar
                    UseShellExecute = true //se visualiza el fichero
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el archivo PDF:\n" + ex.Message);
            }
        }


        //Redirige a la carpeta donde están los ficheros para descargar los informes
        private void MenuItemInfo_Click(object sender, RoutedEventArgs e)
        {
            //Se le pasa el link entero de la carpeta en donde están los informes
            Process.Start("https://drive.google.com/drive/folders/15HvfGjh3DeUqbhHiYGJcROIBnRRBiJmp?usp=drive_link");
        }


        //Abre la ventana de 'VentanaRanking'
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VentanaRanking ranking = new VentanaRanking(_dbService);
            ranking.ShowDialog();
        }
    }
}
