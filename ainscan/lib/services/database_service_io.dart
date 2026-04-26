import 'database_service_interface.dart';
import 'database_service_native.dart';

DatabaseServiceInterface createDatabaseService() => DatabaseServiceNative();
