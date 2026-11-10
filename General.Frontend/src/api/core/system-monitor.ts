import type { createApiClient } from '../client';

export namespace SystemMonitorApi {
  export interface Disk {
    driveFormat: string;
    freeSpaceBytes: number;
    name: string;
    totalSizeBytes: number;
    type: string;
  }
  export interface Snapshot {
    availableMemoryBytes: number;
    cpuUsageNote: null | string;
    memoryUsageNote: null | string;
    processCpuUsagePercent: null | number;
    systemCpuUsagePercent: null | number;
    cpuTimeSeconds: number;
    disks: Disk[];
    environmentName: string;
    frameworkDescription: string;
    handleCount: null | number;
    managedMemoryBytes: number;
    machineName: string;
    osArchitecture: string;
    osDescription: string;
    processorCount: number;
    processArchitecture: string;
    processMemoryDisplayDenominatorBytes: number;
    processStartTime: string;
    processMemoryUsagePercent: null | number;
    systemMemoryUsagePercent: null | number;
    systemUsedMemoryBytes: number;
    totalMemoryBytes: number;
    threadCount: number;
    uptimeMinutes: number;
    workingSetBytes: number;
  }
}

export function createSystemMonitorApi(client: ReturnType<typeof createApiClient>) {
  return {
    getSnapshot: (signal?: AbortSignal) => client.request<SystemMonitorApi.Snapshot>('/app/system-monitor', { signal }),
  };
}
