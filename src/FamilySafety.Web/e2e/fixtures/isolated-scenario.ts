/**
 * Per-spec isolated scenarios for the Playwright suite (WI-009-001).
 *
 * Usage: call `useIsolatedScenario({ tripAudienceAdult })` once at the top of a spec file. For that file it
 *  1. confirms API port 5080 is free (setup fails if another process holds it; that process is never stopped),
 *  2. creates a fresh `FamilySafety_Invitation_E2E_<hex>` name and records it in the run ownership manifest,
 *  3. starts the API with `scripts/start-invitation-scenario.ps1 -NoBuild`, which migrates and seeds the new
 *     database, and waits for `/health`,
 *  4. after the file's tests, stops the API process tree and confirms the port is free, then
 *  5. removes the database through `scripts/remove-scenario-database.ps1` only if every test in the file passed.
 *     A failed spec's database is retained, marked `retained` in the manifest, and reported with its removal command.
 *     Set FAMILY_SAFETY_E2E_KEEP_DATABASES=1 to retain every database.
 *
 * Tests in one file share that file's scenario. The Angular dev server is shared by the whole run, and the API is
 * built once by `global-setup.ts`. See docs/local-development.md, "Isolated Playwright scenarios".
 */
import { test } from '@playwright/test';
import { spawn, spawnSync, type ChildProcess } from 'node:child_process';
import { randomUUID } from 'node:crypto';
import { closeSync, existsSync, mkdirSync, openSync, readFileSync, renameSync, writeFileSync } from 'node:fs';
import { createServer } from 'node:net';
import { basename, dirname, resolve } from 'node:path';

/** What a spec file needs seeded in its scenario. Adults A and B are always present; B starts with no membership. */
export interface ScenarioOptions {
  /** Seed Adult C ("Development Audience Member") as an active Family member (`-IncludeTripAudienceAdult`). */
  tripAudienceAdult: boolean;
  /**
   * Seed "Development Second Family" with A and B as active members (`-IncludeSecondFamily`; WI-009-005, OD-E).
   * Off unless a spec requests it. Test support only; it seeds no items.
   */
  secondFamily?: boolean;
}

export type ScenarioOutcome = 'pending' | 'passed' | 'failed';
export type ScenarioStatus = 'starting' | 'running' | 'removed' | 'retained' | 'cleanup-failed';

/** One earlier lifecycle state of an entry (WI-009-002). History is only ever appended to. */
export interface ScenarioHistoryEntry {
  at: string;
  from: ScenarioStatus;
  to: ScenarioStatus;
  by: string;
  note?: string;
  previousNote?: string;
}

export interface ScenarioRecord {
  name: string;
  spec: string;
  scenario: ScenarioOptions;
  createdAt: string;
  apiPid: number | null;
  apiLog: string;
  outcome: ScenarioOutcome;
  status: ScenarioStatus;
  note?: string;
  /** Set by `remove-scenario-database.ps1` after a confirmed drop. Absent in manifests written before WI-009-002. */
  removedAt?: string;
  removedBy?: 'automatic' | 'manual';
  history?: ScenarioHistoryEntry[];
}

export interface RunManifest {
  runId: string;
  createdAt: string;
  server: string;
  preExistingListPath: string;
  /** Every FamilySafety* database present when the run started. None of them is ever removed. */
  preExistingAtRunStart: string[];
  /** Databases this run created. Only these can ever be removed, one at a time, by the guarded script. */
  databases: ScenarioRecord[];
  /** The Playwright runner process, so `scripts/test-artifacts.ps1` can tell an active run from an interrupted one. */
  runnerPid?: number;
  /** Written by `global-teardown.ts`. A manifest without it belongs to an active or interrupted run (or predates WI-009-002). */
  completedAt?: string;
}

export const scenarioDatabasePattern = /^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$/;
/** Run IDs that global-setup.ts generates, for example `20261002T061132-1247b7e7`. */
export const runIdPattern = /^\d{8}T\d{6}-[0-9a-f]{8}$/;
export const repositoryRoot = resolve(__dirname, '../../../..');
/** Run manifests and API logs. Kept between runs while a run's databases exist; see `scripts/test-artifacts.ps1`. */
export const scenarioRunsDirectory = resolve(__dirname, '..', 'scenario-runs');
/** Transient journey screenshots (WI-009-002). Routine runs never write under `ai-context/`. */
export const capturesDirectory = resolve(__dirname, '..', 'captures');
export const apiPort = 5080;
export const runManifestVariable = 'FAMILY_SAFETY_E2E_RUN_MANIFEST';
const apiHealthUrl = `http://localhost:${apiPort}/health`;
const scriptsDirectory = resolve(repositoryRoot, 'scripts');

let currentDatabase: string | null = null;

/** The current spec file's scenario database. Throws outside a spec that called `useIsolatedScenario`. */
export function currentScenarioDatabase(): string {
  if (!currentDatabase) {
    throw new Error('No isolated scenario is active. Call useIsolatedScenario(...) at the top of the spec file.');
  }
  return currentDatabase;
}

export function useIsolatedScenario(options: ScenarioOptions): void {
  let api: ChildProcess | null = null;
  let databaseName: string | null = null;
  let setupCompleted = false;
  let anyTestFailed = false;

  test.beforeAll(async ({}, testInfo) => {
    // Same allowance the API webServer entry had before WI-009-001 (180 s); not raised.
    testInfo.setTimeout(180_000);
    const manifestPath = requireManifestPath();
    await assertApiPortFree('before starting the scenario API');

    const name = `FamilySafety_Invitation_E2E_${randomUUID().replaceAll('-', '')}`;
    if (!scenarioDatabasePattern.test(name)) {
      throw new Error(`Generated an unsafe scenario database name: ${name}`);
    }
    if (listScenarioDatabases().some(existing => existing.toLowerCase() === name.toLowerCase())) {
      throw new Error(`Scenario database ${name} already exists; refusing to reuse it.`);
    }

    // Ownership is recorded before the API can create the database.
    const apiLog = resolve(dirname(manifestPath), `${name}.api.log`);
    const spec = basename(testInfo.file);
    updateManifest(manifestPath, manifest => {
      manifest.databases.push({
        name, spec, scenario: { ...options }, createdAt: new Date().toISOString(),
        apiPid: null, apiLog, outcome: 'pending', status: 'starting',
      });
    });
    databaseName = name;

    const args = [
      '-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass',
      '-File', resolve(scriptsDirectory, 'start-invitation-scenario.ps1'),
      '-ScenarioName', name.slice('FamilySafety_Invitation_'.length),
      '-NoBuild',
    ];
    if (options.tripAudienceAdult) {
      args.push('-IncludeTripAudienceAdult');
    }
    if (options.secondFamily) {
      args.push('-IncludeSecondFamily');
    }
    const logFile = openSync(apiLog, 'a');
    try {
      api = spawn('powershell.exe', args, { cwd: repositoryRoot, stdio: ['ignore', logFile, logFile], windowsHide: true });
    } finally {
      closeSync(logFile);
    }
    const pid = api.pid ?? null;
    updateManifest(manifestPath, manifest => { record(manifest, name).apiPid = pid; });
    console.log(`[scenario] ${spec}: starting API (pid ${pid}) on port ${apiPort} for ${name}`
      + ` (tripAudienceAdult=${options.tripAudienceAdult}, secondFamily=${options.secondFamily === true})`);

    await waitForHealthyApi(api, apiLog);
    updateManifest(manifestPath, manifest => { record(manifest, name).status = 'running'; });
    currentDatabase = name;
    setupCompleted = true;
  });

  test.afterEach(async ({}, testInfo) => {
    if (testInfo.status !== testInfo.expectedStatus) {
      anyTestFailed = true;
    }
  });

  test.afterAll(async ({}, testInfo) => {
    currentDatabase = null;
    const manifestPath = requireManifestPath();
    const spec = basename(testInfo.file);
    const name = databaseName;
    databaseName = null;

    // 1. Stop this spec's API process tree and confirm the port is free. Never clean up before this succeeds.
    let stopError: Error | null = null;
    if (api) {
      try {
        await stopProcessTree(api);
        await waitForApiPortFree();
        console.log(`[scenario] ${spec}: API stopped; port ${apiPort} is free`);
      } catch (error) {
        stopError = error as Error;
      }
      api = null;
    }
    if (!name) {
      if (stopError) throw stopError;
      return;
    }

    // 2. Decide: remove only when the spec passed, the API stopped, and retention was not requested.
    const passed = setupCompleted && !anyTestFailed;
    const keepAll = process.env.FAMILY_SAFETY_E2E_KEEP_DATABASES === '1';
    updateManifest(manifestPath, manifest => { record(manifest, name).outcome = passed ? 'passed' : 'failed'; });

    if (!passed || keepAll || stopError) {
      const reason = [
        !passed && 'spec failed',
        keepAll && 'FAMILY_SAFETY_E2E_KEEP_DATABASES=1',
        stopError && 'API stop not confirmed',
      ].filter(Boolean).join('; ');
      updateManifest(manifestPath, manifest => { setStatus(record(manifest, name), 'retained', 'harness', reason); });
      reportRetained(name, spec, reason, manifestPath);
      if (stopError) throw stopError;
      return;
    }

    // 3. Guarded removal: exactly one database, owned by this run. No retry on failure.
    //    The script itself records `removed`, `removedAt`, `removedBy` and a history entry after a confirmed drop.
    const removal = runPowerShellFile('remove-scenario-database.ps1', [
      '-DatabaseName', name, '-ManifestPath', manifestPath,
    ]);
    console.log(removal.output.trim());
    const gone = !listScenarioDatabases().some(existing => existing.toLowerCase() === name.toLowerCase());
    if ((removal.status === 0 || removal.status === 4) && gone) {
      if (record(readManifest(manifestPath), name).status !== 'removed') {
        // Exit 4: dropped and confirmed gone, but the script could not write the manifest. Record it here instead.
        updateManifest(manifestPath, manifest => {
          const entry = record(manifest, name);
          setStatus(entry, 'removed', 'automatic', `remove-scenario-database.ps1 exit ${removal.status}; manifest recorded by the harness`);
          entry.removedAt = new Date().toISOString();
          entry.removedBy = 'automatic';
        });
      }
      console.log(`[scenario] ${spec}: removed ${name} (confirmed absent from sys.databases)`);
      return;
    }
    updateManifest(manifestPath, manifest => {
      setStatus(record(manifest, name), 'cleanup-failed', 'harness',
        `remove-scenario-database.ps1 exit ${removal.status}; still present: ${!gone}`);
    });
    throw new Error(`Cleanup of ${name} failed (exit ${removal.status}). It was not retried. Manifest: ${manifestPath}`);
  });
}

export function reportRetained(name: string, spec: string, reason: string, manifestPath: string): void {
  const message = [
    `[scenario] RETAINED DATABASE: ${name} (${spec}; ${reason})`,
    `[scenario]   Review it (dry run): .\\scripts\\test-artifacts.ps1 -RemoveDatabase ${name}`,
    `[scenario]   Remove it later with: .\\scripts\\test-artifacts.ps1 -RemoveDatabase ${name} -Execute`
      + ` (owning manifest: ${manifestPath})`,
  ].join('\n');
  console.log(message);
}

/** Captures folder for one spec's transient journey screenshots: `e2e/captures/<spec>/`. Never under `ai-context/`. */
export function captureDirectory(spec: string): string {
  if (!/^[a-z0-9-]+$/.test(spec)) {
    throw new Error(`Invalid capture folder name: ${spec}`);
  }
  const directory = resolve(capturesDirectory, spec);
  mkdirSync(directory, { recursive: true });
  return directory;
}

/** Changes an entry's current status and appends the earlier state to its history (WI-009-002, D5). */
export function setStatus(entry: ScenarioRecord, to: ScenarioStatus, by: string, note: string): void {
  const change: ScenarioHistoryEntry = { at: new Date().toISOString(), from: entry.status, to, by, note };
  if (entry.note !== undefined && entry.note !== note) {
    change.previousNote = entry.note;
  }
  entry.history = [...(entry.history ?? []), change];
  entry.status = to;
  entry.note = note;
}

// ---- Run manifest -------------------------------------------------------------------------------------------

export function requireManifestPath(): string {
  const manifestPath = process.env[runManifestVariable];
  if (!manifestPath || !existsSync(manifestPath)) {
    throw new Error(`${runManifestVariable} is not set or missing; e2e/global-setup.ts must create the run manifest.`);
  }
  return manifestPath;
}

export function readManifest(manifestPath: string): RunManifest {
  return JSON.parse(readFileSync(manifestPath, 'utf8')) as RunManifest;
}

export function writeManifest(manifestPath: string, manifest: RunManifest): void {
  const temporary = `${manifestPath}.tmp`;
  writeFileSync(temporary, `${JSON.stringify(manifest, null, 2)}\n`, 'utf8');
  renameSync(temporary, manifestPath);
}

function updateManifest(manifestPath: string, change: (manifest: RunManifest) => void): void {
  const manifest = readManifest(manifestPath);
  change(manifest);
  writeManifest(manifestPath, manifest);
}

function record(manifest: RunManifest, name: string): ScenarioRecord {
  const entries = manifest.databases.filter(entry => entry.name === name);
  if (entries.length !== 1) {
    throw new Error(`Run manifest records ${name} ${entries.length} times; expected exactly one.`);
  }
  return entries[0];
}

// ---- PowerShell and SQL --------------------------------------------------------------------------------------

export function runPowerShellFile(script: string, args: string[]): { status: number; output: string } {
  const result = spawnSync('powershell.exe', [
    '-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass',
    '-File', resolve(scriptsDirectory, script), ...args,
  ], { cwd: repositoryRoot, encoding: 'utf8', windowsHide: true });
  return { status: result.status ?? -1, output: `${result.stdout ?? ''}${result.stderr ?? ''}` };
}

/** Exact names of every FamilySafety* database on the local server (read-only query). */
export function listScenarioDatabases(): string[] {
  const result = runPowerShellFile('list-scenario-databases.ps1', []);
  if (result.status !== 0) {
    throw new Error(`Could not list databases: ${result.output}`);
  }
  return result.output.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
}

// ---- API process and port ------------------------------------------------------------------------------------

async function waitForHealthyApi(api: ChildProcess, apiLog: string): Promise<void> {
  const deadline = Date.now() + 170_000;
  while (Date.now() < deadline) {
    if (api.exitCode !== null) {
      throw new Error(`Scenario API exited with code ${api.exitCode} before it was healthy. Log: ${apiLog}`);
    }
    try {
      const response = await fetch(apiHealthUrl);
      if (response.ok && (await response.text()).trim() === 'Healthy') {
        return;
      }
    } catch {
      // Not listening yet.
    }
    await delay(500);
  }
  throw new Error(`Scenario API did not report Healthy at ${apiHealthUrl}. Log: ${apiLog}`);
}

interface ProcessRow { ProcessId: number; ParentProcessId: number; Name: string; CommandLine: string | null; Created: number }

export function listProcesses(): ProcessRow[] {
  const result = spawnSync('powershell.exe', [
    '-NoLogo', '-NoProfile', '-NonInteractive', '-Command',
    'Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId, Name, CommandLine,'
      + ' @{ Name = "Created"; Expression = { [long]($_.CreationDate.ToFileTimeUtc() / 10000) } } | ConvertTo-Json -Compress',
  ], { encoding: 'utf8', windowsHide: true, maxBuffer: 32 * 1024 * 1024 });
  if (result.status !== 0) {
    throw new Error(`Could not list processes: ${result.stderr}`);
  }
  return JSON.parse(result.stdout) as ProcessRow[];
}

/**
 * Windows keeps a dead parent's PID in ParentProcessId, and PIDs are reused, so an unrelated older process can
 * appear to be a child. A real child is never created before its parent.
 */
function descendantsOf(rootPid: number, rows: ProcessRow[]): number[] {
  const byId = new Map(rows.map(row => [row.ProcessId, row]));
  const found: number[] = [];
  const queue = [rootPid];
  while (queue.length) {
    const parent = byId.get(queue.shift()!);
    if (!parent) continue;
    for (const row of rows) {
      if (row.ParentProcessId === parent.ProcessId && row.Created >= parent.Created
        && row.ProcessId !== rootPid && !found.includes(row.ProcessId)) {
        found.push(row.ProcessId);
        queue.push(row.ProcessId);
      }
    }
  }
  return found;
}

export function isAlive(pid: number): boolean {
  try {
    process.kill(pid, 0);
    return true;
  } catch {
    return false;
  }
}

/** Stops a process the harness started, and every descendant (dotnet run, the API host), then confirms all exited. */
export async function stopProcessTree(child: ChildProcess | number): Promise<void> {
  const rootPid = typeof child === 'number' ? child : child.pid;
  if (!rootPid) return;
  const tree = [rootPid, ...descendantsOf(rootPid, listProcesses())];
  spawnSync('taskkill', ['/PID', String(rootPid), '/T', '/F'], { windowsHide: true });
  const deadline = Date.now() + 15_000;
  while (Date.now() < deadline) {
    if (!tree.some(isAlive)) return;
    await delay(250);
  }
  const survivors = tree.filter(isAlive);
  throw new Error(`API process tree did not exit (still running: ${survivors.join(', ')}).`);
}

function canListen(host: string): Promise<'free' | 'in-use' | 'unavailable'> {
  return new Promise(resolvePromise => {
    const server = createServer();
    server.once('error', (error: NodeJS.ErrnoException) => {
      resolvePromise(error.code === 'EADDRINUSE' || error.code === 'EACCES' ? 'in-use' : 'unavailable');
    });
    server.listen({ port: apiPort, host, exclusive: true }, () => server.close(() => resolvePromise('free')));
  });
}

export async function isApiPortFree(): Promise<boolean> {
  const results = await Promise.all([canListen('127.0.0.1'), canListen('::1')]);
  return !results.includes('in-use');
}

async function assertApiPortFree(context: string): Promise<void> {
  if (!(await isApiPortFree())) {
    throw new Error(`Port ${apiPort} is already in use ${context}. The harness does not stop unknown processes; `
      + 'stop whatever is listening on the port and rerun.');
  }
}

async function waitForApiPortFree(): Promise<void> {
  const deadline = Date.now() + 15_000;
  while (Date.now() < deadline) {
    if (await isApiPortFree()) return;
    await delay(250);
  }
  throw new Error(`Port ${apiPort} is still in use after the scenario API stopped.`);
}

function delay(milliseconds: number): Promise<void> {
  return new Promise(resolvePromise => setTimeout(resolvePromise, milliseconds));
}
