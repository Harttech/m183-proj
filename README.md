# m183-proj

## 7.1
Für die Sicherung von Passwörtern habe ich ein sehr simples Verfahren genommen. Ich habe BCRypt für die Generierung von Salts und das Hashen von Passwörtern verwendet. Primitive Logik: Neuer Benutzer wird erstellt, Salt wird generiert, Passwort gehasht und der Hash wird dann zusammen mit dem Salt abgespeichert. Ich habe mich bewusst dafür entschieden, kein "Pepper" zu verwenden, da ich das Login einfach halten wollte, um keine Fehler zu machen und damit ich mich auf andere Sachen konzentrieren kann.

Ursprünglich habe ich den Kostenfaktor 15 verwendet. Diesen habe ich jedoch dann auf 13 runtergeschraubt, da das Aufrufen des API Tokens ansonsten zu lange dauerte (und damit das Laden der "API Documentation" Seite).
Für das API Token habe ich einen Ähnlichen Ansatz wie das Passwort gewählt, jedoch ohne das effektive Token in der Datenbank zu speichern.

Wenn ein API Token für einen Benutzer generiert wird, wird mittels BCrypt ein neuer Salt erstellt, welcher als Token für den Benutzer abgespeichert wird. Dieses Token kann jedoch nicht für das API verwendet werden. Das effektive Token ist ein Hash eines globalen Secrets. Als Salt wird der gespeicherte Token des Benutzers verwendet. Die Idee dahinter war, dass das Token selber keine Informationen des Benutzers beinhaltet. Sollte jemand also z.B. versuchen das API mittels HTTP aufzurufen, kann selbst wenn jemand die Übertragung abfängt nicht herausfinden wem das Token gehört, da es lediglich ein Hash ist. Ich habe zwar eine API Methode implementiert die den Benutzer des Tokens anzeigt, jedoch war die nur zum Testen der Authentifizierung des Tokens. Im Produktiven Einsatz wäre so eine Methode natürlich kontraproduktiv. Als Serversecret habe ich ebenfalls einen BCrypt Salt verwendet, um Zeit zu sparen. 

## 7.2
Die meisten Bibliotheken welche ich einsetze sind lediglich dafür da, die Webseite zum Laufen zu bringen. Sprich: Microsoft Bibliotheken für EntityFramework und Entwicklungsunterstützungen für ASP.Net Core. Auf diese werde ich nicht näher eingehen.

Die einzige Bibliothek welche nicht von Microsoft stammt ist "BCrypt.Net-Next.StrongName" von Chris McKee, Ryan D.Emerl und Damien Miller ([Projektlink](https://github.com/BcryptNet/bcrypt.net)). Diese stellt eine BCrypt Implementation für .NET bereit. Zum einfachen Verwenden dieser (ohnehin schon leicht zu bedienenden) Bibliothek, habe ich sie in eine Wrapperklasse (CryptoHelper) gepackt.

## 7.3
Meines Wissens nach sind EntityFramework und ASP.Net (bzw Razor) bereits ziemlich gut was XSS-Prevention angeht. Ich hatte Probleme mit Zeilenabständen, da Razor \r\n nicht als Zeilenabstand interpretiert. Zuerst habe ich \r\n manuell durch <br> ersetzt. Jedoch hat Razor das immernoch als Text interpretiert anstatt als HTML. Vorübergehend habe ich dann mit Html.Raw den Text als HTML interpretieren lassen, was natürlich eine Schwachstelle ist.

Letzendlich habe ich auf StackOverflow eine bessere Lösung gefunden. Mit dem CSS Attribut whitespace (pre-line) werden die Zeilenabstände richtig dargestellt, ohne eine XSS Schwachstelle zu öffnen.

Ansonsten sollte es keine XSS Schwachstellen geben, da keiner der eingegebenen Texte zu HTML forciert wird.

## 7.4
Auf der Website gibt es einen "Create Default" Knopf welcher vordefinierte Benutzer, Posts und Kommentare erstellt. Dies funktioniert jedoch nur, wenn noch keine Einträge in der Datenbank existieren. Beim Erstellen dieser Standard Einträge wird nach der Telefonnummer gefragt, welche für die Benutzer verwendet werden soll. 

Es werden drei Benutzer erstellt: Ein Administrator, ein Moderator und ein Benutzer (Der Moderator hat im Moment jedoch die gleichen Rechte wie der Administrator). Das Passwort ist für alle Benutzer gleich: "TestPass1!". Es werden zwei öffentliche Posts erstellt, welche die Anmeldedaten für den Administrator und den Benutzer anzeigen. Diese werden aber auch folgend aufgelistet:

- Administrator:
  - Benutzername: Admin
  - Passwort: TestPass1!
  
 - Benutzer
   - Benutzername: Member
   - Passwort: TestPass1!
 
 - Moderator
   - Benutzername: Moderator
   - Passwort: TestPass1!
  
 Bei der Loginmaske existieren zu Testzwecken auch Knöpfe um sich schnell und ohne Authentifizierung in die Benutzer einzuloggen. Dabei gehen diese auf die Benutzernamen. Die restlichen Informationen sind egal. Ebenfalls ist es möglich bei der SMS Authentifizierung "DEBUG" als Wert mitzugeben.
 
 Diese Funktionen sollten natürlich bei einem Produktiveinsatz entfernt werden.
