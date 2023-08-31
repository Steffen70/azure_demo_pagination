SELECT Anstellungen.AnstID,
       Anstellungen.PersID,
       Anstellungen.STID,
       Stiftungen.STName,
       Stiftungen.LaufJahr,
       Anstellungen.AGID,
       Arbeitgeber.AGName,
       Arbeitgeber.Typ       AS AGTyp,
       Arbeitgeber.Status    AS AGStatus,
       Arbeitgeber.Priv      AS AGPriv,
       Arbeitgeber.AGID_H,
       Anstellungen.AbtID,
       Abteilungen.AbtNr,
       Abteilungen.AbtName,
       Anstellungen.AnstStatus,
       Anstellungen.Eintritt,
       Anstellungen.Austritt,
       Anstellungen.AustrittGrund,
       Anstellungen.IVGrad,
       Anstellungen.AUFGrad,
       Personen.PersNr,
       Personen.Name,
       Personen.Vorname,
       Personen.Geschlecht,
       Personen.TodesDatum,
       Personen.Simulation,
       Rentner.EintrittGrund AS REG,
       Anstellungen.TreeInfo,
       Arbeitgeber.TreeInfoAG
FROM   Anstellungen
       INNER JOIN Personen
               ON Anstellungen.PersID = Personen.PersID
       INNER JOIN Arbeitgeber
               ON Anstellungen.AGID = Arbeitgeber.AGID
       INNER JOIN Abteilungen
               ON Anstellungen.AbtID = Abteilungen.AbtID
       INNER JOIN Stiftungen
               ON Anstellungen.STID = Stiftungen.STID
       /*TreeDX:JOIN*/
       LEFT JOIN (SELECT Max(ID) ID,
                         AnstID
                  FROM   Rentner
                  GROUP  BY Rentner.AnstID) MaxRentner
              ON MaxRentner.AnstID = Anstellungen.AnstID
       LEFT JOIN Rentner -- SS - 27.05.2016: Rentner Icon Pens/IV
              ON Rentner.ID = MaxRentner.ID
WHERE  Anstellungen.STID > 0
       AND Anstellungen.AGID > 0
