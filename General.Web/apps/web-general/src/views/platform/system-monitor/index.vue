<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Col, Descriptions, Empty, Row, Statistic } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getSystemMonitorApi, type SystemMonitorApi } from '#/api/core';

defineOptions({ name: 'PlatformSystemMonitorPage' });

const loading = ref(false);
const monitor = ref<null | SystemMonitorApi.MonitorItem>(null);

const diskColumns = [
  { align: 'left', field: 'name', title: '磁盘' },
  { field: 'type', title: '类型', width: 120 },
  { field: 'driveFormat', title: '格式', width: 120 },
  { field: 'totalSizeBytes', slots: { default: 'totalSizeBytes' }, title: '总容量', width: 140 },
  { field: 'freeSpaceBytes', slots: { default: 'freeSpaceBytes' }, title: '可用容量', width: 140 },
] satisfies VxeGridProps<SystemMonitorApi.DiskItem>['columns'];

const diskGridOptions = computed<VxeGridProps<SystemMonitorApi.DiskItem>>(() => ({
  columns: diskColumns,
  data: monitor.value?.disks || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'name' },
  size: 'small',
}));

const [DiskGrid] = useVbenVxeGrid<SystemMonitorApi.DiskItem>({ gridOptions: {} });

async function loadMonitor() {
  loading.value = true;
  try {
    monitor.value = await getSystemMonitorApi();
  } finally {
    loading.value = false;
  }
}

function formatBytes(value: number) {
  if (value <= 0) {
    return '0 B';
  }
  if (value < 1024) {
    return `${value} B`;
  }
  if (value < 1024 * 1024) {
    return `${(value / 1024).toFixed(1)} KB`;
  }
  if (value < 1024 * 1024 * 1024) {
    return `${(value / 1024 / 1024).toFixed(1)} MB`;
  }
  return `${(value / 1024 / 1024 / 1024).toFixed(1)} GB`;
}

function formatMemoryUsage(usedBytes?: number, totalBytes?: number) {
  return `${formatBytes(usedBytes || 0)}/${formatBytes(totalBytes || 0)}`;
}

function formatPercent(value?: null | number) {
  return value === null || value === undefined ? '-' : `${value.toFixed(2)}%`;
}

onMounted(loadMonitor);
</script>

<template>
  <Page description="系统监控提供当前服务器环境、进程资源占用和磁盘情况。" title="系统监控">
    <section class="platform-monitor">
      <Alert
        v-if="monitor?.cpuUsageNote"
        :message="monitor.cpuUsageNote"
        show-icon
        type="info"
      />
      <Alert
        v-if="monitor?.memoryUsageNote"
        :message="monitor.memoryUsageNote"
        show-icon
        type="info"
      />

      <Row :gutter="[16, 16]">
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false">
            <Statistic title="系统 CPU 使用率" :value="formatPercent(monitor?.systemCpuUsagePercent)" />
          </Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false">
            <Statistic title="进程 CPU 使用率" :value="formatPercent(monitor?.processCpuUsagePercent)" />
          </Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false">
            <Statistic
              title="系统内存使用"
              :value="formatMemoryUsage(monitor?.systemUsedMemoryBytes, monitor?.totalMemoryBytes)"
            />
          </Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false">
            <Statistic
              title="进程内存使用"
              :value="formatMemoryUsage(monitor?.workingSetBytes, monitor?.processMemoryDisplayDenominatorBytes)"
            />
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="12" :span="24">
          <Card class="rounded-2xl" :bordered="false" title="运行环境">
            <template #extra>
              <Button :loading="loading" type="primary" @click="loadMonitor">刷新</Button>
            </template>

            <div v-if="!loading && !monitor" class="platform-monitor__empty">
              <Empty description="暂无监控数据" />
            </div>
            <Descriptions v-else :column="1" bordered size="small">
              <Descriptions.Item label="环境">{{ monitor?.environmentName || '-' }}</Descriptions.Item>
              <Descriptions.Item label="机器名">{{ monitor?.machineName || '-' }}</Descriptions.Item>
              <Descriptions.Item label="操作系统">{{ monitor?.osDescription || '-' }}</Descriptions.Item>
              <Descriptions.Item label="系统架构">{{ monitor?.osArchitecture || '-' }}</Descriptions.Item>
              <Descriptions.Item label="进程架构">{{ monitor?.processArchitecture || '-' }}</Descriptions.Item>
              <Descriptions.Item label=".NET">{{ monitor?.frameworkDescription || '-' }}</Descriptions.Item>
              <Descriptions.Item label="进程启动">
                {{ monitor ? new Date(monitor.processStartTime).toLocaleString() : '-' }}
              </Descriptions.Item>
              <Descriptions.Item label="运行时长">
                {{ monitor ? `${monitor.uptimeMinutes.toFixed(1)} 分钟` : '-' }}
              </Descriptions.Item>
              <Descriptions.Item label="CPU 总耗时">
                {{ monitor ? `${monitor.cpuTimeSeconds.toFixed(1)} 秒` : '-' }}
              </Descriptions.Item>
              <Descriptions.Item label="进程 CPU">
                {{ monitor?.processCpuUsagePercent?.toFixed(2) || '0.00' }}%
              </Descriptions.Item>
              <Descriptions.Item label="系统 CPU">
                {{ monitor?.systemCpuUsagePercent?.toFixed(2) || '0.00' }}%
              </Descriptions.Item>
              <Descriptions.Item label="进程线程数">{{ monitor?.threadCount ?? '-' }}</Descriptions.Item>
              <Descriptions.Item label="句柄数">{{ monitor?.handleCount ?? '-' }}</Descriptions.Item>
              <Descriptions.Item label="总物理内存">
                {{ formatBytes(monitor?.totalMemoryBytes || 0) }}
              </Descriptions.Item>
              <Descriptions.Item label="工作集">
                {{ formatBytes(monitor?.workingSetBytes || 0) }}
              </Descriptions.Item>
              <Descriptions.Item label="系统内存占比">
                {{ formatPercent(monitor?.systemMemoryUsagePercent) }}
              </Descriptions.Item>
              <Descriptions.Item label="进程内存占比">
                {{ formatPercent(monitor?.processMemoryUsagePercent) }}
              </Descriptions.Item>
              <Descriptions.Item label="托管堆">
                {{ formatBytes(monitor?.managedMemoryBytes || 0) }}
              </Descriptions.Item>
              <Descriptions.Item label="可用内存上限">
                {{ formatBytes(monitor?.availableMemoryBytes || 0) }}
              </Descriptions.Item>
              <Descriptions.Item label="CPU 核心数">{{ monitor?.processorCount || 0 }}</Descriptions.Item>
            </Descriptions>
          </Card>
        </Col>

        <Col :lg="12" :span="24">
          <Card class="rounded-2xl" :bordered="false" title="磁盘资源">
            <DiskGrid class="app-vxe-card-grid" :grid-options="diskGridOptions">
              <template #totalSizeBytes="{ row }">
                {{ formatBytes(row.totalSizeBytes) }}
              </template>
              <template #freeSpaceBytes="{ row }">
                {{ formatBytes(row.freeSpaceBytes) }}
              </template>
            </DiskGrid>
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.platform-monitor {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.platform-monitor__empty {
  display: grid;
  min-height: 280px;
  place-items: center;
}

.platform-monitor__note {
  @apply mt-1.5;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}
</style>
