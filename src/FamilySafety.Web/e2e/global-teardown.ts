import { existsSync, lstatSync, rmSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import {
  isAlive,
  isApiPortFree,
  listProcesses,
  listScenarioDatabases,
  readManifest,
  reportRetained,
  runIdPattern,
  runManifestVariable,
  scenarioRunsDirectory,
  setStatus,
  stopProcessTree,
  writeManifest,
} from './fixtures/isolated-scenario';

/**
 * WI-009-001 run teardown, extended by WI-009-002. It never removes a database. It
 *  - stops any scenario API this run started that is somehow still alive (only a PID recorded in this run's
 *    manifest whose command line names that scenario), and marks its database retained, keeping the earlier
 *    status in the entry's history;
 *  - records `completedAt`, so `scripts/test-artifacts.ps1` can tell a finished run from an interrupted one;
 *  - reports every retained database with its removal command;
 *  - confirms port 5080 is free and no FamilySafety.Api process remains;
 *  - removes this run's own `scenario-runs/<run id>/` folder only when every database the run created passed, is
 *    recorded `removed`, and is confirmed absent by a live query, and FAMILY_SAFETY_E2E_KEEP_RUN_FILES is not `1`.
 *    A run with any failed, retained, or unconfirmed database keeps its folder.
 */
export default async function globalTeardown(): Promise<void> {
  const manifestPath = process.env[runManifestVariable];
  if (!manifestPath) return;
  const manifest = readManifest(manifestPath);

  for (const entry of manifest.databases.filter(item => item.status === 'starting' || item.status === 'running')) {
    const scenarioName = entry.name.slice('FamilySafety_Invitation_'.length);
    const owned = entry.apiPid !== null && isAlive(entry.apiPid)
      && listProcesses().some(row => row.ProcessId === entry.apiPid && (row.CommandLine ?? '').includes(scenarioName));
    if (owned) {
      await stopProcessTree(entry.apiPid!);
    }
    setStatus(entry, 'retained', 'teardown', 'spec did not finish normally; outcome unknown');
  }
  manifest.completedAt = new Date().toISOString();
  writeManifest(manifestPath, manifest);

  const removed = manifest.databases.filter(entry => entry.status === 'removed');
  const retained = manifest.databases.filter(entry => entry.status !== 'removed');
  console.log(`[scenario] Run ${manifest.runId}: ${manifest.databases.length} scenario database(s) created, `
    + `${removed.length} removed, ${retained.length} retained.`);
  for (const entry of retained) {
    reportRetained(entry.name, entry.spec, entry.note ?? entry.status, manifestPath);
  }

  // The API runs as `dotnet run` plus the FamilySafety.Api host; other programs that merely mention the path are ignored.
  const leftovers = listProcesses().filter(row => /^(dotnet|FamilySafety\.Api)\.exe$/i.test(row.Name)
    && /FamilySafety\.Api/i.test(row.CommandLine ?? ''));
  const portFree = await isApiPortFree();
  console.log(`[scenario] Port 5080 free: ${portFree}. FamilySafety.Api processes remaining: ${leftovers.length}.`);
  if (!portFree || leftovers.length) {
    throw new Error(`Run ended with port 5080 in use or API processes remaining: ${leftovers.map(row => row.ProcessId).join(', ')}`);
  }

  removeCleanRunFolder(manifestPath);
}

/** Deletes this run's own folder only when the run is fully clean (WI-009-002, D3). Any doubt keeps the folder. */
function removeCleanRunFolder(manifestPath: string): void {
  const manifest = readManifest(manifestPath);
  const runDirectory = resolve(scenarioRunsDirectory, manifest.runId);
  const keep = (reason: string) => console.log(`[scenario] Run files kept: ${runDirectory} (${reason}).`);

  if (process.env.FAMILY_SAFETY_E2E_KEEP_RUN_FILES === '1') {
    keep('FAMILY_SAFETY_E2E_KEEP_RUN_FILES=1');
    return;
  }
  if (!runIdPattern.test(manifest.runId) || resolve(dirname(manifestPath)) !== runDirectory
    || !existsSync(runDirectory) || lstatSync(runDirectory).isSymbolicLink()) {
    keep('the folder is not exactly scenario-runs/<run id>');
    return;
  }
  if (manifest.databases.length === 0) {
    keep('the run created no scenario database, so it cannot be shown to be successful');
    return;
  }
  if (!manifest.databases.every(entry => entry.outcome === 'passed' && entry.status === 'removed')) {
    keep('at least one database failed, was retained, or was not removed');
    return;
  }
  let present: string[];
  try {
    present = listScenarioDatabases().map(name => name.toLowerCase());
  } catch (error) {
    keep(`the database list could not be queried: ${(error as Error).message}`);
    return;
  }
  const stillPresent = manifest.databases.filter(entry => present.includes(entry.name.toLowerCase()));
  if (stillPresent.length) {
    keep(`still present on the server: ${stillPresent.map(entry => entry.name).join(', ')}`);
    return;
  }

  console.log(`[scenario] Removing run files of fully successful run: ${runDirectory}`);
  rmSync(runDirectory, { recursive: true });
  console.log(`[scenario] Run files removed (confirmed absent: ${!existsSync(runDirectory)}).`
    + ' Set FAMILY_SAFETY_E2E_KEEP_RUN_FILES=1 to keep them.');
}
