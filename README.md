## Witaj , jest to projekt symulujący system rejestracji/logowania z możliwością nadawania prostych uprawnień , generowania pdf i czytaniu logów 

## Opis projektu
System symulujący proces rejestracji/logowania z:
- Nadawaniem ról (Admin/User)
- Generowaniem raportów PDF
- Kompleksowym logowaniem operacji

### Wymagania obowiązkowe
-Visual Studio 2022 z .NET 6+
-SQL Server Express 

# Opcjonalnie 
- SSMS dla wygodnego zarządzania bazą 

---------------------------------------------

## Szybki start 
Konfiguracja bazy danych 

CREATE DATABSE #nazwa_bazy;

USE #nazwa_bazy

CREATE TABLE uzytkownicy (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    Username NVARCHAR(20) NOT NULL,
    Role NVARCHAR(15) NOT NULL
);

#Dane 

INSERT INTO uzytkownicy (Login, Password, Username, Role)
VALUES
-- Administratorzy
('admin1', 'Admin123!', 'Jan Kowalski', 'Admin'),
('admin2', 'ZAQ!2wsx', 'Anna Nowak', 'Admin'),
('admin3', 'HasloAdmina#1', 'Piotr Wiśniewski', 'Admin'),
('admin4', 'TrudneHaslo$2024', 'Maria Lewandowska', 'Admin'),
('admin5', 'Admin$Secret123', 'Tomasz Zieliński', 'Admin'),

-- Zwykli użytkownicy
('user1', 'User123$', 'Krzysztof Wójcik', 'User'),
('user2', 'MojeHaslo123', 'Agnieszka Szymańska', 'User'),
('user3', 'Password#2024', 'Marek Dąbrowski', 'User'),
('user4', 'BezpieczneHaslo1!', 'Magdalena Kozłowska', 'User'),
('user5', 'Qwerty123$', 'Adam Mazur', 'User');

# Przejdź do pliku z projektem -> w klasie AppConfig wpisz:
public static string DatabaseConnectionString { get; set; } = @"twoj_serwer;Database= nazwa_bazy ;Trusted_Connection=True;";

----------------------------------------------------

### Funkcjonalność aplikacji 

# System logowania i rejestracji 
- Rejestracja nowych kont z walidacją:
  - Wymagane silne hasło (8+ znaków, wielkie/małe litery, znaki specjalne)
  - Sprawdzanie unikalności loginu
- Logowanie z zachowaniem sesji
- Wylogowywanie z czyszczeniem danych sesji

# Zarządzanie uprawnieniami 
- Dwie role użytkowników:
  - **Administrator** (pełne uprawnienia)
  - **Zwykły użytkownik** (podstawowe funkcje)
- Panel admina do nadawania/odbierania uprawnień

# Generowanie raportów pdf 
- **Raport indywidualny**:
  - Dane użytkownika (login, nazwa, haslo, rola)
- **Raport zbiorczy**:
  - Lista wszystkich użytkowników
-zapis daty tworzenia pliku 
- Zapis PDF na pulpit

# System logowania zdarzeń
- Automatyczny zapis operacji do pliku `appLogs.txt` 
Plik znajduje się bin->Debug->Net
- Rejestrowane akcje:
  - Logowania/wylogowania
  - Rejestracje nowych kont
  - Zmiany uprawnień
  - Generowanie raportów
- Format logów: `[Data] [Użytkownik] [Akcja]`

# Interfejs użytkownika
- Tekstowe menu z nawigacją klawiszami (↑/↓/Enter)
- Komunikaty o statusie operacji
- Animowane ładowanie (progress bar)


-------------------------------------------------------


### Planowane funkcje 
-Hashowanie haseł
-Aktywne sesje użytkownika
-Przejście na aplikacje webową 


------------------
# Kontakt z autorem 
e-mail : narodzonekkacper2004@gmail.com


