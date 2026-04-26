# Keep JTDS SQL Server JDBC driver classes
-dontwarn jcifs.**
-dontwarn org.ietf.jgss.**
-dontwarn net.sourceforge.jtds.**

-keep class jcifs.** { *; }
-keep class org.ietf.jgss.** { *; }
-keep class net.sourceforge.jtds.** { *; }
