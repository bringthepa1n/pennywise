# Domácí úkol – mobilní aplikace

- **Termín odevzdání:** 1. 11. 2026 23:59
- **Platforma:** .NET MAUI (Cíl: Android) – na této platformě budu aplikaci i testovat
- **Forma hodnocení:** 2 samostatné známky (1. UI/UX & Design, 2. Programová logika & C#)

## Rozdělení hodnocení a kritéria

### 1. Známka váhy 3: UI, UX a Grafické zpracování (Design)

- **Konzistentní designový systém:** Správná práce se soubory `Colors.xaml` a `Styles.xaml`. Vytvoření vlastních pojmenovaných stylů (např. pro karty, primární a varovná tlačítka).
- **Správa motivů:** Plná podpora pro světlý a tmavý motiv v rámci celé aplikace. Aplikace bude mít samostatnou stránku `Settings.xaml`, ve které bude uživatel moct vybrat nastavení 3 stavů: **Světlý**, **Tmavý** a **Podle systému** (`AppTheme.Unspecified`).
- **Responzivita a Layouty:** Využijte responzivní rozložení komponent. Ať už využití grid, flex nebo stack layoutu. Počítejte s tím, že každý telefon má jiné rozlišení a velikost. Případně se dá telefon otočit na šířku a rozložení je najednou také jiné. Nezapomeňte na `ScrollView`.
- **Vizuální prvky:** Použití moderních komponent `Border` se zakulacenými rohy (zákaz starého `Frame`), použití ikon/obrázků (`Image`) a stavových indikátorů (`ProgressBar`, `ActivityIndicator`) na vhodná místa, při načítání a podobně.

### 2. Známka váhy 5: Programová logika, Navigace a C# (Kód)

- **Navigace:** Pro navigaci využijte novější způsob pomocí Shell Navigace, jak děláme i ve škole.
  - **Shell Navigace:** Použití `AppShell` s bočním menu (`FlyoutPage`) nebo záložkami (Tabs). Dynamické přechody na podstránky pomocí URI adres (`Shell.Current.GoToAsync`) a registrace tras v `AppShell.xaml.cs`.
- **Asynchronní programování & Ošetření chyb:** Používání `async`/`await`. Ošetření tlačítek proti vícenásobnému rychlému kliknutí.
- **Validace a logika vstupů:** Živé vyhodnocování textových polí (`TextChanged`), posuvníků (`ValueChanged`) a přepínačů (`Toggled`).
- **Uživatelská odezva:** Práce s dialogovými okny `DisplayAlert`.

## Téma aplikace a volba vlastního zaměření

Téma aplikace si volíte zcela sami. Může jít například o:

- Správu osobních financí / Transakce
- Ovládání chytré domácnosti (Smart Home)
- Katalog her / Sledování filmů a seriálů
- Fitness trenér / Deník cvičení a stravy
- Plánovač cestování / Správa vozidla
- Kuchařku / Správu zásob v domácnosti
- Případně i jiné podle vlastního uvážení

Ať už si zvolíte jakékoliv téma, vaše aplikace musí obsahovat následující 4 strukturované obrazovky a splňovat jejich technické požadavky. Využijte vzhledu **Material Design 3**.

## Požadované obrazovky a funkcionalita

### Obrazovka 1: Hlavní přehled / Dashboard (Hlavní položka v Shellu)

- **Přehledová mřížka (Grid):** Využijte `Grid` (kombinaci rozměrů `*`, `Auto` a pevných hodnot) pro vytvoření přehledových karet s pomocí `Border`. Karty budou zobrazovat klíčové údaje vašeho tématu (např. Zůstatek, Teplotu v domě, Odehrané hodiny, Dnešní kalorie).
- **Ukazatel stavu `ProgressBar`:** Komponenta indikující průběh či limit vztahující se k vašemu tématu (např. Čerpání rozpočtu, Cíl ušlých kroků, Využití paměti, Zaplnění kapacity). Pod ukazatel umístěte `Label` s číselným vyjádřením pro lepší vyjádření stavu.
- **Rychlé akce (`FlexLayout`):** Široký / zalamovaný layout obsahující tlačítka s rychlými akcemi nebo kategorickými filtry (`Button`, `Switch`, `Checkbox`).
- **Seznam položek pod sebou:** Seznam alespoň 5 simulovaných záznamů (historie transakcí, aktivní senzory, poslední aktivity).

### Obrazovka 2: Formulář / Přidat nový záznam (URI Navigace přes Route)

- **Navigace:** Stránka nesmí být přímo viditelná v hlavním menu Shellu. Přechází se na ni tlačítkem z Dashboardu pomocí URI adresy a trasa musí být zaregistrována.
- **Vstupní prvky:**
  - `Entry` pro název položky.
  - `Entry` pro číselný údaj s numerickou klávesnicí.
  - `Slider` pro nastavení rozsahu (např. Priorita 1–5, Intenzita, Hodnocení, Počet kusů). Zajistěte, aby se hodnota v posuvníku posouvala vždy o celá čísla. (Dejte si pozor při zpracování, pokud `Slider.Value = číslo`, dojde k opětovnému zavolání obslužné události.) K posuvníku nezapomeňte přidat živě aktualizující `Label`.
  - `CheckBox` nebo `Switch` pro doplňkovou volbu (např. Pravidelná položka, Důležité, Aktivní).
- **Odeslání a Asynchronní logika:**
  - Při stisku tlačítka „Uložit / Potvrdit“ se tlačítko ihned deaktivuje (ochrana proti vícenásobnému kliknutí).
  - Spustí se `ActivityIndicator` simulující ukládání dat (asynchronní pauza cca 2 sekundy).
  - Po dokončení vyskočí okno `DisplayAlert("Úspěch", "Záznam byl uložen", "OK")`.
    - Přidejte možnost selhání odeslání formuláře. Pokud nedojde k úspěšnému odeslání, formulář zůstane vyplněný a nedojde k automatickému zavření stránky.
  - Pokud formulář bude úspěšný, stránka se automaticky zavře a vrátí uživatele zpět odkud přišel.
  - Vše pečlivě ošetřete.

### Obrazovka 3: Externí náhled na webovou stránku

- Stránka bude obsahovat popis zobrazované stránky.
- Dále komponentu `WebView`, která načte web relevantní k vašemu tématu (např. Kurzy měn, Předpověď počasí, Online herní žebříček, Webkamera, Strojový návod).
- `WebView` bude blokovat návštěvu jiných stránek.
- Zajistěte, aby uživatel nemohl přejít na jiné stránky (příklad: bude moct zůstat jen na www.seznam.cz, a na google.com už nepůjde atd.).
- Pokud se o toto pokusí, vypište mu pomocí `DisplayAlert` chybovou hlášku.

### Obrazovka 4: Nastavení a Zabezpečení (`Settings.xaml`)

- **Přepínání motivu:** Přepínač nebo výběr mezi 3 stavy motivu: **Světlý**, **Tmavý** a **Podle systému**.
- **Interaktivní validátor hesla se zápisem do JSON souboru:**
  - `Entry` pro zadání jména.
  - `Entry` pro zadání hesla.
  - `Switch` s popiskem „Zobrazit heslo“, který zapíná/vypíná skrytí znaků.
  - Živý indikátor síly hesla: Zjednodušený, jen kontrolovat délku a zda obsahuje jen číslice, malá, velká písmena a speciální znaky.
  - Uložení do JSON souboru jsme sice v hodině nedělali, ale pokuste se ho udělat. Pokud nebude fungovat, přijdete komentář, že Vám nefunguje, a popište ve zkratce, co jste zkoušeli.
  - Odkaz na zápis do JSON: Zápis do JSON

### Obrazovka 5: Zobrazení JSON (volitelné)

- Načtení uložených dat v JSON souboru a zobrazení jich.

## Pokyny k odevzdání

Studenti mají na výběr ze dvou způsobů odevzdání:

**Možnost A: Archiv (ZIP)**

1. Odevzdává se zabalený celý projekt v archivu `.ZIP` (přísný zákaz vkládání složek `bin` a `obj` – dokáží zvětšit soubor klidně na 50 GB).
2. Do kořene projektu přiložte soubor `AUTOR.txt` nebo `README.md` se jménem, třídou, zvoleným tématem a stručným popisem aplikace.

**Možnost B: GitHub repozitář (E-mail)**

1. Kód nahrajte do svého GitHub repozitáře. Repozitář musí obsahovat smysluplnou historii commitů.
2. Soubor `README.md` v repozitáři bude obsahovat jméno autora, zvolené téma aplikace, popis funkcionality a přiložené snímky obrazovky ze světlého i tmavého režimu.
3. Na e-mail učitele zašlete do termínu odevzdání zprávu:
   - **Předmět:** PRG4 - MAUI Projekt - [Vaše Jméno a Příjmení]
   - **Obsah:** Odkaz na GitHub repozitář a stručné průvodní slovo.
