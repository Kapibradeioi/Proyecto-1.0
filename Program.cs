using System;

class MultiplayerMazeGame
{
    private static int size_maze = 19;
    private static int width = size_maze;
    private static int height = size_maze;
    private static char[,] maze;
    private static Random rand = new Random();
    private static (int x, int y)[] players;
    private static Ficha[] fichasJugadores;
    private static int numPlayers;
    private static char[] traps = { 'T', 'L', 'R' }; // Trampas: Teletransporte, Lodo, Red

    static void Main()
    {
        Console.Write("Seleccione un número del 2 al 4 (cantidad de jugadores): ");
        numPlayers = Math.Clamp(int.Parse(Console.ReadLine()), 2, 4);

        maze = new char[height, width];
        GenerateMaze();
        AddEntranceAndExit();
        PlaceTraps();
        SeleccionarFichas();
        PlayGame();
    }

    static void GenerateMaze()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                maze[y, x] = '#';

        int startX = 1, startY = 1;
        maze[startY, startX] = ' ';
        CarveMaze(startX, startY);
    }

    static void CarveMaze(int x, int y)
    {
        int[] dx = { 2, -2, 0, 0 };
        int[] dy = { 0, 0, 2, -2 };
        int[] directions = { 0, 1, 2, 3 };

        for (int i = 0; i < directions.Length; i++)
        {
            int r = rand.Next(i, directions.Length);
            (directions[i], directions[r]) = (directions[r], directions[i]);
        }

        foreach (int dir in directions)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[ny, nx] == '#')
            {
                maze[y + dy[dir] / 2, x + dx[dir] / 2] = ' ';
                maze[ny, nx] = ' ';
                CarveMaze(nx, ny);
            }
        }
    }

    static void AddEntranceAndExit()
    {
        maze[0, 1] = ' ';
        maze[height - 1, width - 2] = 'E'; // Salida marcada con 'E'
    }

    static void PlaceTraps()
    {
        for (int i = 0; i < 10; i++) // Coloca 10 trampas
        {
            int x, y;
            do
            {
                x = rand.Next(1, width - 1);
                y = rand.Next(1, height - 1);
            } while (maze[y, x] != ' ');

            maze[y, x] = traps[rand.Next(traps.Length)];
        }
    }

    static void SeleccionarFichas()
    {
        fichasJugadores = new Ficha[numPlayers];
        players = new (int, int)[numPlayers];

        Console.WriteLine("Seleccione sus fichas:");
        for (int i = 0; i < numPlayers; i++)
        {
            Console.WriteLine($"Jugador {i + 1}:");
            Console.WriteLine("1. Capibara (Habilidad: Cavar túnel)");
            Console.WriteLine("2. Ardilla (Habilidad: Esquivar trampas)");
            Console.WriteLine("3. Zorro (Habilidad: Mover otras fichas)");
            Console.WriteLine("4. Vaca (Habilidad: Destruir trampas)");
            Console.WriteLine("5. Mono (Habilidad: Caminar 2 pasos extra)");
            Console.WriteLine("6. Koala (Habilidad: Paralizar a otro jugador)");
            int opcion = int.Parse(Console.ReadLine());

            fichasJugadores[i] = opcion switch
            {
                1 => new Ficha('C', "Capibara", "Cavar túnel", 3),
                2 => new Ficha('A', "Ardilla", "Esquivar trampas", 3),
                3 => new Ficha('Z', "Zorro", "Mover otras fichas", 3),
                4 => new Ficha('V', "Vaca", "Destruir trampas", 3),
                5 => new Ficha('M', "Mono", "Caminar 2 pasos extra", 3),
                6 => new Ficha('K', "Koala", "Paralizar a otro jugador", 3),
                _ => throw new Exception("Opción no válida")
            };

            players[i] = (1, 0); // Posición inicial
        }
    }

    static void PlayGame()
    {
        int turnoActual = 0; // Contador de turnos global

        while (true)
        {
            turnoActual++; // Incrementar el contador de turnos
            PrintMaze();
            for (int i = 0; i < numPlayers; i++)
            {
                if (fichasJugadores[i].EstaParalizado)
                {
                    Console.WriteLine($"¡{fichasJugadores[i].Nombre} está paralizado y pierde su turno!");
                    fichasJugadores[i].EstaParalizado = false; // Restablecer el estado
                    continue; // Saltar el turno
                }

                int movimientos = rand.Next(4, 11); // Movimientos aleatorios entre 4 y 10
                Console.WriteLine($"Turno del Jugador {i + 1} ({fichasJugadores[i].Nombre})");
                Console.WriteLine($"Movimientos disponibles: {movimientos}");
                Console.WriteLine($"Habilidad: {fichasJugadores[i].Habilidad}");

                // Verificar si la habilidad está en cooldown
                if (fichasJugadores[i].CooldownActual > 0)
                {
                    Console.WriteLine($"Habilidad en cooldown. Turnos restantes: {fichasJugadores[i].CooldownActual}");
                }

                // Usar habilidad (si no está en cooldown)
                if (fichasJugadores[i].CooldownActual == 0)
                {
                    // Usar habilidad del Capibara
                    if (fichasJugadores[i].Nombre == "Capibara")
                    {
                        Console.Write("¿Deseas usar tu habilidad para cavar un túnel? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            UsarHabilidadCapibara(i);
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                        }
                    }

                    // Usar habilidad de la Ardilla
                    if (fichasJugadores[i].Nombre == "Ardilla")
                    {
                        Console.Write("¿Deseas usar tu habilidad para esquivar trampas? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            fichasJugadores[i].EsquivandoTrampas = true; // Activar la habilidad
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                            Console.WriteLine($"¡{fichasJugadores[i].Nombre} activó su habilidad para esquivar trampas durante este turno!");
                        }
                    }

                    // Usar habilidad del Zorro
                    if (fichasJugadores[i].Nombre == "Zorro")
                    {
                        Console.Write("¿Deseas usar tu habilidad para mover otra ficha? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            UsarHabilidadZorro(i);
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                        }
                    }

                    // Usar habilidad de la Vaca
                    if (fichasJugadores[i].Nombre == "Vaca")
                    {
                        Console.Write("¿Deseas usar tu habilidad para destruir trampas? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            UsarHabilidadVaca(i);
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                        }
                    }

                    // Usar habilidad del Mono
                    if (fichasJugadores[i].Nombre == "Mono")
                    {
                        Console.Write("¿Deseas usar tu habilidad para caminar 2 pasos extra? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            movimientos += 2; // Añade 2 movimientos extra
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                            Console.WriteLine($"¡{fichasJugadores[i].Nombre} usa su habilidad y ahora tiene {movimientos} movimientos!");
                        }
                    }

                    // Usar habilidad del Koala
                    if (fichasJugadores[i].Nombre == "Koala")
                    {
                        Console.Write("¿Deseas usar tu habilidad para paralizar a otro jugador? (S/N): ");
                        char usarHabilidad = char.ToUpper(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (usarHabilidad == 'S')
                        {
                            UsarHabilidadKoala(i);
                            fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Iniciar cooldown
                        }
                    }
                }

                // Reducir el cooldown de la habilidad
                if (fichasJugadores[i].CooldownActual > 0)
                {
                    fichasJugadores[i].CooldownActual--;
                }

                // Movimientos del jugador
                for (int j = 0; j < movimientos; j++)
                {
                    Console.Write("Movimiento (WASD): ");
                    char move = char.ToUpper(Console.ReadKey().KeyChar);
                    Console.WriteLine();

                    if (MovePlayer(i, move))
                    {
                        Console.WriteLine($"¡Jugador {i + 1} ha ganado!");
                        return;
                    }

                    PrintMaze();

                    // Verificar si el jugador quedó paralizado después de moverse
                    if (fichasJugadores[i].EstaParalizado)
                    {
                        Console.WriteLine($"¡{fichasJugadores[i].Nombre} está paralizado y pierde los movimientos restantes!");
                        break; // Terminar el bucle de movimientos
                    }
                }

                // Desactivar la habilidad de esquivar trampas al final del turno
                if (fichasJugadores[i].Nombre == "Ardilla" && fichasJugadores[i].EsquivandoTrampas)
                {
                    fichasJugadores[i].EsquivandoTrampas = false;
                    Console.WriteLine($"¡{fichasJugadores[i].Nombre} ya no está esquivando trampas!");
                }
            }
        }
    }

    static bool MovePlayer(int playerIndex, char move)
    {
        int dx = 0, dy = 0;
        if (move == 'W') dy = -1;
        else if (move == 'S') dy = 1;
        else if (move == 'A') dx = -1;
        else if (move == 'D') dx = 1;

        int newX = players[playerIndex].x + dx;
        int newY = players[playerIndex].y + dy;

        if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX] != '#')
        {
            char currentTile = maze[newY, newX];
            players[playerIndex] = (newX, newY);

            // Verificar si la habilidad de esquivar trampas está activa
            if (fichasJugadores[playerIndex].EsquivandoTrampas && Array.Exists(traps, t => t == currentTile))
            {
                Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} esquiva una trampa!");
                maze[newY, newX] = ' '; // La trampa desaparece
            }
            else
            {
                // Aplicar efectos de las trampas si no está esquivando
                if (currentTile == 'T') // Trampa de teletransporte
                {
                    Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} cayó en una trampa de teletransporte!");
                    maze[newY, newX] = ' '; // La trampa desaparece
                    players[playerIndex] = (1, 0); // Teletransporta al inicio
                    return false; // No termina el juego, solo aplica el efecto
                }
                else if (currentTile == 'L') // Trampa de lodo
                {
                    Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} está atrapado en lodo! Pierde los movimientos restantes y su próximo turno.");
                    maze[newY, newX] = ' '; // La trampa desaparece
                    fichasJugadores[playerIndex].EstaParalizado = true; // El jugador pierde su siguiente turno
                    return false; // No termina el juego, solo aplica el efecto
                }
                else if (currentTile == 'R') // Trampa de red
                {
                    Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} cayó en una red! Su habilidad estará en cooldown por 3 turnos adicionales.");
                    maze[newY, newX] = ' '; // La trampa desaparece
                    fichasJugadores[playerIndex].CooldownActual += 3; // Añade 3 turnos adicionales al cooldown
                    return false; // No termina el juego, solo aplica el efecto
                }
            }

            if (currentTile == 'E') // Salida
            {
                return true; // El jugador ha llegado a la salida y gana
            }
        }
        return false; // No ha ganado, el juego continúa
    }

    static void PrintMaze()
    {
        Console.Clear();
        char[,] displayMaze = (char[,])maze.Clone();
        for (int i = 0; i < numPlayers; i++)
        {
            displayMaze[players[i].y, players[i].x] = fichasJugadores[i].Icono;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Console.ForegroundColor = displayMaze[y, x] switch
                {
                    'C' => ConsoleColor.Green,
                    'A' => ConsoleColor.Yellow,
                    'Z' => ConsoleColor.Red,
                    'V' => ConsoleColor.Blue,
                    'M' => ConsoleColor.DarkMagenta,
                    'K' => ConsoleColor.DarkCyan,
                    'T' => ConsoleColor.Magenta,
                    'L' => ConsoleColor.DarkYellow,
                    'R' => ConsoleColor.DarkGray,
                    'E' => ConsoleColor.Cyan,
                    _ => ConsoleColor.White
                };
                Console.Write(displayMaze[y, x] + " ");
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    // Métodos para las habilidades
    static void UsarHabilidadCapibara(int playerIndex)
    {
        Console.Write("Dirección del túnel (WASD): ");
        char direccion = char.ToUpper(Console.ReadKey().KeyChar);
        Console.WriteLine();

        int dx = 0, dy = 0;
        if (direccion == 'W') dy = -1;
        else if (direccion == 'S') dy = 1;
        else if (direccion == 'A') dx = -1;
        else if (direccion == 'D') dx = 1;

        int newX = players[playerIndex].x + dx;
        int newY = players[playerIndex].y + dy;

        if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX] == '#')
        {
            maze[newY, newX] = ' '; // Abre un túnel
            Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} ha cavado un túnel!");
        }
        else
        {
            Console.WriteLine("No se puede cavar un túnel en esa dirección.");
        }
    }

    static void UsarHabilidadZorro(int playerIndex)
    {
        Console.WriteLine("Selecciona una ficha para mover:");
        for (int i = 0; i < numPlayers; i++)
        {
            if (i != playerIndex)
            {
                Console.WriteLine($"{i + 1}. Jugador {i + 1} ({fichasJugadores[i].Nombre})");
            }
        }

        int opcion = int.Parse(Console.ReadLine()) - 1;
        if (opcion >= 0 && opcion < numPlayers && opcion != playerIndex)
        {
            Console.Write("Dirección del movimiento (WASD): ");
            char direccion = char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();

            int dx = 0, dy = 0;
            if (direccion == 'W') dy = -1;
            else if (direccion == 'S') dy = 1;
            else if (direccion == 'A') dx = -1;
            else if (direccion == 'D') dx = 1;

            int newX = players[opcion].x + dx;
            int newY = players[opcion].y + dy;

            if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX] != '#')
            {
                players[opcion] = (newX, newY);
                Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} ha movido a {fichasJugadores[opcion].Nombre}!");
            }
            else
            {
                Console.WriteLine("Movimiento no válido.");
            }
        }
        else
        {
            Console.WriteLine("Opción no válida.");
        }
    }

    static void UsarHabilidadVaca(int playerIndex)
    {
        Console.Write("Dirección para destruir trampa (WASD): ");
        char direccion = char.ToUpper(Console.ReadKey().KeyChar);
        Console.WriteLine();

        int dx = 0, dy = 0;
        if (direccion == 'W') dy = -1;
        else if (direccion == 'S') dy = 1;
        else if (direccion == 'A') dx = -1;
        else if (direccion == 'D') dx = 1;

        int playerX = players[playerIndex].x;
        int playerY = players[playerIndex].y;

        bool trampaDestruida = false;

        // Recorrer 5 pasos en la dirección seleccionada
        for (int paso = 1; paso <= 5; paso++)
        {
            int newX = playerX + dx * paso;
            int newY = playerY + dy * paso;

            if (newX >= 0 && newX < width && newY >= 0 && newY < height)
            {
                if (Array.Exists(traps, t => t == maze[newY, newX]))
                {
                    Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} ha destruido una trampa en ({newX}, {newY})!");
                    maze[newY, newX] = ' '; // Elimina la trampa
                    trampaDestruida = true;
                    break; // Romper el bucle después de destruir la primera trampa
                }
            }
            else
            {
                break; // Salir del bucle si se sale del tablero
            }
        }

        if (!trampaDestruida)
        {
            Console.WriteLine("No se encontró ninguna trampa en esa dirección.");
        }
    }

    static void UsarHabilidadKoala(int playerIndex)
    {
        Console.WriteLine("Selecciona un jugador para paralizar:");
        for (int i = 0; i < numPlayers; i++)
        {
            if (i != playerIndex)
            {
                int distancia = Math.Abs(players[i].x - players[playerIndex].x) + Math.Abs(players[i].y - players[playerIndex].y);
                Console.WriteLine($"{i + 1}. Jugador {i + 1} ({fichasJugadores[i].Nombre}) - Distancia: {distancia}");
            }
        }

        int opcion = int.Parse(Console.ReadLine()) - 1;
        if (opcion >= 0 && opcion < numPlayers && opcion != playerIndex)
        {
            int distancia = Math.Abs(players[opcion].x - players[playerIndex].x) + Math.Abs(players[opcion].y - players[playerIndex].y);
            if (distancia <= 5)
            {
                Console.WriteLine($"¡{fichasJugadores[playerIndex].Nombre} ha paralizado a {fichasJugadores[opcion].Nombre}!");
                fichasJugadores[opcion].EstaParalizado = true; // Marcar como paralizado
            }
            else
            {
                Console.WriteLine("El jugador seleccionado está demasiado lejos.");
            }
        }
        else
        {
            Console.WriteLine("Opción no válida.");
        }
    }
}

public class Ficha
{
    public char Icono { get; set; }
    public string Nombre { get; set; }
    public string Habilidad { get; set; }
    public int Cooldown { get; set; } // Cooldown base (3 turnos)
    public int CooldownActual { get; set; } // Cooldown restante
    public bool EstaParalizado { get; set; }
    public bool EsquivandoTrampas { get; set; } // Nueva propiedad

    public Ficha(char icono, string nombre, string habilidad, int cooldown)
    {
        Icono = icono;
        Nombre = nombre;
        Habilidad = habilidad;
        Cooldown = cooldown;
        CooldownActual = 0; // Inicialmente, la habilidad está disponible
        EstaParalizado = false;
        EsquivandoTrampas = false; // Inicialmente, no está esquivando trampas
    }
}