# Stock-In: VB.NET vs Flutter — Comprehensive Comparison Report

## Menu Structure

### VB.NET (frmMenu_4.vb)
The Stock-In menu has **4 sub-menus**:
1. **Receiving** → opens `frmReceiving`
2. **Rack In** → opens `frmStockIn_1`
3. **Bincard** → opens `frmBincard`
4. **TST** → opens `frmStockIN_TST`

### Flutter (stock_in_menu_screen.dart)
Has all **4 matching** sub-menus:
1. **RECEIVING** → `ReceivingScreen`
2. **RACK IN** → `RackInScreen`
3. **BINCARD** → `BincardScreen`
4. **TST** → `StockInTstScreen`

**Verdict**: ✅ Menu structure matches perfectly.

---

## 1. RACK IN: frmStockIn_1.vb → rack_in_screen.dart

### 1A. SQL Queries/Operations in VB.NET

#### Pallet Scan (bcr_BarcodeRead — pallet-length barcodes)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 1 | `SELECT PCode FROM TA_PLT001 WHERE PltNo=@PltNo` | ✅ Same query | Match |
| 2 | `SELECT * FROM IV_0250 WHERE Pallet=@Pallet AND Loct='TST4' AND OnHand > 0` (Kulim check) | ✅ Same query | Match |
| 3 | `SELECT Batch,Cycle FROM TA_PLT001 WHERE PltNo=@PltNo` | ✅ Same query | Match |
| 4 | `SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus='Transfer'` | ✅ Same query | Match |
| 5 | `SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus='Transfer'` | ✅ Same query | Match |
| 6 | `SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus='Transfer' AND Status='C'` | ✅ Same query | Match |
| 7 | `SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus='Transfer' AND Status='C'` | ✅ Same query | Match |
| 8 | `SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` | ✅ Same query | Match |
| 9 | `SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo` | ✅ Same query | Match |
| 10 | `SELECT Batch,PCode,FullQty,Unit,lsQty,Cycle FROM TA_PLT001 WHERE PltNo=@PltNo` (normal) | ✅ Same query | Match |
| 11 | `SELECT PCode,Unit FROM TA_PLT001 WHERE PltNo=@PltNo` (loose) | ✅ Same query | Match |

#### Rack Scan (bcr_BarcodeRead — rack-length barcodes)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 12 | `SELECT Rack FROM BD_0010 WHERE Rack=@Rack` | ✅ Same query | Match |

#### Normal_Pallet()
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 13 | `SELECT COUNT(Loct) FROM IV_0250 WHERE Loct=@Loct AND OnHand > 0` (check existing stock at rack) | ✅ Same query | Match |
| 14 | `SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` | ✅ Same query | Match |
| 15 | `SELECT * FROM IV_0250 WHERE Loct='TST2' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0` (non-Kulim) | ✅ Same query | Match |
| 15a | `SELECT * FROM IV_0250 WHERE Loct='TST4' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0` (Kulim) | ✅ Same query | Match |
| 16 | `UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand ... WHERE Loct='TST2'/'TST4' And Pallet=@Pallet...` | ✅ Same logic | Match |
| 17 | `SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet` (check rack) | ✅ Same query | Match |
| 18 | `INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)` | ✅ Same INSERT | Match |
| 19 | `UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty ... WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet` | ✅ Same UPDATE | Match |
| 20 | `SELECT COUNT(Status) FROM TA_PLT001 WHERE PltNo=@PltNo` → `UPDATE TA_PLT001 SET Status='C'` (close pallet) | ✅ Same logic | Match |
| 21 | `SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet` → `UPDATE TA_PLT002 SET Status='C'` (close transfer sheet) | ✅ Same logic | Match |
| 22 | `SELECT SUM(OnHand) AS Qty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0 GROUP BY Batch,Run` | ✅ Same query | Match |
| 23 | `SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` | ✅ Same query | Match |
| 24 | `UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` (non-Kulim) | ✅ Same UPDATE | Match |
| 24a | `UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack, Balance=@Balance WHERE ...` (Kulim) | ✅ Same UPDATE | Match |
| 25 | `SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run` | ✅ Same query | Match |
| 26 | `UPDATE TA_LOC0600 SET ...` or `INSERT INTO TA_LOC0600 ...` (upsert log) | ✅ Same logic | Match |
| 27 | `SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet` (ListView) | ✅ Same query | Match |

#### Loose_Pallet()
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 28-38 | Same patterns as Normal_Pallet but loops through TA_PLL001 entries | ✅ Same loop logic | Match |

### 1B. Logic Differences

| Area | VB.NET | Flutter | Difference |
|------|--------|---------|------------|
| **Barcode reader** | Intermec hardware BarcodeReader with `bcr_BarcodeRead` event | Camera-based scanner dialog + text input | Expected platform difference |
| **Local DB log** | Saves to SqlCe `RackIn` table on device | Saves to SQLite via `LocalDatabaseService.insertRackInLog` | Equivalent, different DB engine |
| **Scan reload button** | `cmdReload_Click` re-initializes barcode reader | Not needed (camera-based) | N/A |
| **ChangeRack button** | Opens `frmChangeRack` form | Not present in Flutter | ⚠️ MISSING |
| **Receiving button** | `cmdRcv_Click` opens `frmReceiving` | Not present (separate screen in menu) | Different navigation approach |
| **Ref field in TA_LOC0600** | Normal=1, Loose=2 | Normal=1, Loose=2 | ✅ Match |
| **Counter display** | `txtCount.Text = ID + 1` from local DB | `_count` variable incremented manually | Same concept |

### 1C. Missing Functionality in Flutter

1. **❌ Change Rack** — VB.NET has `cmdChangeRack_Click` → opens `frmChangeRack`. Flutter has no equivalent screen/feature.
2. **❌ Navigate to Receiving** — VB.NET `cmdRcv_Click` opens `frmReceiving` from within Rack In. Flutter navigates via menu instead (acceptable redesign).
3. **❌ List button navigation** — VB.NET `btnList_Click` opens `frmList` with `txtForm = "R2"`. Flutter opens `LogListScreen(formType: 'S1')` — form type R2 vs S1 difference.

---

## 2. RECEIVING: frmReceiving.vb → receiving_screen.dart

### 2A. SQL Queries/Operations in VB.NET

#### Pallet Scan (bcr_BarcodeRead)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 1 | `SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus=@TStatus` (check received) | ⚠️ Different: Flutter uses `SELECT TStatus FROM TA_PLT001 WHERE PalletNo=@PalletNo` | **Column name diff**: VB uses `PltNo`, Flutter uses `PalletNo` |
| 2 | `SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus=@TStatus` (check received in old table) | ⚠️ Different: Flutter uses `SELECT TStatus FROM TA_PLT002 WHERE PalletNo=@PalletNo` | **Column name diff**: VB uses `Pallet`, Flutter uses `PalletNo` |
| 3 | `SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo` | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 4 | `SELECT Batch,PCode,FullQty,Unit,lsQty,Cycle FROM TA_PLT001 WHERE PltNo=@PltNo` (normal) | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 5 | `SELECT Batch,Cycle,PCode,PltNo FROM TA_PLT001 WHERE PltNo=@PltNo` → `SELECT COUNT(loct) FROM IV_0250 WHERE Loct='TST1' AND Batch=@Batch AND Pallet=@Pallet` (TST1 stock check) | ⚠️ Flutter _checkStockAtTST1 uses `Location=@Location` instead of `Loct` | **Column name diff**: `Loct` vs `Location` |

#### Normal_Pallet() — TST1→TST2 Transfer
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 6 | `SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet AND OnHand > 0` | ⚠️ Flutter uses `Location=@Location` column name | **Column name diff** |
| 7 | `UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand ... WHERE Loct='TST1' AND Pallet=@Pallet AND OnHand >0` | ⚠️ Flutter uses `OutputQty = OutputQty + @Qty, OnHand = OnHand - @Qty WHERE ... Location=@Location` — **inline arithmetic** vs pre-computed values | **Different approach** |
| 8 | `SELECT * FROM IV_0250 WHERE Loct='TST2' AND Pallet=@Pallet` → INSERT or UPDATE TST2 | ⚠️ Flutter uses `Location=@Location` | **Column name diff** |
| 9 | INSERT IV_0250 at TST2 with full column list: `(Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)` | ⚠️ Flutter INSERT only has `(Batch, PCode, Run, Location, InputQty, OnHand)` | **❌ MISSING COLUMNS**: PGroup, PName, Unit, Status, OpenQty, OutputQty, Pallet, AddUser, AddDate, AddTime |
| 10 | `UPDATE TA_PLT001 SET TStatus='Transfer', PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo=@PltNo` | ⚠️ Flutter uses `PalletNo=@PalletNo` and includes `PickBy, RecUser, RecDate, RecTime` | **Column name diff** |
| 11 | `UPDATE TA_PLT002 SET TStatus='Transfer', PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet=@Pallet` | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |

#### Loose_Pallet() — Same TST1→TST2 but loops through TA_PLL001
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 12 | Full loop reading `TA_PLL001`, deducting TST1, adding TST2 per run entry | ✅ Same logic structure | Match |
| 13 | Local SQLite log per entry via `SqlCeCommand` | ✅ Flutter uses `_localDb` | Match |

### 2B. Critical Differences

| Area | VB.NET | Flutter | Impact |
|------|--------|---------|--------|
| **Column name `PltNo` vs `PalletNo`** | Uses `PltNo` in TA_PLT001, `Pallet` in TA_PLT002 | Uses `PalletNo` everywhere | ❌ **SQL will fail** unless the actual DB column is `PalletNo` |
| **Column name `Loct` vs `Location`** | Uses `Loct` in IV_0250 | Uses `Location` in several queries | ❌ **SQL will fail** unless column was renamed |
| **IV_0250 INSERT column set** | Full 16-column INSERT including PGroup, PName, Unit, Status, OpenQty, etc. | Minimal 6-column INSERT (Batch, PCode, Run, Location, InputQty, OnHand) | ❌ **Missing data** — PGroup, PName, Unit, Status, Pallet, AddUser not saved |
| **PD_0800 TS_OUT update** | Commented out in VB.NET (was planned but disabled) | Not implemented | ✅ Correctly omitted |
| **Local DB COUNT check** | `SELECT COUNT(Id) FROM Receiving` for incrementing ID | Uses `_localDb.insertReceivingLog` with auto-increment | Acceptable |
| **TST1 stock check** | VB.NET checks `WHERE Loct='TST1' AND Batch=@Batch AND Pallet=@Pallet` | Flutter checks `WHERE Batch=@Batch AND PCode=@PCode AND Run=@Run AND Location=@Location` — **includes PCode and Run** | Different WHERE clause |
| **Beep on success** | `Beep()` | SnackBar message "Pallet ... berjaya diterima" | Acceptable UX difference |
| **Seq_Check()** | Generates transfer sheet number from SY_0040 `KeyCD1/KeyCD2=17` | Not implemented | ⚠️ May not be needed (was button-triggered) |

### 2C. Missing Functionality in Flutter

1. **❌ Consistent column naming** — Multiple queries use `PalletNo` and `Location` instead of VB.NET's `PltNo`/`Pallet` and `Loct`. These will cause SQL errors against the same database.
2. **❌ Complete IV_0250 INSERT** — When inserting new TST2 records, Flutter omits PGroup, PName, Unit, Status, OpenQty, OutputQty, Pallet, AddUser, AddDate, AddTime.
3. **❌ Seq_Check / Transfer Sheet Number** — VB.NET has a function to generate TS numbers from SY_0040. Not in Flutter. (May be unused in production.)
4. **❌ cmdStockIn navigation** — VB.NET `cmdStockIn_Click` opens Rack In from Receiving screen. Not in Flutter.
5. **❌ frmList (R1 form)** — VB.NET has `Button1_Click` opening `frmList` with `txtForm = "R1"`. Flutter has a `LogListScreen(formType: 'R1')` — needs verification that it works the same.

---

## 3. TST: frmStockIN_TST.vb → stock_in_tst_screen.dart

### 3A. SQL Queries/Operations in VB.NET

#### Pallet Scan (txtPalletNo_KeyPress)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 1 | `SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo` (pallet type check) | ⚠️ Flutter uses `SELECT * FROM TA_PLL001 WHERE PalletNo=@PalletNo` | **Column name diff** |
| 2 | `SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo` (read pallet info) | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 3 | `SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo` (loose runs) | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 4 | `SELECT AddDate FROM TA_PLT002 WHERE Pallet=@Pallet` (pallet date) | ⚠️ Flutter uses `SELECT GetDate()` for server date instead | **Different approach** — VB reads pallet creation date, Flutter gets current server time |

#### btnInbound_Click — NORMAL path
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 5 | `SELECT Rack FROM BD_0010 WHERE Rack=@Rack` | ⚠️ Flutter uses `SELECT * FROM BD_0010 WHERE Loct=@Loct` | **Column name diff**: `Rack` vs `Loct` |
| 6 | `SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` | ✅ Same query | Match |
| 7 | `SELECT COUNT(PltNo) FROM TA_PLT001 WHERE TStatus=@TStatus AND PltNo=@PltNo` (received check) | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 8 | TST1 create/update: `INSERT/UPDATE IV_0250 WHERE Loct='TST1'...` | ✅ Similar logic in `_createOrUpdateTst1` | Match |
| 9 | `UPDATE TA_PLT001 SET TStatus='Transfer', PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo=@PltNo` | ⚠️ Flutter: `UPDATE TA_PLT001 SET TStatus='Transfer' WHERE PalletNo=@PalletNo` — **missing PickBy, RecUser, RecDate, RecTime** | **❌ MISSING** |
| 10 | `UPDATE TA_PLT002 SET TStatus='Transfer', PickBy=@PickBy, RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet=@Pallet` | ⚠️ Flutter: `UPDATE TA_PLT002 SET TStatus='Transfer' WHERE PalletNo=@PalletNo` — **missing PickBy, RecUser, RecDate, RecTime** | **❌ MISSING** |
| 11 | `SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Status=@Status AND Pallet=@Pallet` (stock-in check, Status='C') | ⚠️ Flutter uses `PalletNo=@PalletNo` | **Column name diff** |
| 12 | TST2 create/update logic | ✅ Similar in `_createOrUpdateTst2` | Match |
| 13 | Minus TST2 (OutputQty increase, OnHand decrease) | ✅ Via `_deductTst1OnHand` and `_registerRackLocation` | Match |
| 14 | Register racking: `INSERT/UPDATE IV_0250 WHERE Loct=@Loct` | ✅ Via `_registerRackLocation` | Match |
| 15 | Close pallet card: `UPDATE TA_PLT001 SET Status='C', TStatus='Transfer'` | ⚠️ Flutter: `UPDATE TA_PLT001 SET Status='C', TStatus='Transfer' WHERE PalletNo=@PalletNo` | **Column name diff** |
| 16 | Close transfer sheet: `UPDATE TA_PLT002 SET Status='C', TStatus='Transfer'` | ⚠️ Flutter: same column name diff | **Column name diff** |
| 17 | `SELECT SUM(OnHand) AS Qty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0 GROUP BY Batch,Run` | ⚠️ Flutter: `SELECT ISNULL(SUM(OnHand),0) FROM IV_0250 WHERE PCode=@PCode AND Batch=@Batch AND Run=@Run AND Loct NOT IN ('TST1','TST2','TST3','TST4','TSTP')` | **Different approach** — Flutter excludes transit locations; VB includes all |
| 18 | `UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack...` | ⚠️ Flutter: `UPDATE PD_0800 SET RackBal=@RackBal...` | **Different column**: VB updates `Rack_In, SORack`; Flutter updates `RackBal` |
| 19 | TA_LOC0600 upsert (check existing, UPDATE or INSERT) | ⚠️ Flutter uses INSERT only with `TrxType='TST-IN'` | **Different**: VB upserts with Ref field; Flutter always inserts with TrxType |
| 20 | ListView: `SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet` | ⚠️ Flutter: `WHERE PCode=@PCode AND Batch=@Batch AND OnHand<>0` | **Different query** — VB filters by Pallet; Flutter filters by PCode/Batch |

### 3B. Critical Differences

| Area | VB.NET | Flutter | Impact |
|------|--------|---------|--------|
| **Tarikh (Date)** | Reads `AddDate` from `TA_PLT002` for the pallet's original creation date. Uses this date for all INSERT operations | Uses `SELECT GetDate()` server current date/time | ❌ **Different dates used** — VB uses pallet creation date; Flutter uses processing date |
| **IV_0250 INSERT columns** | Full 16 columns: Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime | Abbreviated: Loct,PCode,Batch,Run,InputQty,OutputQty,OnHand,QS,CreateBy,CreateDate | ❌ **Missing**: PGroup, PName, Unit, OpenQty, Pallet, AddTime |
| **PD_0800 update** | Updates `Rack_In` (cumulative) and `SORack` (total on-hand) | Updates `RackBal` only | ❌ **Missing Rack_In increment** |
| **TA_LOC0600 logging** | Upsert pattern (UPDATE if exists, INSERT if not) with fields: Pallet,Rack,Batch,Run,PCode,PName,PGroup,Qty,Unit,Ref | Always INSERT with fields: Loct,PCode,Batch,Run,Qty,EmpNo,Tarikh,PalletNo,TrxType | ❌ **Different schema** |
| **PickBy/RecUser/RecDate/RecTime** | Updated on TA_PLT001 and TA_PLT002 during receiving step | Not updated in Flutter | ❌ **Missing audit fields** |
| **Actual Qty editing** | User can edit actual qty before inbound | ✅ Flutter has `_actualQtyController` with edit capability | Match |
| **Pallet Date display** | Shows pallet creation date from TA_PLT002 | Not displayed | ⚠️ Minor UI difference |

### 3C. Missing Functionality in Flutter

1. **❌ Consistent column naming** — `PalletNo` vs `PltNo`/`Pallet`, `Loct` vs column names used
2. **❌ TA_PLT001/TA_PLT002 receiving audit** — VB.NET updates PickBy, RecUser, RecDate, RecTime. Flutter only updates TStatus.
3. **❌ Pallet creation date (Tarikh)** — VB.NET reads the pallet's original transfer sheet date and uses it for AddDate in new records. Flutter uses current server date.
4. **❌ PD_0800 Rack_In increment** — VB.NET reads existing Rack_In and adds the qty. Flutter sets RackBal as sum instead. Different approach may yield different results.
5. **❌ TA_LOC0600 upsert** — VB.NET checks for existing log and updates or inserts. Flutter always inserts (may create duplicates).
6. **❌ IV_0250 INSERT completeness** — Flutter INSERT omits PGroup, PName, Unit, Pallet, OpenQty columns.
7. **❌ Connected_To_Network()** — VB.NET has a network check function. Not in Flutter (may not be needed with different architecture).

---

## 4. BINCARD: frmBincard.vb → bincard_screen.dart

### 4A. SQL Queries/Operations in VB.NET

#### Form Load
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 1 | `SELECT MSEQ,MMIN FROM SY_0040 WHERE KEYCD1=@KEYCD1 AND KEYCD2=@KEYCD2` (KEYCD1=110, KEYCD2='FGWH') | ⚠️ Flutter: `SELECT KEYCD3,KEYCD4 FROM SY_0040 WHERE KEYCD1=110 AND KEYCD2='FGWH'` | **Different columns**: VB reads `MSEQ,MMIN`; Flutter reads `KEYCD3,KEYCD4` |

#### Bincard Scan (txtBincard_KeyPress)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 2 | `SELECT * FROM TA_PLT003 WHERE BinCardNo=@BinCardNo OR PltNo=@PltNo` | ⚠️ Flutter: `SELECT BinCardNo,PltNo,Actual,Batch,PCode FROM TA_PLT003 WHERE BinCardNo=@BinCardNo OR PltNo=@PltNo` | Similar — Flutter specifies columns |

#### Rack Validation (txtRack_KeyPress)
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 3 | `SELECT Rack FROM BD_0010 WHERE Rack=@Rack` | ⚠️ Flutter: `SELECT RackNo FROM BD_0010 WHERE RackNo=@RackNo` | **Column name diff**: `Rack` vs `RackNo` |

#### Inbound Processing (btnInbound_Click) — Loops through TA_PLT003 entries
| # | VB.NET SQL | Flutter Equivalent | Status |
|---|-----------|-------------------|--------|
| 4 | `SELECT Batch,QS,PCode FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` | ⚠️ Flutter: `SELECT PCode,Batch,TotQty FROM PD_0800 WHERE PCode=@PCode AND Batch=@Batch` | **Missing Run** in Flutter WHERE clause; **Different columns read**: VB reads QS; Flutter reads TotQty |
| 5 | `SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode` | ⚠️ Flutter: `SELECT ... FROM IV_0250 WHERE PCode=@PCode AND Batch=@Batch AND Loct=@Loct AND Pallet=@Pallet` | **Missing Run** in Flutter WHERE clause |
| 6 | INSERT IV_0250: Full 16-column insert (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status[=QS],OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime) | ⚠️ Flutter INSERT: `(Loct,OnHand,PCode,Batch,Run,Pallet,AddUser,AddDate)` | **❌ MISSING**: PGroup, PName, Unit, Status, OpenQty, InputQty, OutputQty, AddTime |
| 7 | UPDATE IV_0250: `SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, EditUser, EditDate, EditTime` | ⚠️ Flutter UPDATE: `SET OnHand=@OnHand, UpdUser, UpdDate` | **Missing InputQty, Pallet, EditTime**; **Different column names**: `EditUser/EditDate` vs `UpdUser/UpdDate` |
| 8 | `SELECT SUM(OnHand) AS Qty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0 GROUP BY Batch,Run` | ⚠️ Flutter: `SELECT ISNULL(SUM(OnHand),0) FROM IV_0250 WHERE PCode=@PCode AND Batch=@Batch` | **Different**: VB groups by Batch/Run; Flutter only filters by PCode/Batch, no Run |
| 9 | `SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode` → `UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack, Balance=@Balance ...` | ⚠️ Flutter: `UPDATE PD_0800 SET TotQty=@TotQty, UpdUser, UpdDate WHERE PCode=@PCode AND Batch=@Batch` | **❌ DIFFERENT**: VB updates Rack_In/SORack/Balance; Flutter updates TotQty. **Missing Run** in WHERE |
| 10 | `SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run` → UPDATE or INSERT | ⚠️ Flutter: Always INSERT with columns `(Loct,PCode,Batch,Pallet,Qty,TrxType,AddUser,AddDate,AddTime)` | **❌ DIFFERENT schema** — VB uses Pallet/Rack/Batch/Run/PCode/PName/PGroup/Qty/Unit/Ref with upsert; Flutter always inserts with TrxType='IN' |
| 11 | Stock Take: `SELECT COUNT(Pallet) FROM TA_STK001 WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run` → INSERT or UPDATE TA_STK001 | ⚠️ Flutter always INSERTs TA_STK001 | **❌ VB upserts** (checks existing and updates if found); **Flutter always inserts** (may create duplicates) |

### 4B. Critical Differences

| Area | VB.NET | Flutter | Impact |
|------|--------|---------|--------|
| **SY_0040 month/year columns** | Reads `MSEQ` (month) and `MMIN` (year) | Reads `KEYCD3` and `KEYCD4` | ❌ **Different column names** — will read wrong data |
| **PD_0800 QS (QC status)** | Used as `Status` in IV_0250 INSERT | Not used — IV_0250 INSERT doesn't include Status | ❌ **Missing QC status** in inventory records |
| **PD_0800 update approach** | Accumulates `Rack_In`, sets `SORack` and `Balance` from rack balance sum | Sets `TotQty` from total balance sum | ❌ **Different update columns and logic** |
| **Run in WHERE clauses** | All queries include `Run` in WHERE | Several Flutter queries **omit Run** from WHERE | ❌ **Data integrity risk** — may match wrong records |
| **IV_0250 INSERT completeness** | 16 columns including PGroup, PName, Unit, Status | 8 columns only | ❌ **Missing critical data** |
| **TA_LOC0600 schema** | Uses Pallet, Rack, PCode, PName, PGroup, Qty, Unit, Ref columns | Uses Loct, PCode, Batch, Pallet, Qty, TrxType | ❌ **Schema mismatch** |
| **TA_STK001 INSERT** | Includes `PMonth, PYear, Pallet, Batch, Run, Rack, Qty, AddUser, AddDate, AddTime` | Includes `Pallet, Batch, Run, Rack, Qty, PCode, StockMonth, StockYear, AddUser, AddDate, AddTime` | ⚠️ Different column names: PMonth→StockMonth, PYear→StockYear; Flutter adds PCode |
| **BD_0010 column** | `Rack` column | `RackNo` column | ❌ **Column name mismatch** |
| **Error handling per entry** | Uses `GoTo STEP1` with Status="Failed" to continue processing other entries and show result in ListView | Flutter catches exception and adds error string to result list | Acceptable different approach |

### 4C. Missing/Different Functionality in Flutter

1. **❌ IV_0250 INSERT missing columns** — PGroup, PName, Unit, Status (QS), OpenQty, InputQty, OutputQty, AddTime not saved.
2. **❌ IV_0250 UPDATE missing fields** — InputQty not accumulated; EditTime not set.
3. **❌ PD_0800 update logic completely different** — VB accumulates Rack_In/SORack/Balance; Flutter sets TotQty.
4. **❌ Run missing from WHERE clauses** — Multiple queries in Flutter don't filter by Run, risking data corruption.
5. **❌ QS (QC status)** not read from PD_0800 and not applied to IV_0250 records.
6. **❌ TA_STK001 upsert** — VB checks if record exists and updates; Flutter always inserts.
7. **❌ SY_0040 column mapping** — MSEQ/MMIN vs KEYCD3/KEYCD4 — will read wrong columns.
8. **❌ BD_0010 column name** — `Rack` vs `RackNo`.
9. **❌ TA_LOC0600 logging** — Completely different schema and approach.

---

## Summary: Cross-Cutting Issues

### Column Name Inconsistencies (appears across multiple screens)

| Table | VB.NET Column | Flutter Column | Affected Screens |
|-------|--------------|---------------|-----------------|
| TA_PLT001 | `PltNo` | `PalletNo` | Receiving, TST |
| TA_PLT002 | `Pallet` | `PalletNo` | Receiving, TST |
| TA_PLL001 | `PltNo` | `PalletNo` | TST |
| IV_0250 | `Loct` | `Location` (sometimes `Loct`) | Receiving |
| BD_0010 | `Rack` | `RackNo` (sometimes `Loct`) | Bincard, TST |
| SY_0040 | `MSEQ`, `MMIN` | `KEYCD3`, `KEYCD4` | Bincard |
| PD_0800 | `Rack_In`, `SORack`, `Balance` | `RackBal`, `TotQty` | TST, Bincard |
| TA_LOC0600 | `Rack`, `Ref` | `Loct`, `TrxType` | TST, Bincard |

> **Note**: The Rack In screen (rack_in_screen.dart) uses the correct VB.NET column names (`PltNo`, `Loct`, `Rack`, etc.) and is the most faithful conversion. The other screens (Receiving, TST, Bincard) have column name mismatches that suggest they were developed at a different time or may target a newer schema.

### Feature Completeness Per Screen

| Screen | SQL Match | Logic Match | Missing Features | Overall |
|--------|----------|------------|-----------------|---------|
| **Rack In** | ✅ ~95% | ✅ ~90% | Change Rack button | 🟢 Good |
| **Receiving** | ⚠️ ~60% | ⚠️ ~70% | Column names, IV_0250 INSERT completeness, Seq_Check | 🟡 Moderate |
| **TST** | ⚠️ ~55% | ⚠️ ~65% | Column names, PickBy/RecUser, Tarikh, PD_0800 logic, TA_LOC0600 schema | 🟡 Moderate |
| **Bincard** | ⚠️ ~40% | ⚠️ ~50% | Column names, QS status, Rack_In logic, Run in WHERE, IV_0250 columns, TA_LOC0600 schema | 🔴 Needs Work |

### Recommendations (Priority Order)

1. **Decide on column naming** — Determine whether the production database uses the VB.NET column names or the Flutter column names. Make all queries consistent.
2. **Complete IV_0250 INSERT statements** — Add all missing columns (PGroup, PName, Unit, Status, OpenQty, Pallet, AddUser, AddDate, AddTime) across Receiving, TST, and Bincard screens.
3. **Fix PD_0800 update logic** — Bincard and TST should accumulate `Rack_In` and set `SORack`/`Balance` to match VB.NET behavior.
4. **Add Run to WHERE clauses** — Bincard queries need Run in their WHERE clauses for data integrity.
5. **Add PickBy/RecUser audit fields** — TST screen should update these when setting TStatus=Transfer.
6. **Fix TA_LOC0600 logging** — Use upsert pattern and VB.NET column names (Pallet, Rack, Ref) instead of always-insert with TrxType.
7. **Fix TA_STK001 upsert** — Bincard should check for existing stock-take records before inserting.
8. **Add Change Rack feature** — VB.NET's `frmChangeRack` is called from Rack In screen; needs Flutter equivalent.
