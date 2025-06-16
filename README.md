Informācija par lietotni:
*	Ietvars: .NET MAUI 9
*	Papildus pakotnes:
    * CommunityToolkit.Maui – Izmantots, lai paplašinātu UX elementu skaitu
    * MongoDB.Driver – izmantots, lai datus turētu datubāzē 
    *	Otp.NET – izmantots, lai ģenerētu TOTP kodus pēc RFC 6238 standarta
    * QRCoder – izmantots, lai ģenerētu un nolasītu QR kodus
*	Drošība – dati tiek glabāti šifrēti izmantojot AES-128-CBC, izmantojot PBKDF2 funkciju, lai ģenerētu atslēgu no lietotāja paroles un izmantotu daļu no rezultāta kā šifrēšanas atslēgu
*	Testētās operētājsistēmas: 
    *	Windows 11
    *	Android 13/14/15 ar One UI 7/8/9 (Samsung skin)

Kā uzstādīt lietotni no GitHub Repo
  0.	Uzstādīt Visual Studio 2022 ar .NET MAUI workload
  1.	Klonēt repozitoriju 
  2.	Atvērt failu `Lockbox.sln` 
  3.	Developer Powershell palaist komandu `dotnet restore`, lai iegūtu papildu NuGet pakotnes
  4.	Uzbūvē , izmantojot Developer Powershell ar komandu ­`dotnet build` un palaid projektu uz vēlamās ierīces.
