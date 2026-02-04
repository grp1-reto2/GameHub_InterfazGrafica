using grupo1_reto2.Models;
using grupo1_reto2.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace grupo1_reto2.Ventanas
{
    /// <summary>
    /// Lógica de interacción para VentanaRanking.xaml
    /// </summary>
    public partial class VentanaRanking : Window
    {
        private readonly BBDDPostgresService _dbService;
        //ObservableCollection para que se notifiquen los cambios automáticamente
        private ObservableCollection<JugadorRanking> _ranking;
        private ICollectionView _rankingView;


        public VentanaRanking(BBDDPostgresService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            CargarRankingCompleto();
        }


        //Obtiene la información desde la base de datos y lo guarda en un ObservableCollection
        private void CargarRankingCompleto()
        {
            _ranking = new ObservableCollection<JugadorRanking>(
            _dbService.GetRankingCompleto()
            );

            //Posiciona los jugadores dependiendo de la puntuación total que tengan
            for (int i = 0; i < _ranking.Count; i++)
            {
                _ranking[i].Posicion = i + 1;
            }

            //se le asigna los datos del ranking a datagrid
            _rankingView = CollectionViewSource.GetDefaultView(_ranking);
            dataGridRankingCompleto.ItemsSource = _rankingView;

        }


        //Filtro de búsqueda
        private void FiltroRanking_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_rankingView == null) return;

            string filtroPosicion = txtBuscarPosicion.Text;
            string filtroNombre = txtBuscarNombreJugadorPartida.Text.ToLower();
            string filtroScore = txtBuscarDuracion.Text;

            _rankingView.Filter = item =>
            {
                var r = item as JugadorRanking;

                bool coincidePosicion =
                    string.IsNullOrWhiteSpace(filtroPosicion)
                    || r.Posicion.ToString().Contains(filtroPosicion);

                bool coincideNombre =
                    string.IsNullOrWhiteSpace(filtroNombre)
                    || r.Nombre.ToLower().Contains(filtroNombre);

                bool coincideScore =
                    string.IsNullOrWhiteSpace(filtroScore)
                    || r.ScoreTotal.ToString().Contains(filtroScore);

                return coincidePosicion && coincideNombre && coincideScore;
            };
        }


        //Limpia los campos de búsqueda
        private void LimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarPosicion.Text = "";
            txtBuscarNombreJugadorPartida.Text = "";
            txtBuscarDuracion.Text = "";

            if (_rankingView != null)
                _rankingView.Filter = null;
        }


        //Función para cerrar la página
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
