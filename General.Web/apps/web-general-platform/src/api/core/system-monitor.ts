import { requestClient } from '#/api/request';

export namespace SystemMonitorApi {
  export interface DiskItem {
    driveFormat: string;
    freeSpaceBytes: number;
    name: string;
    totalSizeBytes: number;
    type: string;
  }

  export interface MonitorItem {
    availableMemoryBytes: number;
    cpuTimeSeconds: number;
    cpuUsagePercent?: null | number;
    disks: DiskItem[];
    environmentName: string;
    frameworkDescription: string;
    handleCount?: null | number;
    machineName: string;
    managedMemoryBytes: number;
    memoryUsageNote?: null | string;
    osArchitecture: string;
    osDescription: string;
    processArchitecture: string;
    processMemoryDisplayDenominatorBytes: number;
    processMemoryUsagePercent?: null | number;
    processStartTime: string;
    processorCount: number;
    systemMemoryUsagePercent?: null | number;
    threadCount: number;
    totalMemoryBytes: number;
    uptimeMinutes: number;
    workingSetBytes: number;
  }
}

export async function getSystemMonitorApi() {
  return requestClient.get<SystemMonitorApi.MonitorItem>('/app/system-monitor');
}
