# Norsk Vinskue — Kravspesifikasjon for applikasjon (requirements.md)

> **Formål.** Utvikle en webapplikasjon (med nettbrettvennlig dommergrensesnitt) som dekker: påmelding, betalingsbekreftelse og anonym nummerering av viner, blind bedømming med poengsetting, automatisk klassifisering og premiering, samt produksjon av resultatlister, dommerskjemaer og diplomer — i henhold til FNDs Vinskue‑regler.

## Omfang

### In scope

- Selvbetjent påmelding av viner med nødvendig metadata og korrekt gruppering (**Gruppe, Klasse, Kategori**) samt **Land** for nordiske gjester.
- Betalingsoppfølging og automatisk **kvittering/bekreftelse** med tildelte **unike vinnumre** (anonymisering).
- Administrasjonsflate for sekretariat: konfigurasjon av arrangement, listehåndtering, nummerering og logistikk.
- Dommeropplevelse for **blindsmaking** med poeng på **Utseende (A)**, **Nese (B)**, **Smak (C)**, én desimal, samt notatfelt.
- Automatikk for gjennomsnitt, totalsum, **klassifiseringsgrenser**, **minimum delpoeng (gate‑verdier)**, **avviksvarsling (> 4,0 poeng spredning)** og **nedjustering av medaljegrenser** dersom ingen Gull oppnås.
- Generering av resultatlister, deltakeres resultatsedler, dommerskjemaer, og diplomer/pokal‑tildelinger basert på reglene.

### Out of scope (første versjon)

- Netthandel/innløsning av betaling (start med manuell avstemming via bankfelter i skjema og kvittering).
- Kjemiske analyser/integrasjoner (ikke påkrevd).
- Vinsalg (ikke tillatt på arrangementet).

## Domenemodell (definisjoner og taksonomi)

- **Grupper**
  - **A1:** Godkjente druesorter dyrket **på friland** (Norge).
  - **A2:** **Nordiske gjesteviner** av godkjente druesorter dyrket på friland (konkurrerer om «Vinskuets beste nordiske vin», men ikke «Årets Vinbonde» eller «Vinskuets beste norske vin»).
  - **B:** Godkjente druesorter dyrket i **veksthus**.
  - **C:** **Ikke‑godkjente (prøve)sorter** dyrket på friland.
  - **D:** Ikke‑godkjente (prøve)sorter dyrket i veksthus.
  - **Merk:** «Åpen gruppe» er **fjernet** og skal ikke kunne velges.

- **Klasser**
  - **Unge:** Viner av høståret **Y** (f.eks. 2016); **rød, musserende og hetvin** fra **Y‑1** kan også regnes som unge.
  - **Eldre:** Viner av høstår **tidligere** enn Y. Blir eldre også dersom blandet med ung.

- **Kategorier**
  - a) Hvit, b) Rosé, c) Dessert, d) Rød, e) Musserende, f) Hetvin.

- **Etterlevelsesreferanser (Vinforskriften/EU)** — vises i hjelpetekster og bekreftes av deltaker ved innsending: Vedlegg XIb (definisjon av vin), Art. 62 (1a) (druesortsnavn; C og D unntatt), Vedlegg VIII Del 1 (anriking/avsyring/syrning; sone A), Vedlegg I.D (søtningsgrenser), Vedlegg IB (SO₂‑grenser).

## Roller og tilganger

- **Deltaker:** registrerer viner; mottar **kvittering** med **unikt vinnummer** etter registrert betaling; sender inn **to flasker per vin** (50–75 cl).
- **Sekretær (admin):** konfigurerer arrangement, avstemmer betalinger, utsteder nummerering, planlegger blindsmaking, skriver ut materiell og publiserer resultater.
- **Dommer:** ser kun anonymiserte viner på nettbrett/web, registrerer A/B/C‑poeng (én desimal) og kommentarer; ingen tilgang til produsentdata.
- **Observatør/presse (lesevisning):** (senere) kun tilgang til publiserte resultatlister.

## Arbeidsflyter

### 1) Påmelding og betaling

- **Skjema** (eller CSV‑import) fanger: medlemsnr., produsentnavn og kontakt, **land**, vinens navn, årgang, alkohol %, prosentvis drueblanding, **Gruppe/Klasse/Kategori**, og Vinbonde‑erklæring (≥ 100 vinstokker).
- Visning av avgift (konfigurerbar), kontoinformasjon inkl. **IBAN/BIC** og **Org.nr.**, samt betalingsfrist; påmelding gyldig når betaling er registrert.
- Ved bekreftet betaling sendes **kvittering/bekreftelse** på e‑post med **vinnummer** per vin; kvitteringen fungerer som regnskapsbilag.

### 2) Anonym nummerering

- Ved avstemt betaling **tildeles unikt nummer** per vin, og nummereringen ordnes etter **kategori‑sekvens som starter med Hvit**, deretter Rosé osv., for å speile smakerekkefølge.
- Deltakerne påfører nummeret på flaskene **før innsending**.

### 3) Logistikk

- Systemet viser innleveringsvindu og mottaksadresse i Norge samt godkjent **importør** for nordiske gjester; sen levering kan medføre at vin ikke bedømmes.

### 4) Blindsmaking

- Dommere arbeider individuelt på **anonymiserte** lister, vanligvis i **bolker (~6 viner)** per flight.
- Per vin registrerer hver dommer:
  - **Utseende (A):** 0, 1, 2, 3 (0 = Feilbeheftet/defekt).
  - **Nese (B):** 0, 1, 2, **3–4**.
  - **Smak (C):** 0–1, 2–5, 6–7, 8–9, **10–13**.
  - Alle som **tall med én desimal** der det er relevant; systemet lagrer råpoeng og beregner gjennomsnitt og sum.
- Ved mistanke om kork-/flaskefeil smakes det fra flaske nr. 2.

### 5) Avvik > 4,0 poeng og ombedømming

- Dersom **(maks dommertotal – min dommertotal) > 4,0** for samme vin, **flagges** vinen for **ombedømming**, og sekretæren varsles.

### 6) Klassifisering, premiering og tie‑break

- **Primære medaljegrenser (standard, konfigurerbare):**
  - Gull ≥ **17,0**, Sølv ≥ **15,5**, Bronse ≥ **14,0**, **Særlig utmerkelse** ≥ **12,0**.
- **Gate‑verdier (må være oppfylt for «Akseptabel» og oppover):**
  - Utseende ≥ **1,8**, Nese ≥ **1,8**, Smak ≥ **5,8**; samt **ingen «Feilbeheftet»** på noen dimensjon.
- **Nedjusteringsregel (hvis ingen Gull):** midlertidig juster til Gull ≥ **15,0**, Sølv ≥ **14,0**, Bronse ≥ **13,0**, Særlig utmerkelse ≥ **11,5**; gate‑verdier uendret. Dersom det fortsatt ikke blir Gull, brukes opprinnelige grenser.
- **Pokaler:**
  - **Årets Vinbonde:** høyest poengsum blant medaljeviner i **Gruppe A1**; «Vinbonde» = friland + ≥ 100 vinstokker.
  - **Vinskuets beste norske vin:** høyest poengsum i **A1, B, C, D** (norske viner).
  - **Vinskuets beste nordiske vin:** høyest poengsum i **A1 og A2** (tildeles til odel og eie).
  - **Tie‑break:** vinn vin med **høyeste enkeltpoeng fra én dommer**; hvis fortsatt likt, **loddtrekning**.

### 7) Publisering og materiell

- Systemet genererer:
  - **Resultatseddel** til deltaker (per vin): panelgjennomsnitt for A/B/C og **sum**, klassifisering og henvisning til dommerskjemaene.
  - **Dommerskjemaer** (per dommer/per vin) for utdeling på arrangementet.
  - **Resultatlister** pr. **Gruppe/Klasse/Kategori** med: Sum poeng, Klassifisering, Klasse, Vinnr., Årgang, Vinnavn, Drueblanding, Produsent og Gruppe.
  - **Diplomer** (Gull/Sølv/Bronse) og rapporter for pokaltildeling.
- Offentlig publisering til presse og foreningens kanaler; **individuelle dommerkommentarer publiseres ikke**.

## Funksjonelle krav

### Arrangementskonfigurasjon (Admin)

- Opprette årsarrangement med: påmeldingsvindu, avgift, bankkonto + **IBAN/BIC**, **Org.nr.**, leveringsadresser/frister, importørinfo for gjester, og kategori‑rekkefølge for nummerering.
- Forvaltning av vokabularer: Grupper (A1, A2, B, C, D), Klasser (Unge/Eldre + årsregler), Kategorier (a–f); mulighet for å aktivere/deaktivere A2 og veksthusvalg; **ingen «Åpen gruppe»**.
- Terskelsett: primære og nedjusterte grenser; gate‑verdier for Akseptabel; avviksterskel (= 4,0) — alt konfigurerbart med standardverdier fra regelverket.

### Påmelding (Deltaker)

- Obligatoriske felt: medlemsnr., produsentnavn, adresse, postnr./sted, telefon, e‑post, **land**; vinnavn; årgang; alkohol %; drueblanding i %; Gruppe/Klasse/Kategori; **Vinbonde‑erklæring**.
- Veiledning til korrekt Gruppe/Klasse/Kategori; validering av at drueblanding summerer til 100 %; støtte for flere viner per deltaker (forskjellig sammensetning, metode, kategori, klasse eller gruppe).
- Oppsummering med avgift per vin og total; betalingsinstruks; informasjon om **bindende påmelding**.

### Betaling og kvittering

- Admin markerer betalinger som mottatt (eller import via CSV); systemet sender da **kvittering** på e‑post med **tildelte vinnumre** og oppsummering; kvitteringen er et **regnskapsbilag**.

### Nummerering og lister

- Generering av **unike vinnumre**, sortert etter **kategori** med start på **Hvit**; utskrift av etikettark (kun vinnummer).
- Generering av **flight‑lister** (~6 viner per flight) til servering/logistikk.

### Dommer‑UI (nettbrett/web)

- Per vin: input A (0–3), B (0–4), C (0–13), hver med **én desimal**; kommentarfelt; «Lagre & Neste».
- Autosave og (opsjon) frakoblet mellomlagring; alle innsendinger tidsstemples og knyttes til dommeridentitet.

### Beregning og validering

- Beregn **per‑dommer total** og **panelgjennomsnitt** for A, B og C; beregn **sum av gjennomsnitt** som **avgjørende poeng**.
- Håndhev **gate‑verdier** og «ikke defekt»‑regelen.
- Evaluer medaljeklasse; hvis **ingen Gull** i arrangementet, tilby admin å aktivere **nedjusterte grenser**; hvis fortsatt ingen Gull, gå tilbake til opprinnelige.
- **Avvikskontroll:** flagg vin for ombedømming når spredning > **4,0**.
- **Tie‑break:** identifiser kandidat med høyeste **enkeltpoeng**; ellers merk som «loddtrekning kreves».

### Rapporter og utdata

- **Resultater per Gruppe/Klasse/Kategori** med påkrevde kolonner som over.
- **Deltakerpakke**: resultatseddel + 5 dommerskjemaer per vin.
- **Pokal‑rapporter** og utskriftsvennlige sertifikater.

### Etterlevelse og policy (UX‑tekster)

- Tydeliggjør at **Vinforskriften** skal følges; A2‑gjester har avvikende pokalberettigelse; **vinssalg ikke tillatt**; **dommerkommentarer er private**.

## Ikke‑funksjonelle krav

- **Sikkerhet og personvern:** Rollebassert tilgang; beskyttelse av personopplysninger; revisjonsspor for score‑endringer og adminhandlinger; eksport for arkiv og **nullstilling til nytt år**.
- **Tilstrekkelig kapasitet:** Skal håndtere samtidige innsendinger fra 5 dommere med lav ventetid.
- **Universell utforming:** Dommer‑UI skal være tastaturnavigerbar; mål om WCAG AA der mulig.
- **Språk:** Norsk som primærspråk; støtte for **Land**‑felt for nordiske gjester; vurder engelsk som sekundær UI.
- **Utskrift:** Ryddige, A4‑optimaliserte stiler for kvitteringer, etiketter, dommerskjema og diplomer.

## Datamodell (indikativ)

```mermaid
erDiagram
  EVENT ||--o{ WINE : includes
  PRODUCER ||--o{ WINE : submits
  WINE ||--o{ JUDGE_SCORE : receives
  JUDGE ||--o{ JUDGE_SCORE : gives
  WINE ||--o| RESULT : has
  WINE {
    uuid id
    int wineNumber
    string group       // A1,A2,B,C,D
    string class       // Unge, Eldre
    string category    // Hvit, Rosé, Dessert, Rød, Musserende, Hetvin
    string name
    int vintage
    decimal alcoholPct
    json blendPctByGrape
    boolean vinbonde
    string country
  }
  JUDGE_SCORE {
    uuid id
    decimal a_appearance // 0..3
    decimal b_nose       // 0..4
    decimal c_taste      // 0..13
    text comment
    decimal total
  }
  RESULT {
    uuid id
    decimal avgA
    decimal avgB
    decimal avgC
    decimal sumAvg
    string classification // Gull, Sølv, Bronse, Særlig, Akseptabel, IkkeGodkjent
    boolean defectiveFlag
    boolean outlierFlag // >4.0 spredning
  }
```

## Algoritmer (pseudokode)

### Aggregering per vin

```python
avgA = mean(scores.a_appearance)
avgB = mean(scores.b_nose)
avgC = mean(scores.c_taste)
sumAvg = round(avgA + avgB + avgC, 1)

acceptable = (avgA >= 1.8) and (avgB >= 1.8) and (avgC >= 5.8)
not_defective = all(s.a_appearance > 0 and s.b_nose > 0 and s.c_taste > 1 for s in scores)

if not acceptable or not not_defective:
    classification = "IkkeGodkjent"
else:
    thresholds = primary_thresholds  # eller nedjusterte hvis aktivert globalt
    classification = classify(sumAvg, thresholds)  # Gull/Sølv/Bronse/Særlig/Akseptabel
```

### Avvikskontroll (per vin)

```python
spredning = max(s.total for s in scores) - min(s.total for s in scores)
outlierFlag = spredning > 4.0
```

### Tie‑break

```python
# blant kandidater med lik sumAvg
vinner = kandidat_med_hoyeste_enkeltpoeng()
if flere_igjen:
    resultat = "Loddtrekning"
```

## Skjemaer og dokumenter (feltlister)

**Påmeldingsskjema** — obligatorisk: Medlemsnr., Produsentnavn, Adresse, Postnr./Sted, Telefon, E‑post, **Land**, Vinnavn, Årgang, Alkohol %, Drueblanding i %, Gruppe, Klasse, Kategori, **Vinbonde‑erklæring**, Avgift & betalingsinstruks (Bank, **IBAN/BIC**, frist).

**Kvittering (bekreftelse)** — inkluderer: **Tildelt vinnummer**, alle påmeldingsfelt, **betalt total**, FND **Org.nr.**; fungerer som regnskapsbilag.

**Dommerskjema (per dommer)** — A/B/C‑rubrikk med tillatte intervaller og én‑desimalsfelt + kommentarfelt; **sum** vises.

**Resultatseddel (per vin)** — Panel**gjennomsnitt** for A/B/C, **sum**, klassifisering og henvisning til dommerskjemaene.

## Akseptansekriterier (eksempler)

1. Når admin markerer betaling mottatt, mottar deltaker **kvittering** som lister hver vin med tildelt **unikt vinnummer**.
2. Dommere kan registrere **én desimal** for A/B/C; systemet lagrer råpoeng og beregner panelgjennomsnitt og **sum**.
3. En vin med avgA=1,7 eller avgB=1,7 eller avgC=5,7, eller med «Feilbeheftet» på noen dimensjon, blir **IkkeGodkjent** uansett totalsum.
4. Hvis ingen vin får **Gull** etter første beregning, kan admin aktivere **nedjusterte grenser**; hvis fortsatt ingen Gull, går systemet tilbake til opprinnelige grenser.
5. Dersom **dommerspredningen** for en vin overstiger **4,0**, flagges vinen og havner i **Ombedømmingskø**.
6. Rapporten **Årets Vinbonde** viser høyest poeng blant medaljeviner innen **Gruppe A1** og verifiserer **Vinbonde**‑status.
7. Eksport av resultater inneholder nødvendige kolonner og grupperes pr. **Gruppe/Klasse/Kategori**.

## Rammer og policy

- **Ingen vinsalg** på arrangementet; norske skjenkebestemmelser skal følges.
- **Dommerkommentarer** deles med deltakerne, men publiseres ikke offentlig.
- Deltakerne er ansvarlige for **etterlevelse av Vinforskriften**; applikasjonen viser lenker/erkjennelse av relevante bestemmelser ved påmelding.

## Åpne spørsmål / antakelser

- Eksakte datoer, avgiftsbeløp og adresser er **konfigurerbare** per år (plassholdere i denne spesifikasjonen).
- Om dommer‑UI skal støtte **frakoblet** bruk (lokal cache + senere synk) eller kreve kontinuerlig nett.
- Etikettmal (Avery e.l.) og visuell utforming av diplom (profil/branding).

---

**Notat:** «2017»‑eksempler i kildedokumentet er normalisert som **konfigurerbare** hendelsesparametere pr. år.
