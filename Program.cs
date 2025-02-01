using System;
using Spectre.Console;

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
        MostrarMenuPrincipal();
    }

    static void MostrarMenuPrincipal()
    {
        while (true)
        {
            Console.Clear();

            // Título grande "UN BOSQUE MENOS" con estilo
            AnsiConsole.Write(
                new FigletText("UN BOSQUE MENOS")
                    .Color(Color.Green) // Color del título
            );

            // Subtítulo "Menú"
            AnsiConsole.MarkupLine("[bold yellow]Menú[/]"); // Subtítulo en amarillo y negrita

            // Opciones del menú
            var opcion = MostrarMenu("Seleccione una opción:", new[] { "Jugar", "Leer la historia", "Salir" });

            switch (opcion)
            {
                case "Jugar":
                    IniciarJuego();
                    break;
                case "Leer la historia":
                    MostrarHistoria();
                    break;
                case "Salir":
                    AnsiConsole.MarkupLine("[bold red]¡Gracias por jugar! Saliendo...[/]");
                    return;
            }
        }
    }

    static void MostrarHistoria()
    {
        Console.Clear();

        // Título de la historia
        AnsiConsole.Write(
            new FigletText("Historia")
                .Color(Color.Blue) // Color del título
        );

        // Contenido de la historia
        AnsiConsole.MarkupLine("[bold]En un bosque lleno de vida, animales como capibaras, ardillas, zorros y vacas viven felices.[/]");
        AnsiConsole.MarkupLine("[bold]Sin embargo, los humanos están destruyendo su hábitat natural.[/]");
        AnsiConsole.MarkupLine("[bold]Los animales deben escapar del laberinto antes de que sea demasiado tarde.[/]");
        AnsiConsole.MarkupLine("[bold]¡Únete a la aventura y ayuda a estos animales a encontrar la salida![/]");

        // Botón para volver al menú
        AnsiConsole.MarkupLine("\n[bold yellow]Presione cualquier tecla para volver al menú principal...[/]");
        Console.ReadKey();
    }

    static void IniciarJuego()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[bold green]Seleccione un número del 2 al 4 (cantidad de jugadores):[/]");
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

        AnsiConsole.MarkupLine("[bold green]Seleccione sus fichas:[/]");
        for (int i = 0; i < numPlayers; i++)
        {
            AnsiConsole.MarkupLine($"[bold]Jugador {i + 1}:[/]");
            var opcion = MostrarMenu("Elija una ficha:", new[] {
                "Capibara (Habilidad: Cavar túnel)",
                "Ardilla (Habilidad: Esquivar trampas)",
                "Zorro (Habilidad: Mover otras fichas)",
                "Vaca (Habilidad: Destruir trampas)",
                "Mono (Habilidad: Caminar 2 pasos extra)",
                "Koala (Habilidad: Paralizar a otro jugador)"
            });

            fichasJugadores[i] = opcion switch
            {
                "Capibara (Habilidad: Cavar túnel)" => new Ficha('C', "Capibara", "Cavar túnel", 3),
                "Ardilla (Habilidad: Esquivar trampas)" => new Ficha('A', "Ardilla", "Esquivar trampas", 3),
                "Zorro (Habilidad: Mover otras fichas)" => new Ficha('Z', "Zorro", "Mover otras fichas", 3),
                "Vaca (Habilidad: Destruir trampas)" => new Ficha('V', "Vaca", "Destruir trampas", 3),
                "Mono (Habilidad: Caminar 2 pasos extra)" => new Ficha('M', "Mono", "Caminar 2 pasos extra", 3),
                "Koala (Habilidad: Paralizar a otro jugador)" => new Ficha('K', "Koala", "Paralizar a otro jugador", 3),
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
                    AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[i].Nombre} está paralizado y pierde su turno![/]");
                    fichasJugadores[i].EstaParalizado = false; // Restablecer el estado
                    continue; // Saltar el turno
                }

                int movimientos = rand.Next(4, 11); // Movimientos aleatorios entre 4 y 10
                AnsiConsole.MarkupLine($"[bold]Turno del Jugador {i + 1} ({fichasJugadores[i].Nombre})[/]");
                AnsiConsole.MarkupLine($"[bold]Movimientos disponibles: {movimientos}[/]");
                AnsiConsole.MarkupLine($"[bold]Habilidad: {fichasJugadores[i].Habilidad}[/]");

                // Verificar si la habilidad está en cooldown
                if (fichasJugadores[i].CooldownActual > 0)
                {
                    AnsiConsole.MarkupLine($"[bold yellow]Habilidad en cooldown. Turnos restantes: {fichasJugadores[i].CooldownActual}[/]");
                }

                // Usar habilidad (si no está en cooldown)
                if (fichasJugadores[i].CooldownActual == 0)
                {
                    var usarHabilidad = MostrarMenu("¿Deseas usar tu habilidad?", new[] { "Sí", "No" });

                    if (usarHabilidad == "Sí")
                    {
                        UsarHabilidad(i);
                        fichasJugadores[i].CooldownActual = fichasJugadores[i].Cooldown; // Activar cooldown
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
                    AnsiConsole.MarkupLine($"[bold]Movimiento {j + 1} de {movimientos}[/]");
                    AnsiConsole.MarkupLine("[bold]Usa las teclas W, A, S, D para moverte.[/]");

                    char move = LeerTeclaMovimiento(); // Leer tecla presionada

                    if (MovePlayer(i, move))
                    {
                        AnsiConsole.MarkupLine($"[bold green]¡Jugador {i + 1} ha ganado![/]");
                        return;
                    }

                    PrintMaze();

                    // Verificar si el jugador quedó paralizado después de moverse
                    if (fichasJugadores[i].EstaParalizado)
                    {
                        AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[i].Nombre} está paralizado y pierde los movimientos restantes![/]");
                        break; // Terminar el bucle de movimientos
                    }
                }
            }
        }
    }

    static void UsarHabilidad(int playerIndex)
    {
        switch (fichasJugadores[playerIndex].Nombre)
        {
            case "Capibara":
                UsarHabilidadCapibara(playerIndex);
                break;
            case "Ardilla":
                UsarHabilidadArdilla(playerIndex);
                break;
            case "Zorro":
                UsarHabilidadZorro(playerIndex);
                break;
            case "Vaca":
                UsarHabilidadVaca(playerIndex);
                break;
            case "Mono":
                UsarHabilidadMono(playerIndex);
                break;
            case "Koala":
                UsarHabilidadKoala(playerIndex);
                break;
        }
    }

    static void UsarHabilidadCapibara(int playerIndex)
    {
        var direccion = MostrarMenu("Dirección del túnel:", new[] { "W", "A", "S", "D" });

        int dx = 0, dy = 0;
        if (direccion == "W") dy = -1;
        else if (direccion == "S") dy = 1;
        else if (direccion == "A") dx = -1;
        else if (direccion == "D") dx = 1;

        int newX = players[playerIndex].x + dx;
        int newY = players[playerIndex].y + dy;

        if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX] == '#')
        {
            maze[newY, newX] = ' '; // Abre un túnel
            AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} ha cavado un túnel![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]No se puede cavar un túnel en esa dirección.[/]");
        }
    }

    static void UsarHabilidadArdilla(int playerIndex)
    {
        fichasJugadores[playerIndex].EsquivandoTrampas = true; // Activar la habilidad
        AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} activó su habilidad para esquivar trampas durante este turno![/]");
    }

    static void UsarHabilidadZorro(int playerIndex)
    {
        var opcion = MostrarMenu("Selecciona una ficha para mover:", GetNombresJugadores(playerIndex));

        int index = Array.FindIndex(fichasJugadores, f => f.Nombre == opcion);
        if (index >= 0 && index < numPlayers && index != playerIndex)
        {
            var direccion = MostrarMenu("Dirección del movimiento:", new[] { "W", "A", "S", "D" });

            int dx = 0, dy = 0;
            if (direccion == "W") dy = -1;
            else if (direccion == "S") dy = 1;
            else if (direccion == "A") dx = -1;
            else if (direccion == "D") dx = 1;

            int newX = players[index].x + dx;
            int newY = players[index].y + dy;

            if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX] != '#')
            {
                char currentTile = maze[newY, newX];
                players[index] = (newX, newY);

                // Verificar si la ficha movida cayó en una trampa
                if (Array.Exists(traps, t => t == currentTile))
                {
                    if (fichasJugadores[index].EsquivandoTrampas)
                    {
                        AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[index].Nombre} esquiva una trampa![/]");
                        maze[newY, newX] = ' '; // La trampa desaparece
                    }
                    else
                    {
                        // Aplicar efectos de las trampas si no está esquivando
                        if (currentTile == 'T') // Trampa de teletransporte
                        {
                            AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[index].Nombre} cayó en una trampa de teletransporte![/]");
                            maze[newY, newX] = ' '; // La trampa desaparece
                            players[index] = (1, 0); // Teletransporta al inicio
                        }
                        else if (currentTile == 'L') // Trampa de lodo
                        {
                            AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[index].Nombre} está atrapado en lodo! Pierde los movimientos restantes y su próximo turno.[/]");
                            maze[newY, newX] = ' '; // La trampa desaparece
                            fichasJugadores[index].EstaParalizado = true; // El jugador pierde su siguiente turno
                        }
                        else if (currentTile == 'R') // Trampa de red
                        {
                            AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[index].Nombre} cayó en una red! Su habilidad estará en cooldown por 3 turnos adicionales.[/]");
                            maze[newY, newX] = ' '; // La trampa desaparece
                            fichasJugadores[index].CooldownActual += 3; // Añade 3 turnos adicionales al cooldown
                        }
                    }
                }

                AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} ha movido a {fichasJugadores[index].Nombre}![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]Movimiento no válido.[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]Opción no válida.[/]");
        }
    }

    static void UsarHabilidadVaca(int playerIndex)
    {
        var direccion = MostrarMenu("Dirección para destruir trampa:", new[] { "W", "A", "S", "D" });

        int dx = 0, dy = 0;
        if (direccion == "W") dy = -1;
        else if (direccion == "S") dy = 1;
        else if (direccion == "A") dx = -1;
        else if (direccion == "D") dx = 1;

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
                    AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} ha destruido una trampa en ({newX}, {newY})![/]");
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
            AnsiConsole.MarkupLine("[bold red]No se encontró ninguna trampa en esa dirección.[/]");
        }
    }

    static void UsarHabilidadMono(int playerIndex)
    {
        fichasJugadores[playerIndex].MovimientosExtra += 2; // Añade 2 movimientos extra
        AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} usa su habilidad y ahora tiene {fichasJugadores[playerIndex].MovimientosExtra} movimientos extra![/]");
    }

    static void UsarHabilidadKoala(int playerIndex)
    {
        var opcion = MostrarMenu("Selecciona un jugador para paralizar:", GetNombresJugadores(playerIndex));

        int index = Array.FindIndex(fichasJugadores, f => f.Nombre == opcion);
        if (index >= 0 && index < numPlayers && index != playerIndex)
        {
            int distancia = Math.Abs(players[index].x - players[playerIndex].x) + Math.Abs(players[index].y - players[playerIndex].y);
            if (distancia <= 5)
            {
                AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} ha paralizado a {fichasJugadores[index].Nombre}![/]");
                fichasJugadores[index].EstaParalizado = true; // Marcar como paralizado
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]El jugador seleccionado está demasiado lejos.[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[bold red]Opción no válida.[/]");
        }
    }

    static char LeerTeclaMovimiento()
    {
        while (true)
        {
            var key = Console.ReadKey(true).KeyChar.ToString().ToUpper()[0];
            if (key == 'W' || key == 'A' || key == 'S' || key == 'D')
            {
                return key;
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]Tecla no válida. Usa W, A, S, D.[/]");
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
                AnsiConsole.MarkupLine($"[bold green]¡{fichasJugadores[playerIndex].Nombre} esquiva una trampa![/]");
                maze[newY, newX] = ' '; // La trampa desaparece
            }
            else
            {
                // Aplicar efectos de las trampas si no está esquivando
                if (currentTile == 'T') // Trampa de teletransporte
                {
                    AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[playerIndex].Nombre} cayó en una trampa de teletransporte![/]");
                    maze[newY, newX] = ' '; // La trampa desaparece
                    players[playerIndex] = (1, 0); // Teletransporta al inicio
                    return false; // No termina el juego, solo aplica el efecto
                }
                else if (currentTile == 'L') // Trampa de lodo
                {
                    AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[playerIndex].Nombre} está atrapado en lodo! Pierde los movimientos restantes y su próximo turno.[/]");
                    maze[newY, newX] = ' '; // La trampa desaparece
                    fichasJugadores[playerIndex].EstaParalizado = true; // El jugador pierde su siguiente turno
                    return false; // No termina el juego, solo aplica el efecto
                }
                else if (currentTile == 'R') // Trampa de red
                {
                    AnsiConsole.MarkupLine($"[bold red]¡{fichasJugadores[playerIndex].Nombre} cayó en una red! Su habilidad estará en cooldown por 3 turnos adicionales.[/]");
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
                char tile = displayMaze[y, x];
                string color = tile switch
                {
                    'C' => "green",
                    'A' => "yellow",
                    'Z' => "red",
                    'V' => "blue",
                    'M' => "purple",
                    'K' => "cyan",
                    'T' => "magenta",
                    'L' => "olive",
                    'R' => "grey",
                    'E' => "aqua",
                    _ => "white"
                };
                AnsiConsole.Markup($"[{color}]{tile}[/] ");
            }
            AnsiConsole.WriteLine();
        }
    }

    // Método auxiliar para mostrar menús
    static string MostrarMenu(string titulo, string[] opciones)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(titulo)
                .PageSize(Math.Max(3, opciones.Length)) // Asegura que el PageSize sea al menos 3
                .AddChoices(opciones)
        );
    }

    static string[] GetNombresJugadores(int excludeIndex)
    {
        var nombres = new System.Collections.Generic.List<string>();
        for (int i = 0; i < numPlayers; i++)
        {
            if (i != excludeIndex)
            {
                nombres.Add(fichasJugadores[i].Nombre);
            }
        }
        return nombres.ToArray();
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
    public int MovimientosExtra { get; set; } // Movimientos extra para el Mono

    public Ficha(char icono, string nombre, string habilidad, int cooldown)
    {
        Icono = icono;
        Nombre = nombre;
        Habilidad = habilidad;
        Cooldown = cooldown;
        CooldownActual = 0; // Inicialmente, la habilidad está disponible
        EstaParalizado = false;
        EsquivandoTrampas = false; // Inicialmente, no está esquivando trampas
        MovimientosExtra = 0; // Inicialmente, no tiene movimientos extra
    }
}