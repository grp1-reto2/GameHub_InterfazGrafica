using grupo1_reto2.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace grupo1_reto2.Services
{
    public class BBDDPostgresService
    {
        private readonly string _connectionString = "Server=gamehubdatabase.duckdns.org;Port=5432;User Id=grupo1;Password=reto2;Database=juego1";


        //Obtiene la información de jugadores
        public List<Jugador> GetJugadores()
        {
            //Crea una nueva lsita para almacenar los Jugadores
            var jugadores = new List<Jugador>();
            //Se conecta a la base de datos de PostgreSQL
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                //Consulta SQL de la tabla 'jugador'
                string query = "Select jugador_id, nombre, email from jugador;";
                using (var cmd = new NpgsqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Guarda cada jugador en la lista, obtenida desde la base de datos
                        jugadores.Add(new Jugador
                        {
                            Jugador_id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Email = reader.GetString(2)
                        });
                    }
                }
            }
            return jugadores; //Devuelve todos los jugadores 
        }


        //Obtiene las partidas de los jugadores
        public List<PartidaNomJugador> GetPartidaNomJugador()
        {
            //Crea una nueva lista para almacenar las partidas con los nombres de los jugadores
            var partidas = new List<PartidaNomJugador>();
            //Se conecta a la base de datos de PostgreSQL
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                //Consulta SQL que obtiene las partidas, la duración, la fecha y los nombres de los jugadores que han jugado cada aprtida
                string query = @" 
                       SELECT 
                        p.partida_id AS partida_id,
                        p.duracion,
                        p.fecha,
                        STRING_AGG(j.nombre, ', ') AS jugadores                    
                    FROM partida p
                    JOIN jugador_partida jp ON p.partida_id = jp.partida_id
                    JOIN jugador j ON j.jugador_id = jp.jugador_id
                    GROUP BY p.partida_id, p.duracion, p.fecha
                    ORDER BY p.partida_id;
                ";
                using (var cmd = new NpgsqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //guarda cada aprtida en la lista
                        partidas.Add(new PartidaNomJugador
                        {
                            Partida_id = reader.GetInt32(0),
                            Duracion = reader.GetInt32(1),
                            Fecha = reader.GetDateTime(2),
                            Jugador = reader.GetString(3)
                        });
                    }
                }
            }
            return partidas; //devuelve la lista completa de partidas
        }


        //Para obtener el ranking de los 3 jugadores con más puntuación
        public List<(string Nombre, int ScoreTotal)> GetTop3Ranking()
        {
            //Crea una lista 'ranking' para almacenar los top 3 jugadores
            var ranking = new List<(string, int)>();
            //Se conecta con la base de datos de PostgreSQL
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                //Consulta SQL que obtiene nombre y puntuación total de cada jugador 
                string query = @"
                SELECT j.nombre, SUM(jp.score) AS score_total
                FROM jugador_partida jp
                JOIN jugador j ON j.jugador_id = jp.jugador_id
                GROUP BY j.nombre
                ORDER BY score_total DESC
                LIMIT 3;
                ";

                using (var cmd = new NpgsqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Guarda en la lista 'raking' el nombre del jugador y la puntuación total
                        ranking.Add((
                            reader.GetString(0), // Nombre del jugador
                            reader.GetInt32(1) // Puntuación total
                        ));
                    }
                }
            }
            return ranking;// devuelve la lista completa con los 3 jugadores con mayor puntuación
        }



        //Obtiene toda la información del ranking
        public List<JugadorRanking> GetRankingCompleto()
        {
            //Crea una lista 'ranking' para almacenar el ranking completo
            var ranking = new List<JugadorRanking>();
            //Se conecta con la base de datos de PostgreSQL
            using (var con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                //consulta SQL para obtener los nombres de los jugadores y las puntuaciones totales
                string query = @"
                SELECT j.nombre, SUM(jp.score) AS score_total
                FROM jugador_partida jp
                JOIN jugador j ON j.jugador_id = jp.jugador_id
                GROUP BY j.nombre
                ORDER BY score_total DESC;
                ";

                using (var cmd = new NpgsqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Guarda em la lista 'ranking' el nombre y la puntuación
                        ranking.Add(new JugadorRanking
                        {
                            Nombre = reader.GetString(0),
                            ScoreTotal = reader.GetInt32(1)
                        });
                    }
                }
            }
            return ranking; //devuelve la lista completa de ranking
        }
    }
}
