# EN

## Installation Guide

1. Install the Arena GUI from [playwitharena.de](https://playwitharena.de).

2. Build the program:
   - Open the terminal and navigate to the `engine` folder.
   - Run the command: `dotnet publish`.

3. Install the engine in the GUI:
   - Navigate to `Engine -> Install new engine`.
   - Select the `.exe` file of the engine. The executable can be found in `engine/bin/Debug/net7.0` and is named `Thesis`.

4. The engine should load automatically. If not:
   - Navigate to `Engines -> Load engine` and select `Thesis`.

---

## User Guide

1. **Load a Position**:
   - Navigate to `Position -> Set-up a position -> Load`.
   - Input a FEN string. For convenience, a file with FEN strings is included to test the engine's functionality.

2. **Start Computing**:
   - The engine begins computing after the first move.
   - A message will appear in the lower panel with either:
     - A series of possible paths leading to a mate-in-3, or
     - A notification indicating no paths were found.
   - If the first move was incorrect, the engine will notify you.

3. **Explore the Position**:
   - Play the position with or without using the hint in the lower panel.
   - If a wrong move is made, or you want to explore another path, use the navigation arrows to go back and select another move.
   - When changing moves, it is recommended to select the overwrite option.

4. **Evaluation Output**:
   - After completing the mate-in-3 sequence, you can find the aesthetics evaluation in the `eval.txt` file, located in the same folder as the executable.

5. **Evaluation Details**:
   - If only one path exists, the evaluation includes:
     - A summary table of interesting themes in the sequence.
     - A detailed analysis of each move in the sequence.
   - If multiple paths exist, the evaluation starts with:
     - A table of average aesthetic scores across all paths.
     - A detailed analysis of the main path chosen by the engine.

---

# CZ

## Návod k instalaci

1. Nainstalujte Arena GUI ze stránky [playwitharena.de](https://playwitharena.de).

2. Zkompilujte program:
   - Otevřete terminál a přesuňte se do složky `engine`.
   - Zadejte příkaz: `dotnet publish`.

3. Nainstalujte engine v GUI:
   - Navigujte do `Engine -> Install new engine`.
   - Vyberte `.exe` soubor enginu. Soubor se nachází v `engine/bin/Debug/net7.0` a jmenuje se `Thesis`.

4. Engine by se měl načíst automaticky. Pokud ne:
   - Navigujte do `Engines -> Load engine` a vyberte engine.

---

## Uživatelská příručka

1. **Načtení pozice**:
   - Navigujte do `Position -> Set-up a position -> Load`.
   - Vložte FEN string. Pro pohodlí je přiložen soubor s FEN stringy k testování funkcionality enginu.

2. **Spuštění výpočtu**:
   - Engine začne počítat po provedení prvního tahu.
   - Ve spodním panelu se objeví zpráva obsahující:
     - Všechny možné cesty vedoucí k matu ve třech tazích, nebo
     - Oznámení, že žádná cesta nebyla nalezena.
   - Pokud byl první tah nesprávný, zobrazí se upozornění.

3. **Prozkoumání pozice**:
   - Hrajte pozici s pomocí nebo bez nápovědy ve spodním panelu.
   - Pokud uděláte chybný tah nebo chcete zkusit jinou cestu, použijte navigační šipky k návratu a zvolení jiného tahu.
   - Při změně tahu doporučujeme vybrat možnost přepsání (`overwrite`).

4. **Výstup hodnocení**:
   - Po dokončení sekvence mate-in-3 najdete hodnocení v souboru `eval.txt`, který je generován ve stejné složce jako spustitelný soubor.

5. **Podrobnosti hodnocení**:
   - Pokud existuje pouze jedna cesta, hodnocení obsahuje:
     - Tabulku zajímavých témat v sekvenci.
     - Detailní analýzu jednotlivých tahů sekvence.
   - Pokud existuje více cest, hodnocení obsahuje:
     - Tabulku průměrných estetických skóre pro všechny cesty.
     - Analýzu hlavní cesty zvolené enginem.

---
