// ===========================================================================
// AINScan – SQL Server API Proxy (Node.js)
// ===========================================================================
//
// A lightweight HTTP server that bridges JSON requests from Flutter Web
// to SQL Server using the `mssql` package (pure JavaScript, no Flutter).
//
// Setup:
//   cd ainscan/api_server
//   npm install
//
// Usage:
//   node server.js
//   node server.js --port 8085 --db-host 194.100.1.222
//
// Then run Flutter:
//   cd ainscan
//   flutter run -d chrome --dart-define=API_URL=http://localhost:8085
// ===========================================================================

const http = require('http');
const sql = require('mssql');

// ── Default config (Live database – AINData) ──────────────────
let config = {
  server: '194.100.1.249',
  port: 1433,
  database: 'AINData',
  user: 'sa',
  password: 'ain06@sql',
  options: {
    encrypt: false,              // On-premise SQL Server
    trustServerCertificate: true,
    enableArithAbort: true,
    requestTimeout: 30000,       // 30 seconds
    connectionTimeout: 15000,    // 15 seconds
  },
  pool: {
    max: 10,
    min: 0,
    idleTimeoutMillis: 30000,
  },
};

let serverPort = 8085;

// ── Parse command-line arguments ───────────────────────────────────────────
const args = process.argv.slice(2);
for (let i = 0; i < args.length; i++) {
  switch (args[i]) {
    case '--port':       serverPort = parseInt(args[++i]); break;
    case '--db-host':    config.server = args[++i]; break;
    case '--db-port':    config.port = parseInt(args[++i]); break;
    case '--db-name':    config.database = args[++i]; break;
    case '--db-user':    config.user = args[++i]; break;
    case '--db-pass':    config.password = args[++i]; break;
  }
}

// ── Connection pool ────────────────────────────────────────────────────────
let pool = null;

async function getPool() {
  if (!pool) {
    pool = await sql.connect(config);
    console.log(`[OK] Connected to ${config.server}:${config.port}/${config.database}`);
  }
  return pool;
}

// ── HTTP Server ────────────────────────────────────────────────────────────
const server = http.createServer(async (req, res) => {
  // CORS headers
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'POST, GET, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
  res.setHeader('Content-Type', 'application/json');

  // Preflight
  if (req.method === 'OPTIONS') {
    res.writeHead(200);
    res.end();
    return;
  }

  const time = new Date().toTimeString().substring(0, 8);

  try {
    const url = new URL(req.url, `http://${req.headers.host}`);
    const path = url.pathname;

    switch (path) {
      case '/connect':
        await handleConnect(req, res);
        break;
      case '/disconnect':
        await handleDisconnect(req, res);
        break;
      case '/query':
        await handleQuery(req, res);
        break;
      case '/execute':
        await handleExecute(req, res);
        break;
      case '/switch-db':
        await handleSwitchDb(req, res);
        break;
      case '/health':
        sendJson(res, { status: 'ok', connected: pool !== null, database: config.database, server: config.server });
        break;
      default:
        res.writeHead(404);
        sendJson(res, { error: `Not found: ${path}` });
    }

    console.log(`[${time}] ${req.method} ${path} → ${res.statusCode}`);
  } catch (err) {
    console.error(`[${time}] ERROR: ${err.message}`);
    res.writeHead(500);
    sendJson(res, { error: err.message });
  }
});

// ── Route handlers ─────────────────────────────────────────────────────────

async function handleConnect(req, res) {
  // DB config is controlled by server command-line args only.
  // The client does NOT override which database to connect to.
  try {
    if (!pool) {
      await getPool();
    }
    sendJson(res, { connected: true });
  } catch (err) {
    pool = null;
    console.error(`[FAIL] Connect: ${err.message}`);
    res.writeHead(500);
    sendJson(res, { connected: false, error: err.message });
  }
}

async function handleDisconnect(req, res) {
  if (pool) {
    try { await pool.close(); } catch (_) {}
    pool = null;
  }
  console.log('[OK] Disconnected');
  sendJson(res, { connected: false });
}

async function handleQuery(req, res) {
  const body = await readBody(req);
  const sqlText = body.sql || '';
  const params = body.params || {};

  if (!sqlText) {
    res.writeHead(400);
    sendJson(res, { error: 'Missing "sql" field' });
    return;
  }

  try {
    const p = await getPool();
    const request = p.request();

    // Bind parameters: @paramName → value
    for (const [key, value] of Object.entries(params)) {
      const paramName = key.startsWith('@') ? key.substring(1) : key;
      if (value === null || value === undefined) {
        request.input(paramName, sql.NVarChar, null);
      } else if (typeof value === 'number') {
        request.input(paramName, Number.isInteger(value) ? sql.Int : sql.Float, value);
      } else {
        request.input(paramName, sql.NVarChar, String(value));
      }
    }

    const result = await request.query(sqlText);
    sendJson(res, { rows: result.recordset || [] });
  } catch (err) {
    if (err.message.includes('connection') || err.message.includes('closed')) {
      pool = null;
    }
    console.error(`[ERROR] Query: ${err.message}`);
    res.writeHead(500);
    sendJson(res, { error: `Query error: ${err.message}` });
  }
}

async function handleExecute(req, res) {
  const body = await readBody(req);
  const sqlText = body.sql || '';
  const params = body.params || {};

  if (!sqlText) {
    res.writeHead(400);
    sendJson(res, { error: 'Missing "sql" field' });
    return;
  }

  try {
    const p = await getPool();
    const request = p.request();

    for (const [key, value] of Object.entries(params)) {
      const paramName = key.startsWith('@') ? key.substring(1) : key;
      if (value === null || value === undefined) {
        request.input(paramName, sql.NVarChar, null);
      } else if (typeof value === 'number') {
        request.input(paramName, Number.isInteger(value) ? sql.Int : sql.Float, value);
      } else {
        request.input(paramName, sql.NVarChar, String(value));
      }
    }

    const result = await request.query(sqlText);
    sendJson(res, { success: true, affectedRows: result.rowsAffected[0] || 0 });
  } catch (err) {
    if (err.message.includes('connection') || err.message.includes('closed')) {
      pool = null;
    }
    console.error(`[ERROR] Execute: ${err.message}`);
    res.writeHead(500);
    sendJson(res, { error: `Execute error: ${err.message}` });
  }
}

// ── Helpers ────────────────────────────────────────────────────────────────

// ── Pre-defined database profiles ──────────────────────────────────────────
const DB_PROFILES = {
  live: { server: '194.100.1.249', port: 1433, database: 'AINData', user: 'sa', password: 'ain06@sql' },
  demo: { server: '192.168.68.123', port: 1433, database: 'DevDB', user: 'sa', password: 'sdm14' },
};

async function handleSwitchDb(req, res) {
  const body = await readBody(req);
  const profile = (body.profile || '').toLowerCase();

  const preset = DB_PROFILES[profile];
  if (!preset) {
    res.writeHead(400);
    sendJson(res, { error: `Unknown profile "${body.profile}". Use "live" or "demo".` });
    return;
  }

  // Close existing pool
  if (pool) {
    try { await pool.close(); } catch (_) {}
    pool = null;
  }

  // Apply new config
  config.server   = preset.server;
  config.port     = preset.port;
  config.database = preset.database;
  config.user     = preset.user;
  config.password = preset.password;

  console.log(`[OK] Switched to ${profile.toUpperCase()} → ${config.server}:${config.port}/${config.database}`);
  sendJson(res, { switched: true, profile, server: config.server, database: config.database });
}



function readBody(req) {
  return new Promise((resolve, reject) => {
    let data = '';
    req.on('data', chunk => data += chunk);
    req.on('end', () => {
      try {
        resolve(data ? JSON.parse(data) : {});
      } catch (e) {
        resolve({});
      }
    });
    req.on('error', reject);
  });
}

function sendJson(res, obj) {
  if (!res.writableEnded) {
    res.end(JSON.stringify(obj));
  }
}

// ── Start ──────────────────────────────────────────────────────────────────

server.listen(serverPort, '0.0.0.0', () => {
  console.log('');
  console.log('=======================================================');
  console.log('  AINScan API Proxy Server (Node.js)');
  console.log('=======================================================');
  console.log(`  Listening on : http://0.0.0.0:${serverPort}`);
  console.log(`  DB Host      : ${config.server}:${config.port}`);
  console.log(`  DB Name      : ${config.database}`);
  console.log('-------------------------------------------------------');
  console.log('  Run Flutter web with:');
  console.log('  flutter run -d chrome \\');
  console.log(`    --dart-define=API_URL=http://localhost:${serverPort}`);
  console.log('=======================================================');
  console.log('');
});
