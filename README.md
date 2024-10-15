# Symulowanie Daltonizmu (Deuteranopia)

## Opis

Projekt symulacji daltonizmu typu **Deuteranopia** pozwala na przekształcenie kolorów obrazu w taki sposób, aby naśladować sposób widzenia osób cierpiących na ten rodzaj ślepoty barw. Osoby z deuteranopią mają trudności z rozróżnianiem odcieni zieleni i czerwieni, co prowadzi do specyficznej percepcji obrazu. Symulacja uwzględnia te zmiany, aby jak najwierniej oddać doświadczanie świata przez osoby z tym zaburzeniem.

## Technologie

Projekt został napisany w języku **C#** i oferuje kilka zaawansowanych funkcji, takich jak:

- Możliwość wyboru liczby wątków, na których ma działać program (wielowątkowość),
- Wybór trybu wykonania programu: za pomocą modułów wbudowanych w **C#** lub przy użyciu kodu w **Asemblerze** (ASM),
- Zliczanie czasu wykonywania programu w obu trybach, aby porównać wydajność kodu w C# z tym napisanym w ASM.

## Funkcjonalność

Program jest dostępny wyłącznie na **Windows** i obsługuje pliki graficzne w formacie **JPEG**. Użytkownik przesyła obraz JPEG, który zostaje przekształcony tak, aby symulował widzenie osoby z deuteranopią. Wynikowy obraz pokazuje, jak wyglądałby dla osoby z tym typem daltonizmu.

## Cel projektu

Głównym celem projektu jest przeprowadzenie analizy, czy wykonywanie kodu napisanego w asemblerze daje znaczące przewagi wydajnościowe w porównaniu do kodu napisanego w C#. Dzięki temu użytkownik może zbadać różnice czasowe oraz lepiej zrozumieć, jak efektywnie można wykorzystać zasoby sprzętowe w obu językach.

## Autor

Projekt został stworzony przez **Kacpra Baryłowicza** (GitHub: [malybaryl](https://github.com/malybaryl)) jako część zaliczenia z przedmiotu **Języki Asemblerowe** na Politechnice Śląskiej w Katowicach.

## Dodatkowe informacje

Projekt pokazuje, jak nowoczesne narzędzia programistyczne, takie jak C#, mogą być zintegrowane z niskopoziomowym programowaniem w asemblerze, aby osiągnąć większą wydajność. Wybór liczby wątków oraz testowanie dwóch podejść wykonawczych (C# vs ASM) czyni ten projekt interesującym eksperymentem dla osób zainteresowanych optymalizacją i architekturą oprogramowania.
