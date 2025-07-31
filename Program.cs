using Microsoft.VisualBasic.FileIO;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Reflection;



//Konfiguracja bazy 
public static class AppConfig
{
    public static string DatabaseConnectionString { get; set; } = @"Server = --twoj_serwer-- ;Database= --baza_danych--;Trusted_Connection=True;";
}

public class UserRaport : IDocument {
    private readonly string login;
    private readonly string password;
    private readonly string username;
    private readonly UserRole role;

    public UserRaport(string login, string password, string username, UserRole role) 
    {
        this.login = login;
        this.password = password;
        this.username = username;
        this.role = role;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container) {
        container.Page(page => {
            page.Size(PageSizes.A4);
            page.Margin(4, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(16));

            page.Content().Column(col =>
            {
                col.Item().Text("Raport użytkownika:").Bold().FontSize(25);
                col.Item().Text($"Login: {login}");
                col.Item().Text($"Hasło: {password}");
                col.Item().Text($"Nazwa użytkownika: {username}");
                col.Item().Text($"Rola: {role}");


            });

            page.Footer()
           .AlignLeft()
           .Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
           .FontSize(10)
           .FontColor(Colors.Grey.Darken2);
        });


    }
}

public class GeneralRaport : IDocument{
    private readonly List<User> tab_users;

    public GeneralRaport(List<User> tab_users) {
        this.tab_users = tab_users;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(4, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(16));

            page.Header().Element(container =>
                container
                .PaddingBottom(15)
                .Text("Raport ogólny użytkowników bazy")
                .FontSize(20)
                .Bold()
                .FontColor(Colors.Blue.Darken3)
    );


            page.Content().Table(table =>

                
            {
               
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); // Login
                    columns.RelativeColumn();    // Hasło
                    columns.ConstantColumn(100); // Username
                    columns.ConstantColumn(80);  // Rola
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Login");
                    header.Cell().Element(HeaderStyle).Text("Hasło");
                    header.Cell().Element(HeaderStyle).Text("Username");
                    header.Cell().Element(HeaderStyle).Text("Rola");
                });

                foreach (var user in tab_users)
                {
                    table.Cell().Element(RowStyle).Text(user.Login);
                    table.Cell().Element(RowStyle).Text(user.Password);
                    table.Cell().Element(RowStyle).Text(user.Username);
                    table.Cell().Element(RowStyle).Text(user.Role.ToString());
                }

                QuestPDF.Infrastructure.IContainer HeaderStyle(QuestPDF.Infrastructure.IContainer container)
                {
                    return container
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .DefaultTextStyle(TextStyle.Default.FontColor(Colors.Blue.Darken3).Bold())
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Darken2);
                }

                QuestPDF.Infrastructure.IContainer RowStyle(QuestPDF.Infrastructure.IContainer container)
                {
                    return container
                        .Padding(10)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2);
                }
            });
            page.Footer()
           .AlignLeft() 
           .Text($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}")
           .FontSize(10)
           .FontColor(Colors.Grey.Darken2);
        });
    }



}

public enum UserRole {
    User,
    Admin
}

public class User
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public UserRole Role { get; set; }
}

public class Authentication() {
    public string CurrentUsername { get; private set; }
    public string CurrentLogin { get; private set; }
    public UserRole CurrentRole { get; private set; }


    public void Login(string username, string login, UserRole role) {
        CurrentUsername = username;
        CurrentLogin = login;
        CurrentRole = role;
        Console.WriteLine($"Zalogowany jako {CurrentUsername}");
    }

    public void Logout() {
        Console.WriteLine($"Wylogowano użytkownika: {CurrentUsername}");
        CurrentUsername = null;
        CurrentLogin = null;
        CurrentRole = UserRole.User;
    }

    public bool isLogged => !string.IsNullOrEmpty(CurrentLogin);
}

public class Log (){

    private static readonly string FileName = "appLogs.txt";
    private static readonly string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private static readonly string path = Path.Combine(directory, FileName);
    private static readonly object lockObject = new object();

    public static void SaveLog(string logEvent , string user = "System" ) {
        lock (lockObject)
        {
            try
            {
                string info = $"{DateTime.Now:dd.MM.yyyy HH:mm} - {user} - {logEvent}";

                if (!File.Exists(path)) {
                    File.WriteAllText(path, "Logi Aplikacji \n");
                    File.AppendAllText(path, "----------- \n");
                }

                File.AppendAllText(path, info + Environment.NewLine);

            }
            catch (Exception ex) {
                Console.WriteLine($"Wystąpił błąd z zapisem Loga {ex.Message}");
            
            }
        }
    
    }


}

class Program
{

    static bool validation(string password)
    {
        bool correct = true;

        if (password.Length < 8)
        {
            Console.WriteLine("Hasło musi składać sie z conajmniej 8 znaków !");
            correct = false;
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            Console.WriteLine("Hasło musi zawierać wielką litere");
            correct = false;
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            Console.WriteLine("Hasło musi zawierać małą litere");
            correct = false;
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\:;{}|<>]"))
        {
            Console.WriteLine("Hasło musi zawierać znak specjalny");
            correct = false;
        }

        return correct;
    }

    static void loadingbar()
    {
        int loadingBarLength = 20;

        for (int i = 0; i <= loadingBarLength; i++)
        {
            Console.Write("\rŁadowanie: [");
            Console.Write(new string('#', i));
            Console.Write(new string('-', loadingBarLength - i));
            Console.Write($"] {i * 5}%");
            Thread.Sleep(100); // opóźnienie
        }

        System.Threading.Thread.Sleep(2000);
        Console.Clear();
    }

    static bool exist(SqlConnection connection, string login)
    {
        string querry = "SELECT * FROM uzytkownicy WHERE Login=@login";

        using (SqlCommand command = new SqlCommand(querry, connection))
        {
            command.Parameters.AddWithValue("@login", login);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                return reader.HasRows;
            }
        }
    }



    static void generate_pdf_user(Authentication authState, SqlConnection connection)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        string password = "";
        string username = "";
        UserRole role = UserRole.User;

        string querry = $"SELECT Password , Username , Role FROM uzytkownicy WHERE Login=@login;";
        using (SqlCommand command = new SqlCommand(querry, connection))
        {
            command.Parameters.AddWithValue("login", authState.CurrentLogin);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    password = reader.GetString(0);
                    username = reader.GetString(1);
                    role = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString(2));
                }
            }
        }

        var userRaport = new UserRaport(authState.CurrentLogin, password, username, role);
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = Path.Combine(desktopPath, "raport_uzytkownika.pdf");
        userRaport.GeneratePdf(path);

        Console.WriteLine($"Raport wygenerowany dla użytkownika {username}");
        Log.SaveLog("Wygenerowano pdf użytkownika", authState.CurrentUsername);

    }

    static void generalRaport(SqlConnection connection,Authentication authState)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        List<User> tab_users = new List<User>();

        string querry = $"SELECT Login,Password , Username , Role FROM uzytkownicy ;";
        using (SqlCommand command = new SqlCommand(querry, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {


                while (reader.Read())
                {
                    tab_users.Add(new User
                    {
                        Login = reader.GetString(0),
                        Password = reader.GetString(1),
                        Username = reader.GetString(2),
                        Role = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString(3)),
                    });
                }
            }
        }
        var generalraport = new GeneralRaport(tab_users);
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = Path.Combine(desktopPath, "raport_ogolny.pdf");
        generalraport.GeneratePdf(path);

        Console.WriteLine($"Raport ogólny został wygenerowany");
        Log.SaveLog("Wygenerowano raport ogólny", authState.CurrentUsername);


    }

    static void show_user_data(string login, SqlConnection connection)
    {
        string querry = $"SELECT Login,Password,Username,Role FROM uzytkownicy WHERE Login =@login;";

        SqlCommand command = new SqlCommand(querry, connection);
        command.Parameters.AddWithValue("login", login);

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string login_u = reader.GetString(0);
            string password_u = reader.GetString(1);
            string username_u = reader.GetString(2);
            UserRole role_u = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString(3));


            Console.WriteLine($"\nTwoje dane to: \nLogin: {login_u} \nHasło: {password_u} \nNazwa użytkownika: {username_u} \nUprawnienia: {role_u}");
        }
        reader.Close();

    }

    static string loadPassword()
    {

        string password = "";

        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");

            }

        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;

    }

    static void register_page(SqlConnection connection, Authentication authState)
    {
        Console.WriteLine("Witaj w centrum rejestracji nowych użytkowników \nAby dodać konto , musimy najpierw poprosić cię o kilka danych ");
        Console.WriteLine("Podaj proszę swój login (będziesz używał go aby móc zalogować się na swoje konto)");
        string login = "";
        bool correct = false;


        while (!correct)
        {
            login = Console.ReadLine();

            if (login.Length < 3 || string.IsNullOrEmpty(login))
            {
                Console.WriteLine("Login musi być kombinacją conajmniej 3 liter/znaków - Proszę podaj dłuższy login:");
                continue;
            }

            string querry = $"SELECT Login FROM uzytkownicy WHERE Login=@login;";
            SqlCommand command = new SqlCommand(querry, connection);
            command.Parameters.AddWithValue("@login", login);

            using (SqlDataReader reader = command.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    Console.WriteLine("Login musi być unikalny , taki login istnieje już w bazie:");
                }
                else
                {
                    correct = true;
                }
            }
            
        }



            Console.WriteLine("Login zaakceptowany");
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("Teraz proszę podać hasło , bedziesz używał go do logowania się w naszym systemie (hasło musi składać się z conajmniej 5 znaków) : ");
            bool correct_password = false;
            string password = "";
            while (!correct_password)
            {

                password = loadPassword();

                if (validation(password)) correct_password = true;

            }
            Console.WriteLine("Hasło zapisane ! ");
            Console.WriteLine("Jak chcesz się nazywać ? Żebyśmy wiedzieli jak się do ciebie zwracać :P ");
            string username = Console.ReadLine();

            string add_user = $"INSERT INTO uzytkownicy(Login,Password,Username,Role) VALUES(@login , @password , @username,@role)";

            SqlCommand command_add = new SqlCommand(add_user, connection);

            command_add.Parameters.AddWithValue("login", login);
            command_add.Parameters.AddWithValue("password", password);
            command_add.Parameters.AddWithValue("username", username);
            command_add.Parameters.AddWithValue("role", UserRole.User.ToString());

            command_add.ExecuteNonQuery();

            loadingbar();
            Console.WriteLine($"Cześć {username} , twoje dane zostały dodane do bazy - zostałeś automatycznie zalogowany !");
            //System.Threading.Thread.Sleep(3000);
            //login_page(connection);
            authState.Login(
                username,
                login,
                UserRole.User
                );
            loadingbar();
            Log.SaveLog("Nowy użytkownik dodany", username);
            main_page(authState, connection);
        

    }


        static void login_page(SqlConnection connection, Authentication authState)
        {
            if (authState.isLogged)
            {
                main_page(authState, connection);
                return;
            }
            ;
            Console.Write("Podaj login:");
            string login = Console.ReadLine();

            string querry_login = $"SELECT Login,Role FROM uzytkownicy WHERE Login=@login;";
            SqlCommand command = new SqlCommand(querry_login, connection);
            command.Parameters.AddWithValue("login", login);

            using (SqlDataReader reader = command.ExecuteReader())
            {

                if (reader.HasRows)
                {
                    reader.Read();
                    UserRole userRole = (UserRole)Enum.Parse(typeof(UserRole), reader.GetString(1));
                    reader.Close();

                    Console.WriteLine("Podaj hasło");
                    string password = loadPassword();

                    string querry_password = $"SELECT Password,Username FROM uzytkownicy WHERE Login=@login AND Password=@haslo";

                    using (SqlCommand command2 = new SqlCommand(querry_password, connection))
                    {

                        command2.Parameters.AddWithValue("login", login);
                        command2.Parameters.AddWithValue("haslo", password);


                        using (SqlDataReader reader2 = command2.ExecuteReader())
                        {
                            if (reader2.HasRows)
                            {
                                reader2.Read();
                                loadingbar();
                              
                                authState.Login(
                                    reader2.GetString(1),
                                    login,
                                    userRole
                                    );
                                System.Threading.Thread.Sleep(3000);
                                reader2.Close();
                                Log.SaveLog("Zalogowano", authState.CurrentUsername);
                                main_page(authState, connection);

                            }
                            else
                            {
                                Console.WriteLine("Błędne hasło - następuje przekierowanie do strony głównej");
                                System.Threading.Thread.Sleep(3000);
                                return;
                            }

                        }
                    }

                }
                else
                {
                    Console.WriteLine("Brak loginu w bazie \nCzy chcesz się zarejestrować  y/n ? ");
                    char choice = Console.ReadKey().KeyChar;
                    if (choice == 'y')
                    {
                        Console.WriteLine();
                        register_page(connection, authState);
                    }
                    else { return; }
                    ;
                }
            }
        }
        static void grant_permission(SqlConnection connection, Authentication authState)
        {

            string querry_view = "SELECT Login , Role FROM uzytkownicy";
            SqlCommand command = new SqlCommand(querry_view, connection);
            SqlDataReader reader = command.ExecuteReader();


            while (reader.Read())
            {
                Console.WriteLine($"Login: {reader.GetString(0)} Role: {reader.GetString(1)}");
            }
            reader.Close();

            Console.WriteLine("Komu chcesz zmienić uprawnienia (login)? ");
            string change_login = Console.ReadLine();

            if (exist(connection, change_login))
            {
                Console.WriteLine("Jakie uprawnienie chcesz nadać? (user/admin)");
                UserRole role;

                if (Enum.TryParse(Console.ReadLine(), out role))
                {
                    string querry = "UPDATE uzytkownicy SET Role=@rola WHERE Login=@login";
                    SqlCommand command2 = new SqlCommand(querry, connection);
                    command2.Parameters.AddWithValue("rola", role.ToString());
                    command2.Parameters.AddWithValue("login", change_login);

                    command2.ExecuteNonQuery();

                    loadingbar();
                    Console.WriteLine($"Uprawnienia {role} dla uzytkownika {change_login} zostały pomyślnie nadane");
                    Log.SaveLog("Nowe uprawnienia", authState.CurrentUsername);
                }
                else
                {
                    Console.WriteLine("Nieprawidłowa rola ! Dopuszczalne wartości to: User , Admin");
                }
                //role set 

            }
            else
            {
                Console.WriteLine("Login nie istnieje w bazie");
            }

        }


         
        static void main_page(Authentication authState, SqlConnection connection)
        {

            string[] option = new string[0];

            if (authState.CurrentRole == UserRole.User)
            {
                option = new string[] { "Wyświetl moje dane", "Przygotuj pdf z moimi danymi", "Wyloguj" };
            }
            else if (authState.CurrentRole == UserRole.Admin)
            {
                option = new string[] { "Wyświetl moje dane", "Przygotuj pdf z moimi danymi", "Dostęp tylko dla admina", "Wyloguj" };
            }

            int choice = 0;
            ConsoleKeyInfo key;


            do
            {
                Console.Clear();
                Console.WriteLine($"Witaj {authState.CurrentUsername} ,wybierz opcję (steruj strzałkami góra/dół , Enter):");

                for (int i = 0; i < option.Length; i++)
                {
                    if (i == choice)
                    {
                        Console.Write("->");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine(option[i]);
                }

                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow)
                {
                    choice = (choice == 0) ? option.Length - 1 : choice - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    choice = (choice + 1) % option.Length;

                }
            } while (key.Key != ConsoleKey.Enter);

            switch (choice)
            {
                case 0:
                    show_user_data(authState.CurrentLogin, connection);
                    Thread.Sleep(3000);
                    break;
                case 1:
                    loadingbar();
                    generate_pdf_user(authState, connection);
                    Thread.Sleep(2000);
                    break;
                case 2 when authState.CurrentRole == UserRole.Admin:
                    admin_page(connection,authState);
                    Thread.Sleep(5000);
                    break;
                case 2:
                case 3:
                    Log.SaveLog("Wylogowano", authState.CurrentUsername);
                    authState.Logout();
                    Thread.Sleep(2000);
                    break;

            }


        }




        static void admin_page(SqlConnection connection, Authentication authState)
        {
            string[] view = { "Nadaj/zabierz uprawnienia użytkownikom", "Wygeneruj raport bazy", "Wróć" };

            int choice = 0;
            ConsoleKeyInfo key;

            do
            {
                Console.Clear();

                for (int i = 0; i < view.Length; i++)
                {
                    if (i == choice)
                    {
                        Console.Write("->");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine(view[i]);
                }

                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow)
                {
                    choice = (choice == 0) ? view.Length - 1 : choice - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    choice = (choice + 1) % view.Length;

                }
            } while (key.Key != ConsoleKey.Enter);

            if (choice == 0)
            {
                grant_permission(connection,authState);
            }
            else if (choice == 1)
            {
                Console.WriteLine("Wybrano generowanie raportu");
                loadingbar();
                generalRaport(connection,authState);
            }
            else
            {
                return;
            }

        }


        static void Main(string[] args)
        {
            var authState = new Authentication();

            while (true)
            {
                string[] option = { "Logowanie", "Rejestracja", "Wyjście" };
                int choice = 0;
                ConsoleKeyInfo key;

                do
                {
                    Console.Clear();
                    Console.WriteLine("Wybierz opcję (steruj strzałkami góra/dół , Enter): ");

                    for (int i = 0; i < option.Length; i++)
                    {
                        if (i == choice)
                        {
                            Console.Write("->");
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                        Console.WriteLine(option[i]);
                    }

                    key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.UpArrow)
                    {
                        choice = (choice == 0) ? option.Length - 1 : choice - 1;
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        choice = (choice + 1) % option.Length;

                    }
                } while (key.Key != ConsoleKey.Enter);

            using (SqlConnection connection = new SqlConnection(AppConfig.DatabaseConnectionString))
            {
                connection.Open();

                switch (choice)
                {
                    case 0:
                      

                        login_page(connection, authState);
                        break;
                    case 1:
                      
                        register_page(connection, authState);
                        break;
                    default:
                        Log.SaveLog("Koniec programu");
                        Console.WriteLine("Koniec programu");
                        return;

                }

            }



            }

        }
    
}
