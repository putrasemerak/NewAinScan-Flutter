// ignore_for_file: avoid_print
import 'dart:async';

// Mocking the behavior of DatabaseService for testing logic
class MockDatabase {
  Map<String, String> daStatus = {}; // DANo -> Status
  Map<String, double> daItems = {}; // DANo_Batch_Run_PCode -> Quantity
  Map<String, double> preparedItems = {}; // DANo_Batch_Run_PCode -> SelQty (DO_0070)

  void setupDA(String daNo, List<Map<String, dynamic>> items) {
    daStatus[daNo] = 'Open';
    for (var item in items) {
      String key = "${daNo}_${item['Batch']}_${item['Run']}_${item['PCode']}";
      daItems[key] = item['Quantity'];
      preparedItems[key] = 0;
    }
  }

  Future<void> prepareItem(String daNo, String batch, String run, String pCode, double qty) async {
    String key = "${daNo}_${batch}_${run}_$pCode";
    preparedItems[key] = (preparedItems[key] ?? 0) + qty;
    
    // Logic from _checkTotalPreparedDO0070
    double totalPrepared = preparedItems[key]!;
    double daQty = daItems[key] ?? 0;
    double outstanding = daQty - totalPrepared;

    if (outstanding <= 0) {
      // Simulate: UPDATE DO_0020 SET Status='Confirmed' WHERE ...
      // In a real test we would track the status per item, but here we just need to see if we can "knock off"
      print("Item $key CONFIRMED (Outstanding: $outstanding)");
    }
  }

  Future<bool> isDAComplete(String daNo) async {
    // Logic from _checkDAComplete
    // SELECT Count(DANo) FROM DO_0020 WHERE DANo=@DANo AND Status='Open'
    
    bool allConfirmed = true;
    daItems.forEach((key, daQty) {
      if (key.startsWith("${daNo}_")) {
        double totalPrepared = preparedItems[key] ?? 0;
        if (daQty - totalPrepared > 0) {
          allConfirmed = false;
        }
      }
    });
    
    return allConfirmed;
  }
}

void main() async {
  print("--- Testing Outbound DA Knock Off Logic ---");
  
  final mock = MockDatabase();
  
  // Setup a DA with 2 items
  String testDA = "DA12345678901";
  mock.setupDA(testDA, [
    {'Batch': 'B1', 'Run': 'R1', 'PCode': 'P1', 'Quantity': 10.0},
    {'Batch': 'B2', 'Run': 'R2', 'PCode': 'P2', 'Quantity': 20.0},
  ]);

  print("Initial state: DA $testDA is Open.");

  // Prepare first item partially
  await mock.prepareItem(testDA, 'B1', 'R1', 'P1', 5.0);
  print("DA Complete? ${await mock.isDAComplete(testDA)}"); // Expected: false

  // Prepare first item fully
  await mock.prepareItem(testDA, 'B1', 'R1', 'P1', 5.0);
  print("DA Complete? ${await mock.isDAComplete(testDA)}"); // Expected: false

  // Prepare second item fully
  await mock.prepareItem(testDA, 'B2', 'R2', 'P2', 20.0);
  bool complete = await mock.isDAComplete(testDA);
  print("DA Complete? $complete"); // Expected: true

  if (complete) {
    print("SUCCESS: DA was knocked off correctly.");
  } else {
    print("FAILURE: DA was not knocked off.");
  }
}
