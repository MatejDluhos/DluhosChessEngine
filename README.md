######################################################################################################
                                                 EN
######################################################################################################

                                         Installation guide:

1) Install the Arena GUI from playwitharena.de

2) Build the program - in terminal navigate into the folder 'engine' and type 'dotnet publish'

3) Install the engine in the GUI (Engine -> Install new engine -> select the .exe file of the engine).
   The executable should be found in engine/bin/Debug/net7.0 and is called Thesis.

4) The engine should automatically load. If not, load it (Engines -> Load engine -> Thesis).

######################################################################################################

                                            User guide:

1) Load a position (Position -> Set-up a position -> Load -> input FEN string).
   For convenience I included a file with FEN strings, which should test
   the entire functionality of the engine.

2) The engine will start computing when the first move is made. After a little while
   a message will appear in the lower panel, containing either a series of possible 
   paths leading to a mate-in-3 or a notification about no paths being found.
   If the first move was incorrect, it will also state that.

3) Play out the position with or without the help of the hint in the lower panel.
   If you make a wrong move or just want to explore a different path, you can use
   the move navigation arrows for going back and choosing another move. When choosing
   different moves, I recommend selecting the overwrite option.

4) After playing through the mate-in-3 sequence you can find the aesthetics evaluation
   in the eval.txt file generated in the same folder the executable is in.

5) If only 1 path exists in the position provided, the evaluation will contain a summary
   table of interesting themes present in the sequence, followed by a detailed analysis
   of every move in the sequence.

6) If multiple paths exist, the file will start with a table of average aesthetic score
   across all paths, and following it will be an analysis of the main path chosen by the engine.

######################################################################################################
                                                 CZ
######################################################################################################

                                         Návod k instalaci:

1) Nainstalujte Arena GUI ze stránky playwitharena.de

2) Zkompilujte program - v terminálu se přesuňte do složky 'engine' a napište 'dotnet publish'

3) Nainstalujte engine v GUI (Engine -> Install new engine -> vyberte .exe soubor enginu).
   Soubor by se měl nacházet v engine/bin/Debug/net7.0 a nazývá se Thesis.

4) Engine by se měl načíst automaticky. Pokud ne, načtěte ho. (Engines -> Load engine -> vyberte engine)

######################################################################################################

                                        Uživatelská příručka:

1) Načtěte pozici (Position -> Set-up a position -> Load -> vložte FEN string).
   Pro příhodnost jsem v projektu zahrnul soubor s FEN stringy, které by měly
   testovat celou funkcionalitu enginu.

2) Engine začne pracovat po uskutečnění prvního tahu. Po malé prodlevě se v dolním 
   panelu objeví zpráva obsahující všechny možné cesty vedoucí k matu ve třech tazích,
   nebo oznámení popisující to, že žádná cesta nebyla nalezena. Pokud byl první tah
   nekorektní, vypíše se zde oznámení i o tomto.

3) Pokračujte v hraní pozice, ať už bez nebo s dopomocí nápovědy v dolní panelu.
   Pokud uděláte špatný tah nebo jen chcete prozkoumat jinou cestu, můžete využít
   navigačních šipek pro vracení se v sekvenci a zvolení jiného tahu. Při vracení se
   a volení jiného tahu je doporučeno používat možnost 'overwrite'.

4) Po dohrání sekvence můžete najít kompletní zhodnocení atraktivity zadané pozice
   v souboru eval.txt, který je generován ve stejné složce, kde se nachází .exe soubor.

5) Pokud v zadané pozici existuje pouze 1 cesta k matu ve 3 tazích, evaluace bude obsahovat
   tabulku zajímavých taktik a témat přítomných v dané cestě, následovat bude podrobná analýza
   každého tahu cesty.

6) Pokud existuje více cest, soubor bude začínat tabulkou průměrného hodnocení atraktivity
   ze všech možných cest, náseldovat bude analýza hlavní cesty zvolené enginem ve stejném
   formátu, který je popsán v bodě 5).

#######################################################################################################