const fs = require("fs");
const path = require("path");
require("dotenv").config(); //Läser env om den finns

//Fallback (temporarily using HTTP to avoid SSL issues)
const DEFAULT_API = "http://localhost:5184";
const DEFAULT_HUB = "ws://localhost:5184/hubs/bookings";
const DEFAULT_IOT_API = "http://localhost:5101";
const DEFAULT_TELEMETRY_HUB = "ws://localhost:5103/hub/telemetry";
const DEFAULT_LOGIN_REDIRECT = "http://localhost:4200/profil";
const DEFAULT_LOGOUT_REDIRECT = "http://localhost:4200/logga-in";

//Läser värden från env
const apiUrl = process.env.NG_APP_API_URL || DEFAULT_API;
const hubUrl = process.env.NG_APP_HUB_URL || DEFAULT_HUB;
const iotApiUrl = process.env.NG_APP_IOT_API_URL || DEFAULT_IOT_API;
const telemetryHubUrl =
  process.env.NG_APP_TELEMETRY_HUB_URL || DEFAULT_TELEMETRY_HUB;
const loginRedirectUrl =
  process.env.NG_APP_LOGIN_REDIRECT_URL || DEFAULT_LOGIN_REDIRECT;
const logoutRedirectUrl =
  process.env.NG_APP_LOGOUT_REDIRECT_URL || DEFAULT_LOGOUT_REDIRECT;

const outFile = path.resolve(__dirname, "../src/assets/env.js");
const content = `window.__env = {
  NG_APP_API_URL: '${apiUrl}',
  NG_APP_HUB_URL: '${hubUrl}',
  NG_APP_IOT_API_URL: '${iotApiUrl}',
  NG_APP_TELEMETRY_HUB_URL: '${telemetryHubUrl}',
  NG_APP_LOGIN_REDIRECT_URL: '${loginRedirectUrl}',
  NG_APP_LOGOUT_REDIRECT_URL: '${logoutRedirectUrl}'
};\n`;

fs.mkdirSync(path.dirname(outFile), { recursive: true });
fs.writeFileSync(outFile, content, "utf8");
console.log("[env] Wrote", outFile, "with:", {
  apiUrl,
  hubUrl,
  iotApiUrl,
  telemetryHubUrl,
  loginRedirectUrl,
  logoutRedirectUrl,
});
