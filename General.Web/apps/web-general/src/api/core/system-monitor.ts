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
    disks: DiskItem[];
    environmentName: string;
    frameworkDescription: string;
    handleCount?: null | number;
    memoryUsageNote?: null | string;
    machineName: string;
    managedMemoryBytes: number;
    osArchitecture: string;
    osDescription: string;
    processArchitecture: string;
    processCpuUsagePercent?: null | number;
    processMemoryDisplayDenominatorBytes: number;
    processMemoryUsagePercent?: null | number;
    processStartTime: string;
    processorCount: number;
    systemCpuUsagePercent?: null | number;
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
