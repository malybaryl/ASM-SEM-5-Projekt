# Symulowanie Daltonizmu
**English description is available below.**

## Opis

Projekt **Symulowanie Daltonizmu** umożliwia przekształcanie obrazów w taki sposób, aby naśladować sposób widzenia osób cierpiących na różne typy daltonizmu, w tym **Deuteranopię**, **Protanopię** i **Tritanopię**. Użytkownik może wybrać typ daltonizmu, którego efekt chce symulować. Program przetwarza obraz, symulując zmiany w postrzeganiu kolorów zgodnie z wybranym rodzajem ślepoty barw.

W szczególności:

- **Deuteranopia** (typ 0) – problemy z rozróżnianiem zieleni i czerwieni;
- **Protanopia** (typ 1) – trudności w rozróżnianiu odcieni czerwieni;
- **Tritanopia** (typ 2) – zaburzenia w rozróżnianiu niebieskich i zielonych odcieni.

### Wzory dla każdego typu daltonizmu

#### 1. **Deuteranopia** (typ 0)
Osoby z deuteranopią mają problemy z rozróżnianiem odcieni zieleni i czerwieni. Wzory przekształcają składniki kolorów w sposób uwzględniający ten defekt:

- **Czerwony kanał (newR)**:  
  `newR = oldR * 0.625 + oldG * 0.375`

- **Zielony kanał (newG)**:  
  `newG = oldG * 0.7`

- **Niebieski kanał (newB)**:  
  `newB = oldB * 0.8`

#### 2. **Protanopia** (typ 1)
Dla osób z protanopią trudność stanowi rozróżnianie odcieni czerwieni. Wzory dla tego typu daltonizmu to:

- **Czerwony kanał (newR)**:  
  `newR = oldR * 0.567 + oldG * 0.433`

- **Zielony kanał (newG)**:  
  `newG = oldG * 0.558`

- **Niebieski kanał (newB)**:  
  `newB = oldB * 0`

#### 3. **Tritanopia** (typ 2)
W przypadku tritanopii problemy występują przy rozróżnianiu odcieni niebieskiego i zielonego. Wzory dla tego typu to:

- **Czerwony kanał (newR)**:  
  `newR = oldR * 0.95`

- **Zielony kanał (newG)**:  
  `newG = oldG * 0.433`

- **Niebieski kanał (newB)**:  
  `newB = oldB * 0.567`


### Kluczowe funkcje:
- **Wybór typu daltonizmu**: Deuteranopia, Protanopia, Tritanopia.
- **Wielowątkowość**: Automatyczne ustawienie liczby wątków oraz możliwość ręcznego dostosowania.
- **Wydajność**: Testy porównawcze między C# i ASM, obliczanie średniego czasu wykonania.
- **ASM z instrukcjami wektorowymi**: Przyspieszenie operacji przetwarzania obrazów.

Program jest napisany w języku **polskim**, ale jego interfejs jest intuicyjny, więc można go używać bez znajomości języka polskiego. **Kod źródłowy** jest w języku **angielskim**.

## Technologie

- **Język**: C# (z użyciem ASM dla zwiększenia wydajności)
- **Platforma**: Windows
- **Formaty**: Obsługuje pliki graficzne **JPEG**
- **Wielowątkowość**: Obsługuje od 1 do 64 wątków
- **Wydajność**: Porównanie C# vs ASM

## Cel projektu

Celem projektu jest porównanie wydajności kodu w **C#** oraz **ASM** przy przetwarzaniu obrazów w celu symulowania daltonizmu. Dodatkowo, użytkownicy mogą eksperymentować z różną liczbą wątków, aby zbadać, jak wielowątkowość wpływa na czas wykonania.

## Autor

Projekt stworzony przez **Kacpra Baryłowicza** (GitHub: [malybaryl](https://github.com/malybaryl)) jako część zaliczenia z przedmiotu **Języki Asemblerowe** na Politechnice Śląskiej w Katowicach.

## Dodatkowe informacje

Projekt demonstruje, jak nowoczesne technologie (C#) mogą współpracować z niskopoziomowym programowaniem (ASM) w celu uzyskania lepszej wydajności. Dzięki różnym podejściom do wielowątkowości oraz testowaniu dwóch języków, użytkownik ma okazję zgłębić zagadnienia związane z optymalizacją i architekturą oprogramowania.

---

# Color Blindness Simulation
**The description in Polish is above.**

## Description

This project simulates color blindness by transforming images to mimic the vision of people suffering from **Deuteranopia**, **Protanopia**, and **Tritanopia**. The user can select the type of color blindness to simulate, and the program processes the image to reflect how it would appear to someone with the chosen type of color vision deficiency.

Specifically:

- **Deuteranopia** (type 0) – difficulty distinguishing green and red shades;
- **Protanopia** (type 1) – problems with distinguishing red hues;
- **Tritanopia** (type 2) – trouble distinguishing blue and green hues.

### Mathematical Formulas for each type of color blindness

#### 1. **Deuteranopia** (type 0)
People with deuteranopia have difficulty distinguishing green and red shades. The transformation formulas are as follows:

- **Red channel (newR)**:  
 `newR = oldR * 0.625 + oldG * 0.375`

- **Green channel (newG)**:  
  `newG = oldG * 0.7`

- **Blue channel (newB)**:  
  `newB = oldB * 0.8`

#### 2. **Protanopia** (type 1)
For people with protanopia, difficulty lies in distinguishing red hues. The formulas for this type are:

- **Red channel (newR)**:  
  `newR = oldR * 0.567 + oldG * 0.433`

- **Green channel (newG)**:  
  `newG = oldG * 0.558`

- **Blue channel (newB)**:  
  `newB = oldB * 0`

#### 3. **Tritanopia** (type 2)
In the case of tritanopia, there are issues with distinguishing blue and green hues. The formulas for this type are:

- **Red channel (newR)**:  
  `newR = oldR * 0.95`

- **Green channel (newG)**:  
  `newG = oldG * 0.433`

- **Blue channel (newB)**:  
  `newB = oldB * 0.567`

### Key Features:
- **Choose color blindness type**: Deuteranopia, Protanopia, Tritanopia.
- **Multithreading**: Automatic or manual thread count adjustment.
- **Performance**: Compare execution times between C# and ASM.
- **ASM with vector instructions**: Speeds up pixel processing.

The program is written in **Polish**, but its interface is intuitive and can be used without knowledge of the Polish language. The **source code** is written in **English**.

## Technologies

- **Language**: C# (with ASM for performance enhancement)
- **Platform**: Windows
- **File formats**: Supports **JPEG** images
- **Multithreading**: Supports 1 to 64 threads
- **Performance**: C# vs ASM comparison

## Project Goal

The goal of this project is to compare the performance of **C#** and **ASM** when processing images to simulate color blindness. Additionally, users can experiment with different thread counts to observe the impact of multithreading on execution time.

## Author

This project was created by **Kacper Baryłowicz** (GitHub: [malybaryl](https://github.com/malybaryl)) as part of an assignment for the **Assembly Languages** course at the Silesian University of Technology in Katowice.

## Additional Information

This project demonstrates how modern technologies (C#) can be integrated with low-level programming (ASM) to achieve better performance. With different approaches to multithreading and testing two languages, users have the opportunity to explore optimization and software architecture.
