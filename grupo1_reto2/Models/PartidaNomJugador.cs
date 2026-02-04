using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grupo1_reto2.Models
{
    public class PartidaNomJugador
    {
        public int Partida_id { get; set; }
        public int Duracion { get; set; }
        public DateTime Fecha { get; set; }
        public string Jugador { get; set; }
    }
}
