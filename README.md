# lab06 - WPF Text Encryption (AES & SHA-3)

---

## 🇵🇱 Wersja Polska

### O projekcie
Projekt akademicki zrealizowany w ramach laboratoriów uniwersyteckich. Jest to aplikacja okienkowa (desktopowa) napisana w technologii **WPF (Windows Presentation Foundation)** oparta na platformie **.NET / C#**. 

Program służy do bezpiecznego szyfrowania wiadomości tekstowych oraz generowania kodów kontrolnych.

### 💡 Jak to działa? (W prostych słowach)
Program wykorzystuje dwa różne algorytmy kryptograficzne, które współpracują ze sobą jak idealny zespół ochroniarzy:

1. **AES tworzy unikalny, zakodowany symbol (Szyfrowanie):** Algorytm AES bierze zwykły tekst i zamienia go w unikalny, nieczytelny bełkot. Dba o to, aby nikt niepożądany nie mógł przeczytać wiadomości bez znajomości tajnego hasła.
2. **SHA-3 wystawia certyfikat autentyczności (Integralność):** Algorytm SHA-3 skanuje ten zakodowany symbol i generuje dla niego unikalny kod kontrolny (odcisk palca pliku). Działa on jak nienaruszalna pieczęć, która gwarantuje odbiorcy, że nikt po drodze nie zmienił ani jednej litery w zaszyfrowanej wiadomości.

---

## 🇬🇧 English Version

### About the Project
An academic application developed as part of university laboratory courses. It is a desktop application built with **WPF (Windows Presentation Foundation)** using **.NET / C#**.

The purpose of the software is secure text encryption and checksum generation.

### 💡 How does it work? (In simple words)
The program combines two different cryptographic standards that work together as a perfect security team:

1. **AES creates a unique, encoded symbol (Encryption):** The AES algorithm takes plain text and transforms it into a unique, unreadable ciphertext. It ensures confidentiality, meaning no one can read the message without the secret password.
2. **SHA-3 issues a certificate of authenticity (Integrity):** The SHA-3 algorithm scans that encoded symbol and generates a unique control code (file fingerprint). It acts as an unbreakable seal, guaranteeing the recipient that not a single character was modified or corrupted during transit.
