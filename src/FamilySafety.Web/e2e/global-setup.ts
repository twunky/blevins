import { spawnSync } from 'node:child_process';
import { randomUUID } from 'node:crypto';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import {
  capturesDirectory,
  isApiPortFree,
  listScenarioDatabases,
  repositoryRoot,
  runManifestVariable,
  scenarioRunsDirectory,
  writeManifest,
  type RunManifest,
} from './fixtures/isolated-scenario';

const preExistingListPath = resolve(repositoryRoot, 'scripts', 'pre-existing-scenario-databases.txt');

/**
 * WI-009-001 run setup: snapshot the existing databases, create this run's ownership manifest, and build the API once.
 * Each spec then starts its own API with `-NoBuild` through `useIsolatedScenario`.
 */
export default async function globalSetup(): Promise<void> {
  const preExistingList = existsSync(preExistingListPath)
    ? readFileSync(preExistingListPath, 'utf8').split(/\r?\n/).map(line => line.trim()).filter(line => line && !line.startsWith('#'))
    : [];
  if (preExistingList.length === 0) {
    throw new Error(`The PRE-EXISTING database list is missing or empty: ${preExistingListPath}`);
  }
  if (!(await isApiPortFree())) {
    throw new Error('Port 5080 is already in use. The harness does not stop unknown processes; free the port and rerun.');
  }

  const runId = `${new Date().toISOString().replace(/[-:]/g, '').replace(/\..*$/, '')}-${randomUUID().slice(0, 8)}`;
  const runDirectory = resolve(scenarioRunsDirectory, runId);
  mkdirSync(runDirectory, { recursive: true });
  writeFileSync(resolve(scenarioRunsDirectory, '.gitignore'), '# Local Playwright run manifests and API logs (WI-009-001).\n*\n', 'utf8');
  mkdirSync(capturesDirectory, { recursive: true });
  writeFileSync(resolve(capturesDirectory, '.gitignore'), '# Transient Playwright journey screenshots (WI-009-002).\n*\n', 'utf8');

  const manifest: RunManifest = {
    runId,
    createdAt: new Date().toISOString(),
    server: 'localhost',
    preExistingListPath,
    preExistingAtRunStart: listScenarioDatabases(),
    databases: [],
    runnerPid: process.pid,
  };
  const manifestPath = resolve(runDirectory, 'manifest.json');
  writeManifest(manifestPath, manifest);
  process.env[runManifestVariable] = manifestPath;
  console.log(`[scenario] Run ${runId}: manifest ${manifestPath}`);
  console.log(`[scenario] ${manifest.preExistingAtRunStart.length} FamilySafety* databases existed at run start; none will be removed.`);

  console.log('[scenario] Building the API once for this run...');
  const started = Date.now();
  const build = spawnSync('dotnet', [
    'build', resolve(repositoryRoot, 'src', 'FamilySafety.Api', 'FamilySafety.Api.csproj'), '--nologo', '-v', 'q',
    // No lingering MSBuild or compiler server processes after the run.
    '--disable-build-servers',
  ], {
    cwd: repositoryRoot,
    encoding: 'utf8',
    windowsHide: true,
    // Same allowance the scenario script makes for `dotnet run`.
    env: { ...process.env, WarningsNotAsErrors: process.env.WarningsNotAsErrors ? `${process.env.WarningsNotAsErrors};NU1900` : 'NU1900' },
  });
  if (build.status !== 0) {
    throw new Error(`API build failed:\n${build.stdout}\n${build.stderr}`);
  }
  console.log(`[scenario] API built in ${Math.round((Date.now() - started) / 1000)} s.`);
}
